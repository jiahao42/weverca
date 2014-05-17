﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    /// <summary>
    /// Represents cloneable and enumerable set of values which can be used in memory collections.
    /// 
    /// This is not imutable class.
    /// </summary>
    /// <typeparam name="T">Type of values</typeparam>
    public class CopySet<T> : IReadonlySet<T>, IWriteableSet<T>, IGenericCloneable<CopySet<T>>
    {
        private HashSet<T> valueSet;

        /// <inheritdoc />
        public CopySet()
        {
            valueSet = new HashSet<T>();
        }

        /// <inheritdoc />
        public CopySet(CopySet<T> set)
        {
            valueSet = new HashSet<T>(set.valueSet);
        }

        /// <inheritdoc />
        public IEnumerable<T> Values
        {
            get { return valueSet; }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return valueSet.Count; }
        }

        /// <inheritdoc />
        public bool Contains(T value)
        {
            return valueSet.Contains(value);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return valueSet.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return valueSet.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T value)
        {
            valueSet.Add(value);
        }

        /// <inheritdoc />
        public void AddAll(IEnumerable<T> values)
        {
            CollectionTools.AddAll(valueSet, values);
        }

        /// <inheritdoc />
        public void Remove(T value)
        {
            valueSet.Remove(value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            valueSet.Clear();
        }

        /// <inheritdoc />
        public CopySet<T> Clone()
        {
            return new CopySet<T>(this);
        }
    }
}
