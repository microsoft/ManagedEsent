//-----------------------------------------------------------------------
// <copyright file="EsentVersion.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Isam.Esent.Interop.Implementation;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Gives information about the version of esent being used.
    /// </summary>
    public static class EsentVersion
    {
        /// <summary>
        /// Initializes static members of the EsentVersion class.
        /// </summary>
        static EsentVersion()
        {
            SetCapabilities(Api.Impl.Capabilities);
        }

        /// <summary>
        /// Gets a value indicating whether the current version of esent
        /// can use non-ASCII paths to access databases.
        /// </summary>
        public static bool SupportsUnicodePaths
        {
            get
            {
                return Capabilities.SupportsUnicodePaths;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current version of esent
        /// supports features available in the Windows Vista version of
        /// esent.
        /// </summary>
        public static bool SupportsVistaFeatures
        {
            get
            {
                return Capabilities.SupportsVistaFeatures;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current version of esent
        /// supports features available in the Windows 7 version of
        /// esent.
        /// </summary>
        public static bool SupportsWindows7Features
        {
            get
            {
                return Capabilities.SupportsWindows7Features;
            }
        }

        /// <summary>
        /// Gets or sets a description of the current Esent capabilities.
        /// </summary>
        /// <remarks>
        /// We allow this to be set separately so that capabilities can
        /// be downgraded for testing.
        /// </remarks>
        private static JetCapabilities Capabilities { get; set; }

        /// <summary>
        /// Sets the Esent capabilities.
        /// </summary>
        /// <param name="newCapabilities">The current capabilities.</param>
        internal static void SetCapabilities(JetCapabilities newCapabilities)
        {
            Capabilities = newCapabilities;
        }
    }
}