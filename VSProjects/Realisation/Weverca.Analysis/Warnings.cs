﻿using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
{

    /// <summary>
    /// Handler, which provides functionality for reading and storiny analysis warnings
    /// </summary>
    public class AnalysisWarningHandler
    {
        /// <summary>
        /// Variable where warning values are stored
        /// </summary>
        private static readonly VariableName WARNING_STORAGE = new VariableName(".analysisWarning");

        /// <summary>
        /// Insert warning inte FlowOutputSet
        /// </summary>
        /// <param name="flowOutSet"></param>
        /// <param name="warning"></param>
        public static void SetWarning(FlowOutputSet flowOutSet, AnalysisWarning warning)
        {
            var previousWarnings = ReadWarnings(flowOutSet);
            var newEntry = new List<Value>(previousWarnings);
            newEntry.Add(flowOutSet.CreateInfo(warning));

            flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            flowOutSet.Assign(WARNING_STORAGE, new MemoryEntry(newEntry));
        }

        /// <summary>
        /// Read warnings from FlowOutputSet
        /// </summary>
        /// <param name="flowOutSet"></param>
        /// <returns></returns>
        public static IEnumerable<Value> ReadWarnings(FlowOutputSet flowOutSet)
        {
            flowOutSet.FetchFromGlobal(WARNING_STORAGE);
            var result = flowOutSet.ReadValue(WARNING_STORAGE).PossibleValues;
            return from value in result where !(value is UndefinedValue) select value;
        }
    }

    /// <summary>
    /// Class, which contains information about analysis warning
    /// </summary>
    public class AnalysisWarning
    {
        /// <summary>
        /// Warning message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Langelement of AST, which produced the warning
        /// </summary>
        public LangElement LangElement { get; private set; }

        /// <summary>
        /// Cause of the warning(Why was the warning added)
        /// </summary>
        public AnalysisWarningCause Cause { get; private set; }

        /// <summary>
        /// Construct new instance of AnalysisWarning, without cause
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="element">Element, where the warning was produced</param>
        public AnalysisWarning(string message, LangElement element)
        {
            Message = message;
            LangElement = element;
        }

        /// <summary>
        /// Construct new instance of AnalysisWarning
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="element">Element, where the warning was produced</param>
        /// <param name="cause">Warning cause</param>
        public AnalysisWarning(string message, LangElement element, AnalysisWarningCause cause)
        {
            Message = message;
            LangElement = element;
            Cause = cause;
        }

        /// <summary>
        /// Return the warning message, with position in source code
        /// </summary>
        /// <returns>Return the warning message, with position in source code</returns>
        public override string ToString()
        {
            return "Warning at line " + LangElement.Position.FirstLine + " char " + LangElement.Position.FirstColumn + ": " + Message.ToString();
        }
    }

    /// <summary>
    /// Posiible warning causes, Fell free to add more.
    /// </summary>
    public enum AnalysisWarningCause
    {
        WRONG_NUMBER_OF_ARGUMENTS,
        WRONG_ARGUMENTS_TYPE,
        DIVISION_BY_ZERO,
        PROPERTY_OF_NON_OBJECT_VARIABLE,
        ELEMENT_OF_NON_ARRAY_VARIABLE,
        METHOD_CALL_ON_NON_OBJECT_VARIABLE,
        UNDEFINED_VALUE,
    }
}