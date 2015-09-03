//-----------------------------------------------------------------------
// <copyright file="InitTermTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Threading;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Implementation;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows7;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Init/Term tests
    /// </summary>
    [TestClass]
    public partial class InitTermTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Setup for BasicDDLTests")]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for BasicDDLTests")]
        public void Teardown()
        {
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Verify that the version returned by JetGetVersion is not zero.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the version returned by JetGetVersion is not zero.")]
        public void VerifyJetVersionIsNotZero()
        {
            JET_INSTANCE instance;
            JET_SESID sesid;
            uint version;

            Api.JetCreateInstance(out instance, "JetGetVersion");

            var parameters = new InstanceParameters(instance);
            parameters.Recovery = false;
            parameters.MaxTemporaryTables = 0;
            parameters.NoInformationEvent = true;

            Api.JetInit(ref instance);
            Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
#if MANAGEDESENT_ON_WSA // Not exposed in MSDK
            version = 0;
#else
            Api.JetGetVersion(sesid, out version);
#endif
            Api.JetTerm(instance);

            Assert.AreNotEqual(0, version);
            EseInteropTestHelper.ConsoleWriteLine("Version = 0x{0:X}", version);
        }

        /// <summary>
        /// Verify that the version returned by JetGetVersion can be overridden.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the version returned by JetGetVersion can be overridden.")]
        public void VerifyJetVersionOverride()
        {
            JET_INSTANCE instance;
            JET_SESID sesid;
            uint version;

            IJetApi savedImpl = Api.Impl;
            try
            {
                Api.Impl = new JetApi(Constants.VistaVersion);

                Api.JetCreateInstance(out instance, "JetGetVersionOverride");

                var parameters = new InstanceParameters(instance);
                parameters.Recovery = false;
                parameters.MaxTemporaryTables = 0;
                parameters.NoInformationEvent = true;

                Api.JetInit(ref instance);
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
#if MANAGEDESENT_ON_WSA // Not exposed in MSDK
                version = Constants.VistaVersion;
#else
                Api.JetGetVersion(sesid, out version);
#endif
                Api.JetTerm(instance);

                Assert.AreEqual(Constants.VistaVersion, version);
                EseInteropTestHelper.ConsoleWriteLine("Version = 0x{0:X}", version);
            }
            finally
            {
                Api.Impl = savedImpl;
            }
        }

        /// <summary>
        /// Initialize and terminate one instance. The instance is allocated
        /// with JetCreateInstance2.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and terminate one instance.")]
        public void CreateInstanceWithJetCreateInstance2()
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, Guid.NewGuid().ToString(), "Instance Display Name", CreateInstanceGrbit.None);

            var systemParameters = new InstanceParameters(instance);
            systemParameters.MaxTemporaryTables = 0;
            systemParameters.Recovery = false;
            systemParameters.NoInformationEvent = true;

            Api.JetInit(ref instance);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Initialize and terminate one instance. The instance is initialized
        /// with JetInit2.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and terminate one instance with JetInit2.")]
        public void InitializeInstanceWithJetInit2()
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, Guid.NewGuid().ToString(), "Instance Display Name", CreateInstanceGrbit.None);

            var systemParameters = new InstanceParameters(instance);
            systemParameters.MaxTemporaryTables = 0;
            systemParameters.Recovery = false;
            systemParameters.NoInformationEvent = true;

            Api.JetInit2(ref instance, InitGrbit.None);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Initialize and terminate one instance. The instance is initialized
        /// with JetInit3.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and terminate one instance with JetInit3.")]
        public void InitializeInstanceWithJetInit3()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                return;
            }

            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, Guid.NewGuid().ToString(), "Instance Display Name", CreateInstanceGrbit.None);

            var systemParameters = new InstanceParameters(instance);
            systemParameters.MaxTemporaryTables = 0;
            systemParameters.Recovery = false;
            systemParameters.NoInformationEvent = true;

            VistaApi.JetInit3(ref instance, null, InitGrbit.None);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Initialize and terminate one instance. The instance is initialized
        /// with JetInit3 and a JET_RSTINFO.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and terminate one instance with JetInit3 and a JET_RSTINFO.")]
        public void InitializeInstanceWithJetInit3AndRstinfo()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                return;
            }

            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, Guid.NewGuid().ToString(), "Instance Display Name", CreateInstanceGrbit.None);

            var systemParameters = new InstanceParameters(instance);
            systemParameters.MaxTemporaryTables = 0;
            systemParameters.Recovery = false;
            systemParameters.NoInformationEvent = true;

            VistaApi.JetInit3(ref instance, new JET_RSTINFO(), InitGrbit.None);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Initialize and terminate one instance. The instance is initialized
        /// with JetInit3 and a JET_RSTINFO, with recovery on.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and terminate one instance with JetInit3 and a JET_RSTINFO with recovery on.")]
        public void InitializeInstanceWithJetInit3AndRstinfoRecoveryOn()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                return;
            }

            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, Guid.NewGuid().ToString(), "Instance Display Name", CreateInstanceGrbit.None);

            var systemParameters = new InstanceParameters(instance);
            systemParameters.MaxTemporaryTables = 0;
            systemParameters.Recovery = true;
            systemParameters.NoInformationEvent = true;
            systemParameters.LogFileDirectory = this.directory;
            systemParameters.SystemDirectory = this.directory;
            systemParameters.TempDirectory = this.directory;

            VistaApi.JetInit3(ref instance, new JET_RSTINFO(), InitGrbit.None);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Terminating an uninitialized instance should work.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Terminating an uninitialized instance should work.")]
        public void VerifyTermUninitializedInstanceDoesNotThrowException()
        {
            var instance = new JET_INSTANCE();
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Terminating an uninitialized instance should work (JetTerm2).
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Terminating an uninitialized instance should work (JetTerm2).")]
        public void VerifyTerm2UninitializedInstanceDoesNotThrowException()
        {
            var instance = new JET_INSTANCE();
            Api.JetTerm2(instance, TermGrbit.None);
        }

        /// <summary>
        /// Initialize and terminate one instance.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and terminate one instance.")]
        public void InitAndTermOneInstance()
        {
            JET_INSTANCE instance = SetupHelper.CreateNewInstance("instance");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Initialize and abruptly terminate one instance.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Initialize and abruptly terminate one instance.")]
        public void InitAndTermOneInstanceAbruptly()
        {
            JET_INSTANCE instance = SetupHelper.CreateNewInstance("instance");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            Api.JetTerm2(instance, TermGrbit.Abrupt);
        }

        /// <summary>
        /// Initialize and terminate one instance twice.
        /// (Init/Term/Init/Term).
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Initialize and terminate one instance twice.")]
        public void InitAndTermOneInstanceTwice()
        {
            JET_INSTANCE instance = SetupHelper.CreateNewInstance("instance");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            Api.JetTerm(instance);
            Api.JetInit(ref instance);
            Api.JetTerm(instance);
        }

        /// <summary>
        /// Initialize and terminate two instances.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Initialize and terminate two instances.")]
        public void InitAndTermTwoInstances()
        {    
            JET_INSTANCE instance1 = SetupHelper.CreateNewInstance("instance1");
            Api.JetSetSystemParameter(instance1, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(instance1, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            JET_INSTANCE instance2 = SetupHelper.CreateNewInstance("instance2");
            Api.JetSetSystemParameter(instance2, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(instance2, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance1);
            Api.JetInit(ref instance2);
            Api.JetTerm(instance1);
            Api.JetTerm(instance2);
        }

#if !MANAGEDESENT_ON_WSA
        /// <summary>
        /// Call JetStopBackupInstance on a running instance.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Call JetStopBackupInstance on a running instance.")]
        public void TestJetStopBackupInstance()
        {
            using (var instance = new Instance("TestJetStopBackupInstance"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();
                Api.JetStopBackupInstance(instance);
            }
        }

        /// <summary>
        /// Call JetStopServiceInstance on a running instance.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Call JetStopServiceInstance on a running instance.")]
        public void TestJetStopServiceInstance()
        {
            using (var instance = new Instance("TestJetStopServiceInstance"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();
                Api.JetStopServiceInstance(instance);
            }
        }

        /// <summary>
        /// Call JetStopServiceInstance on a running instance and try another operation.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Call JetStopServiceInstance on a running instance and try another operation.")]
        public void TestJetStopServiceInstanceStopsSubsequentOperations()
        {
            using (var instance = new Instance("TestJetStopServiceInstance"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();
                Api.JetStopServiceInstance(instance);
                try
                {
                    JET_SESID sesid;
                    Api.JetBeginSession(instance, out sesid, null, null);
                    Assert.Fail("JetBeginSession should have thrown an exception when the service is stopped.");
                }
                catch (EsentClientRequestToStopJetServiceException)
                {
                }
            }
        }

        /// <summary>
        /// Call JetStopServiceInstance2 on a running instance.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Call JetStopServiceInstance2 on a running instance.")]
        public void TestJetStopServiceInstance2()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            using (var instance = new Instance("TestJetStopServiceInstance"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();
                Windows8Api.JetStopServiceInstance2(instance, StopServiceGrbit.All);
            }
        }

        /// <summary>
        /// Call JetStopServiceInstance2 on a running instance and try another operation.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Call JetStopServiceInstance2 on a running instance and try another operation.")]
        public void TestJetStopServiceInstance2StopsSubsequentOperations()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            using (var instance = new Instance("TestJetStopServiceInstance"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();

                StopServiceGrbit[] resumableGrbits = new StopServiceGrbit[]
                {
                    StopServiceGrbit.QuiesceCaches,
                    StopServiceGrbit.BackgroundUserTasks,
                };

                foreach (StopServiceGrbit grbit in resumableGrbits)
                {
                    JET_SESID sesid;
                    Windows8Api.JetStopServiceInstance2(instance, grbit);
                    try
                    {
                        Api.JetBeginSession(instance, out sesid, null, null);
                        Api.JetEndSession(sesid, EndSessionGrbit.None);
                    }
                    catch (EsentClientRequestToStopJetServiceException)
                    {
                        Assert.Fail("JetBeginSession should not have thrown an exception when the service is stopped with {0}.", grbit);
                    }

                    // Resuming the service should allow BeginSession to work.
                    Windows8Api.JetStopServiceInstance2(instance, StopServiceGrbit.Resume);
                    Api.JetBeginSession(instance, out sesid, null, null);
                    Api.JetEndSession(sesid, EndSessionGrbit.None);
                }
            }
        }

        /// <summary>
        /// Call JetConfigureProcessForCrashDump.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Call JetConfigureProcessForCrashDump.")]
        public void TestJetConfigureProcessForCrashDump()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            using (var instance = new Instance("TestJetConfigureProcessForCrashDump"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Init();
                Windows7Api.JetConfigureProcessForCrashDump(CrashDumpGrbit.Maximum);                
            }
        }

        /// <summary>
        /// Duplicate a session.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Duplicate a session.")]
        public void TestJetDupSession()
        {
            JET_INSTANCE instance = SetupHelper.CreateNewInstance("instance");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            JET_SESID sesid;
            Api.JetBeginSession(instance, out sesid, null, null);
            JET_SESID sesidDup;
            Api.JetDupSession(sesid, out sesidDup);
            Assert.AreNotEqual(sesid, sesidDup);
            Assert.AreNotEqual(sesidDup, JET_SESID.Nil);
            Api.JetTerm(instance);
        }
#endif // !MANAGEDESENT_ON_WSA

#if !MANAGEDESENT_ON_WSA // The threading model in Windows Store Apps has changed.
        /// <summary>
        /// Test moving a transaction between threads.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test moving a transaction between threads.")]
        public void VerifyJetSetSessionContextAllowsThreadMigration()
        {
            using (var instance = new Instance("JetSetSessionContext"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Init();
                using (var session = new Session(instance))
                {
                    // Without the calls to JetSetSessionContext/JetResetSessionContext
                    // this will generate a session sharing violation.
                    var context = new IntPtr(Any.Int32);

                    var thread = new Thread(() =>
                    {
                        EseInteropTestHelper.ThreadBeginThreadAffinity();
                        Api.JetSetSessionContext(session, context);
                        Api.JetBeginTransaction(session);
                        Api.JetResetSessionContext(session);
                        EseInteropTestHelper.ThreadEndThreadAffinity();
                    });
                    thread.Start();
                    thread.Join();

                    Api.JetSetSessionContext(session, context);
                    Api.JetCommitTransaction(session, CommitTransactionGrbit.None);
                    Api.JetResetSessionContext(session);                    
                }
            }
        }
#endif // !MANAGEDESENT_ON_WSA
    }
}