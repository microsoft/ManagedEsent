//-----------------------------------------------------------------------
// <copyright file="CreateSampleDb.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Exchange.Isam.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;

    /// <summary>
    /// Database utilities
    /// </summary>
    internal partial class Dbutil
    {
        /// <summary>
        /// Create a sample database
        /// </summary>
        /// <param name="args">Arguments for the command.</param>
        private void CreateSampleDb(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("specify the database", "args");
            }

            string database = args[0];

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "createsampledb");

            Api.JetInit(ref instance);
            try
            {
                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, null, null);
                Api.JetCreateDatabase(sesid, database, null, out dbid, CreateDatabaseGrbit.None);

                JET_TABLEID tableid;
                Api.JetCreateTable(sesid, dbid, "table", 16, 100, out tableid);

                JET_COLUMNID columnidAutoinc;
                JET_COLUMNID columnidText;
                JET_COLUMNID columnidDouble;
                JET_COLUMNID columnidLong;

                Api.JetAddColumn(
                    sesid,
                    tableid,
                    "key", 
                    new JET_COLUMNDEF() { coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnAutoincrement },
                    null,
                    0,
                    out columnidAutoinc);
                Api.JetAddColumn(
                    sesid,
                    tableid,
                    "text",
                    new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode },
                    null,
                    0,
                    out columnidText);
                Api.JetAddColumn(
                    sesid,
                    tableid,
                    "double",
                    new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEEDouble },
                    null,
                    0,
                    out columnidDouble);
                Api.JetAddColumn(
                    sesid,
                    tableid,
                    "long",
                    new JET_COLUMNDEF() { coltyp = JET_coltyp.Currency },
                    null,
                    0,
                    out columnidLong);

                string indexdef1 = "+key\0\0";
                Api.JetCreateIndex(sesid, tableid, "primary", CreateIndexGrbit.IndexPrimary, indexdef1, indexdef1.Length, 100);

                string indexdef2 = "+double\0-text\0\0";
                Api.JetCreateIndex(sesid, tableid, "secondary", CreateIndexGrbit.None, indexdef2, indexdef2.Length, 100);

                int ignored;

                Api.JetBeginTransaction(sesid);

                // Null record
                Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                Api.JetUpdate(sesid, tableid, null, 0, out ignored);

                // Record that requires CSV quoting
                Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                Api.SetColumn(sesid, tableid, columnidText, " \"quoting\" ", Encoding.Unicode);
                Api.SetColumn(sesid, tableid, columnidDouble, Math.PI);
                Api.SetColumn(sesid, tableid, columnidLong, Int64.MinValue);
                Api.JetUpdate(sesid, tableid, null, 0, out ignored);

                for (int i = 0; i < 10; ++i)
                {
                    Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                    Api.SetColumn(sesid, tableid, columnidText, String.Format("Record {0}", i), Encoding.Unicode);
                    Api.SetColumn(sesid, tableid, columnidDouble, (double)i * 1.1);
                    Api.SetColumn(sesid, tableid, columnidLong, (long)i);
                    Api.JetUpdate(sesid, tableid, null, 0, out ignored);
                }

                Api.JetCommitTransaction(sesid, CommitTransactionGrbit.None);
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }
    }
}
