// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryKeyCollection.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Code that implements a collection of the keys in a PersistentDictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collection of the keys in a PersistentDictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class PersistentDictionaryKeyCollection<TKey, TValue> : PersistentDictionaryCollection<TKey, TValue, TKey> 
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

        /// <summary>
        /// Returns the first key in the collection.
        /// </summary>
        /// <returns>The first key.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the dictionary is empty.
        /// </exception>
        public TKey First()
        {
            return this.Dictionary.First().Key;
        }

        /// <summary>
        /// Returns the first key in the collection or a default value.
        /// </summary>
        /// <returns>The first key or a defaut value.</returns>
        public TKey FirstOrDefault()
        {
            return this.Dictionary.FirstOrDefault().Key;
        }

        /// <summary>
        /// Returns the minimum key.
        /// </summary>
        /// <returns>The minimum key.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the key collection is empty.
        /// </exception>
        public TKey Min()
        {
            // The dictionary is sorted so the first element is the minimum
            return this.Dictionary.First().Key;
        }

        /// <summary>
        /// Returns the last key in the collection.
        /// </summary>
        /// <returns>The Last key.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the dictionary is empty.
        /// </exception>
        public TKey Last()
        {
            return this.Dictionary.Last().Key;
        }

        /// <summary>
        /// Returns the last key in the collection or a default value.
        /// </summary>
        /// <returns>The last key or a defaut value.</returns>
        public TKey LastOrDefault()
        {
            return this.Dictionary.LastOrDefault().Key;
        }

        /// <summary>
        /// Returns the maximum key.
        /// </summary>
        /// <returns>The maximum key.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the key collection is empty.
        /// </exception>
        public TKey Max()
        {
            // The dictionary is sorted so the Last element is the maximum
            return this.Dictionary.Last().Key;
        }
    }
}