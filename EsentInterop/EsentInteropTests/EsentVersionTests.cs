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
    }
}