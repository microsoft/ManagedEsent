//-----------------------------------------------------------------------
// <copyright file="EsentErrorException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Isam.Esent.Interop
{
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
        /// Initializes a new instance of the EsentErrorException class. This constructor
        /// is used to deserialize a serialized exception.
        /// </summary>
        /// <param name="info">The data needed to deserialize the object.</param>
        /// <param name="context">The deserialization context.</param>
        protected EsentErrorException(SerializationInfo info, StreamingContext context) :
                base(info, context)
        {
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
                var errNum = (int)this.Data["error"];

                try 
                {
                    string description;
                    var wrn = Api.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.ErrorToString, ref errNum, out description, 1024);
                    if (JET_wrn.Success == wrn)
                    {
                        return description;
                    }
                }
                catch (EsentException)
                {
                    // ignore the error
                }

                return "<unknown>";
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
