//-----------------------------------------------------------------------
// <copyright file="InstanceTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
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
    }
}