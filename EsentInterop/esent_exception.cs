//-----------------------------------------------------------------------
// <copyright file="esent_exception.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;

    /// <summary>
    /// Base class for ESENT exceptions
    /// </summary>
    [Serializable]
    public class EsentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the EsentException class.
        /// </summary>
        /// <param name="err">The error code of the exception.</param>
        internal EsentException(int err)
        {
            this.Error = err;
        }

        /// <summary>
        /// Gets a text message describing the error.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error {0} (<unknown>)", this.Error);
            }
        }

        /// <summary>
        /// Gets the error code of the error.
        /// </summary>
        public int Error { get; private set; }
    }
}
