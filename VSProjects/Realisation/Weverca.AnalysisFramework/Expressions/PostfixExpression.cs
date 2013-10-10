﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Postfix representation of LangElement
    /// </summary>
    public class Postfix:IEnumerable<LangElement>
    {
        private List<LangElement> _elements = new List<LangElement>();

        /// <summary>
        /// Length of postfix representation
        /// </summary>
        public int Length { get { return _elements.Count; } }

        /// <summary>
        /// Represented LangElement
        /// </summary>
        public readonly LangElement SourceElement;
        
        /// <summary>
        /// Get element at specified index
        /// </summary>
        /// <param name="elementIndex">Index of element</param>
        /// <returns>Element at specified index</returns>
        public LangElement GetElement(int elementIndex)
        {
            return _elements[elementIndex];
        }

        /// <summary>
        /// Creates postfix epxression for given source element 
        /// NOTE:
        ///     expressions will be filled from outside - converter
        /// </summary>
        /// <param name="sourceElement"></param>
        internal Postfix(LangElement sourceElement)
        {
            SourceElement = sourceElement;
        }

        /// <summary>
        /// Append element into postfix representation
        /// </summary>
        /// <param name="element"></param>
        internal void Append(LangElement element)
        {
            _elements.Add(element);
        }

        public IEnumerator<LangElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}