﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    class FunctionProgramPointBuilder:AbstractValueVisitor
    {
        internal ProgramPointGraph Output;

        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Creating program point from given value is not supported");
        }

        public override void VisitFunctionValue(FunctionValue value)
        {
            throw new NotImplementedException("Building progrma point from given function value is not implemented yet");
        }

        public override void VisitSourceFunctionValue(SourceFunctionValue value)
        {
            Output = ProgramPointGraph.FromSource(value.Declaration);
        }

        public override void VisitSourceMethodValue(SourceMethodValue value)
        {
            Output = ProgramPointGraph.FromSource(value.Declaration);
        }

        public override void VisitNativeAnalyzerValue(NativeAnalyzerValue value)
        {
            Output = ProgramPointGraph.FromNative(value.Analyzer);
        }
    }
}