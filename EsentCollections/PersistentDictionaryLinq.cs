//-----------------------------------------------------------------------
// <copyright file="PersistentDictionaryLinq.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Isam.Esent.Interop;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Represents a collection of persistent keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public partial class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Optimize a where statement which uses this dictionary.
        /// </summary>
        /// <param name="expression">
        /// The predicate determining which items should be enumerated.
        /// </param>
        /// <returns>
        /// An enumerator matching only the records matched by the predicate.
        /// </returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> Where(
            Expression<Predicate<KeyValuePair<TKey, TValue>>> expression)
        {
            if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            Predicate<KeyValuePair<TKey, TValue>> predicate = expression.Compile();
            KeyRange<TKey> range = KeyExpressionEvaluator<TKey, TValue>.GetKeyRange(expression);

            Console.WriteLine("WHERE: {0}", range);
            IEnumerable<KeyValuePair<TKey, TValue>> enumerator = new PersistentDictionaryEnumerator<TKey, TValue>(this, range);
            foreach (KeyValuePair<TKey, TValue> element in enumerator)
            {
                if (predicate(element))
                {
                    yield return element;
                }
            }
        }
    }
}