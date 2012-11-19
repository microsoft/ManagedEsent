//-----------------------------------------------------------------------
// <copyright file="TemporaryTable2Tests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the temporary table APIs
    /// </summary>
    [TestClass]
    public class TemporaryTable2Tests
    {
        /// <summary>
        /// The instance being used for testing.
        /// </summary>
        private Instance instance;

        /// <summary>
        /// The session used for testing.
        /// </summary>
        private Session session;

        /// <summary>
        /// Create the instance. Recovery is turned off for speed.
        /// </summary>
        [TestInitialize]
        [Description("Setup the TemporaryTable2Tests test fixture")]
        public void Setup()
        {
            this.instance = new Instance(Guid.NewGuid().ToString(), "TemporaryTable2Tests");
            this.instance.Parameters.Recovery = false;
            this.instance.Parameters.NoInformationEvent = true;
            this.instance.Parameters.PageTempDBMin = SystemParameters.PageTempDBSmallest;
            this.instance.Init();
            this.session = new Session(this.instance);
        }

        /// <summary>
        /// Cleanup our test instance
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the TemporaryTable2Tests test fixture")]
        public void Teardown()
        {
            this.instance.Term();
            SetupHelper.CheckProcessForInstanceLeaks();
        }

        #region Sort data with a temporary table

        /// <summary>
        /// Sort data with a temporary table
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Sort case-sensitive with JetOpenTemporaryTable3")]
        public void SortDataCaseSensitiveWithJetOpenTemporaryTable3()
        {
            const string LocaleName = "pt-BR";

            var columns = new[]
            {
                new JET_COLUMNDEF { coltyp = JET_coltyp.Text, cp = JET_CP.Unicode, grbit = ColumndefGrbit.TTKey },
            };
            var columnids = new JET_COLUMNID[columns.Length];

            var idxunicode = new JET_UNICODEINDEX
            {
                dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None),
                szLocaleName = LocaleName,
            };

            var opentemporarytable = new JET_OPENTEMPORARYTABLE
            {
                cbKeyMost = SystemParameters.KeyMost,
                ccolumn = columns.Length,
                grbit = TempTableGrbit.Scrollable,
                pidxunicode = idxunicode,
                prgcolumndef = columns,
                prgcolumnid = columnids,
            };
            Windows8Api.JetOpenTemporaryTable2(this.session, opentemporarytable);

            var data = new[] { "g", "a", "A", "aa", "x", "b", "X" };
            foreach (string s in data)
            {
                using (var update = new Update(this.session, opentemporarytable.tableid, JET_prep.Insert))
                {
                    Api.SetColumn(this.session, opentemporarytable.tableid, columnids[0], s, Encoding.Unicode);
                    update.Save();
                }
            }

            Array.Sort(data, new CultureInfo(LocaleName).CompareInfo.Compare);
            CollectionAssert.AreEqual(
                data, this.RetrieveAllRecordsAsString(opentemporarytable.tableid, columnids[0]).ToArray());
            Api.JetCloseTable(this.session, opentemporarytable.tableid);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Enumerate all records and retrieve the specified column as a string.
        /// </summary>
        /// <param name="tableid">The table to enumerate.</param>
        /// <param name="columnid">The column to retrieve.</param>
        /// <returns>An enumeration of the column in all the records.</returns>
        private IEnumerable<string> RetrieveAllRecordsAsString(JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            Api.MoveBeforeFirst(this.session, tableid);
            while (Api.TryMoveNext(this.session, tableid))
            {
                yield return Api.RetrieveColumnAsString(this.session, tableid, columnid);
            }
        }

        #endregion
    }
}