//-----------------------------------------------------------------------
// <copyright file="SetGetSystemParameterTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows7;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.Isam.Esent.Interop.Windows81;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Jet{Get,Set}SystemParameter tests
    /// </summary>
    [TestClass]
    public partial class SetGetSystemParameterTests
    {
        #region Setup/Teardown

        /// <summary>
        /// Setup the SetGetSystemParameterTests fixture.
        /// </summary>
        [TestInitialize]
        [Description("Setup the SetGetSystemParameterTests fixture")]
        public void Setup()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, VistaParam.EnableAdvanced, 1, null);
            }
        }

        /// <summary>
        /// Verifies no instances are leaked.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            SetupHelper.CheckProcessForInstanceLeaks();
            SystemParameters.Configuration = 1;
        }
        
        #endregion

#if !MANAGEDESENT_ON_WSA // String interning is not supported.
        /// <summary>
        /// Verify that retrieving a string parameter tries to intern the string.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that retrieving a string parameter tries to intern the string")]
        public void VerifyGetSystemParameterTriesToInternStrings()
        {
            string expected = string.Intern("edb");

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "StringParameterTest");
            try
            {
                Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.BaseName, 0, expected);

                int ignored = 0;
                string actual;
                Api.JetGetSystemParameter(instance, JET_SESID.Nil, JET_param.BaseName, ref ignored, out actual, 256);
                Assert.AreEqual(expected, actual);
                Assert.AreSame(expected, actual, "string wasn't interned");
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }
#endif // !MANAGEDESENT_ON_WSA

        /// <summary>
        /// Test setting and retrieving the system path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the system path")]
        public void SystemPathParameter()
        {
            PathParameterTest(JET_param.SystemPath, @"foo\system\");
        }

        /// <summary>
        /// Test setting and retrieving the log path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the log path")]
        public void LogPathParameter()
        {
            PathParameterTest(JET_param.LogFilePath, @"foo\log\");
        }

        /// <summary>
        /// Test setting and retrieving the temp path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the temp path")]
        public void TempPathParameter()
        {
            string dir = @"foo\temp\";

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "TempPathParameterTest");
            try
            {
                Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.TempPath, 0, dir);

                int ignored = 0;
                string actual;
                Api.JetGetSystemParameter(instance, JET_SESID.Nil, JET_param.TempPath, ref ignored, out actual, 256);

#if MANAGEDESENT_ON_WSA
                Assert.IsNotNull(actual);
#else
                // Older versions of esent (e.g. Windows XP) return the name of the temporary database
                // even when the temp path is configured as a directory. This means that setting
                // "temp\" will give back "temp\tmp.edb". Here we just assert that the returned string
                // starts with the expected value.
                string expected = Path.GetFullPath(dir);
                StringAssert.StartsWith(actual, expected);
#endif
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// Test setting and retrieving the base name.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the base name")]
        public void BaseNameParameter()
        {
            StringParameterTest(JET_param.BaseName, "foo");
        }

        /// <summary>
        /// Test setting and retrieving the event source.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the event source")]
        public void EventSourceParameter()
        {
            StringParameterTest(JET_param.EventSource, "My source");
        }

        /// <summary>
        /// Test setting and retrieving the max sessions setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max sessions setting")]
        public void MaxSessionsParameter()
        {
            IntegerParameterTest(JET_param.MaxSessions, 4);
        }

        /// <summary>
        /// Test setting and retrieving the max open tables setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max open tables setting")]
        public void MaxOpenTablesParameter()
        {
            IntegerParameterTest(JET_param.MaxOpenTables, 100);
        }

        /// <summary>
        /// Test setting and retrieving the max cursors setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max cursors setting")]
        public void MaxCursorsParameter()
        {
            IntegerParameterTest(JET_param.MaxCursors, 2500);
        }

        /// <summary>
        /// Test setting and retrieving the max ver pages setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max ver pages setting")]
        public void MaxVerPagesParameter()
        {
            IntegerParameterTest(JET_param.MaxVerPages, 100);
        }

        /// <summary>
        /// Test setting and retrieving the preferred ver pages setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the preferred ver pages setting")]
        public void PreferredVerPagesParameter()
        {
            IntegerParameterTest(JET_param.PreferredVerPages, 8);
        }

        /// <summary>
        /// Test setting and retrieving the version store task queue max setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the version store task queue max setting")]
        public void VersionStoreTaskQueueMaxParameter()
        {
            IntegerParameterTest(JET_param.VersionStoreTaskQueueMax, 17);
        }

        /// <summary>
        /// Test setting and retrieving the max temporary tables setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max temporary tables setting")]
        public void MaxTemporaryTablesParameter()
        {
            IntegerParameterTest(JET_param.MaxTemporaryTables, 0);
        }

        /// <summary>
        /// Test setting and retrieving the logfile size setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the logfile size setting")]
        public void LogFileSizeParameter()
        {
            IntegerParameterTest(JET_param.LogFileSize, 2048);
        }

        /// <summary>
        /// Test setting and retrieving the log buffers setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the log buffers setting")]
        public void LogBuffersParameter()
        {
            IntegerParameterTest(JET_param.LogBuffers, 128);
        }

        /// <summary>
        /// Test setting and retrieving the circular logging setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the circular logging setting")]
        public void CircularLogParameter()
        {
            IntegerParameterTest(JET_param.CircularLog, 1);
        }

        /// <summary>
        /// Test setting and retrieving the temp db min setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the temp db min setting")]
        public void PageTempDbMinParameter()
        {
            IntegerParameterTest(JET_param.PageTempDBMin, 50);
        }

        /// <summary>
        /// Test setting and retrieving the database extension size setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the database extension size setting")]
        public void PageTempDbExtensionSizeParameter()
        {
            IntegerParameterTest(JET_param.DbExtensionSize, 256);
        }

        /// <summary>
        /// Test setting and retrieving the checkpoint depth setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the checkpoint depth setting")]
        public void CheckpointDepthMaxParameter()
        {
            IntegerParameterTest(JET_param.CheckpointDepthMax, 20000);
        }

        /// <summary>
        /// Test setting and retrieving the recovery parameter.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the recovery parameter")]
        public void RecoveryParameter()
        {
            StringParameterTest(JET_param.Recovery, "off");
        }

        /// <summary>
        /// Test setting and retrieving the enable online defrag setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the enable online defrag setting")]
        public void EnableOnlineDefragParameter()
        {
            BooleanParameterTest(JET_param.EnableOnlineDefrag, Any.Boolean);
        }

        /// <summary>
        /// Test setting and retrieving the index checking setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the index checking setting")]
        public void EnableIndexCheckingParameter()
        {
            BooleanParameterTest(JET_param.EnableIndexChecking, Any.Boolean);
        }

        /// <summary>
        /// Test setting and retrieving the event source key setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the event source key setting")]
        public void EventSourceKeyParameter()
        {
            StringParameterTest(JET_param.EventSourceKey, Any.String);
        }

        /// <summary>
        /// Test setting and retrieving the no information event setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the no information event setting")]
        public void NoInformationEventParameter()
        {
            BooleanParameterTest(JET_param.NoInformationEvent, Any.Boolean);
        }

        /// <summary>
        /// Test setting and retrieving the create path setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the create path setting")]
        public void CreatePathIfNotExistParameter()
        {
            BooleanParameterTest(JET_param.CreatePathIfNotExist, Any.Boolean);
        }

        /// <summary>
        /// Test setting and retrieving the cleanup mismatched logfiles setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the cleanup mismatched logfiles setting")]
        public void CleanupMismatchedLogFilesParameter()
        {
            BooleanParameterTest(JET_param.CleanupMismatchedLogFiles, Any.Boolean);
        }

#if !MANAGEDESENT_ON_WSA // Not exposed in MSDK
        /// <summary>
        /// Test setting the runtime callback to null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting the runtime callback to null")]
        public void SetRuntimeCallbackToNull()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                Assert.Inconclusive("Cannot set runtime callback to null on this version of ESENT");
            }

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "ResetRuntimeCallbackTest");
            try
            {
                Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.RuntimeCallback, null, null);
            }
            finally
            {
                Api.JetTerm(instance);
            }            
        }

        /// <summary>
        /// Test setting the global runtime callback to null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting the global runtime callback to null")]
        public void SetGlobalRuntimeCallbackToNull()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                Assert.Inconclusive("Cannot set runtime callback to null on this version of ESENT");
            }

            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "RuntimeCallbackTest");
            try
            {
                Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.RuntimeCallback, null, null);
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }
#endif // !MANAGEDESENT_ON_WSA

        /// <summary>
        /// Test setting and retrieving the Configuration parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the Configuration parameter (if esent supports it)")]
        public void ConfigurationVistaParameter()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                IntegerParameterTest(VistaParam.Configuration, 1);
            }
        }

        /// <summary>
        /// Test setting and retrieving the EnableAdvanced parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the EnableAdvanced parameter (if esent supports it)")]
        public void EnableAdvancedVistaParameter()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                BooleanParameterTest(VistaParam.EnableAdvanced, Any.Boolean);
            }
        }

        /// <summary>
        /// Test setting and retrieving the CachedClosedTables parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the CachedClosedTables parameter (if esent supports it)")]
        public void CachedClosedTablesVistaParameter()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                IntegerParameterTest(VistaParam.CachedClosedTables, 500);
            }
        }

        /// <summary>
        /// Test setting and retrieving the EnableFileCache parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the EnableFileCache parameter (if esent supports it)")]
        public void EnableFileCacheVistaParameter()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                BooleanParameterTest(JET_INSTANCE.Nil, VistaParam.EnableFileCache, Any.Boolean);
            }
        }

        /// <summary>
        /// Test setting and retrieving the EnableViewCache parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the EnableViewCache parameter (if esent supports it)")]
        public void EnableViewCacheVistaParameter()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                BooleanParameterTest(JET_INSTANCE.Nil, VistaParam.EnableViewCache, Any.Boolean);
            }
        }

        /// <summary>
        /// Test retrieving the KeyMost parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test retrieving the KeyMost parameter (if esent supports it)")]
        public void KeyMostVistaParameter()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                JET_INSTANCE instance;
                Api.JetCreateInstance(out instance, "KeyMostParameterTest");
                try
                {
                    int keyMost = 0;
                    string ignored;
                    Api.JetGetSystemParameter(instance, JET_SESID.Nil, VistaParam.KeyMost, ref keyMost, out ignored, 0);

                    Assert.IsTrue(keyMost > 255);
                }
                finally
                {
                    Api.JetTerm(instance);
                }
            }
        }

        /// <summary>
        /// Test setting and retrieving the WaypointLatency parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the WaypointLatency parameter (if esent supports it)")]
        public void WaypointLatencyWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(Windows7Param.WaypointLatency, 1);
            }
        }

        /// <summary>
        /// Test setting and retrieving the MaxCoalesceReadSize parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the MaxCoalesceReadSize parameter (if esent supports it)")]
        public void MaxCoalesceReadSizeWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(JET_INSTANCE.Nil, Windows7Param.MaxCoalesceReadSize, 32 * 1024);
            }
        }

        /// <summary>
        /// Test setting and retrieving the MaxCoalesceWriteSize parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the MaxCoalesceWriteSize parameter (if esent supports it)")]
        public void MaxCoalesceWriteSizeWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(JET_INSTANCE.Nil, Windows7Param.MaxCoalesceWriteSize, 32 * 1024);
            }
        }

        /// <summary>
        /// Test setting and retrieving the MaxCoalesceReadGapSize parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the MaxCoalesceReadGapSize parameter (if esent supports it)")]
        public void MaxCoalesceReadGapSizeWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(JET_INSTANCE.Nil, Windows7Param.MaxCoalesceReadGapSize, 32 * 1024);
            }
        }

        /// <summary>
        /// Test setting and retrieving the MaxCoalesceWriteGapSize parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the MaxCoalesceWriteGapSize parameter (if esent supports it)")]
        public void MaxCoalesceWriteGapSizeWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(JET_INSTANCE.Nil, Windows7Param.MaxCoalesceWriteGapSize, 32 * 1024);
            }
        }
        
        /// <summary>
        /// Test setting and retrieving the DbScanThrottle parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the DbScanThrottle parameter (if esent supports it)")]
        public void DbScanThrottleWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(Windows7Param.DbScanThrottle, 1);
            }
        }

        /// <summary>
        /// Test setting and retrieving the DbScanIntervalMinSec parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the DbScanIntervalMinSec parameter (if esent supports it)")]
        public void DbScanIntervalMinSecWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(Windows7Param.DbScanIntervalMinSec, 3600);
            }
        }

        /// <summary>
        /// Test setting and retrieving the DbScanIntervalMaxSec parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the DbScanIntervalMaxSec parameter (if esent supports it)")]
        public void DbScanIntervalMaxSecWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                IntegerParameterTest(Windows7Param.DbScanIntervalMaxSec, 7200);
            }
        }

        /// <summary>
        /// Test retrieving the LVChunkSizeMost parameter (if esent supports it)
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test retrieving the LVChunkSizeMost parameter (if esent supports it)")]
        public void RetrieveLvChunkSizeMostWin7Parameter()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                int chunkSize = 0;
                string ignored;
                Api.JetGetSystemParameter(
                    JET_INSTANCE.Nil,
                    JET_SESID.Nil,
                    Windows7Param.LVChunkSizeMost,
                    ref chunkSize,
                    out ignored,
                    0);
                Assert.AreNotEqual(0, chunkSize);
            }
        }

        /// <summary>
        /// Check that the BookmarkMost system parameter is at least 
        /// the legacy minimum.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that the BookmarkMost system parameter is at least the legacy minimum")]
        public void VerifyBookmarkMostIsAtLeast255()
        {
            Assert.IsTrue(SystemParameters.BookmarkMost >= 255);
        }

        /// <summary>
        /// Test that SystemParameters.CacheSize can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.CacheSize can be set")]
        public void VerifyGetAndSetCacheSize()
        {
            int cacheSizeOld = SystemParameters.CacheSize;
            SystemParameters.CacheSize = 4096;

            // The setting doesn't take effect immediately,
            // so no assert here.
            SystemParameters.CacheSize = cacheSizeOld;
        }

        /// <summary>
        /// Test that SystemParameters.CacheSizeMax can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.CacheSizeMax can be set and retrieved")]
        public void VerifyGetAndSetCacheSizeMax()
        {
            int cacheSizeMaxOld = SystemParameters.CacheSizeMax;
            SystemParameters.CacheSizeMax = 4096;
            Assert.AreEqual(4096, SystemParameters.CacheSizeMax);
            SystemParameters.CacheSizeMax = cacheSizeMaxOld;
        }

        /// <summary>
        /// Test that SystemParameters.CacheSizeMin can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.CacheSizeMin can be set and retrieved")]
        public void VerifyGetAndSetCacheSizeMin()
        {
            int cacheSizeMinOld = SystemParameters.CacheSizeMin;
            SystemParameters.CacheSizeMin = 4096;
            Assert.AreEqual(4096, SystemParameters.CacheSizeMin);
            SystemParameters.CacheSizeMin = cacheSizeMinOld;
        }

        /// <summary>
        /// Check that the ColumnsKeyMost system parameter is at least 
        /// the legacy minimum.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that the ColumnsKeyMost system parameter is at least the legacy minimum")]
        public void VerifyColumnsKeyMostIsAtLeast12()
        {
            Assert.IsTrue(SystemParameters.ColumnsKeyMost >= 12);
        }

        /// <summary>
        /// Test that SystemParameters.OutstandingIOMax can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.OutstandingIOMax can be set and retrieved")]
        public void VerifyGetAndSetOutstandingIOMax()
        {
            int outstandingIOMaxOld = SystemParameters.OutstandingIOMax;
            SystemParameters.OutstandingIOMax = 13;
            Assert.AreEqual(13, SystemParameters.OutstandingIOMax);
            SystemParameters.OutstandingIOMax = outstandingIOMaxOld;
        }

        /// <summary>
        /// Test that SystemParameters.StartFlushThreshold can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.StartFlushThreshold can be set and retrieved")]
        public void VerifyGetAndSetStartFlushThreshold()
        {
            int startFlushThresholdOld = SystemParameters.StartFlushThreshold;
            SystemParameters.StartFlushThreshold = 13;
            Assert.AreEqual(13, SystemParameters.StartFlushThreshold);
            SystemParameters.StartFlushThreshold = startFlushThresholdOld;
        }

        /// <summary>
        /// Test that SystemParameters.StopFlushThreshold can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.StopFlushThreshold can be set and retrieved")]
        public void VerifyGetAndSetStopFlushThreshold()
        {
            int stopFlushThresholdOld = SystemParameters.StopFlushThreshold;
            SystemParameters.StopFlushThreshold = 17;
            Assert.AreEqual(17, SystemParameters.StopFlushThreshold);
            SystemParameters.StopFlushThreshold = stopFlushThresholdOld;
        }

        /// <summary>
        /// Test that SystemParameters.EventLoggingLevel can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.EventLoggingLevel can be set and retrieved")]
        public void VerifyGetAndSetEventLoggingLevel()
        {
            int eventLoggingLevelOld = SystemParameters.EventLoggingLevel;
            SystemParameters.EventLoggingLevel = (int)EventLoggingLevels.High;
            Assert.AreEqual((int)EventLoggingLevels.High, SystemParameters.EventLoggingLevel);
            Assert.AreNotEqual(eventLoggingLevelOld, SystemParameters.EventLoggingLevel);
            SystemParameters.EventLoggingLevel = eventLoggingLevelOld;
        }

        /// <summary>
        /// Test that SystemParameters.DatabasePageSize can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.DatabasePageSize can be set and retrieved")]
        public void VerifyGetAndSetDatabasePageSize()
        {
            int databasePageSizeOld = SystemParameters.DatabasePageSize;
            SystemParameters.DatabasePageSize = 4096;
            Assert.AreEqual(4096, SystemParameters.DatabasePageSize);
            SystemParameters.DatabasePageSize = databasePageSizeOld;
        }

        /// <summary>
        /// Check that the KeyMost system parameter is at least 
        /// the legacy minimum.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that the KeyMost system parameter is at least the legacy minimum")]
        public void VerifyKeyMostIsAtLeast255()
        {
            Assert.IsTrue(SystemParameters.KeyMost >= 255);
        }

        /// <summary>
        /// Check that the LVChunkSizeMost system parameter is at least 
        /// a sensible minimum.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that the LVChunkSizeMost system parameter is at least a sensible minimum")]
        public void VerifyLvChunkSizeMostIsNonZero()
        {
            // 1966 is the chunk size of 2Kb pages.
            Assert.IsTrue(SystemParameters.LVChunkSizeMost >= 1966);
        }

        /// <summary>
        /// Test that SystemParameters.MaxInstances can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.MaxInstances can be set and retrieved")]
        public void VerifyGetAndSetMaxInstances()
        {
            int maxInstancesOld = SystemParameters.MaxInstances;
            SystemParameters.MaxInstances = 16;
            Assert.AreEqual(16, SystemParameters.MaxInstances);
            SystemParameters.MaxInstances = maxInstancesOld;
        }

        /// <summary>
        /// Test that SystemParameters.LegacyFileNames can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.LegacyFileNames can be set and retrieved")]
        public void VerifyGetAndSetLegacyFileNames()
        {
            int oldValue = SystemParameters.LegacyFileNames;
            int newValue = (oldValue == 0) ? 1 : 0;
            SystemParameters.LegacyFileNames = newValue;
            Assert.AreEqual(newValue, SystemParameters.LegacyFileNames);

            SystemParameters.LegacyFileNames = oldValue;
        }

        /// <summary>
        /// Test that SystemParameters.EnableFileCache can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.EnableFileCache can be set and retrieved")]
        public void VerifyGetAndSetEnableFileCache()
        {
            bool oldValue = SystemParameters.EnableFileCache;
            bool newValue = !oldValue;
            SystemParameters.EnableFileCache = newValue;
            Assert.AreEqual(newValue, SystemParameters.EnableFileCache);

            SystemParameters.EnableFileCache = oldValue;
        }

        /// <summary>
        /// Test that SystemParameters.EnableViewCache can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.EnableViewCache can be set and retrieved")]
        public void VerifyGetAndSetEnableViewCache()
        {
            bool oldValue = SystemParameters.EnableViewCache;
            bool newValue = !oldValue;
            SystemParameters.EnableViewCache = newValue;
            Assert.AreEqual(newValue, SystemParameters.EnableViewCache);

            SystemParameters.EnableViewCache = oldValue;
        }

        /// <summary>
        /// Test that SystemParameters.ExceptionAction can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.ExceptionAction can be set and retrieved")]
        public void VerifyGetAndSetExceptionAction()
        {
            JET_ExceptionAction exceptionActionOld = SystemParameters.ExceptionAction;
            SystemParameters.ExceptionAction = JET_ExceptionAction.MsgBox;
            Assert.AreEqual(JET_ExceptionAction.MsgBox, SystemParameters.ExceptionAction);
            SystemParameters.ExceptionAction = exceptionActionOld;
        }

#region Vista parameters
        /// <summary>
        /// Test that SystemParameters.Configuration can be set and retrieved.
        /// This test only works on Windows Vista and up. An ESENT bug stops
        /// the configuration from being retrieved properly.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.Configuration can be set and retrieved")]
        public void VerifySetConfiguration()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                return;
            }

            int configurationOld = SystemParameters.Configuration;
            SystemParameters.Configuration = 0;
            SystemParameters.Configuration = configurationOld;
        }

        /// <summary>
        /// Test that SystemParameters.EnableAdvanced can be set and retrieved.
        /// This test only works on Windows Vista and up.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.EnableAdvanced can be set and retrieved")]
        public void VerifyGetAndSetEnableAdvanced()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                return;
            }

            bool enableAdvanced = SystemParameters.EnableAdvanced;
            SystemParameters.EnableAdvanced = true;
            Assert.AreEqual(true, SystemParameters.EnableAdvanced);
            SystemParameters.EnableAdvanced = enableAdvanced;
        }

        /// <summary>
        /// Test that SystemParameters.Configuration can be set and retrieved.
        /// This test only works on Windows Vista and up.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.Configuration can be set and retrieved")]
        public void VerifyGetAndSetConfigurationIsFixed()
        {
            if (!EsentVersion.SupportsVistaFeatures)
            {
                return;
            }

            int configurationOld = SystemParameters.Configuration;
            try
            {
                SystemParameters.Configuration = 0;
                Assert.AreEqual(0, SystemParameters.Configuration);
            }
            finally
            {
                SystemParameters.Configuration = configurationOld;
            }
        }

    #endregion

#region Windows 8 Parameters.
        /// <summary>
        /// Test setting and retrieving the max transaction size setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max transaction size setting")]
        public void MaxTransactionSizeParameter()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            IntegerParameterTest(Windows8Param.MaxTransactionSize, 55);
        }

        /// <summary>
        /// Test setting and retrieving the cache priority setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the cache priority setting")]
        public void CachePriorityParameter()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            IntegerParameterTest(Windows8Param.CachePriority, 57);
        }

        /// <summary>
        /// Test setting and retrieving the max i/o for preread setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test setting and retrieving the max i/o for preread setting")]
        public void PrereadIOMaxParameter()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            IntegerParameterTest(Windows8Param.PrereadIOMax, 58);
        }

        /// <summary>
        /// Test that SystemParameters.MinDataForXpress can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.MinDataForXpress can be set and retrieved")]
        public void VerifyGetAndSetMinDataForXpress()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            int minDataForXpressOld = SystemParameters.MinDataForXpress;
            SystemParameters.MinDataForXpress = 128;
            Assert.AreEqual(128, SystemParameters.MinDataForXpress);
            SystemParameters.MinDataForXpress = minDataForXpressOld;
        }

        /// <summary>
        /// Test that SystemParameters.HungIOThreshold can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.HungIOThreshold can be set and retrieved")]
        public void VerifyGetAndSetHungIOThreshold()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            int hungIOThresholdOld = SystemParameters.HungIOThreshold;
            SystemParameters.HungIOThreshold = 75 * 1000;
            Assert.AreEqual(75 * 1000, SystemParameters.HungIOThreshold);
            SystemParameters.HungIOThreshold = hungIOThresholdOld;
        } 

        /// <summary>
        /// Test that SystemParameters.ProcessFriendlyName can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.ProcessFriendlyName can be set and retrieved")]
        public void VerifyGetAndSetProcessFriendlyName()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            StringParameterTest(JET_INSTANCE.Nil, Windows8Param.ProcessFriendlyName, "AProcessFriendlyName");
        }

        /// <summary>
        /// Test that SystemParameters.ProcessFriendlyName can be set and retrieved using static properties.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.ProcessFriendlyName can be set and retrieved using static properties")]
        public void VerifyGetAndSetProcessFriendlyNameStatic()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            string friendlyNameOld = SystemParameters.ProcessFriendlyName;
            string friendlyNameNew = friendlyNameOld + "v2";
            SystemParameters.ProcessFriendlyName = friendlyNameNew;
            Assert.IsTrue(friendlyNameNew == SystemParameters.ProcessFriendlyName);
            SystemParameters.ProcessFriendlyName = friendlyNameOld;
        }

#endregion

        #region Windows 8.1 Parameters
        /// <summary>
        /// Test that SystemParameters.EnablePeriodicShrinkDatabase can be set and retrieved.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test that SystemParameters.EnableShrinkDatabase can be set and retrieved")]
        public void VerifyGetAndSetEnableShrinkDatabase()
        {
            if (!EsentVersion.SupportsWindows81Features)
            {
                return;
            }

            IntegerParameterTest(Windows81Param.EnableShrinkDatabase, 0x3);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Test setting and retrieving a system parameter that uses a path. A relative
        /// path is set but a full path is retrieved.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The path to set it to.</param>
        private static void PathParameterTest(JET_param param, string expected)
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "PathParameterTest");
            try
            {
                Api.JetSetSystemParameter(instance, JET_SESID.Nil, param, 0, expected);

                int ignored = 0;
                string actual;
                Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref ignored, out actual, 256);

#if MANAGEDESENT_ON_WSA
                // We can't fetch the full path in Windows Store Apps, but we can check if the last part of the path is correct.
                Assert.IsNotNull(actual);
                Assert.IsTrue(actual.EndsWith(expected));
#else
                Assert.AreEqual(Path.GetFullPath(expected), actual);
#endif
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// Test setting and retrieving a system parameter that uses a string.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private static void StringParameterTest(JET_param param, string expected)
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "StringParameterTest");
            try
            {
                SetGetSystemParameterTests.StringParameterTest(instance, param, expected);
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// Test setting and retrieving a system parameter that uses a string.
        /// </summary>
        /// <param name="instance">The ESE instance.</param>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to expect when reading the parameter.</param>
        private static void StringParameterTest(JET_INSTANCE instance, JET_param param, string expected)
        {
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, param, 0, expected);

            int ignored = 0;
            string actual;
            Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref ignored, out actual, 256);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting and retrieving an integer system parameter.
        /// </summary>
        /// <param name="instance">The ESE instance.</param>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private static void IntegerParameterTest(JET_INSTANCE instance, JET_param param, int expected)
        {
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, param, expected, null);

            int actual = 0;
            string ignored;
            Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref actual, out ignored, 0);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting and retrieving an integer system parameter.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private static void IntegerParameterTest(JET_param param, int expected)
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "IntParameterTest");
            try
            {
                SetGetSystemParameterTests.IntegerParameterTest(instance, param, expected);
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// Test setting and retrieving an integer system parameter which
        /// is treated as a boolean.
        /// </summary>
        /// <param name="instance">The ESE instance.</param>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private static void BooleanParameterTest(JET_INSTANCE instance, JET_param param, bool expected)
        {
            int value = expected ? 1 : 0;
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, param, value, null);

            int actual = 0;
            string ignored;
            Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref actual, out ignored, 0);

            if (expected)
            {
                Assert.AreNotEqual(0, actual);
            }
            else
            {
                Assert.AreEqual(0, actual);
            }
        }

        /// <summary>
        /// Test setting and retrieving an integer system parameter which
        /// is treated as a boolean.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="expected">The string to set it to.</param>
        private static void BooleanParameterTest(JET_param param, bool expected)
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance(out instance, "BoolParameterTest");
            try
            {
                SetGetSystemParameterTests.BooleanParameterTest(instance, param, expected);
            }
            finally
            {
                Api.JetTerm(instance);
            }
        }

        #endregion Helper Methods
    }
}
