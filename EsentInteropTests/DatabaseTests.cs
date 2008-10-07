//-----------------------------------------------------------------------
// <copyright file="DatabaseTests.cs" company="Microsoft Corporation">
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
    /// Test creating, opening and closing databases. 
    /// </summary>
    [TestClass]
    public class DatabaseTests
    {
        /// <summary>
        /// Create a database, attach, open, close and detach
        /// </summary>
        [TestMethod]
        public void CreateAndOpenDatabase()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            API.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                API.JetBeginSession(instance, out sesid, String.Empty, String.Empty);
                API.JetCreateDatabase(sesid, database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                API.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                API.JetDetachDatabase(sesid, database);

                API.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.None);
                API.JetOpenDatabase(sesid, database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                API.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                API.JetDetachDatabase(sesid, database);
            }
            finally
            {
                API.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Create a database, open read-only
        /// </summary>
        [TestMethod]
        public void CreateDatabaseAndOpenReadOnly()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            API.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                API.JetBeginSession(instance, out sesid, String.Empty, String.Empty);
                API.JetCreateDatabase(sesid, database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                API.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                API.JetDetachDatabase(sesid, database);

                API.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.ReadOnly);
                API.JetOpenDatabase(sesid, database, String.Empty, out dbid, OpenDatabaseGrbit.ReadOnly);
                API.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                API.JetDetachDatabase(sesid, database);
            }
            finally
            {
                API.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }
    }
}
