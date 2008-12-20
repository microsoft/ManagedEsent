//-----------------------------------------------------------------------
// <copyright file="DbUtil.cs" company="Microsoft Corporation">
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
    internal class Dbutil
    {
        /// <summary>
        /// Maps a commad to the method that implements it.
        /// </summary>
        Dictionary<string, Action<string[]>> actions;

        /// <summary>
        /// Create a new Dbutil object.
        /// </summary>
        public Dbutil()
        {
            this.actions = new Dictionary<string, Action<string[]>>();
            this.actions.Add("dumpmetadata", this.DumpMetaData);
            this.actions.Add("createsample", this.CreateSampleDb);
        }

        /// <summary>
        /// Execute the command given by the arguments.
        /// </summary>
        /// <param name="args">The arguments to the program.</param>
        public void Execute(string[] args)
        {
            if (null == args)
            {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 1)
            {
                throw new ArgumentException("specify arguments", "args");
            }

            var methods = from x in this.actions
                                      where 0 == String.Compare(x.Key, args[0], true)
                                      select x.Value;
            if (methods.Count() != 1)
            {
                throw new ArgumentException("unknown command", "args");
            }

            // now shift off the first argument
            string[] newArgs = new string[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            methods.Single()(newArgs);
        }

        /// <summary>
        /// Dump the meta-data of the table.
        /// </summary>
        /// <param name="args"></param>
        private void DumpMetaData(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("specify the database", "args");
            }

            string database = args[0];

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "dumpmetadata");

            InstanceParameters parameters = new InstanceParameters(instance);
            parameters.Recovery = false;

            Api.JetInit(ref instance);
            try
            {
                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, null, null);
                Api.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.ReadOnly);
                Api.JetOpenDatabase(sesid, database, null, out dbid, OpenDatabaseGrbit.ReadOnly);

                foreach (string table in Api.GetTableNames(sesid, dbid))
                {
                    Console.WriteLine(table);
                    foreach (ColumnInfo column in Api.GetTableColumns(sesid, dbid, table))
                    {
                        Console.WriteLine("\t{0}", column.Name);
                        Console.WriteLine("\t\tColtyp:     {0}", column.Coltyp);
                        Console.WriteLine("\t\tColumnid:   {0}", column.Columnid);
                        if (JET_coltyp.LongText == column.Coltyp || JET_coltyp.Text == column.Coltyp)
                        {
                            Console.WriteLine("\t\tCode page:  {0}", column.Cp);
                        }
                        Console.WriteLine("\t\tMax length: {0}", column.MaxLength);
                        Console.WriteLine("\t\tGrbit:      {0}", column.Grbit);
                    }
                }
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// Create a sample database
        /// </summary>
        /// <param name="args"></param>
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
                    new JET_COLUMNDEF() { coltyp = JET_coltyp.Long },
                    null,
                    0,
                    out columnidLong);

                string indexdef1 = "+key\0\0";
                Api.JetCreateIndex(sesid, tableid, "primary", CreateIndexGrbit.IndexPrimary, indexdef1, indexdef1.Length, 100);

                string indexdef2 = "+double\0-text\0\0";
                Api.JetCreateIndex(sesid, tableid, "secondary", CreateIndexGrbit.None, indexdef2, indexdef2.Length, 100);

                Api.JetBeginTransaction(sesid);
                for (int i = 0; i < 10; ++i)
                {
                    int ignored;
                    Api.JetPrepareUpdate(sesid, tableid, JET_prep.Insert);
                    // TODO: insert data
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
