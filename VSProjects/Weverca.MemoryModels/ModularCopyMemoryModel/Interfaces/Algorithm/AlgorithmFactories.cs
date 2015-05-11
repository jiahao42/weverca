/*
Copyright (c) 2012-2014 Pavel Bastecky.

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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    /// <summary>
    /// Contains factories for creating all algorithms for memory model.
    /// 
    /// Imutable class. For creating new instances use the builder class AlgorithmFactoriesBuilder. 
    /// </summary>
    public class AlgorithmFactories
    {
        /// <summary>
        /// Gets the assign algorithm factory.
        /// </summary>
        /// <value>
        /// The assign algorithm factory.
        /// </value>
        public IAlgorithmFactory<IAssignAlgorithm> AssignAlgorithmFactory { get; private set; }
        
        /// <summary>
        /// Gets the read algorithm factory.
        /// </summary>
        /// <value>
        /// The read algorithm factory.
        /// </value>
        public IAlgorithmFactory<IReadAlgorithm> ReadAlgorithmFactory { get; private set; }
        
        /// <summary>
        /// Gets the commit algorithm factory.
        /// </summary>
        /// <value>
        /// The commit algorithm factory.
        /// </value>
        public IAlgorithmFactory<ICommitAlgorithm> CommitAlgorithmFactory { get; private set; }

        /// <summary>
        /// Gets the memory algorithm factory.
        /// </summary>
        /// <value>
        /// The memory algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMemoryAlgorithm> MemoryAlgorithmFactory { get; private set; }
        
        /// <summary>
        /// Gets the merge algorithm factory.
        /// </summary>
        /// <value>
        /// The merge algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMergeAlgorithm> MergeAlgorithmFactory { get; private set; }

        /// <summary>
        /// Gets the print algorithm factory.
        /// </summary>
        /// <value>
        /// The print algorithm factory.
        /// </value>
        public IAlgorithmFactory<IPrintAlgorithm> PrintAlgorithmFactory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmFactories"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public AlgorithmFactories(AlgorithmFactoriesBuilder builder)
        {
            this.AssignAlgorithmFactory = builder.AssignAlgorithmFactory;
            this.ReadAlgorithmFactory = builder.ReadAlgorithmFactory;
            this.CommitAlgorithmFactory = builder.CommitAlgorithmFactory;
            this.MemoryAlgorithmFactory = builder.MemoryAlgorithmFactory;
            this.MergeAlgorithmFactory = builder.MergeAlgorithmFactory;
            this.PrintAlgorithmFactory = builder.PrintAlgorithmFactory;
        }
    }


    /// <summary>
    /// Mutable version of AlgorithmFactories class.
    /// 
    /// Allows programmer to set factories within this builder class.
    /// </summary>
    public class AlgorithmFactoriesBuilder 
    {
        /// <summary>
        /// Gets or sets the assign algorithm factory.
        /// </summary>
        /// <value>
        /// The assign algorithm factory.
        /// </value>
        public IAlgorithmFactory<IAssignAlgorithm> AssignAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the read algorithm factory.
        /// </summary>
        /// <value>
        /// The read algorithm factory.
        /// </value>
        public IAlgorithmFactory<IReadAlgorithm> ReadAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the commit algorithm factory.
        /// </summary>
        /// <value>
        /// The commit algorithm factory.
        /// </value>
        public IAlgorithmFactory<ICommitAlgorithm> CommitAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the memory algorithm factory.
        /// </summary>
        /// <value>
        /// The memory algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMemoryAlgorithm> MemoryAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the merge algorithm factory.
        /// </summary>
        /// <value>
        /// The merge algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMergeAlgorithm> MergeAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the print algorithm factory.
        /// </summary>
        /// <value>
        /// The print algorithm factory.
        /// </value>
        public IAlgorithmFactory<IPrintAlgorithm> PrintAlgorithmFactory { get; set; }

        /// <summary>
        /// Creates new AlgorithmFactories collection.
        /// </summary>
        /// <returns>New AlgorithmFactories collection.</returns>
        public AlgorithmFactories Build()
        {
            return new AlgorithmFactories(this);
        }
    }
}