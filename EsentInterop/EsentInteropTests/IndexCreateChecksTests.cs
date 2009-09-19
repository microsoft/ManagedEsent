//-----------------------------------------------------------------------
// <copyright file="IndexcreateChecksTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the checking of the JET_INDEXCREATE members
    /// </summary>
    [TestClass]
    public class IndexcreateCheckTests
    {
        /// <summary>
        /// Index key.
        /// </summary>
        private const string Key = "+column\0";

        /// <summary>
        /// JET_INDEXCREATE structure being tested.
        /// </summary>
        private JET_INDEXCREATE indexcreate;

        /// <summary>
        /// Create a valid JET_INDEXCREATE. The test methods
        /// will invalidate various members.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.indexcreate = new JET_INDEXCREATE
            {
                szIndexName = "index",
                szKey = Key,
                cbKey = Key.Length + 1,
                cbKeyMost = 255,
                cbVarSegMac = 255,                
            };
        }

        /// <summary>
        /// The intitial JET_INDEXCREATE should be valid.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyFixtureSetup()
        {
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A null name should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCheckThrowsExceptionWhenNameIsNull()
        {
            this.indexcreate.szIndexName = null;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A null key should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCheckThrowsExceptionWhenKeyIsNull()
        {
            this.indexcreate.szKey = null;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A negative cbkey should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenCbKeyIsNegative()
        {
            this.indexcreate.cbKey = -1;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A too-long cbkey should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenCbKeyIsTooLong()
        {
            this.indexcreate.cbKey = this.indexcreate.cbKey + 1;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A negative density should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenDensityIsNegative()
        {
            this.indexcreate.ulDensity = -1;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A negative cbKeyMost should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenCbKeyMostIsNegative()
        {
            this.indexcreate.cbKeyMost = -1;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// A negative cbVarSegMac should generate an exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenCbVarSegMacIsNegative()
        {
            this.indexcreate.cbVarSegMac = -1;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// Null conditional columns array with non-zero count.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenConditionalColumnsAreNullAndCountIsNonZero()
        {
            this.indexcreate.cConditionalColumn = 1;
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// Negative conditional column count.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenConditionalColumnCountIsNegative()
        {
            this.indexcreate.cConditionalColumn = -1;
            this.indexcreate.rgconditionalcolumn = new JET_CONDITIONALCOLUMN[1];
            this.indexcreate.CheckMembersAreValid();
        }

        /// <summary>
        /// Too long conditional column count.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenConditionalColumnCountIsTooLong()
        {
            this.indexcreate.rgconditionalcolumn = new JET_CONDITIONALCOLUMN[1];
            this.indexcreate.cConditionalColumn = this.indexcreate.rgconditionalcolumn.Length + 1;
            this.indexcreate.CheckMembersAreValid();
        }
    }
}
