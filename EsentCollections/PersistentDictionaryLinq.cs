// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryLinq.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   PersistentDictionary methods that deal with Linq methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.Isam.Esent.Interop;

    /// <content>
    /// Represents a collection of persistent keys and values.
    /// </content>
    public partial class PersistentDictionary<TKey, TValue>
    {
        /// <summary>
        /// Returns the first element of the dictionary.
        /// </summary>
        /// <returns>The first element.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the dictionary is empty.
        /// </exception>
        public KeyValuePair<TKey, TValue> First()
        {
            PersistentDictionaryCursor<TKey, TValue> cursor = this.cursors.GetCursor();
            try
            {
                using (var transaction = cursor.BeginReadOnlyTransaction())
                {
                    cursor.MoveBeforeFirst();
                    if (!cursor.TryMoveNext())
                    {
                        throw new InvalidOperationException("Sequence contains no elements");
                    }

                    var first = cursor.RetrieveCurrent();
                    return first;
                }
            }
            finally
            {
                this.cursors.FreeCursor(cursor);
            }
        }

        /// <summary>
        /// Returns the first element of the dictionary or a default value.
        /// </summary>
        /// <returns>The first element.</returns>
        public KeyValuePair<TKey, TValue> FirstOrDefault()
        {
            PersistentDictionaryCursor<TKey, TValue> cursor = this.cursors.GetCursor();
            try
            {
                using (var transaction = cursor.BeginReadOnlyTransaction())
                {
                    cursor.MoveBeforeFirst();
                    var first = cursor.TryMoveNext() ? cursor.RetrieveCurrent() : new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
                    return first;
                }
            }
            finally
            {
                this.cursors.FreeCursor(cursor);
            }
        }

        /// <summary>
        /// Returns the last element of the dictionary.
        /// </summary>
        /// <returns>The last element.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the dictionary is empty.
        /// </exception>
        public KeyValuePair<TKey, TValue> Last()
        {
            PersistentDictionaryCursor<TKey, TValue> cursor = this.cursors.GetCursor();
            try
            {
                using (var transaction = cursor.BeginReadOnlyTransaction())
                {
                    cursor.MoveAfterLast();
                    if (!cursor.TryMovePrevious())
                    {
                        throw new InvalidOperationException("Sequence contains no elements");
                    }

                    var last = cursor.RetrieveCurrent();
                    return last;
                }
            }
            finally
            {
                this.cursors.FreeCursor(cursor);
            }
        }

        /// <summary>
        /// Returns the last element of the dictionary or a default value.
        /// </summary>
        /// <returns>The last element.</returns>
        public KeyValuePair<TKey, TValue> LastOrDefault()
        {
            PersistentDictionaryCursor<TKey, TValue> cursor = this.cursors.GetCursor();
            try
            {
                using (var transaction = cursor.BeginReadOnlyTransaction())
                {
                    cursor.MoveAfterLast();
                    var last = cursor.TryMovePrevious() ? cursor.RetrieveCurrent() : new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
                    return last;
                }
            }
            finally
            {
                this.cursors.FreeCursor(cursor);
            }
        }

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

            // Consider: we could get the enumeration of key ranges, sort them and union overlapping ranges.
            // Enumerating the data as several different ranges would be more efficient when the expression
            // specifies an OR and the ranges are highly disjoint.
            KeyRange<TKey> range = KeyValueExpressionEvaluator<TKey, TValue>.GetKeyRange(expression);

#if DEBUG
            Console.WriteLine("WHERE: {0}", range);
#endif
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