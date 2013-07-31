﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.ControlFlowGraph;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{

    delegate ProgramPoint ProgramPointCreator();

    public class ProgramPointGraph
    {
        /// <summary>
        /// Program points according to their defining objects (conditions, statements,..)
        /// </summary>
        private readonly Dictionary<object, ProgramPoint> _points = new Dictionary<object, ProgramPoint>();
        /// <summary>
        /// Partial extensions where program point graph is included
        /// </summary>
        private readonly HashSet<PartialExtension<LangElement>> _containingCallExtensions = new HashSet<PartialExtension<LangElement>>();
        private readonly HashSet<PartialExtension<string>> _containingIncludeExtensions = new HashSet<PartialExtension<string>>();

        /// <summary>
        /// All program points defined in program point graph
        /// </summary>
        public IEnumerable<ProgramPoint> Points { get { return _points.Values; } }

        /// <summary>
        /// Partial extensions where program point graph is included
        /// </summary>
        public IEnumerable<PartialExtension<LangElement>> ContainingCallExtensions { get { return _containingCallExtensions; } }
        public IEnumerable<PartialExtension<string>> ContainingIncludeExtensions { get { return _containingIncludeExtensions; } }

        /// <summary>
        /// Input program point into program point graph
        /// </summary>
        public readonly ProgramPoint Start;
        /// <summary>
        /// Output program point from program point graph
        /// </summary>
        public readonly ProgramPoint End;

    

        public ProgramPointGraph(BasicBlock entryPoint)
        {
            Start = empty(entryPoint);
            addChildren(Start, entryPoint);

            var endPoints = new List<ProgramPoint>();
            foreach (var point in _points.Values)
            {
                if (point.Children.Any())
                    continue;

                endPoints.Add(point);
            }

            if (endPoints.Count == 0)
            {
                //End point is not reachable
                End = null;
                return;
            }

            End = empty(Start);
            foreach (var endPoint in endPoints)
            {
                endPoint.AddChild(End);
            }
        }

        /// <summary>
        /// Creates program point graph for native analyzer
        /// </summary>
        /// <param name="analyzer">Native analyzer</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromNative(NativeAnalyzer analyzer)
        {
            var basicBlock = new BasicBlock();
            basicBlock.AddElement(analyzer);
            return new ProgramPointGraph(basicBlock);
        }

        /// <summary>
        /// Creates program point graph for given function declaration
        /// </summary>
        /// <param name="declarations"></param>
        /// <returns></returns>
        internal static ProgramPointGraph FromSource(FunctionDecl declaration)
        {
            var cfg = new ControlFlowGraph.ControlFlowGraph(declaration);

            return new ProgramPointGraph(cfg.start);
        }

        /// <summary>
        /// Creates program point graph for given function declaration
        /// </summary>
        /// <param name="declarations"></param>
        /// <returns></returns>
        internal static ProgramPointGraph FromSource(MethodDecl declaration)
        {
            var cfg = new ControlFlowGraph.ControlFlowGraph(declaration);

            return new ProgramPointGraph(cfg.start);
        }

        public static ProgramPointGraph From(LangElement declaration)
        {
            if (declaration is NativeAnalyzer)
            {
                return FromNative(declaration as NativeAnalyzer);
            }
            else if (declaration is MethodDecl)
            {
                return FromSource(declaration as MethodDecl);
            }else{
                return FromSource(declaration as FunctionDecl);
            }
        }


        internal void AddCallExtension(PartialExtension<LangElement> extension)
        {
            _containingCallExtensions.Add(extension);
        }

        internal void RemoveCallExtension(PartialExtension<LangElement> extension)
        {
            _containingCallExtensions.Remove(extension);
        }

        internal void AddIncludeExtension(PartialExtension<string> extension)
        {
            _containingIncludeExtensions.Add(extension);
        }

        internal void RemoveIncludeExtension(PartialExtension<string> extension)
        {
            _containingIncludeExtensions.Remove(extension);
        }

        private void addChildren(ProgramPoint parent, BasicBlock block)
        {
            var current = parent;
            foreach (var stmt in block.Statements)
            {
                //create chain of program points
                var child = fromStatement(stmt, block);
                current.AddChild(child);
                current = child;
            }


            foreach (var edge in block.OutgoingEdges)
            {
                var condition = fromCondition(edge.Condition, block);
                addChildren(condition, edge.To);
                current.AddChild(condition);
            }

            if (block.DefaultBranch != null)
            {
                var conditionExpressions = from edge in block.OutgoingEdges select edge.Condition;

                if (conditionExpressions.Any())
                {
                    var defaultPoint = fromDefaultBranch(conditionExpressions, block);
                    addChildren(defaultPoint, block.DefaultBranch.To);
                    current.AddChild(defaultPoint);
                }
                else
                {
                    //there is no condition on edge
                    addChildren(current, block.DefaultBranch.To);
                }
            }
        }

        private ProgramPoint fromDefaultBranch(IEnumerable<Expression> conditionalExpressions, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.SomeNot, conditionalExpressions.ToArray());
            return fromCondition(assumption, outerBlock);
        }


        private ProgramPoint fromStatement(LangElement statement, BasicBlock outerBlock)
        {
            return getPoint(statement, () => new ProgramPoint(statement, outerBlock));
        }

        private ProgramPoint fromCondition(Expression condition, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.All, condition);
            return fromCondition(assumption, outerBlock);
        }

        private ProgramPoint fromCondition(AssumptionCondition condition, BasicBlock outerBlock)
        {
            return getPoint(condition, () => new ProgramPoint(condition, outerBlock));
        }

        private ProgramPoint empty(object key)
        {
            return getPoint(key, () => new ProgramPoint());
        }

        private ProgramPoint getPoint(object obj, ProgramPointCreator creator)
        {
            ProgramPoint result;

            if (!_points.TryGetValue(obj, out result))
            {
                result = creator();
                _points.Add(obj, result);
            }
            return result;
        }

    }
}
