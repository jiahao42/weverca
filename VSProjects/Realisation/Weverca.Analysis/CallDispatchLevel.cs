﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis
{
    /// <summary>
    /// Specifies type of call (In analyzis there are multiple types of calls e.g. include is handled as special type of call)
    /// </summary>
    public enum CallType
    {
        /// <summary>
        /// There can be multiple calls processed at one level         
        /// </summary>
        ParallelCall,
        /// <summary>
        /// There can be processed multiple includes processed at one level
        /// NOTE:
        ///     This call type doesn't increase call stack depth
        /// </summary>
        ParallelInclude,
    }

    /// <summary>
    /// Handle multiple call dispatches on single stack level
    /// </summary>
    class CallDispatchLevel
    {
        #region Private members
        /// <summary>
        /// Available analysis services
        /// </summary>
        private readonly AnalysisServices _services;
        /// <summary>
        /// All dispatches on level
        /// </summary>
        private readonly DispatchInfo[] _dispatches;
        /// <summary>
        /// Index of currently processed dispatch
        /// </summary>
        private int _dispatchIndex;
        /// <summary>
        /// Results of every dispatch in level
        /// </summary>        
        private List<AnalysisCallContext> _callResults = new List<AnalysisCallContext>();

        #endregion

        public readonly CallType CallType;

        /// <summary>
        /// Current analysis call context 
        /// </summary>
        public AnalysisCallContext CurrentContext { get; private set; }


        /// <summary>
        /// Create call dispatch level from given dispatches 
        /// </summary>
        /// <param name="dispatches">Dispaches at same stack level</param>
        /// <param name="services">Available services</param>
        public CallDispatchLevel(IEnumerable<DispatchInfo> dispatches, AnalysisServices services, CallType callType)
        {            
            _dispatches = dispatches.ToArray();
            _services = services;
            CallType = callType;

            setCurrentDispatch(_dispatchIndex);
        }

        /// <summary>
        /// Create call dispatch level from given dispatch
        /// </summary>
        /// <param name="dispatch">Single dispatch in level</param>
        /// <param name="services">Available services</param>
        public CallDispatchLevel(DispatchInfo dispatch, AnalysisServices services, CallType callType)
            : this(new DispatchInfo[] { dispatch }, services, callType)
        {
        }

        /// <summary>
        /// Shift to next dispatch in level
        /// </summary>
        /// <returns>False if there is no next dispatch, true otherwise</returns>
        public bool ShiftToNextDispatch()
        {
            _callResults.Add(CurrentContext);
            ++_dispatchIndex;
            if (_dispatchIndex >= _dispatches.Length)
            {
                return false;
            }

            setCurrentDispatch(_dispatchIndex);
            return true;
        }

        /// <summary>
        /// Get results from dispatches
        /// </summary>
        /// <returns>Dispatches results</returns>
        public AnalysisCallContext[] GetResult()
        {
            if (_callResults.Count != _dispatches.Length)
                throw new InvalidOperationException("Cannot get result in given dispatch level state");

            return _callResults.ToArray();
        }

        /// <summary>
        /// Set current dispatch according to dispatchIndex
        /// </summary>
        /// <param name="dispatchIndex">Index of dispatch to set</param>
        private void setCurrentDispatch(int dispatchIndex)
        {
            CurrentContext = createContext(_dispatches[dispatchIndex]);

        }

        /// <summary>
        /// Create call context for given dispatch
        /// </summary>
        /// <param name="dispatch">Call dispatch</param>
        /// <returns>Created context</returns>
        private AnalysisCallContext createContext(DispatchInfo dispatch)
        {
            var context = new AnalysisCallContext(dispatch.MethodGraph, _services, CallType, dispatch.InSet);
            return context;
        }
    }
}
