// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryDefaultConfig.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//  Code that supports meta-data configuration for the dictionary database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using Microsoft.Database.Isam.Config;

    /// <summary>
    /// Default configuration used by the dictionary.
    /// </summary>
    internal sealed class PersistentDictionaryDefaultConfig
    {
        /// <summary>
        /// Returns the default Ese configuration params.
        /// </summary>
        /// <returns>The default configuration params.</returns>
        public static DatabaseConfig GetDefaultDatabaseConfig()
        {
            return new DatabaseConfig()
            {
                // Global params
                Configuration = 1,
                EnableAdvanced = true,
                MaxInstances = 1024,
                DatabasePageSize = 8192,
                CacheSizeMin = 8192, // 64MB
                CacheSizeMax = int.MaxValue,

                // Instance params
                Identifier = Guid.NewGuid().ToString(),
                DisplayName = "PersistentDictionary",
                CreatePathIfNotExist = true,
                BaseName = "epc",
                DatabaseFilename = "PersistentDictionary.edb",
                EnableIndexChecking = false, // TODO: fix unicode indexes
                CircularLog = true,
                CheckpointDepthMax = 64 * 1024 * 1024,
                LogFileSize = 1024, // 1MB logs
                LogBuffers = 1024, // buffers = 1/2 of logfile
                MaxTemporaryTables = 0,
                MaxVerPages = 1024,
                NoInformationEvent = true,
                WaypointLatency = 1,
                MaxSessions = 256,
                MaxOpenTables = 256,
            };
        }
    }
}