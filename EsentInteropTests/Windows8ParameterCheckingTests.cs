//-----------------------------------------------------------------------
// <copyright file="Windows8ParameterCheckingTests.cs" company="Microsoft Corporation">
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
    /// Test for API parameter validation code
    /// </summary>
    public partial class ParameterCheckingTests
    {
        #region Database API

        /// <summary>
        /// Check that an exception is thrown when JetResizeDatabase gets
        /// a negative page count.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Description("Check that an exception is thrown when JetResizeDatabase gets a negative page count")]
        public void VerifyJetResizeDatabaseThrowsExceptionWhenDesiredPagesIsNegative()
        {
            int ignored;
            Windows8Api.JetResizeDatabase(this.sesid, this.dbid, -1, out ignored, ResizeDatabaseGrbit.None);
        }

        #endregion

        #region DDL
        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex4 gets 
        /// null indexcreates.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that an exception is thrown when JetCreateIndex4 gets null indexcreates")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndex4ThrowsExceptionWhenIndexcreatesAreNull()
        {
            Windows8Api.JetCreateIndex4(this.sesid, this.tableid, null, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex4 gets 
        /// a negative indexcreate count.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that an exception is thrown when JetCreateIndex4 gets a negative indexcreate count")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndex4ThrowsExceptionWhenNumIndexcreatesIsNegative()
        {
            var indexcreates = new[] { new JET_INDEXCREATE() };
            Windows8Api.JetCreateIndex4(this.sesid, this.tableid, indexcreates, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex4 gets 
        /// an indexcreate count that is too long.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that an exception is thrown when JetCreateIndex4 gets an indexcreate count that is too long")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndex4ThrowsExceptionWhenNumIndexcreatesIsTooLong()
        {
            var indexcreates = new[] { new JET_INDEXCREATE() };
            Windows8Api.JetCreateIndex4(this.sesid, this.tableid, indexcreates, indexcreates.Length + 1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex4 gets a 
        /// null index name.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that an exception is thrown when JetCreateIndex4 gets a null index name")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndex4ThrowsExceptionWhenIndexNameIsNull()
        {
            const string Key = "+column\0";
            var indexcreates = new[]
            {
                new JET_INDEXCREATE
                {
                    cbKey = Key.Length,
                    szKey = Key,
                },
            };
            Windows8Api.JetCreateIndex4(this.sesid, this.tableid, indexcreates, indexcreates.Length);
        }
        #endregion
    }
}