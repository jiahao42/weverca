﻿using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyDeclarationContainer<T>
    {
        private CopyDeclarationContainer<T> declarations;

        private bool copied;

        /// <summary>
        /// Gets the count of declarations in the collection.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get { return declarations.Count; } }

        /// <summary>
        /// Gets the list of declarations.
        /// </summary>
        /// <value>
        /// The list of declarations.
        /// </value>
        public IEnumerable<KeyValuePair<QualifiedName, HashSet<T>>> Declarations {
            get
            {
                return declarations.Declarations;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDeclarationContainer{T}"/> class.
        /// </summary>
        public LazyCopyDeclarationContainer()
        {
            declarations = new CopyDeclarationContainer<T>();
            copied = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDeclarationContainer{T}"/> class and copy data from the given one.
        /// </summary>
        /// <param name="container">The container.</param>
        public LazyCopyDeclarationContainer(LazyCopyDeclarationContainer<T> container)
        {
            declarations = container.declarations;
            copied = false;
        }

        /// <summary>
        /// Determines whether the collection contains specified qualified name.
        /// </summary>
        /// <param name="key">The key qualified name.</param>
        /// <returns>True whether the container contains declaration with the given name.</returns>
        public bool Contains(QualifiedName key)
        {
            return declarations.Contains(key);
        }

        /// <summary>
        /// Tries to get the declaration by given qualified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True whether the container contains declaration with the given name.</returns>
        public bool TryGetValue(QualifiedName key, out IEnumerable<T> value)
        {
            return declarations.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the declaration by given qualified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Declaration with the given name.</returns>
        public IEnumerable<T> GetValue(QualifiedName key)
        {
            return declarations.GetValue(key);
        }

        /// <summary>
        /// Adds new declaration into the structure. If there
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(QualifiedName key, T value)
        {
            if (!copied)
            {
                declarations = new CopyDeclarationContainer<T>(declarations);
                copied = true;
            }

            declarations.Add(key, value);
        }

        /// <summary>
        /// Gets all qualified names which are defined in the container.
        /// </summary>
        /// <returns>List of names of all declarations in this container.</returns>
        public IEnumerable<QualifiedName> GetNames()
        {
            return declarations.GetNames();
        }
    }
}