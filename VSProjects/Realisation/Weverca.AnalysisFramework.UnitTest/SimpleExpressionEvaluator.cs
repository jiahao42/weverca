﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    internal class SimpleExpressionEvaluator : ExpressionEvaluatorBase
    {
        public override void Assign(ReadWriteSnapshotEntryBase target, MemoryEntry value)
        {
            target.WriteMemory(OutSnapshot, value);
        }

        public override ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue, VariableIdentifier field)
        {
            return objectValue.ReadField(OutSnapshot, field);
        }

        public override ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase arrayValue, MemberIdentifier index)
        {
            return arrayValue.ReadIndex(OutSnapshot, index);
        }

        public override void AliasAssign(ReadWriteSnapshotEntryBase target, ReadSnapshotEntryBase aliasedValue)
        {
            target.SetAliases(OutSnapshot, aliasedValue);
        }

        public override MemoryEntry ResolveIndexedVariable(VariableIdentifier variable)
        {
            var snapshotEntry = ResolveVariable(variable);
            var entry = snapshotEntry.ReadMemory(OutSnapshot);
            Debug.Assert(entry.Count > 0, "Every resolved variable must give at least one value");

            // NOTE: there should be precise resolution of multiple values
            var arrayValue = entry.PossibleValues.First();
            var undefinedValue = arrayValue as UndefinedValue;
            if (undefinedValue != null)
            {
                arrayValue = OutSet.CreateArray();
                var newEntry = new MemoryEntry(arrayValue);
                snapshotEntry.WriteMemory(OutSnapshot, newEntry);
                return newEntry;
            }
            else
            {
                return entry;
            }
        }

        public override ReadWriteSnapshotEntryBase ResolveVariable(VariableIdentifier variable)
        {
            return OutSet.GetVariable(variable);
        }

        public override IEnumerable<string> VariableNames(MemoryEntry value)
        {
            //TODO convert all value types
            return from StringValue possible in value.PossibleValues select possible.Value;
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return areEqual(leftOperand, rightOperand);
                case Operations.Add:
                    return add(leftOperand, rightOperand);
                case Operations.Sub:
                    return sub(leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return gte(leftOperand, rightOperand);
                case Operations.LessThan:
                    return gte(rightOperand, leftOperand);
                default:
                    throw new NotImplementedException();
            }
        }

        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            var result = new HashSet<IntegerValue>();
            switch (operation)
            {
                case Operations.Minus:
                    var negations = from IntegerValue number in operand.PossibleValues select Flow.OutSet.CreateInt(-number.Value);
                    result.UnionWith(negations);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new MemoryEntry(result.ToArray());
        }

        public override MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue)
        {
            var inc = operation.Inc ? 1 : -1;

            var values = new List<Value>();

            foreach (var incremented in incrementedValue.PossibleValues)
            {
                var integer = incremented as IntegerValue;
                if (integer == null)
                    return new MemoryEntry(OutSet.AnyValue);

                var result = OutSet.CreateInt(integer.Value + inc);
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            throw new NotImplementedException();
        }

        #region Expression evaluation helpers

        private MemoryEntry add(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as IntegerValue;
            var rightValue = right.PossibleValues.First() as IntegerValue;

            return new MemoryEntry(OutSet.CreateInt(leftValue.Value + rightValue.Value));
        }

        private MemoryEntry sub(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as IntegerValue;
            var rightValue = right.PossibleValues.First() as IntegerValue;

            return new MemoryEntry(OutSet.CreateInt(leftValue.Value - rightValue.Value));
        }

        private MemoryEntry gte(MemoryEntry left, MemoryEntry right)
        {
            var canBeTrue = false;
            var canBeFalse = false;
            foreach (var leftVal in left.PossibleValues)
            {
                var leftInt = leftVal as IntegerValue;
                if (leftInt == null)
                    canBeTrue = canBeFalse = true;

                if (canBeTrue && canBeFalse)
                    //no need for continuation
                    break;

                foreach (var rightVal in right.PossibleValues)
                {
                    var rightInt = rightVal as IntegerValue;

                    if (rightInt == null)
                    {
                        canBeTrue = canBeFalse = true;
                        break;
                    }

                    if (leftInt.Value > rightInt.Value)
                        canBeTrue = true;
                    else
                        canBeFalse = true;
                }
            }

            var values = new List<Value>();
            if (canBeTrue)
                values.Add(OutSet.CreateBool(true));

            if (canBeFalse)
                values.Add(OutSet.CreateBool(false));

            return new MemoryEntry(values);
        }

        private void keepParentInfo(MemoryEntry parent, MemoryEntry child)
        {
            AnalysisTestUtils.CopyInfo(OutSet, parent, child);
        }

        private MemoryEntry areEqual(MemoryEntry left, MemoryEntry right)
        {
            var result = new List<BooleanValue>();
            if (canBeDifferent(left, right))
            {
                result.Add(OutSet.CreateBool(false));
            }

            if (canBeSame(left, right))
            {
                result.Add(OutSet.CreateBool(true));
            }

            return new MemoryEntry(result.ToArray());
        }

        private bool canBeSame(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
            {
                return true;
            }

            foreach (var possibleValue in left.PossibleValues)
            {
                if (right.PossibleValues.Contains(possibleValue))
                {
                    return true;
                }
            }

            return false;
        }

        private bool canBeDifferent(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
            {
                return true;
            }

            if (left.PossibleValues.Count() > 1 || left.PossibleValues.Count() > 1)
            {
                return true;
            }

            return !left.Equals(right);
        }

        private bool containsAnyValue(MemoryEntry entry)
        {
            //TODO Undefined value maybe is not correct to be treated as any value
            return entry.PossibleValues.Any((val) => val is AnyValue);
        }

        #endregion

        public override void Foreach(MemoryEntry enumeree, ReadWriteSnapshotEntryBase keyVariable,
            ReadWriteSnapshotEntryBase valueVariable)
        {
            var values = new HashSet<Value>();

            var array = enumeree.PossibleValues.First() as AssociativeArray;
            var arrayEntry = OutSet.CreateSnapshotEntry(new MemoryEntry(array));

            var indexes = OutSet.IterateArray(array);
            foreach (var index in indexes)
            {
                var indexIdentifier = new MemberIdentifier(index.Identifier);
                var indexEntry = arrayEntry.ReadIndex(OutSnapshot, indexIdentifier);
                var element = indexEntry.ReadMemory(OutSnapshot);
                values.UnionWith(element.PossibleValues);
            }

            valueVariable.WriteMemory(OutSnapshot, new MemoryEntry(values));
        }

        public override MemoryEntry Constant(GlobalConstUse x)
        {
            Value result;
            switch (x.Name.Name.Value)
            {
                case "true":
                    result = OutSet.CreateBool(true);
                    break;
                case "false":
                    result = OutSet.CreateBool(false);
                    break;
                default:
                    var constantName = ".constant_" + x.Name;
                    var constantVar = new VariableName(constantName);
                    OutSet.FetchFromGlobal(constantVar);
                    return OutSet.ReadValue(constantVar);
            }

            return new MemoryEntry(result);
        }

        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var constName = new VariableName(".constant_" + x.Name);
            OutSet.FetchFromGlobal(constName);
            OutSet.Assign(constName, constantValue);
        }

        public override MemoryEntry Concat(IEnumerable<MemoryEntry> parts)
        {
            var result = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Count != 1)
                {
                    throw new NotImplementedException();
                }

                var partValue = part.PossibleValues.First() as ScalarValue;
                result.Append(partValue.RawValue);
            }

            return new MemoryEntry(Flow.OutSet.CreateString(result.ToString()));
        }

        public override void Echo(EchoStmt echo, MemoryEntry[] values)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IssetEx(IEnumerable<VariableIdentifier> variables)
        {
            Debug.Assert(variables.GetEnumerator().MoveNext(),
                "isset expression must have at least one parameter");

            foreach (var variable in variables)
            {
                var snapshotEntry = OutSet.GetVariable(variable);
                if (!snapshotEntry.IsDefined(OutSnapshot))
                {
                    return new MemoryEntry(OutSet.CreateBool(false));
                }
            }

            return new MemoryEntry(OutSet.CreateBool(true));
        }

        public override MemoryEntry EmptyEx(VariableIdentifier variable)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry Exit(ExitEx exit, MemoryEntry status)
        {
            // TODO: It must jump to the end of program and print status, if it is a string

            // Exit expression never returns, but it is still expression so it must return something
            return new MemoryEntry(OutSet.AnyValue);
        }

        public override MemoryEntry CreateObject(QualifiedName typeName)
        {
            var types = OutSet.ResolveType(typeName);
            if (!types.GetEnumerator().MoveNext())
            {
                // TODO: If no type is resolved, exception should be thrown
                Debug.Fail("No type resolved");
            }

            var values = new List<ObjectValue>();
            foreach (var type in types)
            {
                var newObject = CreateInitializedObject(type);
                values.Add(newObject);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry IndirectCreateObject(MemoryEntry possibleNames)
        {
            var declarations = new HashSet<TypeValueBase>();

            foreach (StringValue name in possibleNames.PossibleValues)
            {
                var qualifiedName = new QualifiedName(new Name(name.Value));
                var types = OutSet.ResolveType(qualifiedName);
                if (!types.GetEnumerator().MoveNext())
                {
                    // TODO: If no type is resolved, exception should be thrown
                    Debug.Fail("No type resolved");
                }

                declarations.UnionWith(types);
            }

            var values = new List<ObjectValue>();
            foreach (var declaration in declarations)
            {
                var newObject = CreateInitializedObject(declaration);
                values.Add(newObject);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry InstanceOfEx(MemoryEntry expression, QualifiedName className)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IndirectInstanceOfEx(MemoryEntry expression, MemoryEntry possibleNames)
        {
            throw new NotImplementedException();
        }

        public override MemberIdentifier MemberIdentifier(MemoryEntry memberRepresentation)
        {
            var possibleNames = new List<string>();
            foreach (var possibleMember in memberRepresentation.PossibleValues)
            {
                var value = possibleMember as ScalarValue;
                if (value == null)
                {
                    continue;
                }

                possibleNames.Add(value.RawValue.ToString());
            }

            return new MemberIdentifier(possibleNames);
        }

        public override void FieldAssign(ReadSnapshotEntryBase objectValue, VariableIdentifier targetField, MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }

        public override void IndexAssign(ReadSnapshotEntryBase indexedValue, MemoryEntry index, MemoryEntry assignedValue)
        {
            throw new NotImplementedException();
        }
    }
}
