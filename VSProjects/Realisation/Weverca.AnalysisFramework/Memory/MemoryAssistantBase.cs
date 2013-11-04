﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Assistant used for resolving and reporting memory based operations by snapshot.
    /// Is usefull for tracking info values, reporting warnings,...
    /// 
    /// TODO: Memory model implementors should design API needed for all operations reporting and resolving.
    /// <remarks>
    /// Analysis creates assistant for every created snapshot. This snapshot is
    /// avalialable in Context member.
    /// </remarks>
    /// </summary>
    public abstract class MemoryAssistantBase
    {
        /// <summary>
        /// Context snapshot for current memory assistant
        /// </summary>
        protected SnapshotBase Context { get; private set; }

        /// <summary>
        /// Read index of any value
        /// </summary>
        /// <param name="value">Any value which index is read</param>
        /// <param name="index">Index used for given value</param>
        /// <returns>Value resolved by reading index on given value</returns>
        public abstract MemoryEntry ReadIndex(AnyValue value, MemberIdentifier index);

        /// <summary>
        /// Read fied of any value
        /// </summary>
        /// <param name="value">Any value which field is read</param>
        /// <param name="field">Field used for given value</param>
        /// <returns>Value resolved by reading field on given value.</returns>
        public abstract MemoryEntry ReadField(AnyValue value, VariableIdentifier field);

        /// <summary>
        /// Initialize context snapshot for current assistant
        /// </summary>
        /// <param name="context">Context snapshot for current memory assistant</param>
        internal void InitContext(SnapshotBase context)
        {
            if (Context != null)
                throw new NotSupportedException("Cannot set context twice");

            Context = context;
        }
    }
}