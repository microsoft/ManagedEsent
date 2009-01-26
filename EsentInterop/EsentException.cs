//-----------------------------------------------------------------------
// <copyright file="EsentException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent
{
    using System;
    using Microsoft.Isam.Esent;

    /// <summary>
    /// Base class for ESENT exceptions
    /// </summary>
    [Serializable]
    public class EsentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the EsentException class.
        /// </summary>
        public EsentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EsentException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EsentException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EsentException class with a specified error message and
        /// a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public EsentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
