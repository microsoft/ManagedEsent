//-----------------------------------------------------------------------
// <copyright file="InstanceParameterTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the InstanceParameters class.
    /// </summary>
    [TestClass]
    public class InstanceParameterTests
    {
        /// <summary>
        /// Instance to use.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// Session to use.
        /// </summary>
        private JET_SESID sesid;

        /// <summary>
        /// The InstanceParameters class to use for testing.
        /// </summary>
        private InstanceParameters instanceParameters;

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Api.JetCreateInstance(out this.instance, "InstanceParametersTest");
            this.sesid = JET_SESID.Nil;
            this.instanceParameters = new InstanceParameters(this.instance, this.sesid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetTerm(this.instance);
        }

        /// <summary>
        /// Test the BaseName property.
        /// </summary>
        [TestMethod]
        public void BaseNameProperty()
        {
            string expected = "abc";
            this.instanceParameters.BaseName = expected;
            Assert.AreEqual(expected, this.instanceParameters.BaseName);            
        }

        /// <summary>
        /// Test the CircularLog property.
        /// </summary>
        [TestMethod]
        public void CircularLogProperty()
        {
            bool expected = true;
            this.instanceParameters.CircularLog = expected;
            Assert.AreEqual(expected, this.instanceParameters.CircularLog);
        }

        /// <summary>
        /// Test the EnableIndexChecking property.
        /// </summary>
        [TestMethod]
        public void EnableIndexCheckingProperty()
        {
            bool expected = true;
            this.instanceParameters.EnableIndexChecking = expected;
            Assert.AreEqual(expected, this.instanceParameters.EnableIndexChecking);
        }

        /// <summary>
        /// Test the Recovery property.
        /// </summary>
        [TestMethod]
        public void RecoveryProperty()
        {
            bool expected = false;
            this.instanceParameters.Recovery = expected;
            Assert.AreEqual(expected, this.instanceParameters.Recovery);
        }
    }
}
