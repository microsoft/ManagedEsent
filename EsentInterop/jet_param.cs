//-----------------------------------------------------------------------
// <copyright file="jet_param.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// ESENT system parameters
    /// </summary>
    public enum JET_param
    {
        /// <summary>
        /// This parameter indicates the relative or absolute file system path of the
        /// folder that will contain the checkpoint file for the instance. The path
        /// must be terminated with a backslash character, which indicates that the
        /// target path is a folder. 
        /// </summary>
        SystemPath = 0,

        /// <summary>
        /// This parameter indicates the relative or absolute file system path of
        /// the folder or file that will contain the temporary database for the instance.
        /// If the path is to a folder that will contain the temporary database then it
        /// must be terminated with a backslash character.
        /// </summary>
        TempPath = 1,

        /// <summary>
        /// This parameter indicates the relative or absolute file system path of the
        /// folder that will contain the transaction logs for the instance. The path must
        /// be terminated with a backslash character, which indicates that the target path
        /// is a folder.
        /// </summary>
        LogFilePath = 2,

        /// <summary>
        /// This parameter sets the three letter prefix used for many of the files used by
        /// the database engine. For example, the checkpoint file is called EDB.CHK by
        /// default because EDB is the default base name.
        /// </summary>
        BaseName = 3,

        /// <summary>
        /// This parameter configures how transaction log files are managed by the database
        /// engine. When circular logging is off, all transaction log files that are generated
        /// are retained on disk until they are no longer needed because a full backup of the
        /// database has been performed. When circular logging is on, only transaction log files
        /// that are younger than the current checkpoint are retained on disk. The benefit of
        /// this mode is that backups are not required to retire old transaction log files. 
        /// </summary>
        CircularLog = 17,

        /// <summary>
        /// This parameter is the master switch that controls crash recovery for an instance.
        /// If this parameter is set to "On" then ARIES style recovery will be used to bring all
        /// databases in the instance to a consistent state in the event of a process or machine
        /// crash. If this parameter is set to "Off" then all databases in the instance will be
        /// managed without the benefit of crash recovery. That is to say, that if the instance
        /// is not shut down cleanly using JetTerm prior to the process exiting or machine shutdown
        /// then the contents of all databases in that instance will be corrupted.
        /// </summary>
        Recovery = 34,

        /// <summary>
        /// This parameter configures the page size for the database. The page
        /// size is the smallest unit of space allocation possible for a database
        /// file. The database page size is also very important because it sets
        /// the upper limit on the size of an individual record in the database. 
        /// </summary>
        /// <remarks>
        /// Only one database page size is supported per process at this time.
        /// This means that if you are in a single process that contains different
        /// applications that use the database engine then they must all agree on
        /// a database page size.
        /// </remarks>
        DatabasePageSize = 64,
    }
}