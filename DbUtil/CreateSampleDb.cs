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
        /// Add a column to the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to add the column to.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="coltyp">The type of the column.</param>
        private void AddColumn(JET_SESID sesid, JET_TABLEID tableid, string name, JET_coltyp coltyp)
        {
            JET_COLUMNID ignored;
            Api.JetAddColumn(
                sesid,
                tableid,
                name,
                new JET_COLUMNDEF() { coltyp = coltyp },
                null,
                0,
                out ignored);
        }

        /// <summary>
        /// Add a column to the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to add the column to.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="coltyp">The type of the column.</param>
        /// <param name="cp">The codepage to use.</param>
        private void AddColumn(JET_SESID sesid, JET_TABLEID tableid, string name, JET_coltyp coltyp, JET_CP cp)
        {
            JET_COLUMNID ignored;
            Api.JetAddColumn(
                sesid,
                tableid,
                name,
                new JET_COLUMNDEF() { coltyp = coltyp, cp = cp },
                null,
                0,
                out ignored);
        }

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
                Api.JetAddColumn(
                    sesid,
                    tableid,
                    "key", 
                    new JET_COLUMNDEF() { coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnAutoincrement },
                    null,
                    0,
                    out columnidAutoinc);

                this.AddColumn(sesid, tableid, "bit", JET_coltyp.Bit);
                this.AddColumn(sesid, tableid, "byte", JET_coltyp.UnsignedByte);
                this.AddColumn(sesid, tableid, "short", JET_coltyp.Short);
                this.AddColumn(sesid, tableid, "long", JET_coltyp.Long);
                this.AddColumn(sesid, tableid, "currency", JET_coltyp.Currency);
                this.AddColumn(sesid, tableid, "single", JET_coltyp.IEEESingle);
                this.AddColumn(sesid, tableid, "double", JET_coltyp.IEEEDouble);
                this.AddColumn(sesid, tableid, "binary", JET_coltyp.LongBinary);
                this.AddColumn(sesid, tableid, "ascii", JET_coltyp.LongText, JET_CP.ASCII);
                this.AddColumn(sesid, tableid, "unicode", JET_coltyp.LongText, JET_CP.Unicode);

                string indexdef1 = "+key\0\0";
                Api.JetCreateIndex(sesid, tableid, "primary", CreateIndexGrbit.IndexPrimary, indexdef1, indexdef1.Length, 100);

                string indexdef2 = "+double\0-ascii\0\0";
                Api.JetCreateIndex(sesid, tableid, "secondary", CreateIndexGrbit.None, indexdef2, indexdef2.Length, 100);

                Dictionary<string, JET_COLUMNID> columnids = Api.GetColumnDictionary(sesid, tableid);

                int ignored;

                Api.JetBeginTransaction(sesid);

                // Null record
                Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                Api.JetUpdate(sesid, tableid, null, 0, out ignored);

                // Record that requires CSV quoting
                Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                Api.SetColumn(sesid, tableid, columnids["bit"], true);
                Api.SetColumn(sesid, tableid, columnids["byte"], (byte)0x1e);
                Api.SetColumn(sesid, tableid, columnids["short"], (short)0);
                Api.SetColumn(sesid, tableid, columnids["long"], 1);
                Api.SetColumn(sesid, tableid, columnids["currency"], Int64.MinValue);
                Api.SetColumn(sesid, tableid, columnids["single"], (float)Math.E);
                Api.SetColumn(sesid, tableid, columnids["double"], Math.PI);
                Api.SetColumn(sesid, tableid, columnids["binary"], new byte[] { 0x1, 0x2, 0xea, 0x4f, 0x0 });
                Api.SetColumn(sesid, tableid, columnids["ascii"], ",", Encoding.ASCII);
                Api.SetColumn(sesid, tableid, columnids["unicode"], " \"quoting\" ", Encoding.Unicode);
                Api.JetUpdate(sesid, tableid, null, 0, out ignored);

                for (int i = 0; i < 10; ++i)
                {
                    Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                    Api.SetColumn(sesid, tableid, columnids["unicode"], String.Format("Record {0}", i), Encoding.Unicode);
                    Api.SetColumn(sesid, tableid, columnids["double"], (double)i * 1.1);
                    Api.SetColumn(sesid, tableid, columnids["long"], i);
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
