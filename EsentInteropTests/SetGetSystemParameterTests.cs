//-----------------------------------------------------------------------
// <copyright file="SetGetSystemParameterTests.cs" company="Microsoft Corporation">
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
    /// Jet{Get,Set}SystemParameter tests
    /// </summary>
    [TestClass]
    public class SetGetSystemParameterTests
    {
        /// <summary>
        /// Test setting and retrieving the system path.
        /// </summary>
        [TestMethod]
        public void SystemPathParameterTest()
        {
            this.PathParameterTest(JET_param.SystemPath, @"foo\system\");
        }

        /// <summary>
        /// Test setting and retrieving the log path.
        /// </summary>
        [TestMethod]
        public void LogPathParameterTest()
        {
            this.PathParameterTest(JET_param.LogFilePath, @"foo\log\");
        }

        /// <summary>
        /// Test setting and retrieving the temp path.
        /// </summary>
        [TestMethod]
        public void TempPathParameterTest()
        {
            this.PathParameterTest(JET_param.TempPath, @"foo\temp\");
        }

        /// <summary>
        /// Test setting and retrieving the base name.
        /// </summary>
        [TestMethod]
        public void BaseNameParameterTest()
        {
            this.StringParameterTest(JET_param.BaseName, @"foo");
        }

        /// <summary>
        /// Test setting and retrieving the base name.
        /// </summary>
        [TestMethod]
        public void RecoveryParameterTest()
        {
            this.StringParameterTest(JET_param.Recovery, @"off");
        }

        /// <summary>
        /// Test setting and retrieving the base name.
        /// </summary>
        [TestMethod]
        public void CircularLogParameterTest()
        {
            this.StringParameterTest(JET_param.CircularLog, 1);
        }

        /// <summary>
        /// Test setting and retrieving a system parameter that uses a path. A relative
        /// path is set but a full path is retrieved.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The path to set it to.</param>
        private void PathParameterTest(JET_param param, string expected)
        {
            JET_INSTANCE instance;
            API.JetCreateInstance(out instance, "PathParameterTest");
            try
            {
                API.JetSetSystemParameter(instance, JET_SESID.Nil, param, 0, expected);

                int ignored;
                string actual;
                API.JetGetSystemParameter(instance, JET_SESID.Nil, param, out ignored, out actual, 256);

                Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, expected), actual);
            }
            finally
            {
                API.JetTerm(instance);
            }
        }

        /// <summary>
        /// Test setting and retrieving a system parameter that uses a string.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private void StringParameterTest(JET_param param, string expected)
        {
            JET_INSTANCE instance;
            API.JetCreateInstance(out instance, "StringParameterTest");
            try
            {
                API.JetSetSystemParameter(instance, JET_SESID.Nil, param, 0, expected);

                int ignored;
                string actual;
                API.JetGetSystemParameter(instance, JET_SESID.Nil, param, out ignored, out actual, 256);

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                API.JetTerm(instance);
            }
        }

        /// <summary>
        /// Test setting and retrieving an integer system parameter..
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private void StringParameterTest(JET_param param, int expected)
        {
            JET_INSTANCE instance;
            API.JetCreateInstance(out instance, "IntParameterTest");
            try
            {
                API.JetSetSystemParameter(instance, JET_SESID.Nil, param, expected, null);

                int actual;
                string ignored;
                API.JetGetSystemParameter(instance, JET_SESID.Nil, param, out actual, out ignored, 0);

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                API.JetTerm(instance);
            }
        }
    }
}
