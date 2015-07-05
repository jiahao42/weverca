/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure
{
    /// <summary>
    /// Factory for <see cref="CopySnapshotStructureProxy"/> class.
    /// 
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified.
    /// </summary>
    public class LazyCopySnapshotStructureFactory : ISnapshotStructureFactory
    {
        /// <inheritdoc />
        public ISnapshotStructureProxy CreateEmptyInstance(ModularMemoryModelFactories factories)
        {
            return LazyCopySnapshotStructure.CreateEmpty(factories);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CopyInstance(ISnapshotStructureProxy oldData)
        {
            LazyCopySnapshotStructure proxy = oldData as LazyCopySnapshotStructure;
            if (proxy != null)
            {
                return proxy.Copy();
            }
            else
            {
                throw new InvalidCastException("Argument is not of type CopySnapshotStructureProxy");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateNewInstanceWithData(IReadOnlySnapshotStructure oldData)
        {
            SnapshotStructureContainer data = oldData as SnapshotStructureContainer;
            if (data != null)
            {
                return LazyCopySnapshotStructure.CreateWithData(data);
            }
            else
            {
                throw new InvalidCastException("Argument is not of type SnapshotStructureContainer");
            }
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy CreateGlobalContextInstance(ModularMemoryModelFactories factories)
        {
            return LazyCopySnapshotStructure.CreateGlobal(factories);
        }
    }

    /// <summary>
    /// This instance of SnapshotStructure container uses the associative arrays and copy semantics. Inner structure
    /// is copied only when structure will be modified.
    /// </summary>
    public class LazyCopySnapshotStructure : ISnapshotStructureProxy
    {
        private ModularMemoryModelFactories factories;

        private SnapshotStructureContainer readonlyInstance;
        private SnapshotStructureContainer snapshotStructure;
        private bool isReadonly = true;

        /// <summary>
        /// Creates the structure with memory stack with global level only.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New structure with memory stack with global level only.</returns>
        public static LazyCopySnapshotStructure CreateGlobal(ModularMemoryModelFactories factories)
        {
            LazyCopySnapshotStructure proxy = new LazyCopySnapshotStructure(factories);
            proxy.snapshotStructure = SnapshotStructureContainer.CreateGlobal(factories);
            proxy.readonlyInstance = proxy.snapshotStructure;
            proxy.isReadonly = false;
            return proxy;
        }

        /// <summary>
        /// Creates empty structure which contains no data in containers.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New empty structure which contains no data in containers.</returns>
        public static LazyCopySnapshotStructure CreateEmpty(ModularMemoryModelFactories factories)
        {
            LazyCopySnapshotStructure proxy = new LazyCopySnapshotStructure(factories);
            proxy.snapshotStructure = SnapshotStructureContainer.CreateEmpty(factories);
            proxy.readonlyInstance = proxy.snapshotStructure;
            proxy.isReadonly = false;
            return proxy;
        }

        /// <summary>
        /// Creates new structure object as copy of this structure which contains the same data as this instace.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New copy of this structure which contains the same data as this instace.</returns>
        public LazyCopySnapshotStructure Copy()
        {
            LazyCopySnapshotStructure proxy = new LazyCopySnapshotStructure(factories);
            proxy.readonlyInstance = this.readonlyInstance;
            return proxy;
        }

        /// <summary>
        /// Creates new structure object with copy of diven data object.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="data">The old data.</param>
        /// <returns>New structure object with copy of diven data object.</returns>
        public static ISnapshotStructureProxy CreateWithData(SnapshotStructureContainer data)
        {
            LazyCopySnapshotStructure proxy = new LazyCopySnapshotStructure(data.Factories);
            proxy.readonlyInstance = data;
            return proxy;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LazyCopySnapshotStructure"/> class from being created.
        /// </summary>
        private LazyCopySnapshotStructure(ModularMemoryModelFactories factories)
        {
            this.factories = factories;
        }

        /// <inheritdoc />
        public bool Locked { get; set; }

        /// <inheritdoc />
        public IReadOnlySnapshotStructure Readonly
        {
            get { return readonlyInstance; }
        }

        /// <inheritdoc />
        public IWriteableSnapshotStructure Writeable
        {
            get
            {
                if (!Locked)
                {
                    if (isReadonly)
                    {
                        snapshotStructure = readonlyInstance.Copy();
                        readonlyInstance = snapshotStructure;
                        isReadonly = false;
                    }
                    return snapshotStructure;
                }
                else
                {
                    throw new InvalidOperationException("Snapshod structure is locked in this mode");
                }
            }
        }

        /// <inheritdoc />
        public bool IsReadonly
        {
            get { return isReadonly; }
        }

        /// <inheritdoc />
        public IObjectDescriptor CreateObjectDescriptor(ObjectValue createdObject, TypeValue type, MemoryIndex memoryIndex)
        {
            CopyObjectDescriptor descriptor = new CopyObjectDescriptor();
            descriptor.SetObjectValue(createdObject);
            descriptor.SetType(type);
            descriptor.SetUnknownIndex(memoryIndex);
            return descriptor;
        }

        /// <inheritdoc />
        public IArrayDescriptor CreateArrayDescriptor(AssociativeArray createdArray, MemoryIndex memoryIndex)
        {
            CopyArrayDescriptor descriptor = new CopyArrayDescriptor();
            descriptor.SetArrayValue(createdArray);
            descriptor.SetParentIndex(memoryIndex);
            descriptor.SetUnknownIndex(memoryIndex.CreateUnknownIndex());
            return descriptor;
        }

        /// <inheritdoc />
        public IMemoryAlias CreateMemoryAlias(Memory.MemoryIndex index)
        {
            CopyMemoryAlias aliases = new CopyMemoryAlias();
            aliases.SetSourceIndex(index);
            return aliases;
        }

        /// <inheritdoc />
        public IObjectValueContainer CreateObjectValueContainer(IEnumerable<ObjectValue> objects)
        {
            CopyObjectValueContainer container = new CopyObjectValueContainer();
            container.AddAll(objects);
            return container;
        }

        /// <inheritdoc />
        public IIndexDefinition CreateIndexDefinition()
        {
            return new CopyIndexDefinition();
        }


        public IWriteableStackContext CreateWriteableStackContext(int level)
        {
            return new CopyStackContext(level);
        }

        public IObjectValueContainer CreateObjectValueContainer()
        {
            return new CopyObjectValueContainer();
        }
    }
}