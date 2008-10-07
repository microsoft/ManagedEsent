//-----------------------------------------------------------------------
// <copyright file="InstanceParameters.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.IO;

    /// <summary>
    /// This class provides properties to set and get system parameters
    /// on an ESENT instance.
    /// </summary>
    public class InstanceParameters
    {
        /// <summary>
        /// The instance to set parameters on.
        /// </summary>
        private readonly JET_INSTANCE instance;

        /// <summary>
        /// The session to set parameters with.
        /// </summary>
        private readonly JET_SESID sesid;

        /// <summary>
        /// Initializes a new instance of the InstanceParameters class.
        /// </summary>
        /// <param name="instance">The instance to set parameters on.</param>
        /// <param name="sesid">The session to set parameters with.</param>
        public InstanceParameters(JET_INSTANCE instance, JET_SESID sesid)
        {
            this.instance = instance;
            this.sesid = sesid;
        }

        /// <summary>
        /// Gets or sets the relative or absolute file system path of the
        /// folder that will contain the checkpoint file for the instance. The path
        /// must be terminated with a backslash character, which indicates that the
        /// target path is a folder. 
        /// </summary>
        public string SystemPath
        {
            get
            {
                return this.GetStringParameter(JET_param.SystemPath);
            }

            set
            {
                this.SetStringParameter(JET_param.SystemPath, value);
            }
        }

        /// <summary>
        /// Gets or sets the relative or absolute file system path of
        /// the folder or file that will contain the temporary database for the instance.
        /// If the path is to a folder that will contain the temporary database then it
        /// must be terminated with a backslash character.
        /// </summary>
        public string TempPath
        {
            get
            {
                return this.GetStringParameter(JET_param.TempPath);
            }

            set
            {
                this.SetStringParameter(JET_param.TempPath, value);
            }
        }

        /// <summary>
        /// Gets or sets the relative or absolute file system path of the
        /// folder that will contain the transaction logs for the instance. The path must
        /// be terminated with a backslash character, which indicates that the target path
        /// is a folder.
        /// </summary>
        public string LogFilePath
        {
            get
            {
                return this.GetStringParameter(JET_param.LogFilePath);
            }

            set
            {
                this.SetStringParameter(JET_param.LogFilePath, value);
            }
        }

        /// <summary>
        /// Gets or sets the three letter prefix used for many of the files used by
        /// the database engine. For example, the checkpoint file is called EDB.CHK by
        /// default because EDB is the default base name.
        /// </summary>
        public string BaseName
        {
            get
            {
                return this.GetStringParameter(JET_param.BaseName);
            }

            set
            {
                this.SetStringParameter(JET_param.BaseName, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether circular logging is on.
        /// When circular logging is off, all transaction log files that are generated
        /// are retained on disk until they are no longer needed because a full backup of the
        /// database has been performed. When circular logging is on, only transaction log files
        /// that are younger than the current checkpoint are retained on disk. The benefit of
        /// this mode is that backups are not required to retire old transaction log files. 
        /// </summary>
        public bool CircularLog
        {
            get
            {
                return 0 != this.GetIntegerParameter(JET_param.CircularLog);
            }

            set
            {
                if (value)
                {
                    this.SetIntegerParameter(JET_param.CircularLog, 1);
                }
                else
                {
                    this.SetIntegerParameter(JET_param.CircularLog, 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether JetAttachDatabase will check for
        /// indexes that were build using an older version of the NLS library in the
        /// operating system.
        /// </summary>
        public bool EnableIndexChecking
        {
            get
            {
                return 0 != this.GetIntegerParameter(JET_param.EnableIndexChecking);
            }

            set
            {
                if (value)
                {
                    this.SetIntegerParameter(JET_param.EnableIndexChecking, 1);
                }
                else
                {
                    this.SetIntegerParameter(JET_param.EnableIndexChecking, 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether crash recovery is on.
        /// </summary>
        public bool Recovery
        {
            get
            {
                return 0 == String.Compare(this.GetStringParameter(JET_param.Recovery), "on", true);
            }

            set
            {
                if (value)
                {
                    this.SetStringParameter(JET_param.Recovery, "on");
                }
                else
                {
                    this.SetStringParameter(JET_param.Recovery, "off");
                }
            }
        }

        /// <summary>
        /// Set a system parameter which is a string.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private void SetStringParameter(JET_param param, string value)
        {
            API.JetSetSystemParameter(this.instance, this.sesid, param, 0, value);
        }

        /// <summary>
        /// Get a system parameter which is a string.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private string GetStringParameter(JET_param param)
        {
            int ignored = 0;
            string value;
            API.JetGetSystemParameter(this.instance, this.sesid, param, ref ignored, out value, 1024);
            return value;
        }

        /// <summary>
        /// Set a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private void SetIntegerParameter(JET_param param, int value)
        {
            API.JetSetSystemParameter(this.instance, this.sesid, param, value, null);
        }

        /// <summary>
        /// Get a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private int GetIntegerParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            API.JetGetSystemParameter(this.instance, this.sesid, param, ref value, out ignored, 0);
            return value;
        }
    }
}