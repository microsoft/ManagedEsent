//-----------------------------------------------------------------------
// <copyright file="CollectionsSystemParameters.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Isam.Esent.Interop;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Global parameters for all collections.
    /// </summary>
    internal static class Globals
    {
        /// <summary>
        /// Used to make sure only one thread can perform the global initialization.
        /// </summary>
        private static readonly object initLock = new object();

        /// <summary>
        /// True if the Init() method has already run.
        /// </summary>
        private static bool isInit;

        /// <summary>
        /// A global initialization function. This should be called
        /// exactly once, before any ESENT instances are created.
        /// </summary>
        public static void Init()
        {
            if (!isInit)
            {
                lock (initLock)
                {
                    if (!isInit)
                    {
                        DoInit();
                        isInit = true;
                    }
                }
            }
        }

        /// <summary>
        /// Perform the global initialization. This sets the page
        /// size, configuration, cache size and other global
        /// parameters.
        /// </summary>
        private static void DoInit()
        {
            SystemParameters.DatabasePageSize = 8192;
            SystemParameters.Configuration = 0;
            SystemParameters.EnableAdvanced = true;
            SystemParameters.CacheSizeMin = 64;
            SystemParameters.CacheSizeMax = Int32.MaxValue;
        }
    }
}

