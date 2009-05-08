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
            Console.WriteLine("{0}.{1}", EsentVersion.MajorVersion, EsentVersion.BuildNumber);

            if (EsentVersion.SupportsVistaFeatures)
            {
                Console.WriteLine("SupportsVistaFeatures");
            }

            if (EsentVersion.SupportsWindows7Features)
            {
                Console.WriteLine("SupportsWindows7Features");
            }
        }

        /// <summary>
        /// Verify the major version is not zero.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyMajorVersionIsNotZero()
        {
            Assert.AreNotEqual(0, EsentVersion.MajorVersion);
        }

        /// <summary>
        /// Verify the build number is not zero.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyBuildNumberIsNotZero()
        {
            Assert.AreNotEqual(0, EsentVersion.BuildNumber);
        }
    }
}