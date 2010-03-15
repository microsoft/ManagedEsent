// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantExpressionEvaluator.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Methods to evaluate an expression which returns a T.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    /// <summary>
    /// Methods to evaluate an expression which returns a T.
    /// </summary>
    /// <typeparam name="T">The type returned by the expression.</typeparam>
    internal static class ConstantExpressionEvaluator<T>
    {
        /// <summary>
        /// Determine if the given expression is a constant expression, and
        /// return the value of the expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="value">The value of the expression.</param>
        /// <returns>True if the expression was a constant, false otherwise.</returns>
        public static bool TryGetConstantExpression(Expression expression, out T value)
        {
            if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            if (expression.Type != typeof(T))
            {
                Debug.Assert(false, "expected to be called with expressions of the correct type");
                value = default(T);
                return false;
            }

            if (IsConstantExpression(expression))
            {
                // Instead of performing the operation we will just compile
                // the expression.
                value = Expression.Lambda<Func<T>>(expression).Compile()();
                return true;                
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Determine if the given expression is a constant expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>True if the expression was a constant, false otherwise.</returns>
        private static bool IsConstantExpression(Expression expression)
        {
            if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return true;

                // Member access is only constant for non-parameter types
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expression;
                    return null == member.Expression || member.Expression.NodeType != ExpressionType.Parameter;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.UnaryPlus:
                    var unary = (UnaryExpression)expression;
                    return IsConstantExpression(unary.Operand);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.Divide:
                case ExpressionType.LeftShift:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Or:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.ExclusiveOr:
                    var binary = (BinaryExpression)expression;
                    return IsConstantExpression(binary.Left) && IsConstantExpression(binary.Right);
            }

            return false;
        }        
    }
}