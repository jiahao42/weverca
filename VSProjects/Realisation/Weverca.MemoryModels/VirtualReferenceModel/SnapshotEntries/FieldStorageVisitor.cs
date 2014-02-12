﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    class FieldStorageVisitor : AbstractValueVisitor
    {

        /// <summary>
        /// Determine that index with active reference is needed
        /// </summary>
        private bool _needsTemporaryField = false;

        /// <summary>
        /// Context of visited snapshot
        /// </summary>
        private readonly Snapshot _context;

        /// <summary>
        /// Storages resolved by walking possible values
        /// </summary>
        private readonly List<VariableKeyBase> _fieldStorages = new List<VariableKeyBase>();

        /// <summary>
        /// Field identifier
        /// </summary>
        private readonly VariableIdentifier _field;

        /// <summary>
        /// Created implicit object (if needed)
        /// </summary>
        private ObjectValue _implicitObject = null;

        /// <summary>
        /// Result of field operation
        /// </summary>
        internal readonly VariableKeyBase[] Storages;

        internal FieldStorageVisitor(SnapshotStorageEntry fieldedEntry, Snapshot context, VariableIdentifier field)
        {
            _context = context;
            _field = field;

            var fieldedValues = fieldedEntry.ReadMemory(context);
            VisitMemoryEntry(fieldedValues);

            if (_implicitObject != null)
                //TODO replace only undefined values
                fieldedEntry.WriteMemory(context, new MemoryEntry(_implicitObject));

            if (_needsTemporaryField)
            {
                foreach (var key in fieldedEntry.Storages)
                {
                    _fieldStorages.Add(new VariableFieldKey(key, _field));
                }
            }

            Storages = _fieldStorages.ToArray();
        }

        public override void VisitValue(Value value)
        {
            //Temporary field is needed for resolving given value
            _needsTemporaryField = true;
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            var obj = getImplicitObject();
            applyField(obj);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            applyField(value);
        }

        public override void VisitAnyValue(AnyValue value)
        {
            //read fielded value through memory assistant
            var fielded = _context.MemoryAssistant.ReadAnyField(value, _field);

            applyFieldWithWriteBack(value, fielded);
        }

        private void applyFieldWithWriteBack(Value value, MemoryEntry fielded)
        {
            var storages = _context.FieldStorages(value, _field).ToArray();
            _context.Write(storages, fielded, false, false);
            _fieldStorages.AddRange(storages);
        }

        private void applyField(ObjectValue objectValue)
        {
            _fieldStorages.AddRange(_context.FieldStorages(objectValue, _field));
        }

        private ObjectValue getImplicitObject()
        {
            if (_implicitObject == null)
                _implicitObject = _context.MemoryAssistant.CreateImplicitObject();

            return _implicitObject;
        }
    }
}
