//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace InteropApiTests
{
    /// <summary>
    /// Esent configuration information.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Gets a value indicating whether the version of Esent being tested supports
        /// Vista-only features.
        /// </summary>
        public static bool SupportsVistaFeatures
        {
            get
            {
                return Environment.OSVersion.Version.Major >= 6;
            }
        }
    }
}