//-----------------------------------------------------------------------
// <copyright file="InitTermTests.cs" company="Microsoft Corporation">
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
    /// Init/Term tests
    /// </summary>
    [TestClass]
    public class InitTermTests
    {
        /// <summary>
        /// Verify that the version returned by JetGetVersion is not zero
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyJetVersionIsNotZero()
        {
            JET_INSTANCE instance;
            JET_SESID sesid;
            int version;

            Api.JetCreateInstance(out instance, "JetGetVersion");
            
            var parameters = new InstanceParameters(instance);
            parameters.Recovery = false;
            parameters.MaxTemporaryTables = 0;
            parameters.NoInformationEvent = true;

            Api.JetInit(ref instance);
            Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
            Api.JetGetVersion(sesid, out version);
            Api.JetTerm(instance);

            Assert.AreNotEqual(0, version);
            System.Console.WriteLine("Version = 0x{0:X}", version);
        }

        /// <summary>
        /// Initialize and terminate one instance. The instance is allocated
        /// with JetCreateInstance2.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void CreateInstanceWithJetCreateInstance2()
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, Guid.NewGuid().ToString(), "Instance Display Name", CreateInstanceGrbit.None);

            var systemParameters = new InstanceParameters(instance);
            systemParameters.MaxTemporaryTables = 0;
            systemParameters.Recovery = false;
            systemParameters.NoInformationEvent = true;

            Api.JetInit(ref instance);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Terminating an uninitialized instance should work
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyTermUninitializedInstanceDoesNotThrowException()
        {
            var instance = new JET_INSTANCE();
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Terminating an uninitialized instance should work
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyTerm2UninitializedInstanceDoesNotThrowException()
        {
            var instance = new JET_INSTANCE();
            Api.JetTerm2(instance, TermGrbit.None);
        }

        /// <summary>
        /// Initialize and terminate one instance.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void InitAndTermOneInstance()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Initialize and terminate one instance abruptly.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void InitAndTermOneInstanceAbruptly()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                Api.JetTerm2(instance, TermGrbit.Abrupt);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Initialize and terminate one instance twice.
        /// (Init/Term/Init/Term).
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void InitAndTermOneInstanceTwice()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                Api.JetTerm(instance);
                Api.JetInit(ref instance);
                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Initialize and terminate two instances
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void InitAndTermTwoInstances()
        {    
            string dir1 = SetupHelper.CreateRandomDirectory();
            string dir2 = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance1 = SetupHelper.CreateNewInstance(dir1);
                JET_INSTANCE instance2 = SetupHelper.CreateNewInstance(dir2);
                Api.JetInit(ref instance1);
                Api.JetInit(ref instance2);
                Api.JetTerm(instance1);
                Api.JetTerm(instance2);
            }
            finally
            {
                Directory.Delete(dir1, true);
                Directory.Delete(dir2, true);
            }
        }

        /// <summary>
        /// Duplicate a session
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void TestJetDupSession()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);
                JET_SESID sesidDup;
                Api.JetDupSession(sesid, out sesidDup);
                Assert.AreNotEqual(sesid, sesidDup);
                Assert.AreNotEqual(sesidDup, JET_SESID.Nil);
                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
