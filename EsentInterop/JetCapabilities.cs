//-----------------------------------------------------------------------
// <copyright file="JetCapabilities.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Implementation
{
    /// <summary>
    /// Describes the functionality exposed by an object which implements IJetApi
    /// </summary>
    internal class JetCapabilities
    {
        /// <summary>
        /// Gets or sets a value indicating whether Vista features (in the
        /// Interop.Vista namespace) are supported.
        /// </summary>
        public bool SupportsVistaFeatures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Win7 features (in the
        /// Interop.Windows7 namespace) are supported.
        /// </summary>
        public bool SupportsWindows7Features { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unicode file paths are supported.
        /// </summary>
        public bool SupportsUnicodePaths { get; set; }
    }
}