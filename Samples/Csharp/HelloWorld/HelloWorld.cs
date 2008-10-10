//-----------------------------------------------------------------------
// <copyright file="HelloWorld.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Esent.Sample.HelloWorld
{
    using System;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;

    /// <summary>
    /// Contains the static Main method.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Runs the program.
        /// </summary>
        public static void Main()
        {
            JET_INSTANCE instance = JET_INSTANCE.Nil;
            JET_SESID sesid;
            JET_DBID dbid;
            JET_TABLEID tableid;
            JET_COLUMNID columnid;

            // Initialize ESENT
            Api.JetInit(ref instance);
            Api.JetBeginSession(instance, out sesid, String.Empty, String.Empty);

            // Create the database
            Api.JetCreateDatabase(sesid, "edbtest.db", String.Empty, out dbid, CreateDatabaseGrbit.OverwriteExisting);

            // Create the table
            Api.JetBeginTransaction(sesid);
            Api.JetCreateTable(sesid, dbid, "table", 0, 100, out tableid);
            var columndef = new JET_COLUMNDEF() { cp = JET_CP.ASCII, coltyp = JET_coltyp.LongText, };
            Api.JetAddColumn(sesid, tableid, "column", columndef, null, 0, out columnid);
            Api.JetCommitTransaction(sesid, CommitTransactionGrbit.LazyFlush);

            // Insert a record
            Api.JetBeginTransaction(sesid);
            Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
            byte[] data = Encoding.ASCII.GetBytes("Hello World");
            Api.JetSetColumn(sesid, tableid, columnid, data, data.Length, SetColumnGrbit.None, null);
            byte[] bookmark = new byte[256];
            int bookmarkSize;
            Api.JetUpdate(sesid, tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetCommitTransaction(sesid, CommitTransactionGrbit.None);
            Api.JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize);

            // Retrieve a column from the record
            byte[] buffer = new byte[1024];
            int retrievedSize;
            Api.JetRetrieveColumn(sesid, tableid, columnid, buffer, buffer.Length, out retrievedSize, RetrieveColumnGrbit.None, null);
            Console.WriteLine("{0}", Encoding.ASCII.GetString(buffer, 0, retrievedSize));

            // Terminate ESENT
            Api.JetCloseTable(sesid, tableid);
            Api.JetEndSession(sesid, EndSessionGrbit.None);
            Api.JetTerm(instance);
        }
    }
}
