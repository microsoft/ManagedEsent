//-----------------------------------------------------------------------
// <copyright file="EmptyDatabaseFixture.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests that use an empty database
    /// </summary>
    [TestClass]
    public class EmptyDatabaseFixture
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesid;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbid;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        public void VerifyEmptyDatabaseFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Verify that TryMoveFirst throws an exception when ESENT
        /// returns an unexpected error;
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void TryMoveFirstThrowsExceptionOnError()
        {
            Api.TryMoveFirst(this.sesid, JET_TABLEID.Nil);
        }

        /// <summary>
        /// Verify that TryMoveLast throws an exception when ESENT
        /// returns an unexpected error;
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void TryMoveLastThrowsExceptionOnError()
        {
            Api.TryMoveLast(this.sesid, JET_TABLEID.Nil);
        }

        /// <summary>
        /// Verify that TryMoveNext throws an exception when ESENT
        /// returns an unexpected error;
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void TryMoveNextThrowsExceptionOnError()
        {
            Api.TryMoveNext(this.sesid, JET_TABLEID.Nil);
        }

        /// <summary>
        /// Verify that TryMovePrevious throws an exception when ESENT
        /// returns an unexpected error;
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void TryMovePreviousThrowsExceptionOnError()
        {
            Api.TryMovePrevious(this.sesid, JET_TABLEID.Nil);
        }

        /// <summary>
        /// Verify that TrySeek throws an exception when ESENT
        /// returns an unexpected error;
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void TrySeekThrowsExceptionOnError()
        {
            Api.TrySeek(this.sesid, JET_TABLEID.Nil, SeekGrbit.SeekEQ);
        }
    }
}
