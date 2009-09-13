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
            return this.UsingCursor(
                cursor =>
                {
                    using (var transaction = cursor.BeginTransaction())
                    {
                        cursor.MoveBeforeFirst();
                        if (!cursor.TryMoveNext())
                        {
                            throw new InvalidOperationException("Sequence contains no elements");
                        }

                        var first = cursor.RetrieveCurrent();
                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                        return first;
                    }
                });
        }

        /// <summary>
        /// Returns the first element of the dictionary or a default value.
        /// </summary>
        /// <returns>The first element.</returns>
        public KeyValuePair<TKey, TValue> FirstOrDefault()
        {
            return this.UsingCursor(
                cursor =>
                {
                    using (var transaction = cursor.BeginTransaction())
                    {
                        cursor.MoveBeforeFirst();
                        var first = cursor.TryMoveNext() ? cursor.RetrieveCurrent() : new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                        return first;
                    }
                });
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
            return this.UsingCursor(
                cursor =>
                {
                    using (var transaction = cursor.BeginTransaction())
                    {
                        cursor.MoveAfterLast();
                        if (!cursor.TryMovePrevious())
                        {
                            throw new InvalidOperationException("Sequence contains no elements");
                        }

                        var last = cursor.RetrieveCurrent();
                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                        return last;
                    }
                });
        }

        /// <summary>
        /// Returns the last element of the dictionary or a default value.
        /// </summary>
        /// <returns>The last element.</returns>
        public KeyValuePair<TKey, TValue> LastOrDefault()
        {
            return this.UsingCursor(
                cursor =>
                {
                    using (var transaction = cursor.BeginTransaction())
                    {
                        cursor.MoveAfterLast();
                        var last = cursor.TryMovePrevious() ? cursor.RetrieveCurrent() : new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                        return last;
                    }
                });
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
        private IEnumerable<KeyValuePair<TKey, TValue>> Where(
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