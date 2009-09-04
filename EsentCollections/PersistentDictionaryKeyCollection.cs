//-----------------------------------------------------------------------
// <copyright file="PersistentDictionaryKeyCollection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Collection of the keys in a PersistentDictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal sealed class PersistentDictionaryKeyCollection<TKey, TValue> : PersistentDictionaryCollection<TKey, TValue, TKey> 
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the PersistentDictionaryKeyCollection class.
        /// </summary>
        /// <param name="dictionary">The dictionary containing the keys.</param>
        public PersistentDictionaryKeyCollection(PersistentDictionary<TKey, TValue> dictionary) :
            base(dictionary)
        {
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<TKey> GetEnumerator()
        {
            return this.Dictionary.GetKeyEnumerator();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// True if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public override bool Contains(TKey item)
        {
            return this.Dictionary.ContainsKey(item);
        }
    }
}