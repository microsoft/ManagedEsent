//-----------------------------------------------------------------------
// <copyright file="InitTermTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.IO;
    using Esent.Interop;
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
                API.JetInit(ref instance);
                API.JetTerm(instance);
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
                API.JetInit(ref instance);
                API.JetTerm(instance);
                API.JetInit(ref instance);
                API.JetTerm(instance);
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
        public void InitAndTermOneTwoInstances()
        {    
            string dir1 = SetupHelper.CreateRandomDirectory();
            string dir2 = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance1 = SetupHelper.CreateNewInstance(dir1);
                JET_INSTANCE instance2 = SetupHelper.CreateNewInstance(dir2);
                API.JetInit(ref instance1);
                API.JetInit(ref instance2);
                API.JetTerm(instance1);
                API.JetTerm(instance2);
            }
            finally
            {
                Directory.Delete(dir1, true);
                Directory.Delete(dir2, true);
            }
        }
    }
}
