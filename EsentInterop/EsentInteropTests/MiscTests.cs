//-----------------------------------------------------------------------
// <copyright file="MiscTests.cs" company="Microsoft Corporation">
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
    /// Miscellaneous tests.
    /// </summary>
    [TestClass]
    public class MiscTests
    {
        /// <summary>
        /// Call JetFreeBuffer on a null buffer.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void FreeNullBuffer()
        {
            Api.JetFreeBuffer(IntPtr.Zero);
        }
    }
}
