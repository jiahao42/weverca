﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Program point computed during fix point algorithm.
    /// </summary>    
    public abstract class ProgramPointBase
    {
        #region Private members

        /// <summary>
        /// Children of this program point
        /// </summary>
        private List<ProgramPointBase> _flowChildren = new List<ProgramPointBase>();

        /// <summary>
        /// Parents of this program point
        /// </summary>
        private List<ProgramPointBase> _flowParents = new List<ProgramPointBase>();

        /// <summary>
        /// Determine that program point has already been intialized (InSet,OutSet assigned)
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Input set of current program point
        /// </summary>
        protected FlowOutputSet _inSet;

        /// <summary>
        /// Output set of current program point
        /// </summary>
        private FlowOutputSet _outSet;

        /// <summary>
        /// Flow controller available for current program point
        /// </summary>
        internal protected FlowController Flow;

        #endregion

        /// <summary>
        /// Extension of this program point
        /// </summary>
        public readonly FlowExtension Extension;

        /// <summary>
        /// Partial that defines this program point. Can be null for some program points.
        /// </summary>
        public abstract LangElement Partial { get; }

        /// <summary>
        /// Input snapshot of this program point
        /// </summary>
        public SnapshotBase InSnapshot { get { return _inSet.Snapshot; } }

        /// <summary>
        /// Output snapshot of this program point
        /// </summary>
        public SnapshotBase OutSnapshot { get { return _outSet.Snapshot; } }

        /// <summary>
        /// Childrens of this program point
        /// </summary>
        public IEnumerable<ProgramPointBase> FlowChildren { get { return _flowChildren; } }

        /// <summary>
        /// Parents of this program point
        /// </summary>
        public IEnumerable<ProgramPointBase> FlowParents { get { return _flowParents; } }

        /// <summary>
        /// Count of parents of this program point
        /// </summary>
        public int FlowParentsCount { get { return _flowParents.Count; } }

        /// <summary>
        /// Count of children of this program point
        /// </summary>
        public int FlowChildrenCount { get { return _flowChildren.Count; } }

        /// <summary>
        /// Determine that program point has already been initialized (OutSet,InSet assigned)
        /// </summary>
        public bool IsInitialized { get { return _isInitialized; } }

        /// <summary>
        /// Input set of this program point
        /// </summary>
        public FlowInputSet InSet { get { return _inSet; } }

        /// <summary>
        /// Output set of this program point
        /// </summary>
        public FlowOutputSet OutSet { get { return _outSet; } }

        /// <summary>
        /// Program point graph that owns this program point
        /// </summary>
        public virtual ProgramPointGraph OwningPPGraph { get; private set; }

        /// <summary>
        /// Analysis services available for subclasses to handle flow through method
        /// </summary>
        internal virtual ForwardAnalysisServices Services { get; private set; }

        internal ProgramPointBase()
        {
            Extension = new FlowExtension(this);
        }

        #region Program point flow controlling

        /// <summary>
        /// Flow through this program point
        /// </summary>
        protected abstract void flowThrough();

        internal void FlowThrough()
        {
            activateResolvers();
            if (prepareFlow())
            {
                flowThrough();
                if (Extension.IsConnected)
                {
                    //because we are flowing into extension - prepareIt
                    prepareExtension();
                }
                commitFlow();
            }
        }


        private void checkInitialized()
        {
            if (!_isInitialized)
            {
                Initialize(Services.CreateEmptySet(), Services.CreateEmptySet());
            }
        }
        
        /// <inheritdoc />
        protected virtual bool prepareFlow()
        {
            checkInitialized();

            extendInput();
            extendOutput();

            return true;
        }

        /// <inheritdoc />
        protected virtual void extendOutput()
        {
            _outSet.StartTransaction();
            _outSet.Extend(_inSet);
        }

        /// <inheritdoc />
        protected virtual void extendInput()
        {
            if (_flowParents.Count > 0)
            {
                var inputs = new List<FlowInputSet>();

                for (int i = 0; i < _flowParents.Count; ++i)
                {
                    var flowParent = _flowParents[i];
                    var assumeParent = flowParent as AssumePoint;

                    var outset = flowParent.OutSet;
                    if (outset == null || (assumeParent != null && !assumeParent.Assumed))
                    {
                        //given parent is not computed yet or is unreachable
                        continue;
                    }
                    inputs.Add(outset);
                }
                _inSet.StartTransaction();
                _inSet.Extend(inputs.ToArray());
                _inSet.CommitTransaction();
            }
        }

        /// <inheritdoc />
        protected virtual void commitFlow()
        {
            _outSet.CommitTransaction();
            if (!OutSet.HasChanges)
            {
                //nothing has changed during last commit
                return;
            }

            enqueueChildren();
        }

        /// <inheritdoc />
        protected virtual void enqueueChildren()
        {
            foreach (var child in _flowChildren)
            {
                Services.Enqueue(child);
            }
        }

        private void prepareExtension()
        {
            foreach (var ext in Extension.Branches)
            {
                //pass call info
                ext.Flow.Arguments = Flow.Arguments;
                ext.Flow.CalledObject = Flow.CalledObject;
            }
        }

        private void activateResolvers()
        {
            Services.Evaluator.SetContext(Flow);
            Services.FunctionResolver.SetContext(Flow);
        }

        private void setNewController()
        {
            Flow = new FlowController(Services, this);
        }

        #endregion

        #region ProgramPoint graph building methods

        /// <summary>
        /// Add flow child to this program point
        /// NOTE:
        ///     Parent of child is also set
        /// </summary>
        /// <param name="child">Added child</param>
        internal void AddFlowChild(ProgramPointBase child)
        {
            _flowChildren.Add(child);
            child._flowParents.Add(this);
        }

        internal void RemoveFlowChild(ProgramPointBase child)
        {
            _flowChildren.Remove(child);
            child._flowParents.Remove(this);
        }

        /// <summary>
        /// Removes all flow children
        /// </summary>
        internal void RemoveFlowChildren()
        {
            foreach (var child in _flowChildren)
            {
                child._flowParents.Remove(this);
            }
            _flowChildren.Clear();
        }

        #endregion

        #region Internal API for changes handling

        /// <summary>
        /// Initialize program point with given input and output sets
        /// </summary>
        /// <param name="input">Input set of program point</param>
        /// <param name="output">Output set of program point</param>
        internal void Initialize(FlowOutputSet input, FlowOutputSet output)
        {
            if (_isInitialized)
            {
                throw new NotSupportedException("Initialization can be run only once");
            }
            _isInitialized = true;

            _inSet = input;
            _outSet = output;

            if (_inSet.Snapshot != null)
                //can be null in TestEntry
                _inSet.Snapshot.Assistant.RegisterProgramPoint(this);

            if (_outSet.Snapshot != null)
                //can be null in TestEntry
                _outSet.Snapshot.Assistant.RegisterProgramPoint(this);
        }

        /// <summary>
        /// Reset program point so that next transaction will 
        /// be marked with HasChanges
        /// </summary>
        internal void ResetInitialization()
        {
            _inSet.ResetInitialization();
            _outSet.ResetInitialization();
        }

        internal void SetServices(ForwardAnalysisServices services)
        {
            Services = services;
            Extension.Sink.Services = services;
            Extension.Sink.setNewController();
            setNewController();
        }

        internal void SetOwningGraph(ProgramPointGraph owningGraph)
        {
            this.OwningPPGraph = owningGraph;
            this.Extension.Sink.OwningPPGraph = owningGraph;
        }

        internal void SetMode(SnapshotMode mode)
        {
            if (_inSet != null)
                _inSet.Snapshot.SetMode(mode);

            if (_outSet != null)
                _outSet.Snapshot.SetMode(mode);
        }


        /// <summary>
        /// Reset changes reported by output set - is used for Fix point computation
        /// </summary>
        internal void ResetChanges()
        {
            OutSet.ResetChanges();
        }

        #endregion

        internal abstract void Accept(ProgramPointVisitor visitor);

    }
}
