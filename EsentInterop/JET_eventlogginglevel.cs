//-----------------------------------------------------------------------
// <copyright file="JET_eventlogginglevel.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;

    /// <summary>
    /// Options for EventLoggingLevel.
    /// </summary>
    public enum EventLoggingLevels
    {
        /// <summary>
        /// Disable all events.
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Default level.
        /// </summary>
        Min = 1,

        /// <summary>
        /// Low verbosity and lower.
        /// </summary>
        Low = 25,

        /// <summary>
        /// Medium verbosity and lower.
        /// </summary>
        Medium = 50,

        /// <summary>
        /// High verbosity and lower.
        /// </summary>
        High = 75,

        /// <summary>
        /// All events.
        /// </summary>
        Max = 100,
    }
}
