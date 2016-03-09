//-----------------------------------------------------------------------
// <copyright file="ConfigSetTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Reflection;
    using Microsoft.Database.Isam.Config;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// EseEngine configuration tests
    /// </summary>
    [TestClass]
    public class ConfigSetTests
    {
        /// <summary>
        /// Verify that param get/set callbacks are called when set.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify that param get/set callbacks are called when set.")]
        public void VerifyParamDelegates()
        {
            bool getCalled = false;
            bool setCalled = false;
            var cset = new MockConfigSet();
            cset.GetParamDelegate = (int key, out object value) =>
            {
                Assert.AreEqual(42, key);
                getCalled = true;
                value = null;
                return false;
            };
            cset.SetParamDelegate = (int key, object value) =>
            {
                Assert.AreEqual(42, key);
                Assert.AreEqual(value, "test");
                setCalled = true;
            };

            Assert.AreEqual(default(string), cset.Param1);
            cset.Param1 = "test";
            Assert.IsTrue(getCalled);
            Assert.IsTrue(setCalled);

            // Test the indexer
            getCalled = false;
            Assert.AreEqual(default(string), cset[42]);
            Assert.IsTrue(getCalled);
        }

        /// <summary>
        /// Tests merging two config sets.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Tests merging two config sets.")]
        public void MergeBasic()
        {
            var cset1 = new MockConfigSet()
            {
                Param1 = "1",
            };
            var cset2 = new MockConfigSet()
            {
                Param2 = 2,
            };

            // Basic merge
            cset1.Merge(cset2); // no conflicst, different params are defined
            Assert.AreEqual(2, cset1.Param2);
            cset1.Merge(cset2); // no conflicts, params have same values
        }

        /// <summary>
        /// Tests merging two conflicting config sets.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Tests merging two conflicting config sets.")]
        public void MergeConflicting()
        {
            var cset1 = new MockConfigSet()
            {
                Param2 = 1,
            };
            var cset2 = new MockConfigSet()
            {
                Param1 = "2",
                Param2 = 2,
            };

            try
            {
                cset1.Merge(cset2);
                Assert.Fail("ConfigSetMergeException expected !");
            }
            catch (ConfigSetMergeException)
            {
                // Esnure that cset1 isn't modified in a conflicting merge
                Assert.IsNull(cset1.Param1);
            }
        }

        /// <summary>
        /// Tests merging two conflicting config sets with overwrite.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Tests merging two conflicting config sets with overwrite.")]
        public void MergeOverwrite()
        {
            var cset1 = new MockConfigSet()
            {
                Param2 = 1,
            };
            var cset2 = new MockConfigSet()
            {
                Param1 = "2",
                Param2 = 2,
            };

            cset1.Merge(cset2, MergeRules.Overwrite);
            Assert.AreEqual("2", cset1.Param1);
            Assert.AreEqual(2, cset1.Param2);
        }

        /// <summary>
        /// Tests merging two conflicting config sets with KeepExisting.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Tests merging two conflicting config sets with KeepExisting.")]
        public void MergeKeepExisting()
        {
            var cset1 = new MockConfigSet()
            {
                Param2 = 1,
            };
            var cset2 = new MockConfigSet()
            {
                Param1 = "2",
                Param2 = 2,
            };

            cset1.Merge(cset2, MergeRules.KeepExisting);
            Assert.AreEqual("2", cset1.Param1);
            Assert.AreEqual(1, cset1.Param2);
        }

        /// <summary>
        /// Tests merging two config sets of different types.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Tests merging two config sets of different types.")]
        public void MergeDifferentTypes()
        {
            var cset1 = new MockConfigSet()
            {
                Param1 = "1",
                Param2 = 1,
            };
            var cset2 = new AnotherMockConfigSet()
            {
                Param1 = "1",
                Param2 = 2,
            };

            cset1.Merge(cset2); // Param1 shouldn't conflict, Param2 shouldn't be overwritten
            cset1.Merge(cset2, MergeRules.Overwrite);
            Assert.AreEqual(1, cset1.Param2);
        }

        /// <summary>
        /// A mock ConfigSet for testing ConfigSetBase implementation.
        /// </summary>
        private class MockConfigSet : ConfigSetBase
        {
            /// <summary>
            /// Gets or sets the a param defined by this class.
            /// </summary>
            public string Param1
            {
                get { return this.GetParam<string>(42); }
                set { this.SetParam(42, value); }
            }

            /// <summary>
            /// Gets or sets the a param defined by this class.
            /// </summary>
            public int Param2
            {
                get { return this.GetParam<int>(43); }
                set { this.SetParam(43, value); }
            }
        }

        /// <summary>
        /// Another mock ConfigSet for testing ConfigSetBase implementation.
        /// </summary>
        private class AnotherMockConfigSet : MockConfigSet
        {
        }
    }
}