/*
Copyright (c) 2012-2014 Miroslav Vodolan.

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    class IndexWriteExecutor : AbstractValueVisitor
    {
        /// <summary>
        /// Assistant available for operation execution
        /// </summary>
        private readonly MemoryAssistantBase _assistant;

        /// <summary>
        /// Written index determine where value will be written
        /// </summary>
        private readonly MemberIdentifier _index;

        /// <summary>
        /// Value that will be written at specified index
        /// </summary>
        private readonly MemoryEntry _writtenValue;

        /// <summary>
        /// Result of index reading (only non array indexes are considered)
        /// </summary>
        public readonly List<Value> Result = new List<Value>();

        public IndexWriteExecutor(MemoryAssistantBase assistant, MemberIdentifier index, MemoryEntry writtenValue)
        {
            _assistant = assistant;
            _index = index;
            _writtenValue = writtenValue;
        }

        public override void VisitValue(Value value)
        {
            var subResult = _assistant.WriteValueIndex(value, _index, _writtenValue);
            reportSubResult(subResult);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            //Nothing to do, arrays are resolved in snapshot entries
        }

        public override void VisitAnyValue(AnyValue value)
        {
            //Nothing to do, AnyValues are resolved in snapshot entries
        }

        public override void VisitStringValue(StringValue value)
        {
            var subResult = _assistant.WriteStringIndex(value, _index, _writtenValue);
            reportSubResult(subResult);
        }

        private void reportSubResult(IEnumerable<Value> subResult)
        {
            Result.AddRange(subResult);
        }
    }
}