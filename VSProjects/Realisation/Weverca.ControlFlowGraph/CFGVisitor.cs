﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using PHP.Core.AST;
using PHP.Core.Parsers;
using PHP.Core.Reflection;

namespace Weverca.ControlFlowGraph
{
    class LoopData
    {
        public BasicBlock LoopStart { get; private set; }
        public BasicBlock LoopEnd { get; set; }

        public LoopData(BasicBlock loopStart)
        {
            LoopStart = loopStart;
        }
        public LoopData(BasicBlock loopStart, BasicBlock loopEnd)
        {
            LoopStart = loopStart;
            LoopEnd = loopEnd;
        }
    }

    class CFGVisitor : TreeVisitor
    {
        ControlFlowGraph graph;
        BasicBlock currentBasicBlock;

        LinkedList<LoopData> loopData = new LinkedList<LoopData>();
        private Dictionary<string, BasicBlock> labels = new Dictionary<string, BasicBlock>();


        public CFGVisitor(ControlFlowGraph graph)
        {
            this.graph = graph;
            currentBasicBlock = new BasicBlock();
            graph.start = currentBasicBlock;
        }

        public override void VisitElement(LangElement element)
        {
            currentBasicBlock.AddElement(element);
        }

        public override void VisitGlobalCode(GlobalCode x)
        {
            foreach(Statement statement in x.Statements)
            {
                statement.VisitMe(this);
            }
        }

        public override void VisitLabelStmt(LabelStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitGotoStmt(GotoStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitIfStmt(IfStmt x)
        {
            //Merge destination for if and else branch
            BasicBlock bottomBox = new BasicBlock();

            foreach (var condition in x.Conditions)
            {
                if (condition.Condition != null)
                {
                    //IF or ELSEIF branch
                    currentBasicBlock = constructIfBranch(bottomBox, condition);
                }
                else
                {
                    //ELSE branch
                    condition.Statement.VisitMe(this);
                }
            }

            //Connect else branch to bottomBox
            //Must be here becouse in the construc phase we dont know whether the else block would split in the future
            BasicBlockEdge.MakeNewAndConnect(currentBasicBlock, bottomBox, new BoolLiteral(Position.Invalid, true));
            currentBasicBlock = bottomBox;
        }


        /// <summary>
        /// Constructs if branch basic block.
        /// </summary>
        /// <param name="bottomBox">Merge destination for if and else branch.</param>
        /// <param name="condition">The condition of the if branch.</param>
        /// <returns>Empty basic block for the else branch</returns>
        private BasicBlock constructIfBranch(BasicBlock bottomBox, ConditionalStmt condition)
        {
            BasicBlock thenBranchBlock = new BasicBlock();
            BasicBlockEdge.MakeNewAndConnect(currentBasicBlock, thenBranchBlock, condition.Condition);

            BasicBlock elseBranchBlock = new BasicBlock();
            currentBasicBlock.EsleEdge= elseBranchBlock;

            currentBasicBlock = thenBranchBlock;
            condition.Statement.VisitMe(this);
            BasicBlockEdge.MakeNewAndConnect(currentBasicBlock, bottomBox, new BoolLiteral(Position.Invalid, true));

            return elseBranchBlock;
        }


        public override void VisitForeachStmt(ForeachStmt x)
        {
            base.VisitForeachStmt(x);
        }

        public override void VisitForStmt(ForStmt x)
        {
            BasicBlock forTest = new BasicBlock();
            BasicBlock forBody = new BasicBlock();
            BasicBlock forEnd = new BasicBlock();

            //Adds initial connection from previos to the test block
            BasicBlockEdge.MakeNewAndConnect(currentBasicBlock, forTest, new BoolLiteral(Position.Invalid, true));

            //Adds connection into the loop body
            Expression forCondition = constructSimpleCondition(x.CondExList);
            BasicBlockEdge.MakeNewAndConnect(forTest, forBody, forCondition);

            //Adds connection behind the cycle
            
           forTest.EsleEdge=forEnd;

            //Loop body
            VisitExpressionList(x.InitExList);
            currentBasicBlock = forBody;
            x.Body.VisitMe(this);
            VisitExpressionList(x.ActionExList);

            //Adds loop connection to test block
            BasicBlockEdge.MakeNewAndConnect(currentBasicBlock, forTest, new BoolLiteral(Position.Invalid, true));

            currentBasicBlock = forEnd;
        }

        /// <summary>
        /// Constructs the simple condition from the given list.
        /// </summary>
        /// <param name="conditionList">The condition list.</param>
        /// <returns></returns>
        private Expression constructSimpleCondition(List<Expression> conditionList)
        {
            Expression groupCondition;
            if (conditionList.Count > 0)
            {
                groupCondition = conditionList[0];
                for (int index = 1; index < conditionList.Count; index++)
                {
                    Position newPosition = mergePositions(groupCondition.Position, conditionList[index].Position);
                    groupCondition = new BinaryEx(newPosition, Operations.And, groupCondition, conditionList[index]);
                }
            }
            else
            {
                groupCondition = new BoolLiteral(Position.Invalid, true);
            }

            return groupCondition;
        }

        /// <summary>
        /// Merges the positions.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        /// <returns></returns>
        private Position mergePositions(Position first, Position last)
        {
            return new Position(
                first.FirstLine,
                first.FirstColumn,
                first.FirstOffset,
                last.LastLine,
                last.LastColumn,
                last.LastOffset);
        }


        public override void VisitSwitchStmt(SwitchStmt x)
        {
            BasicBlock above=currentBasicBlock;
            BasicBlock last;
            bool containsDefault=false;
            currentBasicBlock = new BasicBlock();
            foreach (var switchItem in x.SwitchItems) {

                Expression right = null;
                if (switchItem.GetType() == typeof(CaseItem))
                {
                    right = ((CaseItem)switchItem).CaseVal;
                    BasicBlockEdge.MakeNewAndConnect(above, currentBasicBlock, new BinaryEx(Operations.Equal, x.SwitchValue, right));
                }
                else 
                {
                    above.EsleEdge=currentBasicBlock;
                    if (containsDefault == false)
                    {
                        containsDefault = true;
                    }
                    else {
                        throw new Exception("more than one default in switch");
                    }
                }
                
                
                switchItem.VisitMe(this);
                last = currentBasicBlock;
                currentBasicBlock = new BasicBlock();
                BasicBlockEdge.MakeNewAndConnect(last, currentBasicBlock, new BoolLiteral(Position.Invalid, true));
                
                
                
                
            }
            
            if (containsDefault == false)
            {
                above.EsleEdge=currentBasicBlock;
            }
        }

        public override void VisitCaseItem(CaseItem x)
        {
            VisitStatementList(x.Statements);
        }

        public override void VisitDefaultItem(DefaultItem x)
        {
            VisitStatementList(x.Statements);
        }


        public override void VisitJumpStmt(JumpStmt x)
        {
            throw new NotImplementedException();
        }

        public override void VisitWhileStmt(WhileStmt x)
        {
            BasicBlock aboveLoop = currentBasicBlock;
            BasicBlock startLoop = new BasicBlock();
            if (x.LoopType == WhileStmt.Type.While)
            {
                BasicBlockEdge.MakeNewAndConnect(aboveLoop, startLoop, x.CondExpr);
            }
            else
            {
                BasicBlockEdge.MakeNewAndConnect(aboveLoop, startLoop, new BoolLiteral(Position.Invalid, true));
            }
            currentBasicBlock = startLoop;
            x.Body.VisitMe(this);
            BasicBlock endLoop = currentBasicBlock;
            BasicBlock underLoop = new BasicBlock();
            endLoop.EsleEdge=underLoop;
            if (x.LoopType == WhileStmt.Type.While)
            {
                aboveLoop.EsleEdge=underLoop;
            }
            BasicBlockEdge.MakeNewAndConnect(endLoop, startLoop, x.CondExpr);

            currentBasicBlock = underLoop;
        }


       

        public override void VisitBlockStmt(BlockStmt x)
        {
            VisitStatementList(x.Statements);

        }

        #region Forwarding to VisitStatementList or VisitExpressionList

        private void VisitStatementList(List<Statement> list)
        {
            foreach (var stmt in list)
                stmt.VisitMe(this);
        }

        private void VisitExpressionList(List<Expression> list)
        {
            foreach (var e in list)
                e.VisitMe(this);
        }

        #endregion

        #region Forwarding to default VisitElement

        public override void VisitStringLiteral(StringLiteral x)
        {
            this.VisitElement(x);
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            this.VisitElement(x);
        }

        public override void VisitConstantUse(ConstantUse x)
        {
            this.VisitElement(x);
        }

        public override void VisitEchoStmt(EchoStmt x)
        {
            this.VisitElement(x);
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            this.VisitElement(x);
        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            this.VisitElement(x);
        }

        public override void VisitIncDecEx(IncDecEx x)
        {
            this.VisitElement(x);
        }

        #endregion

        

        private void VisitConditionalStatements(List<ConditionalStmt> list)
        {
            throw new NotImplementedException();
        }
    }
}
