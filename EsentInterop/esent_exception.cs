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
        internal EsentException(JET_err err)
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
                return String.Format("Error {0} ({1})", this.Error, this.ErrorDescription);
            }
        }

        /// <summary>
        /// Gets a text description of the error.
        /// </summary>
        public string ErrorDescription
        {
            get
            {
                int errNum = (int)this.Error;
                string description;
                JET_err err = (JET_err)ErrApi.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.ErrorToString, ref errNum, out description, 1024);
                if (JET_err.Success == err)
                {
                    return description;
                }
                else
                {
                    return "<unknown>";
                }
            }
        }

        /// <summary>
        /// Gets the error code of the error.
        /// </summary>
        public JET_err Error { get; private set; }
    }
}
