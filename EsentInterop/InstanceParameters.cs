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
        public InstanceParameters(JET_INSTANCE instance)
        {
            this.instance = instance;
            this.sesid = JET_SESID.Nil;
        }

        /// <summary>
        /// Gets or sets the relative or absolute file system path of the
        /// folder that will contain the checkpoint file for the instance.
        /// </summary>
        public string SystemDirectory
        {
            get
            {
                return this.AddTrailingDirectorySeparator(this.GetStringParameter(JET_param.SystemPath));
            }

            set
            {
                this.SetStringParameter(JET_param.SystemPath, this.AddTrailingDirectorySeparator(value));
            }
        }

        /// <summary>
        /// Gets or sets the relative or absolute file system path of
        /// the folder that will contain the temporary database for the instance.
        /// </summary>
        public string TempDirectory
        {
            get
            {
                // Older versions of Esent (e.g. Windows XP) will return the
                // full path of the temporary database. Extract the directory name.
                string path = this.GetStringParameter(JET_param.TempPath);
                string dir = Path.GetDirectoryName(path);
                return this.AddTrailingDirectorySeparator(dir);
            }

            set
            {
                this.SetStringParameter(JET_param.TempPath, this.AddTrailingDirectorySeparator(value));
            }
        }

        /// <summary>
        /// Gets or sets the relative or absolute file system path of the
        /// folder that will contain the transaction logs for the instance.
        /// </summary>
        public string LogFileDirectory
        {
            get
            {
                return this.AddTrailingDirectorySeparator(this.GetStringParameter(JET_param.LogFilePath));
            }

            set
            {
                this.SetStringParameter(JET_param.LogFilePath, this.AddTrailingDirectorySeparator(value));
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
        /// Gets or sets an application specific string that will be added to
        /// any event log messages that are emitted by the database engine. This allows
        /// easy correlation of event log messages with the source application. By default
        /// the host application executable name will be used.
        /// </summary>
        public string EventSource
        {
            get
            {
                return this.GetStringParameter(JET_param.EventSource);
            }

            set
            {
                this.SetStringParameter(JET_param.EventSource, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of sessions resources reserved for this instance.
        /// A session resource directly corresponds to a JET_SESID.
        /// </summary>
        public int MaxSessions
        {
            get
            {
                return this.GetIntegerParameter(JET_param.MaxSessions);
            }

            set
            {
                this.SetIntegerParameter(JET_param.MaxSessions, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of B+ Tree resources reserved for this instance.
        /// </summary>
        public int MaxOpenTables
        {
            get
            {
                return this.GetIntegerParameter(JET_param.MaxOpenTables);
            }

            set
            {
                this.SetIntegerParameter(JET_param.MaxOpenTables, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of cursor resources reserved for this instance.
        /// A cursor resource directly corresponds to a JET_TABLEID.
        /// </summary>
        public int MaxCursors
        {
            get
            {
                return this.GetIntegerParameter(JET_param.MaxCursors);
            }

            set
            {
                this.SetIntegerParameter(JET_param.MaxCursors, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of version store pages reserved
        /// for this instance.
        /// </summary>
        public int MaxVerPages
        {
            get
            {
                return this.GetIntegerParameter(JET_param.MaxVerPages);
            }

            set
            {
                this.SetIntegerParameter(JET_param.MaxVerPages, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of temporary table resources for use
        /// by an instance. This setting will affect how many temporary tables can be used at
        /// the same time. If this system parameter is set to zero then no temporary database
        /// will be created and any activity that requires use of the temporary database will
        /// fail. This setting can be useful to avoid the I/O required to create the temporary
        /// database if it is known that it will not be used.
        /// </summary>
        /// <remarks>
        /// The use of a temporary table also requires a cursor resource.
        /// </remarks>
        public int MaxTemporaryTables
        {
            get
            {
                return this.GetIntegerParameter(JET_param.MaxTemporaryTables);
            }

            set
            {
                this.SetIntegerParameter(JET_param.MaxTemporaryTables, value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the transaction log files. This parameter
        /// should be set in units of 1024 bytes (e.g. a setting of 2048 will
        /// give 2MB logfiles).
        /// </summary>
        public int LogFileSize
        {
            get
            {
                return this.GetIntegerParameter(JET_param.LogFileSize);
            }

            set
            {
                this.SetIntegerParameter(JET_param.LogFileSize, value);
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
                return this.GetBoolParameter(JET_param.CircularLog);
            }

            set
            {
                this.SetBoolParameter(JET_param.CircularLog, value);
            }
        }

        /// <summary>
        /// Gets or sets the threshold in bytes for about how many transaction log
        /// files will need to be replayed after a crash. If circular logging is enabled using
        /// CircularLog then this parameter will also control the approximate amount
        /// of transaction log files that will be retained on disk.
        /// </summary>
        public int CheckpointDepthMax
        {
            get
            {
                return this.GetIntegerParameter(JET_param.CheckpointDepthMax);
            }

            set
            {
                this.SetIntegerParameter(JET_param.CheckpointDepthMax, value);
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
        /// Gets or sets a value indicating whether JetAttachDatabase will check for
        /// indexes that were build using an older version of the NLS library in the
        /// operating system.
        /// </summary>
        public bool EnableIndexChecking
        {
            get
            {
                return this.GetBoolParameter(JET_param.EnableIndexChecking);
            }

            set
            {
                this.SetBoolParameter(JET_param.EnableIndexChecking, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether informational event 
        /// log messages that would ordinarily be generated by the
        /// database engine will be suppressed.
        /// </summary>
        public bool NoInformationEvent
        {
            get
            {
                return this.GetBoolParameter(JET_param.NoInformationEvent);
            }

            set
            {
                this.SetBoolParameter(JET_param.NoInformationEvent, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ESENT will silently create folders
        /// that are missing in its filesystem paths.
        /// </summary>
        public bool CreatePathIfNotExist
        {
            get
            {
                return this.GetBoolParameter(JET_param.CreatePathIfNotExist);
            }

            set
            {
                this.SetBoolParameter(JET_param.CreatePathIfNotExist, value);
            }
        }

        /// <summary>
        /// Add a trailing directory separator character to the string.
        /// </summary>
        /// <param name="dir">The directory.</param>
        /// <returns>The directory with a separator character added (if necesary).</returns>
        private string AddTrailingDirectorySeparator(string dir)
        {
            char[] sepChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            return string.Concat(dir.TrimEnd(sepChars), Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Set a system parameter which is a string.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private void SetStringParameter(JET_param param, string value)
        {
            Api.JetSetSystemParameter(this.instance, this.sesid, param, 0, value);
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
            Api.JetGetSystemParameter(this.instance, this.sesid, param, ref ignored, out value, 1024);
            return value;
        }

        /// <summary>
        /// Set a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private void SetIntegerParameter(JET_param param, int value)
        {
            Api.JetSetSystemParameter(this.instance, this.sesid, param, value, null);
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
            Api.JetGetSystemParameter(this.instance, this.sesid, param, ref value, out ignored, 0);
            return value;
        }

        /// <summary>
        /// Set a system parameter which is a boolean.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private void SetBoolParameter(JET_param param, bool value)
        {
            if (value)
            {
                Api.JetSetSystemParameter(this.instance, this.sesid, param, 1, null);
            }
            else
            {
                Api.JetSetSystemParameter(this.instance, this.sesid, param, 0, null);
            }
        }

        /// <summary>
        /// Get a system parameter which is a boolean.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private bool GetBoolParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            Api.JetGetSystemParameter(this.instance, this.sesid, param, ref value, out ignored, 0);
            return value != 0;
        }
    }
}