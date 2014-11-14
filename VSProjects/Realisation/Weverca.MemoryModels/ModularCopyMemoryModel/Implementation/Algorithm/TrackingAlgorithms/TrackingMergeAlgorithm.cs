﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms
{
    class TrackingMergeAlgorithm : IMergeAlgorithm, IAlgorithmFactory<IMergeAlgorithm>
    {
        private ISnapshotStructureProxy structure;
        private ISnapshotDataProxy data;

        /// <inheritdoc />
        public IMergeAlgorithm CreateInstance()
        {
            return new TrackingMergeAlgorithm();
        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            switch (extendedSnapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    structure = Snapshot.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);
                    break;

                case SnapshotMode.InfoLevel:
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Infos);
                    assignCreatedAliases(extendedSnapshot);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + extendedSnapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, MemoryEntry thisObject)
        {
            switch (extendedSnapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    structure = Snapshot.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);

                    structure.Writeable.AddLocalLevel();
                    break;

                case SnapshotMode.InfoLevel:
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Infos);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + extendedSnapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(snapshot, snapshots);
                        structureWorker.MergeStructure();
                        structure = structureWorker.Structure;

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.MergeData(structure);
                        data = dataWorker.Data;
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.MergeData(snapshot.Structure);

                        data = dataWorker.Data;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }



        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, List<Snapshot> snapshots)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        MergeWorker worker = new MergeWorker(snapshot, snapshots, true);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Data;
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        MergeInfoWorker worker = new MergeInfoWorker(snapshot, snapshots, true);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Infos;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy GetMergedStructure()
        {
            return structure;
        }

        /// <inheritdoc />
        public ISnapshotDataProxy GetMergedData()
        {
            return data;
        }

        private void assignCreatedAliases(Snapshot snapshot)
        {
            foreach (IMemoryAlias aliasData in snapshot.CreatedAliases)
            {
                MemoryEntry entry = data.Readonly.GetMemoryEntry(aliasData.SourceIndex);
                foreach (MemoryIndex mustAlias in aliasData.MustAliases)
                {
                    if (mustAlias != null)
                    {
                        data.Writeable.SetMemoryEntry(mustAlias, entry);
                    }
                }

                foreach (MemoryIndex mayAlias in aliasData.MayAliases)
                {
                    if (mayAlias != null)
                    {
                        MemoryEntry aliasEntry = data.Readonly.GetMemoryEntry(mayAlias);
                        HashSet<Value> values = new HashSet<Value>(aliasEntry.PossibleValues);
                        CollectionTools.AddAll(values, entry.PossibleValues);
                        data.Writeable.SetMemoryEntry(mayAlias, snapshot.CreateMemoryEntry(values));
                    }
                }
            }
        }
    }
}