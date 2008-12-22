//-----------------------------------------------------------------------
// <copyright file="CreateSampleDb.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Utilities
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

            using (Instance instance = new Instance("createsampledb"))
            {
                instance.Init();

                using (Session session = new Session(instance.JetInstance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session.JetSesid, database, null, out dbid, CreateDatabaseGrbit.None);

                    // Creating a table opens it exclusively. That is when we can define the primary index.
                    JET_TABLEID tableid;
                    Api.JetCreateTable(session.JetSesid, dbid, "table", 16, 100, out tableid);
                    JET_COLUMNID columnidAutoinc;
                    Api.JetAddColumn(
                        session.JetSesid,
                        tableid,
                        "key",
                        new JET_COLUMNDEF() { coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnAutoincrement },
                        null,
                        0,
                        out columnidAutoinc);
                    string indexdef1 = "+key\0\0";
                    Api.JetCreateIndex(session.JetSesid, tableid, "primary", CreateIndexGrbit.IndexPrimary, indexdef1, indexdef1.Length, 100);
                    Api.JetCloseTable(session.JetSesid, tableid);

                    using (Table table = new Table(session.JetSesid, dbid, "table", OpenTableGrbit.None))
                    {
                        // Columns and secondary indexes can be added while the table is opened normally
                        this.AddColumn(session.JetSesid, table.JetTableid, "bit", JET_coltyp.Bit);
                        this.AddColumn(session.JetSesid, table.JetTableid, "byte", JET_coltyp.UnsignedByte);
                        this.AddColumn(session.JetSesid, table.JetTableid, "short", JET_coltyp.Short);
                        this.AddColumn(session.JetSesid, table.JetTableid, "long", JET_coltyp.Long);
                        this.AddColumn(session.JetSesid, table.JetTableid, "currency", JET_coltyp.Currency);
                        this.AddColumn(session.JetSesid, table.JetTableid, "single", JET_coltyp.IEEESingle);
                        this.AddColumn(session.JetSesid, table.JetTableid, "double", JET_coltyp.IEEEDouble);
                        this.AddColumn(session.JetSesid, table.JetTableid, "binary", JET_coltyp.LongBinary);
                        this.AddColumn(session.JetSesid, table.JetTableid, "ascii", JET_coltyp.LongText, JET_CP.ASCII);
                        this.AddColumn(session.JetSesid, table.JetTableid, "unicode", JET_coltyp.LongText, JET_CP.Unicode);

                        string indexdef2 = "+double\0-ascii\0\0";
                        Api.JetCreateIndex(session.JetSesid, table.JetTableid, "secondary", CreateIndexGrbit.None, indexdef2, indexdef2.Length, 100);

                        using (Transaction transaction = new Transaction(session.JetSesid))
                        {
                            Dictionary<string, JET_COLUMNID> columnids = Api.GetColumnDictionary(session.JetSesid, table.JetTableid);

                            // Null record
                            int ignored;
                            Api.JetPrepareUpdate(session.JetSesid, table.JetTableid, JET_prep.Insert);
                            Api.JetUpdate(session.JetSesid, table.JetTableid, null, 0, out ignored);

                            // Record that requires CSV quoting
                            using (Update update = new Update(session.JetSesid, table.JetTableid, JET_prep.Insert))
                            {
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["bit"], true);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["byte"], (byte)0x1e);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["short"], (short)0);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["long"], 1);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["currency"], Int64.MinValue);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["single"], (float)Math.E);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["double"], Math.PI);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["binary"], new byte[] { 0x1, 0x2, 0xea, 0x4f, 0x0 });
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["ascii"], ",", Encoding.ASCII);
                                Api.SetColumn(session.JetSesid, table.JetTableid, columnids["unicode"], " \"quoting\" ", Encoding.Unicode);

                                update.Save();
                            }

                            for (int i = 0; i < 10; ++i)
                            {
                                using (Update update = new Update(session.JetSesid, table.JetTableid, JET_prep.Insert))
                                {
                                    Api.SetColumn(session.JetSesid, table.JetTableid, columnids["unicode"], String.Format("Record {0}", i), Encoding.Unicode);
                                    Api.SetColumn(session.JetSesid, table.JetTableid, columnids["double"], (double)i * 1.1);
                                    Api.SetColumn(session.JetSesid, table.JetTableid, columnids["long"], i);

                                    update.Save();
                                }
                            }

                            transaction.Commit(CommitTransactionGrbit.LazyFlush);
                        }
                    }
                }
            }
        }
    }
}
