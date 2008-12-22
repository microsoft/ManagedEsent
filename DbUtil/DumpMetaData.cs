//-----------------------------------------------------------------------
// <copyright file="DumpMetaData.cs" company="Microsoft Corporation">
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

            using (Instance instance = new Instance("dumpmetadata"))
            {
                instance.Parameters.Recovery = false;
                instance.Init();

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance.JetInstance, out sesid, null, null);
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
        }
    }
}
