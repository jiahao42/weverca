﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data
{
    /// <summary>
    /// Factory for <see cref="CopySnapshotDataProxy"/> class.
    /// 
    /// This instance of SnapshotData container uses the associative array to store whole inner data 
    /// structure. The container is always deeply copied when new instance is created.
    /// </summary>
    public class TrackingSnapshotDataFactory : ISnapshotDataFactory
    {
        /// <inheritdoc />
        public ISnapshotDataProxy CreateEmptyInstance(Snapshot snapshot)
        {
            return new TrackingSnapshotDataProxy(snapshot);
        }

        /// <inheritdoc />
        public ISnapshotDataProxy CopyInstance(Snapshot snapshot, ISnapshotDataProxy oldData)
        {
            TrackingSnapshotDataProxy proxy = TrackingSnapshotDataProxy.Convert(oldData);
            return new TrackingSnapshotDataProxy(snapshot, proxy);
        }
    }

    /// <summary>
    /// This instance of SnapshotData container uses the associative array to store whole inner data 
    /// structure. The container is always deeply copied when new instance is created.
    /// </summary>
    public class TrackingSnapshotDataProxy : ISnapshotDataProxy
    {
        private TrackingSnapshotDataAssociativeContainer snapshotData;
        private TrackingSnapshotDataAssociativeContainer readonlyInstance;
        private bool isReadonly = true;
        private Snapshot snapshot;

        /// <summary>
        /// Converts the specified proxy to CopySnapshotDataProxy type.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns>Casted object to CopySnapshotDataProxy type.</returns>
        /// <exception cref="System.InvalidCastException">Argument is not of type CopySnapshotDataProxy</exception>
        public static TrackingSnapshotDataProxy Convert(ISnapshotDataProxy proxy)
        {
            TrackingSnapshotDataProxy converted = proxy as TrackingSnapshotDataProxy;
            if (converted != null)
            {
                return converted;
            }
            else
            {
                throw new InvalidCastException("Argument is not of type CopySnapshotDataProxy");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopySnapshotDataProxy"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public TrackingSnapshotDataProxy(Snapshot snapshot)
        {
            snapshotData = new TrackingSnapshotDataAssociativeContainer(snapshot);
            this.snapshot = snapshot;
            isReadonly = false;
            readonlyInstance = snapshotData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopySnapshotDataProxy"/> class.
        /// Deeply copies given proxy instance.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="proxy">The proxy.</param>
        public TrackingSnapshotDataProxy(Snapshot snapshot, TrackingSnapshotDataProxy proxy)
        {
            readonlyInstance = proxy.readonlyInstance;
            this.snapshot = snapshot;
        }

        /// <inheritdoc />
        public IReadOnlySnapshotData Readonly
        {
            get { return readonlyInstance; }
        }

        /// <inheritdoc />
        public IWriteableSnapshotData Writeable
        {
            get
            {
                if (isReadonly)
                {
                    snapshotData = readonlyInstance.Copy(snapshot);
                    readonlyInstance = snapshotData;
                    isReadonly = false;
                }

                return snapshotData;
            }
        }

        /// <inheritdoc />
        public bool IsReadonly
        {
            get { return isReadonly; }
        }
    }
}
