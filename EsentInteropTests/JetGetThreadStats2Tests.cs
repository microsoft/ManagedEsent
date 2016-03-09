//-----------------------------------------------------------------------
// <copyright file="JetGetThreadStats2Tests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JetGetThreadStats
    /// </summary>
    public partial class JetGetThreadStatsTests
    {
        /// <summary>
        /// Call JetGetThreadStats on JET_THREADSTATS2.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Call JetGetThreadStats on JET_THREADSTATS2")]
        public void JetGetThreadStats2()
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            byte[] data = new byte[65536];
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, data, data.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            this.ResetCache();

            JET_THREADSTATS2 threadstatsBefore;
            Windows10Api.JetGetThreadStats(out threadstatsBefore);

            Api.JetBeginTransaction(this.sesid);
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            byte[] actual = Api.RetrieveColumn(this.sesid, this.tableid, this.columnid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            data = new byte[65536];
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, data, data.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesid, this.tableid);
            Api.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, data, data.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_THREADSTATS2 threadstatsAfter;
            Windows10Api.JetGetThreadStats(out threadstatsAfter);

            JET_THREADSTATS2 threadstats = threadstatsAfter - threadstatsBefore;

            Assert.AreNotEqual(0, threadstats.cPageReferenced);
            Assert.AreNotEqual(0, threadstats.cPageRead);
            ////Assert.AreNotEqual(0, threadstats.cPagePreread);
            Assert.AreNotEqual(0, threadstats.cPageDirtied);
            Assert.AreNotEqual(0, threadstats.cPageRedirtied);
            Assert.AreNotEqual(0, threadstats.cLogRecord);
            Assert.AreNotEqual(0, threadstats.cbLogRecord);
            Assert.AreNotEqual(0, threadstats.cusecPageCacheMiss);
            Assert.AreNotEqual(0, threadstats.cPageCacheMiss);
        }
    }
}