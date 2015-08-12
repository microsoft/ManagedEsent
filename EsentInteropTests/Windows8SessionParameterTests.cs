//-----------------------------------------------------------------------
// <copyright file="Windows8SessionParameterTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the SessionParameters class.
    /// </summary>
    [TestClass]
    public class Windows8SessionParameterTests
    {
        /// <summary>
        /// The instance to use.
        /// </summary>
        private Instance instance;

        /// <summary>
        /// The session to use.
        /// </summary>
        private Session session;

        /// <summary>
        /// The DBID to use.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// The table id to use.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// The column id to use.
        /// </summary>
        private JET_COLUMNID autoincColumn;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        [Description("Fixture setup for the Session Parameters tests")]
        public void Setup()
        {
            // Note this doesn't act as a suite initialize, which was not what I intended. :P
            // Console.WriteLine("SetSessionParam Setup");
            this.instance = new Instance(@".\", "SessionParameterTests");
            this.instance.Parameters.NoInformationEvent = true;
            this.instance.Init();

            this.session = new Session(this.instance);

            Api.JetCreateDatabase(this.session, @"SetSessParam.edb", null, out this.dbid, CreateDatabaseGrbit.OverwriteExisting);

            JET_COLUMNDEF columndef = new JET_COLUMNDEF();

            // First create the table. There is one autoinc column.
            Api.JetCreateTable(this.session, this.dbid, "PulseUpdTable", 0, 100, out this.tableid);
            columndef.coltyp = JET_coltyp.Long;
            columndef.grbit = ColumndefGrbit.ColumnAutoincrement;
            Api.JetAddColumn(this.session, this.tableid, "TheAutoInc", columndef, null, 0, out this.autoincColumn);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Fixture cleanup for RetrieveColumnAsStringPerfTests")]
        public void Teardown()
        {
            this.session.End();
            this.instance.Term();
        }

        #endregion // Setup-Teardown

        #region Tests

        /// <summary>
        /// Verify that we can set a simple GRBIT session parameter, such as the CommitDefault.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify JetSetSessionParameter( CommitDefault )")]
        public void VerifySetSessionParamCommitDefault()
        {
            Windows8Api.JetSetSessionParameter(this.session, JET_sesparam.CommitDefault, (int)CommitTransactionGrbit.LazyFlush);
        }
        
        /// <summary>
        /// Verify that we can set a simple small commit context.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify JetSetSessionParameter( CommitGenericContext ) Simple")]
        public void VerifySetSessionParamCommitGenericContextSimple()
        {
            int cb = 3;
            byte[] trxContext = new byte[cb];
            trxContext[0] = 0x23;
            trxContext[1] = 0x32;
            trxContext[2] = 0x33;

            Windows8Api.JetSetSessionParameter(this.session, JET_sesparam.CommitGenericContext, trxContext, cb);

            this.PulseUpdateTrx();
        }

        /// <summary>
        /// Verify that we can set a simple commit context, and then later reset it.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify JetSetSessionParameter( CommitGenericContext ) and Reset")]
        public void VerifySetSessionParamCommitGenericContextAndReset()
        {
            int cb = 5;
            byte[] trxContext = new byte[cb];
            trxContext[0] = 0x23;
            trxContext[1] = 0x32;
            trxContext[2] = 0x33;
            trxContext[3] = 0x22;
            trxContext[4] = 0x11;

            Windows8Api.JetSetSessionParameter(this.session, JET_sesparam.CommitGenericContext, trxContext, cb);

            this.PulseUpdateTrx();

            Windows8Api.JetSetSessionParameter(this.session, JET_sesparam.CommitGenericContext, null, 0);

            this.PulseUpdateTrx();
        }

        #endregion // Tests

        #region Helpers

        /// <summary>
        /// Does a small update transaction to push some log commit0 LRs out.
        /// </summary>
        private void PulseUpdateTrx()
        {
            using (var transaction = new Transaction(this.session))
            {
                using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
                {
                    int? autoinc = Api.RetrieveColumnAsInt32(
                        this.session,
                        this.tableid,
                        this.autoincColumn,
                        RetrieveColumnGrbit.RetrieveCopy);
                    update.Save();
                }

            transaction.Commit(CommitTransactionGrbit.None);
            }
        }

        #endregion // Helpers
    }
}