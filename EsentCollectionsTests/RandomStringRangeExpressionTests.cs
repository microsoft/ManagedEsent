// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomStringRangeExpressionTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Compare a PersistentDictionary against a generic dictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Generate string expressions that can be optimized.
    /// </summary>
    [TestClass]
    public class RandomStringRangeExpressionTests
    {
        /// <summary>
        /// The first string.
        /// </summary>
        private const string MinValue = "a";

        /// <summary>
        /// The last string.
        /// </summary>
        private const string MaxValue = "zzz";

        /// <summary>
        /// The location of the dictionary.
        /// </summary>
        private const string DictionaryLocation = "RandomStringDictionary";

        /// <summary>
        /// A MethodInfo describes String.Compare(string, string).
        /// </summary>
        private static readonly MethodInfo stringCompareMethod = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) });

        /// <summary>
        /// The parameter expression used to build out expression trees. This 
        /// should be the same object in all places so we need a singleton.
        /// </summary>
        private static readonly ParameterExpression parameterExpression = Expression.Parameter(typeof(KeyValuePair<string, string>), "x");

        /// <summary>
        /// MemberInfo that describes the Key member of the KeyValuePair.
        /// </summary>
        private static readonly MemberInfo keyMemberInfo = typeof(KeyValuePair<string, string>).GetProperty("Key", typeof(string));

        /// <summary>
        /// Random number generator.
        /// </summary>
        private readonly Random rand = new Random();

        /// <summary>
        /// Data for tests.
        /// </summary>
        private static KeyValuePair<string, string>[] data;

        /// <summary>
        /// The dictionary we are testing.
        /// </summary>
        private PersistentDictionary<string, string> dictionary;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Cleanup.DeleteDirectoryWithRetry(DictionaryLocation);
            CreateTestData();
            this.dictionary = new PersistentDictionary<string, string>(data, DictionaryLocation);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            this.dictionary.Dispose();
            Cleanup.DeleteDirectoryWithRetry(DictionaryLocation);
        }

        /// <summary>
        /// Test the Key expression evaluator with randomly generated ranges.
        /// </summary>
        [TestMethod]
        [Priority(3)]
        [Description("Test the KeyExpressionEvaluator with random ranges")]
        public void TestRandomStringKeyRangeExpressions()
        {
            DateTime endTime = DateTime.UtcNow + TimeSpan.FromSeconds(19.5);
            int trials = 0;
            while (DateTime.UtcNow < endTime)
            {
                this.DoOneTest();
                ++trials;
            }

            Console.WriteLine("{0:N0} trials", trials);
        }

        /// <summary>
        /// Assert that two enumerable sequences are identical.
        /// </summary>
        /// <typeparam name="T">The type of object being enumerated.</typeparam>
        /// <param name="expected">The expected sequence.</param>
        /// <param name="actual">The actual sequence.</param>
        private static void AssertEnumerableEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            using (IEnumerator<T> expectedEnumerator = expected.GetEnumerator())
            using (IEnumerator<T> actualEnumerator = actual.GetEnumerator())
            {
                int i = 0;
                while (expectedEnumerator.MoveNext())
                {
                    Assert.IsTrue(
                        actualEnumerator.MoveNext(),
                        "Error at entry {0}. Not enough entries in actual. First missing entry is {1}",
                        i,
                        expectedEnumerator.Current);
                    Assert.AreEqual(
                        expectedEnumerator.Current,
                        actualEnumerator.Current,
                        "Error at entry {0}. Enumerators differ",
                        i);
                    i++;
                }

                Assert.IsFalse(
                    actualEnumerator.MoveNext(),
                    "Error. Expected enumerator has {0} entries. Actual enumerator has more. First extra entry is {1}",
                    i,
                    actualEnumerator.Current);
            }
        }

        /// <summary>
        /// Creates the test data.
        /// </summary>
        private static void CreateTestData()
        {
            const int MinChar = 97;
            const int MaxChar = 123;

            List<KeyValuePair<string, string>> tempData = new List<KeyValuePair<string, string>>(26 * 26 * 26);
            for (int i = MinChar; i < MaxChar; ++i)
            {
                var sb1 = new StringBuilder(1);
                sb1.Append((char)i);
                tempData.Add(new KeyValuePair<string, string>(sb1.ToString(), sb1.ToString()));
                for (int j = MinChar; j < MaxChar; ++j)
                {
                    var sb2 = new StringBuilder(2);
                    sb2.Append((char)i);
                    sb2.Append((char)j);
                    tempData.Add(new KeyValuePair<string, string>(sb2.ToString(), sb2.ToString()));
                    for (int k = MinChar; k < MaxChar; ++k)
                    {
                        var sb3 = new StringBuilder(3);
                        sb3.Append((char)i);
                        sb3.Append((char)j);
                        sb3.Append((char)k);
                        tempData.Add(new KeyValuePair<string, string>(sb3.ToString(), sb3.ToString()));
                    }
                }
            }

            data = tempData.ToArray();
        }

        /// <summary>
        /// Create a new parameter expression.
        /// </summary>
        /// <returns>
        /// A constant expression with a value between
        /// <see cref="MinValue"/> (inclusive) and <see cref="MaxValue"/>
        /// (exclusive).
        /// </returns>
        private static ParameterExpression CreateParameterExpression()
        {
            return parameterExpression;
        }

        /// <summary>
        /// Run a test with one randomly generated expression tree.
        /// </summary>
        private void DoOneTest()
        {
            // Unfortunately this generates a Func<KeyValuePair<int, int>, bool> instead
            // of a Predicate<KeyValuePair<int, int>. We work around that by calling
            // KeyExpressionEvaluator directly.
            Expression<Predicate<KeyValuePair<string, string>>> expression = this.CreateExpression();
            Predicate<KeyValuePair<string, string>> func = expression.Compile();

            var expected = data.Where(x => func(x));
            var actual = this.dictionary.Where(expression);
            AssertEnumerableEqual(expected, actual);
        }

        /// <summary>
        /// Create a random expression tree.
        /// </summary>
        /// <returns>A random expression tree.</returns>
        private Expression<Predicate<KeyValuePair<string, string>>> CreateExpression()
        {
            return Expression.Lambda<Predicate<KeyValuePair<string, string>>>(this.CreateBooleanExpression(), CreateParameterExpression());
        }

        /// <summary>
        /// Create a boolean expression. This is the top-level expression.
        /// It is either AND, OR, key comparison or a negation of the same.
        /// </summary>
        /// <returns>A boolean expression.</returns>
        private Expression CreateBooleanExpression()
        {
            switch (this.rand.Next(16))
            {
                case 0:
                case 1:
                    return Expression.AndAlso(this.CreateBooleanExpression(), this.CreateBooleanExpression());
                case 2:
                case 3:
                    return Expression.OrElse(this.CreateBooleanExpression(), this.CreateBooleanExpression());
                case 4:
                    return Expression.Not(Expression.AndAlso(this.CreateBooleanExpression(), this.CreateBooleanExpression()));
                case 5:
                    return Expression.Not(Expression.OrElse(this.CreateBooleanExpression(), this.CreateBooleanExpression()));
                case 6:
                    return Expression.Not(this.CreateKeyComparisonExpression());
                default:
                    return this.CreateKeyComparisonExpression();
            }
        }

        /// <summary>
        /// Create a key comparison expression.
        /// </summary>
        /// <returns>A key comparison expression.</returns>
        private BinaryExpression CreateKeyComparisonExpression()
        {
            switch (this.rand.Next(2))
            {
                case 0:
                    return this.CreateKeyEqualityComparisonExpression();
                default:
                    return this.CreateStringCompareExpression();
            }
        }

        /// <summary>
        /// Create a key comparison expression.
        /// </summary>
        /// <returns>A key comparison expression.</returns>
        private BinaryExpression CreateStringCompareExpression()
        {
            Expression parameter = CreateParameterExpression();
            Expression key = Expression.MakeMemberAccess(parameter, keyMemberInfo);
            Expression value = this.CreateConstantExpression();
            Expression stringCompare = Expression.Call(null, stringCompareMethod, key, value);

            Expression zero = Expression.Constant(0);

            Expression left;
            Expression right;
            if (0 == this.rand.Next(2))
            {
                left = stringCompare;
                right = zero;
            }
            else
            {
                left = zero;
                right = stringCompare;
            }

            switch (this.rand.Next(6))
            {
                case 0:
                    return Expression.LessThan(left, right);
                case 1:
                    return Expression.LessThanOrEqual(left, right);
                case 2:
                    return Expression.Equal(left, right);
                case 3:
                    return Expression.GreaterThan(left, right);
                case 4:
                    return Expression.NotEqual(left, right);
                default:
                    return Expression.GreaterThanOrEqual(left, right);
            }
        }

        /// <summary>
        /// Create a key equality comparison expression.
        /// </summary>
        /// <returns>A key comparison expression.</returns>
        private BinaryExpression CreateKeyEqualityComparisonExpression()
        {
            Expression parameter = CreateParameterExpression();
            Expression key = Expression.MakeMemberAccess(parameter, keyMemberInfo);
            Expression value = this.CreateConstantExpression();

            Expression left;
            Expression right;
            if (0 == this.rand.Next(2))
            {
                left = key;
                right = value;
            }
            else
            {
                left = value;
                right = key;
            }

            switch (this.rand.Next(2))
            {
                case 0:
                    return Expression.Equal(left, right);
                default:
                    return Expression.NotEqual(left, right);
            }
        }

        /// <summary>
        /// Create a new (random) constant expression.
        /// </summary>
        /// <returns>
        /// A constant expression with a value between
        /// <see cref="MinValue"/> (inclusive) and <see cref="MaxValue"/>
        /// (exclusive).
        /// </returns>
        private ConstantExpression CreateConstantExpression()
        {
            return Expression.Constant(data[this.rand.Next(0, data.Length)].Key);
        }
    }
}