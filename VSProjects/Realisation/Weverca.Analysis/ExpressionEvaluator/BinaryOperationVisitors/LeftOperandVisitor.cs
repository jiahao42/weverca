using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed left operand during the analysis
    /// </summary>
    /// <remarks>
    /// The visitor must resolve only the right operand, left operand of a concrete type is set
    /// in a derived class. The class can evaluate the following binary operations:
    /// <list type="bullet">
    /// <item><term><see cref="Operations.Equal" /></term></item>
    /// <item><term><see cref="Operations.Identical" /></term></item>
    /// <item><term><see cref="Operations.NotEqual" /></term></item>
    /// <item><term><see cref="Operations.NotIdentical" /></term></item>
    /// <item><term><see cref="Operations.LessThan" /></term></item>
    /// <item><term><see cref="Operations.LessThanOrEqual" /></term></item>
    /// <item><term><see cref="Operations.GreaterThan" /></term></item>
    /// <item><term><see cref="Operations.GreaterThanOrEqual" /></term></item>
    /// <item><term><see cref="Operations.Add" /></term></item>
    /// <item><term><see cref="Operations.Sub" /></term></item>
    /// <item><term><see cref="Operations.Mul" /></term></item>
    /// <item><term><see cref="Operations.Div" /></term></item>
    /// <item><term><see cref="Operations.Mod" /></term></item>
    /// <item><term><see cref="Operations.And" /></term></item>
    /// <item><term><see cref="Operations.Or" /></term></item>
    /// <item><term><see cref="Operations.Xor" /></term></item>
    /// <item><term><see cref="Operations.BitAnd" /></term></item>
    /// <item><term><see cref="Operations.BitOr" /></term></item>
    /// <item><term><see cref="Operations.BitXor" /></term></item>
    /// <item><term><see cref="Operations.ShiftLeft" /></term></item>
    /// <item><term><see cref="Operations.ShiftRight" /></term></item>
    /// </list>
    /// The <see cref="Operations.Concat" /> is provided by <see cref="StringConverter" />
    /// </remarks>
    public abstract class LeftOperandVisitor : PartialExpressionEvaluator
    {
        /// <summary>
        /// Binary operation that determines the proper action with operands
        /// </summary>
        protected Operations operation;

        /// <summary>
        /// Result of performing the binary operation of the left and right operand
        /// </summary>
        protected Value result;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        protected LeftOperandVisitor(FlowController flowController)
            : base(flowController) { }

        /// <summary>
        /// Evaluates binary operation with left operand of this visitor and the given right operand
        /// </summary>
        /// <param name="binaryOperation">Binary operation to be performed</param>
        /// <param name="rightOperand">The right operand of binary operation</param>
        /// <returns>Result of performing the binary operation on the operands</returns>
        public Value Evaluate(Operations binaryOperation, Value rightOperand)
        {
            // Sets current operation
            operation = binaryOperation;

            // Gets type of right operand and evaluate expression for given operation
            result = null;
            rightOperand.Accept(this);

            // Returns result of binary operation
            Debug.Assert(result != null, "The result must be assigned after visiting the value");
            return result;
        }

        /// <summary>
        /// Evaluates binary operation with one left operand and all possible values of right operand
        /// </summary>
        /// <param name="binaryOperation">Binary operation to be performed</param>
        /// <param name="rightOperand">Entry with all possible right operands of binary operation</param>
        /// <returns>Resulting entry after performing the binary operation on all possible operands</returns>
        public MemoryEntry Evaluate(Operations binaryOperation, MemoryEntry rightOperand)
        {
            // Sets current operation
            operation = binaryOperation;

            var values = new HashSet<Value>();
            foreach (var value in rightOperand.PossibleValues)
            {
                // Gets type of right operand and evaluate expression for given operation
                result = null;
                value.Accept(this);

                // Returns result of binary operation
                Debug.Assert(result != null, "The result must be assigned after visiting the value");
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new InvalidOperationException("Resolving of non-binary operation");
        }

        #endregion AbstractValueVisitor Members

        #region Helper methods

        protected void DivisionByZero()
        {
            SetWarning("Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division or modulo by zero returns false boolean value
            result = OutSet.CreateBool(false);
        }

        protected void DivisionByFloatingPointZero()
        {
            SetWarning("Division by floating-point zero", AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division by floating-point zero does not return NaN or infinite, but false boolean value
            result = OutSet.CreateBool(false);
        }

        protected void DivisionByBooleanValue(bool value)
        {
            if (value)
            {
                // Modulo by 1 (true) is always 0
                result = OutSet.CreateInt(0);
            }
            else
            {
                DivisionByFalse();
            }
        }

        protected void DivisionByAnyBooleanValue()
        {
            SetWarning("Possible division by zero (converted from boolean false)",
                AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division or modulo by false returns false boolean value
            result = OutSet.AnyValue;
        }

        protected void DivisionByFalse()
        {
            SetWarning("Division by zero (converted from boolean false)",
                AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division or modulo by false returns false boolean value
            result = OutSet.CreateBool(false);
        }

        protected void DivisionByNull()
        {
            SetWarning("Division by zero (converted from null)", AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division or modulo by null returns false boolean value
            result = OutSet.CreateBool(false);
        }

        #endregion Helper methods
    }

    /// <summary>
    /// Evaluates one binary operation with typed fixed left operand during the analysis
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    /// <typeparam name="T">Type of left operand</typeparam>
    public abstract class GenericLeftOperandVisitor<T> : LeftOperandVisitor where T : Value
    {
        /// <summary>
        /// A value of specified type representing the left operand of binary operation
        /// </summary>
        protected T leftOperand;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLeftOperandVisitor{T}" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        protected GenericLeftOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        /// <summary>
        /// Set a value of specified type as left operand of binary operation
        /// </summary>
        /// <param name="value">A concrete integer value</param>
        public void SetLeftOperand(T value)
        {
            leftOperand = value;
        }
    }
}
