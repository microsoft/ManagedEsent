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
        public static bool SupportsVistaFeatures
        {
            get
            {
                return Environment.OSVersion.Version.Major >= 6;
            }
        }
    }
}