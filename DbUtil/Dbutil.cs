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
        private Dictionary<string, Action<string[]>> actions;

        /// <summary>
        /// Initializes a new instance of the Dbutil class.
        /// </summary>
        public Dbutil()
        {
            this.actions = new Dictionary<string, Action<string[]>>();
            this.actions.Add("dumpmetadata", this.DumpMetaData);
            this.actions.Add("createsample", this.CreateSampleDb);
            this.actions.Add("dumptocsv", this.DumpToCsv);
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
        /// <param name="args">Arguments for the command.</param>
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
        /// Quote a string for use in a CSV dump.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>
        /// A quoted version of the string, or the original string
        /// if no quoting is needed.
        /// </returns>
        private string QuoteForCsv(string s)
        {
            // first, double any existing quotes
            if (s.Contains('"'))
            {
                s = s.Replace("\"", "\"\"");
            }

            // check to see if we need to add quotes
            // there are four cases where this is needed:
            //  1. Value starts with whitespace
            //  2. Value ends with whitespace
            //  3. Value contains a comma
            //  4. Value contains a newline
            if (s.StartsWith(" ")
                || s.StartsWith("\t")
                || s.EndsWith(" ")
                || s.EndsWith("\t")
                || s.Contains(',')
                || s.Contains(Environment.NewLine))
            {
                s = String.Format("\"{0}\"", s);
            }

            return s;
        }

        /// <summary>
        /// Return the string format of a byte array.
        /// </summary>
        /// <param name="data">The data to format.</param>
        /// <returns>A string representation of the data.</returns>
        private string FormatBytes(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Dump a table in CSV format.
        /// </summary>
        /// <param name="args">Arguments for the command.</param>
        private void DumpToCsv(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("specify the database and table", "args");
            }

            string database = args[0];
            string table = args[1];

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "dumptocsv");

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

                List<Func<JET_SESID, JET_TABLEID, string>> columnFormatters = new List<Func<JET_SESID, JET_TABLEID, string>>();

                StringBuilder sb = new StringBuilder();
                foreach (ColumnInfo column in Api.GetTableColumns(sesid, dbid, table))
                {
                    sb.AppendFormat("{0},", column.Name);

                    // create a local variable that will be captured by the lambda functions below
                    var columnid = column.Columnid;
                    switch (column.Coltyp)
                    {
                        case JET_coltyp.Bit:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsBoolean(s, t, columnid)));
                            break;
                        case JET_coltyp.Currency:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsInt64(s, t, columnid)));
                            break;
                        case JET_coltyp.IEEEDouble:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsDouble(s, t, columnid)));
                            break;
                        case JET_coltyp.IEEESingle:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsFloat(s, t, columnid)));
                            break;
                        case JET_coltyp.Long:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsInt32(s, t, columnid)));
                            break;
                        case JET_coltyp.Text:
                        case JET_coltyp.LongText:
                            var encoding = (column.Cp == JET_CP.Unicode) ? Encoding.Unicode : Encoding.ASCII;
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsString(s, t, columnid, encoding)));
                            break;
                        case JET_coltyp.Short:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsInt16(s, t, columnid)));
                            break;
                        case JET_coltyp.UnsignedByte:
                            columnFormatters.Add((s, t) => String.Format("{0}", Api.RetrieveColumnAsByte(s, t, columnid)));
                            break;
                        case JET_coltyp.Binary:
                        case JET_coltyp.LongBinary:
                        case JET_coltyp.DateTime:
                        default:
                            columnFormatters.Add((s, t) => this.FormatBytes(Api.RetrieveColumn(s, t, columnid)));
                            break;
                    }
                }

                // remove the trailing comma
                Console.WriteLine(sb.ToString().TrimEnd(new char[] { ',' }));

                JET_TABLEID tableid;
                Api.JetOpenTable(sesid, dbid, table, null, 0, OpenTableGrbit.ReadOnly, out tableid);
                Api.JetSetTableSequential(sesid, tableid, SetTableSequentialGrbit.None);
                if (Api.TryMoveFirst(sesid, tableid))
                {
                    do
                    {
                        var recordBuilder = new StringBuilder();
                        foreach (var formatter in columnFormatters)
                        {
                            recordBuilder.AppendFormat("{0},", this.QuoteForCsv(formatter(sesid, tableid)));
                        }

                        // remove the trailing comma (null columns can result in a comma
                        // as the first character or multiple trailing commas)
                        string recordText = recordBuilder.ToString();
                        recordText = recordText.Substring(0, recordText.Length - 1);
                        Console.WriteLine(recordText);
                    }
                    while (Api.TryMoveNext(sesid, tableid));
                }
                Api.JetResetTableSequential(sesid, tableid, ResetTableSequentialGrbit.None);

            }
            finally
            {
                Api.JetTerm(instance);
            }
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
