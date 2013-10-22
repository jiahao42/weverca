﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Stores information about object value - variables where the reference is stored, possible types of object, fields
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    internal class ObjectDescriptor
    {
        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public ReadOnlyDictionary<string, MemoryIndex> Fields { get; private set; }

        /// <summary>
        /// Gets the must references.
        /// </summary>
        /// <value>
        /// The must references.
        /// </value>
        public ReadOnlyCollection<MemoryIndex> MustReferences { get; private set; }

        /// <summary>
        /// Gets the may references.
        /// </summary>
        /// <value>
        /// The may references.
        /// </value>
        public ReadOnlyCollection<MemoryIndex> MayReferences { get; private set; }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public ReadOnlyCollection<TypeValueBase> Types { get; private set; }

        /// <summary>
        /// List of variables where the object is stored in
        /// </summary>
        public MemoryIndex ParentVariable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MemoryIndex UnknownField { get; private set; }

        public ObjectValue ObjectValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor"/> class from the builder object.
        /// </summary>
        /// <param name="objectDescriptorBuilder">The object descriptor builder.</param>
        public ObjectDescriptor(ObjectDescriptorBuilder objectDescriptorBuilder)
        {
            Fields = new ReadOnlyDictionary<string, MemoryIndex>(objectDescriptorBuilder.Fields);
            MustReferences = new ReadOnlyCollection<MemoryIndex>(objectDescriptorBuilder.MustReferences);
            MayReferences = new ReadOnlyCollection<MemoryIndex>(objectDescriptorBuilder.MayReferences);
            Types = new ReadOnlyCollection<TypeValueBase>(objectDescriptorBuilder.Types);

            UnknownField = objectDescriptorBuilder.UnknownField;
            ParentVariable = objectDescriptorBuilder.ParentVariable;
            ObjectValue = objectDescriptorBuilder.ObjectValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type of object.</param>
        public ObjectDescriptor(ObjectValue objectValue, TypeValueBase type)
        {
            Fields = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());

            MustReferences = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
            MayReferences = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
            Types = new ReadOnlyCollection<TypeValueBase>(new List<TypeValueBase>() { type });

            ObjectValue = objectValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type of object.</param>
        public ObjectDescriptor(ObjectValue objectValue, MemoryIndex parentVariable, TypeValueBase type)
        {
            Fields = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());

            MustReferences = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
            MayReferences = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
            Types = new ReadOnlyCollection<TypeValueBase>(new List<TypeValueBase>() { type });

            UnknownField = MemoryIndex.MakeIndexAnyField(parentVariable);
            ParentVariable = parentVariable;

            ObjectValue = objectValue;
        }

        /// <summary>
        /// Creates new builder to modify this descriptor 
        /// </summary>
        /// <returns></returns>
        public ObjectDescriptorBuilder Builder()
        {
            return new ObjectDescriptorBuilder(this);
        }
    }

    /// <summary>
    /// Mutable variant of ObjectDescriptor - use for creating new structure
    /// </summary>
    internal class ObjectDescriptorBuilder
    {
        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public Dictionary<string, MemoryIndex> Fields { get; private set; }

        /// <summary>
        /// Gets the must references.
        /// </summary>
        /// <value>
        /// The must references.
        /// </value>
        public List<MemoryIndex> MustReferences { get; private set; }

        /// <summary>
        /// Gets the may references.
        /// </summary>
        /// <value>
        /// The may references.
        /// </value>
        public List<MemoryIndex> MayReferences { get; private set; }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public List<TypeValueBase> Types { get; private set; }

        /// <summary>
        /// List of variables where the object is stored in
        /// </summary>
        public MemoryIndex ParentVariable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MemoryIndex UnknownField { get; private set; }

        public ObjectValue ObjectValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptorBuilder"/> class from the given object descriptor.
        /// </summary>
        /// <param name="objectDescriptor">The object descriptor.</param>
        public ObjectDescriptorBuilder(ObjectDescriptor objectDescriptor)
        {
            Fields = new Dictionary<string, MemoryIndex>(objectDescriptor.Fields);
            MustReferences = new List<MemoryIndex>(objectDescriptor.MustReferences);
            MayReferences = new List<MemoryIndex>(objectDescriptor.MayReferences);
            Types = new List<TypeValueBase>(objectDescriptor.Types);

            ParentVariable = objectDescriptor.ParentVariable;
            UnknownField = objectDescriptor.UnknownField;
            ObjectValue = objectDescriptor.ObjectValue;
        }

        /// <summary>
        /// Adds the specified container index.
        /// </summary>
        /// <param name="containerIndex">Index of the container.</param>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        public ObjectDescriptorBuilder add(string containerIndex, MemoryIndex fields)
        {
            Fields[containerIndex] = fields;
            return this;
        }

        /// <summary>
        /// Adds the must reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        public ObjectDescriptorBuilder addMustReference(MemoryIndex reference)
        {
            MustReferences.Add(reference);
            return this;
        }

        /// <summary>
        /// Adds the may reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        public ObjectDescriptorBuilder addMayReference(MemoryIndex reference)
        {
            MayReferences.Add(reference);
            return this;
        }

        /// <summary>
        /// Removes the must reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        public ObjectDescriptorBuilder removeMustReference(MemoryIndex reference)
        {
            MustReferences.Remove(reference);
            return this;
        }

        /// <summary>
        /// Removes the may reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        public ObjectDescriptorBuilder removeMayReference(MemoryIndex reference)
        {
            MayReferences.Remove(reference);
            return this;
        }

        /// <summary>
        /// Builds new descriptor object from this instance.
        /// </summary>
        /// <returns></returns>
        public ObjectDescriptor Build()
        {
            return new ObjectDescriptor(this);
        }

        internal ObjectDescriptorBuilder SetParentVariable(MemoryIndex parentIndex)
        {
            ParentVariable = parentIndex;
            return this;
        }

        internal ObjectDescriptorBuilder SetUnknownField(MemoryIndex unknownIndex)
        {
            UnknownField = unknownIndex;
            return this;
        }
    }
}