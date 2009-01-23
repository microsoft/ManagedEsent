//-----------------------------------------------------------------------
// <copyright file="EsentErrorException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using Microsoft.Isam.Esent;

    /// <summary>
    /// Base class for ESENT exceptions
    /// </summary>
    [Serializable]
    public class EsentErrorException : EsentException
    {
        /// <summary>
        /// Initializes a new instance of the EsentErrorException class.
        /// </summary>
        /// <param name="err">The error code of the exception.</param>
        internal EsentErrorException(JET_err err)
        {
            this.Data["error"] = err;
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
                int errNum = (int)this.Data["error"];
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
        /// Gets the underlying Esent error for this exception.
        /// </summary>
        public JET_err Error
        {
            get
            {
                return (JET_err)this.Data["error"];
            }
        }
    }
}
