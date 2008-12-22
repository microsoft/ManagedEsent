//-----------------------------------------------------------------------
// <copyright file="InstanceTests.cs" company="Microsoft Corporation">
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
    /// Test the disposable Instance class, which wraps a JET_INSTANCE.
    /// </summary>
    [TestClass]
    public class InstanceTests
    {
        /// <summary>
        /// Allocate an instance, but don't initialize it.
        /// </summary>
        [TestMethod]
        public void CreateInstanceNoInit()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (Instance instance = new Instance("theinstance"))
            {
                Assert.AreNotEqual(JET_INSTANCE.Nil, instance.JetInstance);
                Assert.IsNotNull(instance.Parameters);

                instance.Parameters.LogFilePath = dir;
                instance.Parameters.SystemPath = dir;
                instance.Parameters.TempPath = dir;
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Allocate an instance and initialize it.
        /// </summary>
        [TestMethod]
        public void CreateInstanceInit()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (Instance instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFilePath = dir;
                instance.Parameters.SystemPath = dir;
                instance.Parameters.TempPath = dir;
                instance.Init();
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Allocate an instance and initialize it and then terminate.
        /// </summary>
        [TestMethod]
        public void CreateInstanceInitTerm()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (Instance instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFilePath = dir;
                instance.Parameters.SystemPath = dir;
                instance.Parameters.TempPath = dir;
                instance.Init();
                instance.Term();
                Directory.Delete(dir, true);    // only works if the instance is terminated
            }
        }

        /// <summary>
        /// Check that terminating the instance zeroes the JetInstance property.
        /// </summary>
        [TestMethod]
        public void InstanceTermZeroesJetInstance()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (Instance instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFilePath = dir;
                instance.Parameters.SystemPath = dir;
                instance.Parameters.TempPath = dir;
                instance.Init();
                instance.Term();
                Assert.AreEqual(JET_INSTANCE.Nil, instance.JetInstance);
            }
            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Check that terminating the instance zeroes the JetInstance property.
        /// </summary>
        [TestMethod]
        public void InstanceTermZeroesParameters()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (Instance instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFilePath = dir;
                instance.Parameters.SystemPath = dir;
                instance.Parameters.TempPath = dir;
                instance.Init();
                instance.Term();
                Assert.IsNull(instance.Parameters);
            }
            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Make sure that accessing the instance of a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetInstanceThrowsExceptionWhenInstanceIsDisposed()
        {
            Instance instance = new Instance("theinstance");
            instance.Dispose();
            var x = instance.JetInstance;
        }

        /// <summary>
        /// Make sure that accessing the parameters of a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ParametersThrowsExceptionWhenInstanceIsDisposed()
        {
            Instance instance = new Instance("theinstance");
            instance.Dispose();
            var x = instance.Parameters;
        }

        /// <summary>
        /// Make sure that calling Init on a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void InitThrowsExceptionWhenInstanceIsDisposed()
        {
            Instance instance = new Instance("theinstance");
            instance.Dispose();
            instance.Init();
        }

        /// <summary>
        /// Make sure that calling Term on a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TermThrowsExceptionWhenInstanceIsDisposed()
        {
            Instance instance = new Instance("theinstance");
            instance.Dispose();
            instance.Term();
        }
    }
}