//-----------------------------------------------------------------------
// <copyright file="KeyExpressionEvaluator.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Contains methods to evaluate a predicate Expression and determine
    /// a key range which contains all items matched by the predicate.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    internal static class KeyExpressionEvaluator<TKey, TValue> where TKey : IComparable<TKey>
    {
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

            return GetKeyRangeOfSubtree(expression.Body);
        }

        /// <summary>
        /// Evaluate a predicate Expression and determine a key range which
        /// contains all items matched by the predicate.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>
        /// A KeyRange that contains all items matched by the predicate. If no
        /// range can be determined the range will include all items.
        /// </returns>
        private static KeyRange<TKey> GetKeyRangeOfSubtree(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                {
                    var binaryExpression = (BinaryExpression) expression;
                    return GetKeyRangeOfSubtree(binaryExpression.Left) & GetKeyRangeOfSubtree(binaryExpression.Right);
                }

                case ExpressionType.Equal:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                {
                    var binaryExpression = (BinaryExpression) expression;
                    TKey value;
                    if (IsConstantComparison(binaryExpression, out value))
                    {
                        switch (expression.NodeType)
                        {
                            case ExpressionType.Equal:
                                var key = new Key<TKey>(value, true);
                                return new KeyRange<TKey>(key, key);
                            case ExpressionType.LessThan:
                                return new KeyRange<TKey>(null, new Key<TKey>(value, false));
                            case ExpressionType.LessThanOrEqual:
                                return new KeyRange<TKey>(null, new Key<TKey>(value, true));
                            case ExpressionType.GreaterThan:
                                return new KeyRange<TKey>(new Key<TKey>(value, false), null);
                            case ExpressionType.GreaterThanOrEqual:
                                return new KeyRange<TKey>(new Key<TKey>(value, true), null);
                        }
                    }

                    break;
                }

                default:
                    break;
            }

            return new KeyRange<TKey>(null, null);
        }

        /// <summary>
        /// Determine if the current binary expression involves the Key of the parameter
        /// and a constant value.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="value">Returns the value being compared to the key.</param>
        /// <returns>
        /// True if the expression involves the key of the parameter and a constant value.
        /// </returns>
        private static bool IsConstantComparison(BinaryExpression expression, out TKey value)
        {
            if (IsKeyAccess(expression.Left)
                && ConstantExpressionEvaluator<TKey>.TryGetConstantExpression(expression.Right, out value))
            {
                return true;
            }

            if (IsKeyAccess(expression.Right)
                && ConstantExpressionEvaluator<TKey>.TryGetConstantExpression(expression.Left, out value))
            {
                return true;
            }

            value = default(TKey);
            return false;
        }

        /// <summary>
        /// Determine if the expression is accessing the key of the expression
        /// parameter.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>True if the expression is accessing the key of the parameter.</returns>
        private static bool IsKeyAccess(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression) expression;
                if (member.Member.Name == "Key")
                {
                    return true;
                }
            }

            return false;
        }
    }
}