//-----------------------------------------------------------------------
// <copyright file="PersistentDictionaryEnumerator.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// An object which can enumerate the specified key range in a PersistentDictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the dictionary.</typeparam>
    /// <typeparam name="TValue">Thne type of the value in the dictionary.</typeparam>
    internal sealed class PersistentDictionaryEnumerator<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The dictionary being iterated.
        /// </summary>
        private readonly PersistentDictionary<TKey, TValue> dict;

        /// <summary>
        /// The key range being iterated.
        /// </summary>
        private readonly KeyRange<TKey> range;

        /// <summary>
        /// Initializes a new instance of the PersistentDictionaryEnumerator class.
        /// </summary>
        /// <param name="dict">The dictionary to enumerate.</param>
        /// <param name="range">The key range being enumerated.</param>
        public PersistentDictionaryEnumerator(PersistentDictionary<TKey, TValue> dict, KeyRange<TKey> range)
        {
            this.dict = dict;
            this.range = range;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dict.GetGenericEnumerator(c => c.RetrieveCurrent(), this.range);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}