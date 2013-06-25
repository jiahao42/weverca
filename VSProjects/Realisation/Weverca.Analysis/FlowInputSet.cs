﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as input for statement analysis.
    /// </summary>    
    public class FlowInputSet : ISnapshotReadonly
    {
        protected internal AbstractSnapshot _snapshot;



        internal FlowInputSet(AbstractSnapshot snapshot)
        {
            _snapshot = snapshot;
        }


        public VariableName ReturnValue { get { return _snapshot.ReturnValue; } }

        public VariableName Argument(int index)
        {
            return _snapshot.Argument(index);
        }

        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            return _snapshot.ReadValue(sourceVar);
        }
    }
}
