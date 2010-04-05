// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryLinqEnumerable.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   PersistentDictionary methods that deal with Linq methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// An object which can enumerate the specified key range in a PersistentDictionary and apply a filter.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value in the dictionary.</typeparam>
    internal sealed class PersistentDictionaryLinqEnumerable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The dictionary being iterated.
        /// </summary>
        private readonly PersistentDictionary<TKey, TValue> dictionary;

        /// <summary>
        /// The expression describing the key range to be iterated.
        /// </summary>
        private readonly Expression<Predicate<KeyValuePair<TKey, TValue>>> expression;

        /// <summary>
        /// A predicate to apply to the return values. Only entries that match 
        /// the predicate are returned.
        /// </summary>
        private readonly Predicate<KeyValuePair<TKey, TValue>> predicate;

        /// <summary>
        /// Initializes a new instance of the PersistentDictionaryLinqEnumerable class.
        /// </summary>
        /// <param name="dict">The dictionary to enumerate.</param>
        /// <param name="expression">The expression describing the range of keys to return.</param>
        public PersistentDictionaryLinqEnumerable(
            PersistentDictionary<TKey, TValue> dict,
            Expression<Predicate<KeyValuePair<TKey, TValue>>> expression)
        {
            this.dictionary = dict;
            this.expression = expression;

            // Consider: compilation is slow. Use another thread if we have to compile?
            this.predicate = expression.Compile();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            // Consider: we could get the enumeration of key ranges, sort them and union overlapping ranges.
            // Enumerating the data as several different ranges would be more efficient when the expression
            // specifies an OR and the ranges are highly disjoint.
            KeyRange<TKey> range = KeyValueExpressionEvaluator<TKey, TValue>.GetKeyRange(this.expression);
            this.dictionary.TraceWhere(range);
            return new PersistentDictionaryEnumerator<TKey, TValue, KeyValuePair<TKey, TValue>>(
                this.dictionary, range, c => c.RetrieveCurrent(), this.predicate);
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