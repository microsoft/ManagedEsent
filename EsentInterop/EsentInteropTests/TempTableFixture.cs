//-----------------------------------------------------------------------
// <copyright file="TempTableFixture.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Isam.Esent;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Tests for the various Set/RetrieveColumn* methods and
    /// the helper methods that retrieve meta-data.
    /// </summary>
    [TestClass]
    public class TempTableFixture
    {
        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private Instance instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private Session session;

        /// <summary>
        /// The tableid being used by the test.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// A dictionary that maps column types to column ids.
        /// </summary>
        private IDictionary<JET_coltyp, JET_COLUMNID> coltypDict;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.instance = new Instance("TempTableFixture");
            this.instance.Parameters.Recovery = false;
            this.instance.Parameters.PageTempDBMin = SystemParameters.PageTempDBSmallest;
            this.instance.Parameters.NoInformationEvent = true;
            this.instance.Init();

            this.session = new Session(this.instance);

            var columndefs = new JET_COLUMNDEF[]
            {
                new JET_COLUMNDEF { coltyp = JET_coltyp.Binary },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Bit },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Currency },
                new JET_COLUMNDEF { coltyp = JET_coltyp.DateTime },
                new JET_COLUMNDEF { coltyp = JET_coltyp.IEEEDouble },
                new JET_COLUMNDEF { coltyp = JET_coltyp.IEEESingle },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Long },
                new JET_COLUMNDEF { coltyp = JET_coltyp.LongBinary },
                new JET_COLUMNDEF { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Short },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Text, cp = JET_CP.Unicode },
                new JET_COLUMNDEF { coltyp = JET_coltyp.UnsignedByte },
            };

            var columnids = new JET_COLUMNID[columndefs.Length];
            Api.JetOpenTempTable(this.session, columndefs, columndefs.Length, TempTableGrbit.ForceMaterialization, out this.tableid, columnids);

            this.coltypDict = new Dictionary<JET_coltyp, JET_COLUMNID>(columndefs.Length);
            for (int i = 0; i < columndefs.Length; ++i)
            {
                this.coltypDict[columndefs[i].coltyp] = columnids[i];
            }
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            this.instance.Term();
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Test JetSetColumns
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void JetSetColumns()
        {
            int i = Any.Int32;
            string s = Any.String;
            double d = Any.Double;

            var setcolumns = new[]
            {
                new JET_SETCOLUMN { cbData = sizeof(int), columnid = this.coltypDict[JET_coltyp.Long], pvData = BitConverter.GetBytes(i) },
                new JET_SETCOLUMN { cbData = s.Length * sizeof(char), columnid = this.coltypDict[JET_coltyp.LongText], pvData = Encoding.Unicode.GetBytes(s) },
                new JET_SETCOLUMN { cbData = sizeof(double), columnid = this.coltypDict[JET_coltyp.IEEEDouble], pvData = BitConverter.GetBytes(d) },
            };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.JetSetColumns(this.session, this.tableid, setcolumns, setcolumns.Length);
                update.Save();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);

            Assert.AreEqual(i, Api.RetrieveColumnAsInt32(this.session, this.tableid, this.coltypDict[JET_coltyp.Long]));
            Assert.AreEqual(s, Api.RetrieveColumnAsString(this.session, this.tableid, this.coltypDict[JET_coltyp.LongText]));
            Assert.AreEqual(d, Api.RetrieveColumnAsDouble(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEEDouble]));
        }
    }
}