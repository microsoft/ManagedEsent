//-----------------------------------------------------------------------
// <copyright file="TestUtilities.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace PixieTests
{
    internal class TestUtilities
    {
        public static void FinalizeAndGCCollect()
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}