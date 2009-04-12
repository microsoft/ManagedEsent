//-----------------------------------------------------------------------
// <copyright file="InstanceTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
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
        [Priority(0)]
        public void CreateInstanceNoInit()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                Assert.AreNotEqual(JET_INSTANCE.Nil, instance.JetInstance);
                Assert.IsNotNull(instance.Parameters);

                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Test implicit conversion to a JET_INSTANCE
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void InstanceCanConvertToJetInstance()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                JET_INSTANCE jetinstance = instance;
                Assert.AreEqual(jetinstance, instance.JetInstance);
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Allocate an instance and initialize it.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateInstanceInit()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
                instance.Parameters.NoInformationEvent = true;
                instance.Init();
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Allocate an instance with a display name.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void CreateInstanceWithDisplayName()
        {
            using (var instance = new Instance(Guid.NewGuid().ToString(), "Friendly Display Name"))
            {
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.Recovery = false;
                instance.Init();
            }
        }

        /// <summary>
        /// Allocate an instance and initialize it and then terminate.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateInstanceInitTerm()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
                instance.Parameters.NoInformationEvent = true;
                instance.Init();
                instance.Term();
                Directory.Delete(dir, true);    // only works if the instance is terminated
            }
        }

        /// <summary>
        /// Check that terminating the instance zeroes the JetInstance property.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void InstanceTermZeroesJetInstance()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
                instance.Parameters.NoInformationEvent = true;
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
        [Priority(1)]
        public void InstanceTermZeroesParameters()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
                instance.Parameters.NoInformationEvent = true;
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
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetInstanceThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            JET_INSTANCE x = instance.JetInstance;
        }

        /// <summary>
        /// Make sure that accessing the parameters of a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ParametersThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            InstanceParameters x = instance.Parameters;
        }

        /// <summary>
        /// Make sure that calling Init on a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void InitThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            instance.Init();
        }

        /// <summary>
        /// Make sure that calling Term on a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TermThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            instance.Term();
        }
    }
}