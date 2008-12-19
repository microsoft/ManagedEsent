//-----------------------------------------------------------------------
// <copyright file="InitTermTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Init/Term tests
    /// </summary>
    [TestClass]
    public class InitTermTests
    {
        /// <summary>
        /// Initialize and terminate one instance.
        /// </summary>
        [TestMethod]
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
        /// Initialize and terminate one instance twice.
        /// (Init/Term/Init/Term).
        /// </summary>
        [TestMethod]
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
