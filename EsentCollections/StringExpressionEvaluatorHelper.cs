// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExpressionEvaluatorHelper.cs" company="Microsoft Corporation">
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
    using System.Reflection;

    /// <summary>
    /// Methods for dealing with string expressions.
    /// </summary>
    internal static class StringExpressionEvaluatorHelper
    {
        /// <summary>
        /// A MethodInfo describing String.Compare(string, string).
        /// </summary>
        private static readonly MethodInfo stringCompareMethod = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) });

        /// <summary>
        /// A MethodInfo describing String.Equals(string).
        /// </summary>
        private static readonly MethodInfo stringEqualsMethod = typeof(string).GetMethod("Equals", new[] { typeof(string) });

        /// <summary>
        /// A MethodInfo describing String.StartsWith(string).
        /// </summary>
        private static readonly MethodInfo stringStartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) }); 
       
        /// <summary>
        /// Gets a MethodInfo describing String.Compare(string, string).
        /// </summary>
        public static MethodInfo StringCompareMethod
        {
            get
            {
                return stringCompareMethod;
            }
        }

        /// <summary>
        /// Gets a MethodInfo describing String.Equals(string).
        /// </summary>
        public static MethodInfo StringEqualsMethod
        {
            get
            {
                return stringEqualsMethod;
            }
        }

        /// <summary>
        /// Gets a MethodInfo describing String.StartsWith(string, string).
        /// </summary>
        public static MethodInfo StringStartWithMethod
        {
            get
            {
                return stringStartsWithMethod;
            }
        }
    }
}