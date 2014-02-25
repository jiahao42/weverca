﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    public class MergeOperation
    {
        /// <summary>
        /// The collection of source indexes for this merge operation.
        /// </summary>
        public readonly List<Tuple<MemoryIndex, Snapshot>> Indexes = new List<Tuple<MemoryIndex, Snapshot>>();

        /// <summary>
        /// Gets a value indicating whether operation is undefined or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is undefined; otherwise, <c>false</c>.
        /// </value>
        public bool IsUndefined { get; private set; }

        /// <summary>
        /// Gets the target index of this operation
        /// </summary>
        /// <value>
        /// The index of the target.
        /// </value>
        public MemoryIndex TargetIndex { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether index is at root level.
        /// </summary>
        /// <value>
        ///   <c>true</c> if index is at root level; otherwise, <c>false</c>.
        /// </value>
        public bool IsRoot { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeOperation"/> class.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        public MergeOperation(MemoryIndex targetIndex)
        {
            IsUndefined = false;
            TargetIndex = targetIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeOperation"/> class.
        /// </summary>
        public MergeOperation()
        {
            IsUndefined = false;
        }

        /// <summary>
        /// Adds the specified memory index into sources for this operation.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <exception cref="System.NullReferenceException"></exception>
        public void Add(MemoryIndex memoryIndex, Snapshot snapshot)
        {
            if (memoryIndex == null)
            {
                throw new NullReferenceException();
            }

            Indexes.Add(new Tuple<MemoryIndex, Snapshot>(memoryIndex, snapshot));
        }

        /// <summary>
        /// Sets the undefined to true.
        /// </summary>
        public void SetUndefined()
        {
            IsUndefined = true;
        }

        /// <summary>
        /// Sets the target indeg of this operation.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        public void SetTargetIndex(MemoryIndex targetIndex)
        {
            TargetIndex = targetIndex;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return TargetIndex.ToString();
        }
    }
}
