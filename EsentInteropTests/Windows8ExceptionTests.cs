//-----------------------------------------------------------------------
// <copyright file="Windows8ExceptionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the exception classes.
    /// </summary>
    public partial class ExceptionTests
    {
       /// <summary>
        /// Check that JetGetErrorInfo returns valid information.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Check that JetGetErrorInfo returns valid information.")]
        public void VerifyEsentErrorInformationIsCorrect()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_ERRINFOBASIC errinfobasic;

            // OutOfCursors is a Memory error.
            // JET_errcatError -> JET_errcatOperation -> JET_errcatResource -> JET_errcatMemory
            Windows8Api.JetGetErrorInfo(JET_err.OutOfCursors, out errinfobasic);

            Assert.AreEqual(JET_ERRCAT.Memory, errinfobasic.errcat);
            Assert.AreEqual(JET_ERRCAT.Error, (JET_ERRCAT)errinfobasic.rgCategoricalHierarchy[0]);
            Assert.AreEqual(JET_ERRCAT.Operation, errinfobasic.rgCategoricalHierarchy[1]);
            Assert.AreEqual(JET_ERRCAT.Resource, errinfobasic.rgCategoricalHierarchy[2]);
            Assert.AreEqual(JET_ERRCAT.Memory, errinfobasic.rgCategoricalHierarchy[3]);
            Assert.AreEqual(JET_ERRCAT.Unknown, errinfobasic.rgCategoricalHierarchy[4]);
            Assert.AreEqual(JET_ERRCAT.Unknown, errinfobasic.rgCategoricalHierarchy[5]);
            Assert.AreEqual(JET_ERRCAT.Unknown, errinfobasic.rgCategoricalHierarchy[6]);
            Assert.AreEqual(JET_ERRCAT.Unknown, errinfobasic.rgCategoricalHierarchy[7]);

            Assert.AreEqual(string.Empty, errinfobasic.rgszSourceFile);
            Assert.AreEqual(0, errinfobasic.lSourceLine);
            Assert.AreEqual(JET_err.OutOfCursors, errinfobasic.errValue);
        }
    }
}