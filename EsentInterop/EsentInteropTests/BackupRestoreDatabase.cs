//-----------------------------------------------------------------------
// <copyright file="BackupRestoreDatabase.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Implementation of a backup/restore test.
    /// </summary>
    public class BackupRestoreDatabase
    {
        /// <summary>
        /// The directory containing the database.
        /// </summary>
        private readonly string databaseDirectory;

        /// <summary>
        /// The path to the database.
        /// </summary>
        private readonly string database;

        /// <summary>
        /// The directory that contains the backup.
        /// </summary>
        private readonly string backupDirectory;

        /// <summary>
        /// True if a status callback should be used.
        /// </summary>
        private readonly bool useStatusCallback;

        /// <summary>
        /// Set by the internal status callback.
        /// </summary>
        private bool statusCallbackWasCalled;

        /// <summary>
        /// Initializes a new instance of the BackupRestoreDatabase class.
        /// </summary>
        /// <param name="databaseDirectory">
        /// The directory to create a database in.
        /// </param>
        /// <param name="backupDirectory">
        /// The directory to backup the database to.
        /// </param>
        /// <param name="useStatusCallback">
        /// True if a status callback should be used.
        /// </param>
        public BackupRestoreDatabase(string databaseDirectory, string backupDirectory, bool useStatusCallback)
        {
            this.databaseDirectory = databaseDirectory;
            this.database = Path.Combine(this.databaseDirectory, "backmeup.edb");
            this.backupDirectory = backupDirectory;
            this.useStatusCallback = useStatusCallback;
        }

        /// <summary>
        /// Create a database, back it up to the backup directory and
        /// then restore it.
        /// </summary>
        public void TestBackupRestore()
        {
            try
            {
                this.CreateDatabase();
                this.BackupDatabase();
                this.DeleteDatabaseFiles();
                this.RestoreDatabase();
                this.CheckDatabase();
            }
            finally
            {
                DeleteDirectoryIfExists(this.databaseDirectory);
                DeleteDirectoryIfExists(this.backupDirectory);
            }
        }

        /// <summary>
        /// Backup a database and have the status callback throw
        /// an exception during backup.
        /// </summary>
        /// <param name="ex">
        /// The exception to throw from the callback.
        /// </param>
        public void TestBackupCallbackExceptionHandling(Exception ex)
        {
            try
            {
                this.CreateDatabase();
                this.BackupDatabaseWithCallbackException(ex);
            }
            finally
            {
                DeleteDirectoryIfExists(this.databaseDirectory);
                DeleteDirectoryIfExists(this.backupDirectory);
            }
        }

        /// <summary>
        /// Backup a database and have the status callback throw
        /// an exception during restore.
        /// </summary>
        /// <param name="ex">
        /// The exception to throw from the callback.
        /// </param>
        public void TestRestoreCallbackExceptionHandling(Exception ex)
        {
            try
            {
                this.CreateDatabase();
                this.BackupDatabase();
                this.DeleteDatabaseFiles();
                this.RestoreDatabaseWithCallbackException(ex);
            }
            finally
            {
                DeleteDirectoryIfExists(this.databaseDirectory);
                DeleteDirectoryIfExists(this.backupDirectory);
            }
        }

        /// <summary>
        /// Delete the directory if it exists.
        /// </summary>
        /// <param name="directory">The directory to delete.</param>
        private static void DeleteDirectoryIfExists(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// Create the database.
        /// </summary>
        private void CreateDatabase()
        {
            using (var instance = this.CreateInstance())
            {
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    using (var transaction = new Transaction(session))
                    {
                        JET_TABLEID tableid;
                        Api.JetCreateTable(session, dbid, "table", 1, 100, out tableid);
                        JET_COLUMNID columnid;
                        Api.JetAddColumn(session, tableid, "column", new JET_COLUMNDEF { coltyp = JET_coltyp.Long }, null, 0, out columnid);
                        using (var update = new Update(session, tableid, JET_prep.Insert))
                        {
                            Api.SetColumn(session, tableid, columnid, 17);
                            update.Save();    
                        }

                        transaction.Commit(CommitTransactionGrbit.None);
                    }
                }
            }
        }

        /// <summary>
        /// Backup the database.
        /// </summary>
        private void BackupDatabase()
        {
            using (var instance = this.CreateInstance())
            {
                instance.Init();
                using (var session = new Session(instance))
                {
                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.None);
                    JET_DBID dbid;
                    Api.JetOpenDatabase(session, this.database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                    if (this.useStatusCallback)
                    {
                        this.statusCallbackWasCalled = false;
                        Api.JetBackupInstance(instance, this.backupDirectory, BackupGrbit.None, this.StatusCallback);
                        Assert.IsTrue(
                            this.statusCallbackWasCalled, "expected the status callback to be called during backup");
                    }
                    else
                    {
                        Api.JetBackupInstance(instance, this.backupDirectory, BackupGrbit.None, null);                        
                    }
                }
            }
        }

        /// <summary>
        /// Backup the database and have the status callback throw an exception.
        /// </summary>
        /// <param name="ex">
        /// The exception to throw from the callback.
        /// </param>
        private void BackupDatabaseWithCallbackException(Exception ex)
        {
            using (var instance = this.CreateInstance())
            {
                instance.Init();
                using (var session = new Session(instance))
                {
                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.None);
                    JET_DBID dbid;
                    Api.JetOpenDatabase(session, this.database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                    Api.JetBackupInstance(
                        instance,
                        this.backupDirectory,
                        BackupGrbit.None,
                        (sesid, snt, snp, snprog) =>
                        {
                            throw ex;
                        });
                }
            }
        }

        /// <summary>
        /// Delete the database files from the database directory.
        /// </summary>
        private void DeleteDatabaseFiles()
        {            
            DeleteDirectoryIfExists(this.databaseDirectory);
            Directory.CreateDirectory(this.databaseDirectory);
        }

        /// <summary>
        /// Restore the database files.
        /// </summary>
        private void RestoreDatabase()
        {
            using (var instance = this.CreateInstance())
            {
                if (this.useStatusCallback)
                {
                    this.statusCallbackWasCalled = false;
                    Api.JetRestoreInstance(instance, this.backupDirectory, this.databaseDirectory, this.StatusCallback);
                    Assert.IsTrue(
                        this.statusCallbackWasCalled, "expected the status callback to be called during restore");
                }
                else
                {
                    Api.JetRestoreInstance(instance, this.backupDirectory, this.databaseDirectory, null);                    
                }
            }
        }

        /// <summary>
        /// Restore the database files and have the status callback throw an exception.
        /// </summary>
        /// <param name="ex">The exception to throw from the status callback.</param>
        private void RestoreDatabaseWithCallbackException(Exception ex)
        {
            using (var instance = this.CreateInstance())
            {
                Api.JetRestoreInstance(
                    instance,
                    this.backupDirectory,
                    this.databaseDirectory,
                    (sesid, snt, snp, snprog) =>
                    {
                        throw ex;
                    });
            }
        }

        /// <summary>
        /// Check the database files have been restored.
        /// </summary>
        private void CheckDatabase()
        {
            using (var instance = this.CreateInstance())
            {
                instance.Init();
                using (var session = new Session(instance))
                {
                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.ReadOnly);
                    JET_DBID dbid;
                    Api.JetOpenDatabase(session, this.database, String.Empty, out dbid, OpenDatabaseGrbit.ReadOnly);

                    JET_TABLEID tableid;
                    Api.JetOpenTable(session, dbid, "table", null, 0, OpenTableGrbit.ReadOnly, out tableid);
                    JET_COLUMNID columnid = Api.GetTableColumnid(session, tableid, "column");
                    Assert.IsTrue(Api.TryMoveFirst(session, tableid));
                    Assert.AreEqual(17, Api.RetrieveColumnAsInt32(session, tableid, columnid));
                }
            }
        }

        /// <summary>
        /// Create a new instance, setting the appropriate system parameters.
        /// </summary>
        /// <returns>A new instance.</returns>
        private Instance CreateInstance()
        {
            var instance = new Instance("BackupRestoreDatabase");
            instance.Parameters.LogFileSize = 256;
            instance.Parameters.LogFileDirectory = this.databaseDirectory;
            instance.Parameters.TempDirectory = this.databaseDirectory;
            instance.Parameters.SystemDirectory = this.databaseDirectory;
            instance.Parameters.CreatePathIfNotExist = true;
            return instance;
        }

        /// <summary>
        /// Progress reporting callback.
        /// </summary>
        /// <param name="sesid">The session performing the operation.</param>
        /// <param name="snp">The operation type.</param>
        /// <param name="snt">The type of the progress report.</param>
        /// <param name="snprog">Progress info.</param>
        /// <returns>An error code.</returns>
        private JET_err StatusCallback(JET_SESID sesid, JET_SNP snp, JET_SNT snt, JET_SNPROG snprog)
        {
            this.statusCallbackWasCalled = true;
            Assert.IsTrue(JET_SNP.Backup == snp || JET_SNP.Restore == snp, "Unexpected snp (progress type)");
            if (JET_SNT.Progress == snt)
            {
                Assert.IsNotNull(snprog, "Expected an snprog in a progress callback");
                Assert.IsTrue(snprog.cunitDone <= snprog.cunitTotal, "done > total in the snprog");
            }

            return JET_err.Success;
        }
    }
}