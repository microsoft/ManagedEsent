// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryEnumerable.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   An object which can enumerate a specified key range in a PersistentDictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// An object which can enumerate the specified key range in a PersistentDictionary and apply a filter.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value in the dictionary.</typeparam>
    /// <typeparam name="TReturn">The type of object returned by the enumerator.</typeparam>
    internal sealed class PersistentDictionaryEnumerable<TKey, TValue, TReturn> : IEnumerable<TReturn>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The dictionary being iterated.
        /// </summary>
        private readonly PersistentDictionary<TKey, TValue> dictionary;

        /// <summary>
        /// The key range being iterated.
        /// </summary>
        private readonly KeyRange<TKey> range;

        /// <summary>
        /// A function that gets the value from a cursor.
        /// </summary>
        private readonly Func<PersistentDictionaryCursor<TKey, TValue>, TReturn> getter;

        /// <summary>
        /// A predicate to apply to the return values. Only entries that match 
        /// the predicate are returned.
        /// </summary>
        private readonly Predicate<TReturn> predicate;

        /// <summary>
        /// Initializes a new instance of the PersistentDictionaryEnumerable class.
        /// </summary>
        /// <param name="dict">The dictionary to enumerate.</param>
        /// <param name="range">The key range being enumerated.</param>
        /// <param name="getter">A function that gets the value from the cursor.</param>
        /// <param name="predicate">A predicate expression to apply to the entries.</param>
        public PersistentDictionaryEnumerable(
            PersistentDictionary<TKey, TValue> dict,
            KeyRange<TKey> range,
            Func<PersistentDictionaryCursor<TKey, TValue>, TReturn> getter,
            Predicate<TReturn> predicate)
        {
            this.dictionary = dict;
            this.range = range;
            this.getter = getter;
            this.predicate = predicate;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TReturn> GetEnumerator()
        {
            return new PersistentDictionaryEnumerator<TKey, TValue, TReturn>(this.dictionary, this.range, this.getter, this.predicate);
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
    }
}