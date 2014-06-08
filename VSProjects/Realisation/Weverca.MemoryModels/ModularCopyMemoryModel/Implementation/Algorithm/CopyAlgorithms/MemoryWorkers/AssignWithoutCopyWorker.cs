﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// This is the implementation of assign algorithm which stores content of memory entry without deep copy.
    /// This approach is used when is necessary to prevent full assign algorithm and just alow analysis to store
    /// some modified data direcly into memory location. The other use case is for assigning values in the second
    /// phase of analysis. In this phase structural informations can not be changed and algorithm just needs to store
    /// given memory entry without arrays and objects. So it is not necesary to run full assign algorithm.
    /// 
    /// The most important thing about this algorithm is that in order to prevent structural changes the algorithm
    /// do not allow to store array value into memory location. Even when there is some array value already stored
    /// and new memory entry does not contain this array algorithm inserts array of this location back into new 
    /// memory entry.
    /// </summary>
    class AssignWithoutCopyWorker
    {
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignWithoutCopyWorker"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AssignWithoutCopyWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Assigns the given memory entry into all collected indexes in the collector.
        /// Must indexes are strongly updated may indexes weakly.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="value">The value.</param>
        internal void Assign(AssignCollector collector, AnalysisFramework.Memory.MemoryEntry value)
        {
            CollectComposedValuesVisitor composedValues = new CollectComposedValuesVisitor();
            composedValues.VisitMemoryEntry(value);

            foreach (MemoryIndex mustIndex in collector.MustIndexes)
            {
                assignMust(mustIndex, composedValues);
            }

            foreach (MemoryIndex mayIndex in collector.MayIndexes)
            {
                assignMay(mayIndex, composedValues);
            }

            if (snapshot.CurrentMode == SnapshotMode.InfoLevel)
            {
                InfoLocationVisitor mustVisitor = new InfoLocationVisitor(snapshot, value, true);
                foreach (ValueLocation mustLocation in collector.MustLocation)
                {
                    mustLocation.Accept(mustVisitor);
                }

                InfoLocationVisitor mayVisitor = new InfoLocationVisitor(snapshot, value, false);
                foreach (ValueLocation mustLocation in collector.MayLocaton)
                {
                    mustLocation.Accept(mayVisitor);
                }
            }
        }

        /// <summary>
        /// Assigns the must memory index.
        /// </summary>
        /// <param name="mustIndex">Index of the must.</param>
        /// <param name="composedValues">The composed values.</param>
        private void assignMust(MemoryIndex mustIndex, CollectComposedValuesVisitor composedValues)
        {
            IIndexDefinition data = snapshot.Structure.Readonly.GetIndexDefinition(mustIndex);
            HashSet<Value> values = new HashSet<Value>(composedValues.Values);

            if (snapshot.CurrentMode == SnapshotMode.MemoryLevel)
            {
                if (data.Array != null)
                {
                    values.Add(data.Array);
                }

                if (composedValues.Objects.Count > 0)
                {
                    IObjectValueContainer objects = snapshot.Structure.CreateObjectValueContainer(composedValues.Objects);
                    snapshot.Structure.Writeable.SetObjects(mustIndex, objects);
                    if (data.Objects != null) CollectionTools.AddAll(values, data.Objects);
                }
            }

            snapshot.CurrentData.Writeable.SetMemoryEntry(mustIndex, new MemoryEntry(values));
        }

        /// <summary>
        /// Assigns the may memory index.
        /// </summary>
        /// <param name="mayIndex">Index of the may.</param>
        /// <param name="composedValues">The composed values.</param>
        private void assignMay(MemoryIndex mayIndex, CollectComposedValuesVisitor composedValues)
        {
            IIndexDefinition data = snapshot.Structure.Readonly.GetIndexDefinition(mayIndex);
            HashSet<Value> values = new HashSet<Value>(composedValues.Values);

            if (composedValues.Objects.Count > 0)
            {
                HashSet<ObjectValue> objectsSet = new HashSet<ObjectValue>(data.Objects);
                CollectionTools.AddAll(objectsSet, composedValues.Objects);
                IObjectValueContainer objects = snapshot.Structure.CreateObjectValueContainer(composedValues.Objects);
                snapshot.Structure.Writeable.SetObjects(mayIndex, objects);

                //if (data.Objects != null) 
                CollectionTools.AddAll(values, data.Objects);
            }

            CollectionTools.AddAll(values, snapshot.CurrentData.Readonly.GetMemoryEntry(mayIndex).PossibleValues);
            snapshot.CurrentData.Writeable.SetMemoryEntry(mayIndex, new MemoryEntry(values));
        }

        /// <summary>
        /// Value location visitor to process valued locations in info level phase.
        /// This visitor just stores new value to the containing index of every location.
        /// </summary>
        class InfoLocationVisitor : IValueLocationVisitor
        {
            Snapshot snapshot;
            MemoryEntry entry;
            bool isMust;

            /// <summary>
            /// Initializes a new instance of the <see cref="InfoLocationVisitor"/> class.
            /// </summary>
            /// <param name="snapshot">The snapshot.</param>
            /// <param name="entry">The entry.</param>
            /// <param name="isMust">if set to <c>true</c> [is must].</param>
            public InfoLocationVisitor(Snapshot snapshot, MemoryEntry entry, bool isMust)
            {
                this.snapshot = snapshot;
                this.entry = entry;
                this.isMust = isMust;
            }

            /// <summary>
            /// Visits the object value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectValueLocation(ObjectValueLocation location)
            {
            }

            /// <summary>
            /// Visits the object any value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayValueLocation(ArrayValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array any value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array string value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array undefined value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
            {
            }

            /// <summary>
            /// Visits the object undefined value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
            {
            }

            /// <summary>
            /// Visits the information value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitInfoValueLocation(InfoValueLocation location)
            {
            }

            /// <summary>
            /// Visits any string value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitAnyStringValueLocation(AnyStringValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            /// <summary>
            /// Assigns new value into specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            private void assign(MemoryIndex index)
            {
                HashSet<Value> newValues = new HashSet<Value>();
                if (!isMust)
                {
                    MemoryEntry oldEntry = snapshot.CurrentData.Readonly.GetMemoryEntry(index);
                    CollectionTools.AddAll(newValues, oldEntry.PossibleValues);
                }

                CollectionTools.AddAll(newValues, entry.PossibleValues);

                snapshot.CurrentData.Writeable.SetMemoryEntry(index, entry);
            }
        }
    }
}
