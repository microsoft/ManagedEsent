// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyValueExpressionEvaluator.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Code to evaluate a predicate Expression and determine
//   a key range which contains all items matched by the predicate.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Contains methods to evaluate a predicate Expression which operates
    /// on KeyValuePair types to determine a key range which
    /// contains all items matched by the predicate.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    internal static class KeyValueExpressionEvaluator<TKey, TValue> where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The MemberInfo for KeyValuePair.Key. This is used to identify the key parameter when
        /// getting the key range.
        /// </summary>
        private static readonly MemberInfo KeyMemberInfo = typeof(KeyValuePair<TKey, TValue>).GetProperty("Key", typeof(TKey));

        /// <summary>
        /// Evaluate a predicate Expression and determine a key range which
        /// contains all items matched by the predicate.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>
        /// A KeyRange that contains all items matched by the predicate. If no
        /// range can be determined the range will include all items.
        /// </returns>
        public static KeyRange<TKey> GetKeyRange(Expression<Predicate<KeyValuePair<TKey, TValue>>> expression)
        {
            if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            return KeyExpressionEvaluator<TKey>.GetKeyRange(expression.Body, KeyMemberInfo);
        }
    }
}