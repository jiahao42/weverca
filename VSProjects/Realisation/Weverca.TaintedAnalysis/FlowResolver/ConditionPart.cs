﻿using System;

using PHP.Core.AST;
using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.FlowResolver
{
    /// <summary>
    /// Class which holds a context of a part of a condition.
    /// </summary>
    class ConditionPart
    {
        #region Enums

        /// <summary>
        /// Possible result of the condition
        /// </summary>
        public enum PossibleValues
        {
            /// <summary>
            /// The only possible result is <c>true</c>.
            /// </summary>
            OnlyTrue,

            /// <summary>
            /// The only possible result is <c>false</c>.
            /// </summary>
            OnlyFalse,

            /// <summary>
            /// The result is uncertain.
            /// </summary>
            Unknown
        }

        #endregion

        #region Members

        Postfix conditionPart;
        MemoryEntry evaluatedPart;

        FlowOutputSet flowOutputSet;

        #endregion

        #region Members

        public PossibleValues ConditionResult { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionPart"/> class.
        /// </summary>
        /// <param name="conditionPart">The definition of the part of the condition.</param>
        /// <param name="evaluatedPart">The evaluated part of the condition.</param>
        public ConditionPart(Postfix conditionPart, MemoryEntry evaluatedPart)
        {
            this.conditionPart = conditionPart;
            this.evaluatedPart = evaluatedPart;

            ConditionResult = GetConditionResult();
        }

        #endregion

        #region Methods

        public void AssumeCondition(FlowOutputSet flowOutputSet)
        {
            this.flowOutputSet = flowOutputSet;

            if (ConditionResult == PossibleValues.OnlyTrue)
            {
                AssumeTrue();
            }
            else if (ConditionResult == PossibleValues.OnlyFalse)
            {
                AssumeFalse();
            }
            else if (ConditionResult == PossibleValues.Unknown)
            {
                // We don't know how the condition can be evaluted, so we assume, that it is true. Therefore it is needed to set up inner environment.
                AssumeTrue();
            }
            else
            {
                throw new NotSupportedException(string.Format("Condition result \"{0}\" is not supported.", ConditionResult));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the condition result.
        /// </summary>
        /// <returns>see <see cref="PossibleValues"/> for details of possible result.</returns>
        PossibleValues GetConditionResult()
        {
            bool onlyTrue = true;
            bool onlyFalse = true;

            foreach (var value in evaluatedPart.PossibleValues)
            {
                var boolean = value as BooleanValue;
                if (boolean != null)
                {
                    if (!boolean.Value)
                    {
                        onlyTrue = false;
                    }
                    else
                    {
                        onlyFalse = false;
                    }
                }
                else
                {
                    onlyFalse = false;
                    onlyTrue = false;
                    //TODO: what to do with non-bool values?
                }
            }

            if (onlyTrue)
            {
                return PossibleValues.OnlyTrue;
            }
            else if (onlyFalse)
            {
                return PossibleValues.OnlyFalse;
            }
            else
            {
                return PossibleValues.Unknown;
            }
        }

        void AssumeTrue()
        {
            if (conditionPart.SourceElement is BinaryEx)
            {
                BinaryEx binaryExpression = conditionPart.SourceElement as BinaryEx;
                //TODO: tady muze byt i AND, OR, ... a pak bude treba nastartovat cely proces od zacatku pro levou a pravou stranu zvlast... od new ConditionParts();
                if (binaryExpression.PublicOperation == Operations.Equal)
                {
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr);
                }
                else if (binaryExpression.PublicOperation == Operations.NotEqual)
                {
                    AssumeNotEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThan)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThanOrEqual)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThan)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThanOrEqual)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", binaryExpression.PublicOperation, conditionPart.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Expression type \"{0}\" is not supported", conditionPart.SourceElement.GetType().Name));
            }
        }

        void AssumeFalse()
        {
            if (conditionPart.SourceElement is BinaryEx)
            {
                BinaryEx binaryExpression = conditionPart.SourceElement as BinaryEx;
                //TODO: tady muze byt i AND, OR, ... a pak bude treba nastartovat cely proces od zacatku pro levou a pravou stranu zvlast... od new ConditionParts();
                if (binaryExpression.PublicOperation == Operations.Equal)
                {
                    AssumeNotEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr);
                }
                else if (binaryExpression.PublicOperation == Operations.NotEqual)
                {
                    AssumeEquals(binaryExpression.LeftExpr, binaryExpression.RightExpr);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThan)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true);
                }
                else if (binaryExpression.PublicOperation == Operations.GreaterThanOrEqual)
                {
                    AssumeLesserThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThan)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, true);
                }
                else if (binaryExpression.PublicOperation == Operations.LessThanOrEqual)
                {
                    AssumeGreaterThan(binaryExpression.LeftExpr, binaryExpression.RightExpr, false);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Operation \"{0}\" is not supported for expression type \"{1}\"", binaryExpression.PublicOperation, conditionPart.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Expression type \"{0}\" is not supported", conditionPart.SourceElement.GetType().Name));
            }
        }

        void AssumeNotEquals(LangElement left, LangElement right)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeNotEquals(right, left);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                //TODO: this can be done more accurate with negative set.
                flowOutputSet.Assign(leftVar.VarName, flowOutputSet.AnyValue);
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        void AssumeEquals(LangElement left, LangElement right)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeEquals(right, left);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    var rigthValue = (StringLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateString((string)rigthValue.Value));
                }
                else if (right is BoolLiteral)
                {
                    var rigthValue = (BoolLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateBool((bool)rigthValue.Value));
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateDouble((double)rigthValue.Value));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateInt((int)rigthValue.Value));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateLong((long)rigthValue.Value));
                }
                else if (right is NullLiteral)
                {
                    //TODO: how to create null?
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                }
                else
                {
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        void AssumeGreaterThan(LangElement left, LangElement right, bool equal)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeGreaterThan(right, left, equal);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.AnyStringValue);
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    double bound = (double)rigthValue.Value;
                    if (!equal)
                    {
                        bound += double.Epsilon;
                    }
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateFloatInterval(bound, double.MaxValue));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    int bound = (int)rigthValue.Value;
                    if (!equal)
                    {
                        bound++;
                    }
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateIntegerInterval(bound, int.MaxValue));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    long bound = (long)rigthValue.Value;
                    if (!equal)
                    {
                        bound++;
                    }
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateLongintInterval(bound, long.MaxValue));
                }
                else
                {
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        void AssumeLesserThan(LangElement left, LangElement right, bool equal)
        {
            if (right is DirectVarUse && !(left is DirectVarUse))
            {
                AssumeLesserThan(right, left, equal);
            }
            else if (left is DirectVarUse)
            {
                var leftVar = (DirectVarUse)left;
                if (right is StringLiteral)
                {
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.AnyStringValue);
                }
                else if (right is DoubleLiteral)
                {
                    var rigthValue = (DoubleLiteral)right;
                    double bound = (double)rigthValue.Value;
                    if (!equal)
                    {
                        bound -= double.Epsilon;
                    }
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateFloatInterval(double.MinValue, bound));
                }
                else if (right is IntLiteral)
                {
                    var rigthValue = (IntLiteral)right;
                    int bound = (int)rigthValue.Value;
                    if (!equal)
                    {
                        bound--;
                    }
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateIntegerInterval(int.MinValue, bound));
                }
                else if (right is LongIntLiteral)
                {
                    var rigthValue = (LongIntLiteral)right;
                    long bound = (long)rigthValue.Value;
                    if (!equal)
                    {
                        bound--;
                    }
                    flowOutputSet.Assign(leftVar.VarName, flowOutputSet.CreateLongintInterval(long.MinValue, bound));
                }
                else
                {
                    throw new NotSupportedException(string.Format("right type \"{0}\" is not supported for \"{1}\"", right.GetType().Name, left.GetType().Name));
                }
            }
            else
            {
                throw new NotSupportedException(string.Format("Element \"{0}\" is not supprted on the left side", left.GetType().Name));
            }
        }

        #endregion
    }
}
