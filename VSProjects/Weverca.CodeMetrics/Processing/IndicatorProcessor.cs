/*
Copyright (c) 2012-2014 Miroslav Vodolan, Matyas Brenner, David Skorvaga, David Hauzar.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System.Collections.Generic;

using PHP.Core.AST;

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing
{
    /// <summary>
    /// Processor of metric that indicates whether a construct is in the source code.
    /// </summary>
    /// <remarks>
    /// The language can contain many constructs that can be problematic for analysis of program.
    /// In this case, we just want to know whether the structure, function or technique is appearing or not
    /// in the code. Moreover, information about places of the property occurrence are also important.
    /// </remarks>
    internal abstract class IndicatorProcessor : MetricProcessor<ConstructIndicator, bool>
    {
        #region MetricProcessor overrides

        /// <remarks>
        /// Merging of almost all indicators should be easy. If the metric property appeared in one
        /// piece of code, it appeared in the entire source code. Derived classes can override this behavior.
        /// </remarks>
        /// <inheritdoc />
        protected override bool Merge(bool firstProperty, bool secondProperty)
        {
            return firstProperty || secondProperty;
        }

        /// <remarks>
        /// Merging of almost all indicators should be easy. All occurrences from the first result are just
        /// appended to occurrences from the second result. Derived classes can override this behavior.
        /// </remarks>
        /// <inheritdoc />
        protected override IEnumerable<AstNode> Merge(IEnumerable<AstNode> firstOccurrences,
            IEnumerable<AstNode> secondOccurrences)
        {
            var occurrences = new List<AstNode>(firstOccurrences);
            occurrences.AddRange(secondOccurrences);
            return occurrences;
        }

        #endregion MetricProcessor overrides

        #region Utility methods for child classes

        /// <summary>
        /// Determine that source in given parser contains any method from calls or not.
        /// </summary>
        /// <param name="parser">Syntax parser of source code.</param>
        /// <param name="calls"></param>
        /// <returns></returns>
        protected static IEnumerable<AstNode> FindCalls(SyntaxParser parser, IEnumerable<string> calls)
        {
            if (!calls.GetEnumerator().MoveNext())
            {
                return new FunctionCall[0];
            }

            var visitor = new CallVisitor(calls);

            parser.Ast.VisitMe(visitor);
            return visitor.GetOccurrences();
        }

        #endregion Utility methods for child classes
    }
}