﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.VirtualReferenceModel
{
    class VariableInfo
    {
        internal List<VirtualReference> References { get; private set; }

        internal VariableInfo Clone()
        {
            var result = new VariableInfo();

            result.References.AddRange(References);
            return result;
        }
    }
}