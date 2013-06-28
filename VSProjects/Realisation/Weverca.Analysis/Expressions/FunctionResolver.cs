﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.ControlFlowGraph;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    public abstract class FunctionResolver
    {
        /// <summary>
        /// Return possible names of function which is represented by given functionName
        /// </summary>
        /// <param name="possibleFunctionNames">Value representing possible function names</param>
        /// <returns>Possible function names</returns>
        public abstract QualifiedName[] GetFunctionNames(MemoryEntry possibleFunctionNames);

        /// <summary>
        /// Initialize call (optional arguments, sharing program points..)
        /// NOTE:
        ///     Is called for every call dispatch
        /// </summary>
        /// <param name="callInput">Input set for call</param>
        /// <param name="name">Name of initialized call</param>
        /// <returns>ProgramPointGraph that will be used for call analyzing</returns>
        public abstract ProgramPointGraph InitializeCall(FlowOutputSet callInput, QualifiedName name);

        /// <summary>
        /// Resolves return value of given program point graphs
        /// </summary>
        /// <param name="programPointGraphs">Program point graphs from call dispatch</param>
        /// <returns>Resolved return value</returns>
        public abstract MemoryEntry ResolveReturnValue(ProgramPointGraph[] programPointGraphs);
    }
}