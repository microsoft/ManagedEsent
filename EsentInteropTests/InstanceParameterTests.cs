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
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        public void VerifyInstanceParametersFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.IsNotNull(this.instanceParameters);
        }

        /// <summary>
        /// Test the BaseName property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersBaseName()
        {
            string expected = "abc";
            this.instanceParameters.BaseName = expected;
            Assert.AreEqual(expected, this.instanceParameters.BaseName);            
        }

        /// <summary>
        /// Test the event source property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersEventSource()
        {
            string expected = "My Application";
            this.instanceParameters.EventSource = expected;
            Assert.AreEqual(expected, this.instanceParameters.EventSource);
        }

        /// <summary>
        /// Test the MaxSessions property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersMaxSessions()
        {
            int expected = 11;
            this.instanceParameters.MaxSessions = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxSessions);
        }

        /// <summary>
        /// Test the MaxOpenTables property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersMaxOpenTables()
        {
            int expected = 400;
            this.instanceParameters.MaxOpenTables = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxOpenTables);
        }

        /// <summary>
        /// Test the MaxCursors property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersMaxCursors()
        {
            int expected = 4000;
            this.instanceParameters.MaxCursors = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxCursors);
        }

        /// <summary>
        /// Test the MaxVerPages property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersMaxVerPages()
        {
            int expected = 128;
            this.instanceParameters.MaxVerPages = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxVerPages);
        }

        /// <summary>
        /// Test the MaxTemporaryTables property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersMaxTemporaryTables()
        {
            int expected = 7;
            this.instanceParameters.MaxTemporaryTables = expected;
            Assert.AreEqual(expected, this.instanceParameters.MaxTemporaryTables);
        }

        /// <summary>
        /// Test the CircularLog property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersLogFileSize()
        {
            int expected = 4096;
            this.instanceParameters.LogFileSize = expected;
            Assert.AreEqual(expected, this.instanceParameters.LogFileSize);
        }

        /// <summary>
        /// Test the CircularLog property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersCircularLog()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.CircularLog = expected;
            Assert.AreEqual(expected, this.instanceParameters.CircularLog);
        }

        /// <summary>
        /// Test the CheckpointDepthMax property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersCheckpointDepthMax()
        {
            int expected = 30000;
            this.instanceParameters.CheckpointDepthMax = expected;
            Assert.AreEqual(expected, this.instanceParameters.CheckpointDepthMax);
        }

        /// <summary>
        /// Test the Recovery property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersRecovery()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.Recovery = expected;
            Assert.AreEqual(expected, this.instanceParameters.Recovery);
        }

        /// <summary>
        /// Test the EnableIndexChecking property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersEnableIndexChecking()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.EnableIndexChecking = expected;
            Assert.AreEqual(expected, this.instanceParameters.EnableIndexChecking);
        }

        /// <summary>
        /// Test the no information event.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersNoInformationEvent()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.NoInformationEvent = expected;
            Assert.AreEqual(expected, this.instanceParameters.NoInformationEvent);
        }

        /// <summary>
        /// Test the create path if not exist property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersCreatePathIfNotExist()
        {
            bool expected = Any.Boolean;
            this.instanceParameters.CreatePathIfNotExist = expected;
            Assert.AreEqual(expected, this.instanceParameters.CreatePathIfNotExist);
        }

        /// <summary>
        /// Test the temporary directory property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersTempDirectory()
        {
            string dir = @"c:\foo\";
            this.instanceParameters.TempDirectory = dir;
            Assert.AreEqual(dir, this.instanceParameters.TempDirectory);
        }

        /// <summary>
        /// Test the temporary directory property without a trailing slash.
        /// Make sure the slash is added when retrieving it.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersTempDirectoryAddsSeparatorChar()
        {
            this.instanceParameters.TempDirectory = @"c:\bar\baz";
            Assert.AreEqual(@"c:\bar\baz\", this.instanceParameters.TempDirectory);
        }

        /// <summary>
        /// Test the log directory property. The trailing slash will be added.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersLogFileDirectory()
        {
            string dir = @"d:\logs";
            this.instanceParameters.LogFileDirectory = dir;
            Assert.AreEqual(@"d:\logs\", this.instanceParameters.LogFileDirectory);
        }

        /// <summary>
        /// Test the system directory property.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveInstanceParametersSystemDirectory()
        {
            string dir = @"d:\a\b\c\system\";
            this.instanceParameters.SystemDirectory = dir;
            Assert.AreEqual(@"d:\a\b\c\system\", this.instanceParameters.SystemDirectory);
        }
    }
}
