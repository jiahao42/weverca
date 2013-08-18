﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis
{
    /// <summary>
    /// Provides the functionality to insert and retrieve constant values from memoryModel.
    /// </summary>
    class UserDefinedConstantHandler
    {
        /// <summary>
        /// Gets Constant value from FlowOutputSet
        /// </summary>
        /// <param name="outset">FlowOutputSet, which contains values</param>
        /// <param name="name">Constant name</param>
        /// <returns>List of possible values</returns>
        public static List<Value> getConstant(FlowOutputSet outset, QualifiedName name)
        {
            List<Value> result = new List<Value>();
            UndefinedValue undefinedValue=outset.UndefinedValue;
            foreach (Value value in outset.ReadValue(new VariableName(".constants")).PossibleValues)
            {
                AssociativeArray constArray = (AssociativeArray)value;
                //case insensitive constants
                foreach(Value it in outset.GetIndex(constArray, outset.CreateIndex(name.Name.LowercaseValue)).PossibleValues)
                {
                    if(!it.Equals(undefinedValue))
                    {
                    result.Add(it);
                    }
                }
                //case sensitive constant
                foreach (Value it in outset.GetIndex(constArray, outset.CreateIndex("."+name)).PossibleValues)
                {
                    if(!it.Equals(undefinedValue))
                    {
                    result.Add(it);
                    }
                }

            }
            if (result.Count == 0)
            {
                result.Add(undefinedValue);
            }
            return result;
        }

        /// <summary>
        /// Inserts new constant into FlowOutputSet.
        /// </summary>
        /// <param name="outset">FlowOutputSet, where to isert the values.</param>
        /// <param name="name">Constant name.</param>
        /// <param name="value">constant value</param>
        /// <param name="caseInsensitive">determins if the constant is case sensitive of insensitive</param>
        public static void insertConstant(FlowOutputSet outset, QualifiedName name, MemoryEntry value, bool caseInsensitive = true)
        {

            foreach (Value array in outset.ReadValue(new VariableName(".constants")).PossibleValues)
            {
                AssociativeArray constArray = (AssociativeArray)array;
                ContainerIndex index;
                if (caseInsensitive == true)
                {
                    index=outset.CreateIndex(name.Name.LowercaseValue);
                }
                else
                {
                    index=outset.CreateIndex(name.Name.LowercaseValue);
                }
                MemoryEntry entry = outset.GetIndex(constArray, index);
                if (entry.PossibleValues.Count() == 0 || (entry.PossibleValues.Count() == 1 && entry.PossibleValues.ElementAt(0).Equals(outset.UndefinedValue)))
                {
                    outset.SetIndex(constArray, outset.CreateIndex("." + name.Name), value);
                }
            }
        }
    }
}
