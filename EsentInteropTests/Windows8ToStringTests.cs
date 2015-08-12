//-----------------------------------------------------------------------
// <copyright file="Windows8ToStringTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Globalization;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing the ToString methods of the basic types.
    /// </summary>
    public partial class ToStringTests
    {
        /// <summary>
        /// Test JET_ERRINFOBASIC.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_ERRINFOBASIC.ToString()")]
        public void JetErrinfobasicToString()
        {
            var errorinfobasic = new JET_ERRINFOBASIC()
            {
                errValue = JET_err.ReadVerifyFailure,
                errcat = JET_ERRCAT.Quota,
                rgszSourceFile = "abc.cxx",
                lSourceLine = 93,
            };

            Assert.AreEqual("JET_ERRINFOBASIC(ReadVerifyFailure:Quota:abc.cxx:93)", errorinfobasic.ToString());
        }

        /// <summary>
        /// Test DurableCommitCallback.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DurableCommitCallback.ToString()")]
        public void DurableCommitCallbackToString()
        {
            JET_INSTANCE inst;
            Api.JetCreateInstance(out inst, "dummy");
            try
            {
                var callback = new DurableCommitCallback(inst, this.TestFlushCallback);
                Assert.AreEqual("DurableCommitCallback(" + inst.ToString() + ")", callback.ToString());
            }
            finally
            {
                Api.JetTerm(inst);
            }
        }

        /// <summary>
        /// Test JET_COMMIT_ID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_COMMIT_ID.ToString()")]
        public void JetCommitIdToString()
        {
            DateTime d = DateTime.Now;
            JET_COMMIT_ID commitId = DurableCommitTests.CreateJetCommitId(1, d, "computer", 2);
            var signature = new JET_SIGNATURE(1, d, "computer");

            string sigString = signature.ToString();
            string expected = string.Format(
                CultureInfo.InvariantCulture,
                string.Format("JET_COMMIT_ID({0}:2", sigString));

            Assert.AreEqual(expected, commitId.ToString());
        }

        /// <summary>
        /// Test JET_INDEX_COLUMN.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INDEX_COLUMN.ToString()")]
        public void JetIndexColumnToString()
        {
            JET_INDEX_COLUMN indexColumn = new JET_INDEX_COLUMN();
            Assert.AreEqual("JET_INDEX_COLUMN(0x0)", indexColumn.ToString());
        }

        /// <summary>
        /// Test JET_INDEX_RANGE.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INDEX_RANGE.ToString()")]
        public void JetIndexRangesToString()
        {
            JET_INDEX_RANGE indexRange = new JET_INDEX_RANGE();
            Assert.AreEqual("JET_INDEX_RANGE", indexRange.ToString());
        }

        /// <summary>
        /// Test callback method
        /// </summary>
        /// <param name="instance">Current instance.</param>
        /// <param name="commitId">Commit id seen.</param>
        /// <param name="grbit">Grbit - reserved.</param>
        /// <returns>Success or error.</returns>
        private JET_err TestFlushCallback(
            JET_INSTANCE instance,
            JET_COMMIT_ID commitId,
            DurableCommitCallbackGrbit grbit)
        {
            return JET_err.Success;
        }
    }
}