//-----------------------------------------------------------------------
// <copyright file="JetIdleTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
	/// <summary>
	/// Test JetIdle
	/// </summary>
	[TestClass]
	public class JetIdleTests
	{
		/// <summary>
		/// The directory being used for the database and its files.
		/// </summary>
		private string directory;

		/// <summary>
		/// The instance used by the test.
		/// </summary>
		private JET_INSTANCE instance;

		/// <summary>
		/// The session used by the test.
		/// </summary>
		private JET_SESID sesid;

		#region Setup/Teardown

		/// <summary>
		/// Initialization method. Called once when the tests are started.
		/// All DDL should be done in this method.
		/// </summary>
		[TestInitialize]
		public void Setup()
		{
			this.directory = SetupHelper.CreateRandomDirectory();
			this.instance = SetupHelper.CreateNewInstance(this.directory);

			// turn off logging so initialization is faster
			Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
			Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
			Api.JetInit(ref this.instance);
			Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
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
		[Priority(1)]
		public void VerifyFixtureSetup()
		{
			Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
			Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
		}

		#endregion Setup/Teardown

		[TestMethod]
		[Priority(0)]
		public void TestDefault()
		{
			Assert.AreEqual(JET_wrn.NoIdleActivity, Api.JetIdle(this.sesid, IdleGrbit.None));
		}

		[TestMethod]
		[Priority(0)]
		public void TestCompact()
		{
			Assert.AreEqual(JET_wrn.NoIdleActivity, Api.JetIdle(this.sesid, IdleGrbit.Compact));
		}

		[TestMethod]
		[Priority(0)]
		public void TestGetStatus()
		{
			Assert.AreEqual(JET_wrn.Success, Api.JetIdle(this.sesid, IdleGrbit.GetStatus));
		}

		[TestMethod]
		[Priority(0)]
		public void TestFlushBuffers()
		{
			try
			{
				Api.JetIdle(this.sesid, IdleGrbit.FlushBuffers);
				Assert.Fail("Expected an EsentErrorException");
			}
			catch(EsentErrorException ex)
			{
				Assert.AreEqual(JET_err.InvalidGrbit, ex.Error);
			}
		}
	}
}
