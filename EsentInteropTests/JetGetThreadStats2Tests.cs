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
            byte[] data = Any.Bytes;
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, data, data.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_THREADSTATS2 threadstats2;
            Windows10Api.JetGetThreadStats(out threadstats2);
            Assert.AreNotEqual(0, threadstats2.cPageReferenced);
            Assert.AreNotEqual(0, threadstats2.cLogRecord);
            Assert.AreNotEqual(0, threadstats2.cbLogRecord);
        }
    }
}