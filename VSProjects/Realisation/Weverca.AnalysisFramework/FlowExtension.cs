﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Specifies type of dispatch (In analyzis there are multiple types of dispatch e.g. include is handled as special type of call)
    /// </summary>
    public enum ExtensionType
    {
        /// <summary>
        /// There can be multiple calls processed at one level         
        /// </summary>
        ParallelCall,
        /// <summary>
        /// There can be processed multiple includes processed at one level
        /// NOTE:
        ///     This dispatch type doesn't increase call stack depth
        /// </summary>
        ParallelInclude,
        /// <summary>
        /// There can be evaluated multiple sources processed at one level
        /// NOTE:
        ///     This dispatch type doesn't increase call stack depth
        /// </summary>
        ParallelEval,
    }

    /// <summary>
    /// Extension used for dynamic adding call/include/eval branches into <see cref="ProgramPointGraph"/>
    /// </summary>
    public class FlowExtension
    {     
        /// <summary>
        /// Storage of extension branches indexed by keys
        /// </summary>
        Dictionary<object, ExtensionPoint> _extensions = new Dictionary<object, ExtensionPoint>();

        /// <summary>
        /// Owner which is exposing current extension
        /// </summary>
        public readonly ProgramPointBase Owner;

        /// <summary>
        /// Keys of extension branches
        /// </summary>
        public IEnumerable<object> Keys { get { return _extensions.Keys; } }

        /// <summary>
        /// Extension branches available within current extension
        /// </summary>
        public IEnumerable<ExtensionPoint> Branches { get { return _extensions.Values; } }

        /// <summary>
        /// Determine that extension is connected
        /// </summary>
        public bool IsConnected { get { return _extensions.Count > 0; } }

        /// <summary>
        /// Sink of current extension - stores merged information from extension branches
        /// </summary>
        internal readonly ExtensionSinkPoint Sink;



        internal FlowExtension(ProgramPointBase owner)
        {
            Owner = owner;

            if (Owner is ExtensionSinkPoint)
            {
                //Because of avoiding recursion - extension sink is also sink for self
                Sink = owner as ExtensionSinkPoint;
            }
            else
            {
                Sink = new ExtensionSinkPoint(this);
            }
        }

        /// <summary>
        /// Add extension branch indexed by given key
        /// </summary>
        /// <param name="key">Key of added extension</param>
        /// <param name="ppGraph">Extending program point graph</param>
        /// <param name="type">Type of extension</param>
        /// <returns>Extension point which connect extending ppGraph with Owner</returns>
        internal ExtensionPoint Add(object key, ProgramPointGraph ppGraph, ExtensionType type)
        {
            if (!IsConnected)
            {
                connect();
            }

            Owner.Services.SetServices(ppGraph);
            var extension = new ExtensionPoint(Owner, ppGraph, type);
            Owner.Services.SetServices(extension);

            _extensions.Add(key, extension);

            //connect ppGraph into current graph
            Owner.AddFlowChild(extension);
            extension.Graph.End.AddFlowChild(Sink);

            return extension;
        }

        /// <summary>
        /// Remove extension branch indexed by given key
        /// </summary>
        /// <param name="key">Key which extension branch will be removed</param>
        internal void Remove(object key)
        {
            if (!IsConnected)
            {
                //nothing to remove
                return;
            }

            ExtensionPoint extension;
            if (!_extensions.TryGetValue(key, out extension))
                //there is nothing with given key - don't need to remove branch
                return;

            _extensions.Remove(key);
            Owner.RemoveFlowChild(extension);
            extension.Graph.End.RemoveFlowChild(Sink);

            if (_extensions.Count == 0)
                disconnect();
        }

        /// <summary>
        /// Connect current extension to its owner
        /// </summary>
        private void connect()
        {
            //copy children
            var children = Owner.FlowChildren.ToArray();

            foreach (var child in children)
            {
                //disconnect original children
                Owner.RemoveFlowChild(child);

                //reconnect behind sink
                Sink.AddFlowChild(child);
            }
        }
        
        /// <summary>
        /// Disconnects current extension from its owner
        /// </summary>
        private void disconnect()
        {
            //copy children
            var children = Sink.FlowChildren.ToArray();

            foreach (var child in children)
            {
                //put children back 
                Owner.AddFlowChild(child);

                //disconnect from sink
                Sink.RemoveFlowChild(child);
            }
        }
    }
}
