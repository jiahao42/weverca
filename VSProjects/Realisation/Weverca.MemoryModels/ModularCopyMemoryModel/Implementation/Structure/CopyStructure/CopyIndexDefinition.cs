﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyIndexDefinition : IIndexDefinition, IIndexDefinitionBuilder
    {
        private IMemoryAlias aliases;
        private IObjectValueContainer objects;
        private AssociativeArray arrayValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexDefinition"/> class.
        /// </summary>
        public CopyIndexDefinition()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexDefinition"/> class.
        /// </summary>
        /// <param name="indexDefinition">The index definition.</param>
        public CopyIndexDefinition(CopyIndexDefinition indexDefinition)
        {
            this.aliases = indexDefinition.aliases;
            this.objects = indexDefinition.objects;
            this.arrayValue = indexDefinition.arrayValue;
        }

        /// <inheritdoc />
        public IMemoryAlias Aliases
        {
            get { return aliases; }
        }

        /// <inheritdoc />
        public IObjectValueContainer Objects
        {
            get { return objects; }
        }

        /// <inheritdoc />
        public AssociativeArray Array
        {
            get { return arrayValue; }
        }

        /// <inheritdoc />
        public IIndexDefinitionBuilder Builder()
        {
            return new CopyIndexDefinition(this);
        }

        /// <inheritdoc />
        public void SetArray(AssociativeArray arrayValue)
        {
            this.arrayValue = arrayValue;
        }

        /// <inheritdoc />
        public void SetObjects(IObjectValueContainer objects)
        {
            this.objects = objects;
        }

        /// <inheritdoc />
        public void SetAliases(IMemoryAlias aliases)
        {
            this.aliases = aliases;
        }

        /// <inheritdoc />
        public IIndexDefinition Build()
        {
            return this;
        }
    }
}