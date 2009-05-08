//-----------------------------------------------------------------------
// <copyright file="EsentVersion.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Gives information about the version of esent being used.
    /// </summary>
    public static class EsentVersion
    {
        /// <summary>
        /// Initializes static members of the Version class.
        /// </summary>
        static EsentVersion()
        {
            var version = (uint)GetVersionFromEsent();
            MajorVersion = (int)(version >> 28);
            BuildNumber = (int) ((version & 0xFFFFFF) >> 8);

            if (BuildNumber >= 6000)
            {
                SupportsVistaFeatures = true;
            }

            if (BuildNumber >= 7000)
            {
                SupportsWindows7Features = true;
            }
        }

        /// <summary>
        /// Gets the major version of esent
        /// </summary>
        public static int MajorVersion { get; internal set; }

        /// <summary>
        /// Gets the build number of esent
        /// </summary>
        public static int BuildNumber { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the current version of esent
        /// supports features available in the Windows Vista version of
        /// esent.
        /// </summary>
        public static bool SupportsVistaFeatures { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the current version of esent
        /// supports features available in the Windows 7 version of
        /// esent.
        /// </summary>
        public static bool SupportsWindows7Features { get; internal set; }

        /// <summary>
        /// Create an instance and get the current version of Esent.
        /// </summary>
        /// <returns>The current version of Esent.</returns>
        private static int GetVersionFromEsent()
        {
            using (var instance = new Instance("GettingEsentVersion"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();

                using (var session = new Session(instance))
                {
                    int version;
                    Api.JetGetVersion(session, out version);
                    return version;
                }
            }
        }
    }
}