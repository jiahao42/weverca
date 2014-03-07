﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Delegate for native methods called during analysis in context of ProgramPointGraph
    /// </summary>
    /// <param name="flow">Flow controller available for method</param>
    public delegate void NativeAnalyzerMethod(FlowController flow);

    /// <summary>
    /// Lang element used for storing native analyzers in ProgramPointGraphs
    /// </summary>
    public class NativeAnalyzer : LangElement
    {
        /// <summary>
        /// Stored native analyzer
        /// </summary>
        public readonly NativeAnalyzerMethod Method;

        /// <summary>
        /// Element which caused invoking of this analyzer - is used for sharing position
        /// </summary>
        public readonly LangElement InvokingElement;

        /// <summary>
        /// Create NativeAnalyzer with specified method
        /// </summary>
        /// <param name="method">Method which is invoked via native analyzer</param>
        /// <param name="invokingElement">Element which caused invoking of this analyzer - is used for sharing position</param>
        public NativeAnalyzer(NativeAnalyzerMethod method,LangElement invokingElement)
            :base(invokingElement.Position)
        {
            InvokingElement = invokingElement;
            Method = method;
        }

        /// <summary>
        /// Override for VisitNative in PartialWalker
        /// </summary>
        /// <param name="visitor">Visitor</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            var expander = visitor as Expressions.ElementExpander;

            if (expander == null)
            {
            }
            else
            {
                expander.VisitNative(this);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o= obj as NativeAnalyzer;

            if (o == null)
                return false;

            return o.Method.Equals(Method);
        }
    }
}
