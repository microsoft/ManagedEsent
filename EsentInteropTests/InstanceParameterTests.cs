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
            this.instanceParameters = new InstanceParameters(this.instance);
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
        /// Test the event source property.
        /// </summary>
        [TestMethod]
        public void EventSourceProperty()
        {
            string expected = "My Application";
            this.instanceParameters.EventSource = expected;
            Assert.AreEqual(expected, this.instanceParameters.EventSource);
        }

        /// <summary>
        /// Test the MaxSessions property.
        /// </summary>
        [TestMethod]
        public void MaxSessionsProperty()
        {
            int expected = 11;
            this.instanceParameters.MaxSessions = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxSessions);
        }

        /// <summary>
        /// Test the MaxOpenTables property.
        /// </summary>
        [TestMethod]
        public void MaxOpenTablesProperty()
        {
            int expected = 400;
            this.instanceParameters.MaxOpenTables = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxOpenTables);
        }

        /// <summary>
        /// Test the MaxCursors property.
        /// </summary>
        [TestMethod]
        public void MaxCursorsProperty()
        {
            int expected = 4000;
            this.instanceParameters.MaxCursors = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxCursors);
        }

        /// <summary>
        /// Test the MaxVerPages property.
        /// </summary>
        [TestMethod]
        public void MaxVerPagesProperty()
        {
            int expected = 128;
            this.instanceParameters.MaxVerPages = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxVerPages);
        }

        /// <summary>
        /// Test the MaxTemporaryTables property.
        /// </summary>
        [TestMethod]
        public void MaxTemporaryTablesProperty()
        {
            int expected = 7;
            this.instanceParameters.MaxTemporaryTables = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxTemporaryTables);
        }

        /// <summary>
        /// Test the CircularLog property.
        /// </summary>
        [TestMethod]
        public void LogFileSizeProperty()
        {
            int expected = 4096;
            this.instanceParameters.LogFileSize = expected;
            Assert.AreEqual(expected, this.instanceParameters.LogFileSize);
        }

        /// <summary>
        /// Test the CircularLog property.
        /// </summary>
        [TestMethod]
        public void CircularLogProperty()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.CircularLog = expected;
            Assert.AreEqual(expected, this.instanceParameters.CircularLog);
        }

        /// <summary>
        /// Test the CheckpointDepthMax property.
        /// </summary>
        [TestMethod]
        public void CheckpointDepthMaxProperty()
        {
            int expected = 30000;
            this.instanceParameters.CheckpointDepthMax = expected;
            Assert.AreEqual(expected, this.instanceParameters.CheckpointDepthMax);
        }

        /// <summary>
        /// Test the Recovery property.
        /// </summary>
        [TestMethod]
        public void RecoveryProperty()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.Recovery = expected;
            Assert.AreEqual(expected, this.instanceParameters.Recovery);
        }

        /// <summary>
        /// Test the EnableIndexChecking property.
        /// </summary>
        [TestMethod]
        public void EnableIndexCheckingProperty()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.EnableIndexChecking = expected;
            Assert.AreEqual(expected, this.instanceParameters.EnableIndexChecking);
        }

        /// <summary>
        /// Test the no information event.
        /// </summary>
        [TestMethod]
        public void NoInformationEventProperty()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.NoInformationEvent = expected;
            Assert.AreEqual(expected, this.instanceParameters.NoInformationEvent);
        }

        /// <summary>
        /// Test the create path if not exist property.
        /// </summary>
        [TestMethod]
        public void CreatePathIfNotExistProperty()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.CreatePathIfNotExist = expected;
            Assert.AreEqual(expected, this.instanceParameters.CreatePathIfNotExist);
        }
    }
}
