//-----------------------------------------------------------------------
// <copyright file="DumpToCsv.cs" company="Microsoft Corporation">
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
        /// Quote a string for use in a CSV dump.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>
        /// A quoted version of the string, or the original string
        /// if no quoting is needed.
        /// </returns>
        internal static string QuoteForCsv(string s)
        {
            if (null == s)
            {
                return null;
            }

            // first, double any existing quotes
            if (s.Contains('"'))
            {
                s = s.Replace("\"", "\"\"");
            }

            // check to see if we need to add quotes
            // there are five cases where this is needed:
            //  1. Value starts with whitespace
            //  2. Value ends with whitespace
            //  3. Value contains a comma
            //  4. Value contains a newline
            //  5. Value contains a quote
            if (s.StartsWith(" ")
                || s.StartsWith("\t")
                || s.EndsWith(" ")
                || s.EndsWith("\t")
                || s.Contains(',')
                || s.Contains("\"")
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
        internal static string FormatBytes(byte[] data)
        {
            if (null == data)
            {
                return null;
            }
            else
            {
                StringBuilder sb = new StringBuilder(data.Length * 2);
                foreach (byte b in data)
                {
                    sb.AppendFormat("{0:x2}", b);
                }

                return sb.ToString();
            }
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
                            columnFormatters.Add((s, t) => Dbutil.FormatBytes(Api.RetrieveColumn(s, t, columnid)));
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
                            recordBuilder.AppendFormat("{0},", Dbutil.QuoteForCsv(formatter(sesid, tableid)));
                        }

                        // remove the trailing comma (null columns can result in a comma
                        // as the first character or multiple trailing commas)
                        string recordText = recordBuilder.ToString();
                        if (String.Empty != recordText)
                        {
                            // the string could be empty if the table has no columns
                            recordText = recordText.Substring(0, recordText.Length - 1);
                        }

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
    }
}
