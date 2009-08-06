//-----------------------------------------------------------------------
// <copyright file="EsentVersionTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test the static version class
    /// </summary>
    [TestClass]
    public class EsentVersionTests
    {
        /// <summary>
        /// Print the current version of Esent (for debugging)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void PrintVersion()
        {
            if (EsentVersion.SupportsServer2003Features)
            {
                Console.WriteLine("SupportsServer2003Features");    
            }

            if (EsentVersion.SupportsVistaFeatures)
            {
                Console.WriteLine("SupportsVistaFeatures");
            }

            if (EsentVersion.SupportsWindows7Features)
            {
                Console.WriteLine("SupportsWindows7Features");
            }

            if (EsentVersion.SupportsUnicodePaths)
            {
                Console.WriteLine("SupportsUnicodePaths");
            }

            if (EsentVersion.SupportsLargeKeys)
            {
                Console.WriteLine("SupportsLargeKeys");
            }
        }

        /// <summary>
        /// If Windows 7 is supported then older features must be 
        /// supported too.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyWindows7FeaturesIncludesOlderFeatures()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                Assert.IsTrue(EsentVersion.SupportsServer2003Features);
                Assert.IsTrue(EsentVersion.SupportsVistaFeatures);
                Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
                Assert.IsTrue(EsentVersion.SupportsLargeKeys);
            }
        }

        /// <summary>
        /// If Windows Vista is supported then older features must be 
        /// supported too.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyWindowsVistaFeaturesIncludesOlderFeatures()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                Assert.IsTrue(EsentVersion.SupportsServer2003Features);
                Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
                Assert.IsTrue(EsentVersion.SupportsLargeKeys);
            }
        }
    }
}