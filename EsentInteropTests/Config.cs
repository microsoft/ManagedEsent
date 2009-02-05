//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;

    /// <summary>
    /// Esent configuration information.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Gets a value specifying whether the version of Esent being tested supports
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