﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;

using PHP.Core;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    /// <summary>
    /// Represents memory snpashot used for fix point analysis.
    /// NOTE: Snapshot will always work in context of some object.
    /// 
    /// NOTE: Global variables behave in different way than objets variables (cross callstack)
    /// <remarks>
    /// For each ProgramPoint is created few snapshots (global memory, local memory,..) - these snapshots are used through all iterations of fix point algorithm
    /// 
    /// Iteration looks like:
    /// StartTransaction
    /// ..changes in snapshot
    /// CommitTransaction
    /// 
    /// If snapshot has changed against state before start transaction is determined by HasChanged
    /// </remarks>
    /// </summary>
    public abstract class Snapshot : ISnapshotReadWrite
    {
        /// <summary>
        /// Any value singleton
        /// </summary>
        private static readonly AnyValue _anyValue = new AnyValue();
        /// <summary>
        /// Undefined value singleton
        /// </summary>
        private static readonly UndefinedValue _undefinedValue = new UndefinedValue();
        /// <summary>
        /// Statistics object - here are stored statistics
        /// </summary>
        private SnapshotStatistics _statistics = new SnapshotStatistics();

        /// <summary>
        /// Determine that transaction of this snapshot is started - updates can be written
        /// </summary>
        public bool IsTransactionStarted { get; private set; }
        /// <summary>
        /// Determine that snapshot is frozen - cannot be updated
        /// </summary>
        public bool IsFrozen { get; private set; }
        /// <summary>
        /// Determine that memory snapshot has changed on last commit
        /// </summary>
        public bool HasChanged { get; private set; }


        #region Template methods - API for implementors

        protected abstract void transactionStart();
        /// <summary>
        /// Commit started transaction - if changes has been detected during transaction must return true, false otherwise
        /// NOTE:
        ///     Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <returns>True if there is semantic change in transaction, false otherwise</returns>
        protected abstract bool commitTransaction();
        protected abstract ObjectValue createObject();
        protected abstract AliasValue createAlias(VariableName sourceVar);
        protected abstract Snapshot createCall(CallInfo info);
        protected abstract void assign(VariableName targetVar, MemoryEntry entry);
        protected abstract void assignAlias(VariableName targetVar, AliasValue alias);
        protected abstract void extend(ISnapshotReadonly[] inputs);
        protected abstract void mergeWithCall(CallResult result, ISnapshotReadonly callOutput);
        protected abstract MemoryEntry readValue(VariableName sourceVar);

        #endregion

        #region Snapshot controll operations
        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        internal void StartTransaction()
        {
            checkFrozenState();

            if (IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot start transaction multiple times");
            }
            IsTransactionStarted = true;

            transactionStart();
        }

        /// <summary>
        /// Commit snapshot transaction - if snapshot has been changed during transaction HasChanged is set to true
        /// </summary>
        internal void CommitTransaction()
        {
            checkFrozenState();

            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot commit transaction because no transaction has been started yet");
            }
            IsTransactionStarted = false;

            commitTransaction();
        }

        /// <summary>
        /// Get statistics about snapshot usage - returned statistics remains to current state and is not updated
        /// during next usage
        /// </summary>
        /// <returns>Snapshot statistis</returns>
        public SnapshotStatistics GetStatistics()
        {
            return _statistics.Clone();            
        }

        /// <summary>
        /// Snapshot can be frozen - no other transaction will be allowed to start/commit
        /// </summary>
        internal void Freeze()
        {
            checkFrozenState();

            if (IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot freeze snapshot when transaction is not commited");
            }

            IsFrozen = true;
        }

        /// <summary>
        /// Creat snapshot that will be used for call invoked from given info
        /// </summary>
        /// <param name="callInfo">Info of invoked call</param>
        /// <returns>Snapshot that will be used as entry point of invoked call</returns>
        public Snapshot CreateCall(CallInfo callInfo)
        {
            checkCanUpdate();

            var result = createCall(callInfo);
            ++_statistics.CreatedCallSnapshots;
            return result;
        }
        #endregion

        #region Implementation of ISnapshotReadWrite interface

        public AnyValue AnyValue { get { return _anyValue; } }
        public UndefinedValue UndefinedValue{get { return _undefinedValue; }}

        public StringValue CreateString(string literal)
        {
            checkCanUpdate();

            ++_statistics.CreatedStringValues;
            return new StringValue(literal);
        }

        public IntegerValue CreateInt(int number)
        {
            checkCanUpdate();

            ++_statistics.CreatedIntValues;
            return new IntegerValue(number);
        }

        public BooleanValue CreateBool(bool boolean)
        {
            checkCanUpdate();

            ++_statistics.CreatedBooleanValues;
            return new BooleanValue(boolean);
        }

        public FloatValue CreateFloat(double number)
        {
            checkCanUpdate();

            ++_statistics.CreatedFloatValues;
            return new FloatValue(number);
        }

        public ObjectValue CreateObject()
        {
            checkCanUpdate();

            var result = createObject();
            ++_statistics.CreatedObjectValues;
            return result;
        }

        public AliasValue CreateAlias(VariableName sourceVar)
        {
            checkCanUpdate();

            var result = createAlias(sourceVar);
            ++_statistics.CreatedAliasValues;
            return result;            
        }
        
        public void Assign(VariableName targetVar, Value value)
        {
            checkCanUpdate();

            if (value is AliasValue)
            {
                ++_statistics.AliasAssigns;
                assignAlias(targetVar, value as AliasValue);
            }
            else
            {
                //create implicit memory entry
                Assign(targetVar, new MemoryEntry(value));
            }            
        }

        public void Assign(VariableName targetVar, MemoryEntry value)
        {
            checkCanUpdate();
            assign(targetVar, value);
            ++_statistics.MemoryEntryAssigns;
        }

        public void Extend(params ISnapshotReadonly[] inputs)
        {
            checkCanUpdate();
            extend(inputs);
            ++_statistics.SnapshotExtendings;
        }

        public void MergeWithCall(CallResult result, ISnapshotReadonly callOutput)
        {
            checkCanUpdate();
            mergeWithCall(result, callOutput);
            ++_statistics.WithCallMerges;
        }

        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            var result = readValue(sourceVar);
            ++_statistics.ValueReads;
            return result;
        }
        #endregion

        #region Snapshot private helpers
        void checkCanUpdate()
        {
            checkFrozenState();
            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot process any updates because snapshot has no started transaction");
            }
        }

        void checkFrozenState()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Cannot process action because snapshot has already been frozen");
            }
        }
        #endregion
    }
}