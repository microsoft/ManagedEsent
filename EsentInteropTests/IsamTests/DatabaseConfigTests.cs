//-----------------------------------------------------------------------
// <copyright file="DatabaseConfigTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Database.Isam;
    using Microsoft.Database.Isam.Config;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Database configuration tests
    /// </summary>
    [TestClass]
    public class DatabaseConfigTests
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
        /// Database configuration.
        /// </summary>
        private DatabaseConfig databaseConfig;

        /// <summary>
        /// Used to restore page size back to original value, to avoid screwing up other tests.
        /// </summary>
        private int pagesize;

        /// <summary>
        /// Initialization method. Called for each test.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "DatabaseConfig.edb");
            this.databaseConfig = new DatabaseConfig()
            {
                DatabaseFilename = this.database,
                SystemPath = this.directory,
                LogFilePath = this.directory,
                TempPath = this.directory,
                Recovery = "off",   // to speed up the test
            };

            string dummy;
            Api.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.DatabasePageSize, ref this.pagesize, out dummy, 0);
        }

        /// <summary>
        /// Cleanup for each test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            this.databaseConfig = null;
            Cleanup.DeleteDirectoryWithRetry(this.directory);
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.DatabasePageSize, this.pagesize, null);
        }

        /// <summary>
        /// Verifies that global parameters are set successfully.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that global parameters are set successfully.")]
        public void VerifySetGlobalParams()
        {
            // Set some random global parameters
            // Make sure to set at least 1 of each type (IntPtr, int, bool, enum, string)
            // Currently there are no global IntPtr parameters.
            this.databaseConfig.DatabasePageSize = 16 * 1024;
            this.databaseConfig.ExceptionAction = Windows7ExceptionAction.FailFast;
            this.databaseConfig.TableClass1Name = "test";
            this.databaseConfig.EnableFileCache = true;

            using (var database = new Database(this.databaseConfig))
            {
                Assert.AreEqual(this.databaseConfig.DatabasePageSize, this.GetIntParam(database.InstanceHandle, JET_param.DatabasePageSize));
                Assert.AreEqual((int)this.databaseConfig.ExceptionAction, this.GetIntParam(database.InstanceHandle, JET_param.ExceptionAction));
                Assert.AreEqual(this.databaseConfig.TableClass1Name, this.GetStringParam(database.InstanceHandle, VistaParam.TableClass1Name));
                Assert.AreEqual(this.databaseConfig.EnableFileCache, this.GetIntParam(database.InstanceHandle, VistaParam.EnableFileCache) == 1);

                // Verify that Database's own config set returns the right values
                Assert.AreEqual(this.databaseConfig.DatabasePageSize, database.Config.DatabasePageSize);
                Assert.AreEqual(this.databaseConfig.ExceptionAction, database.Config.ExceptionAction);
                Assert.AreEqual(this.databaseConfig.TableClass1Name, database.Config.TableClass1Name);
                Assert.AreEqual(this.databaseConfig.EnableFileCache, database.Config.EnableFileCache);
            }
        }

        /// <summary>
        /// Verifies that instance parameters are set successfully.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that instance parameters are set successfully.")]
        public void VerifySetInstanceParams()
        {
            // Set some random instance parameters
            // Make sure to set at least 1 of each type (IntPtr, int, bool, enum, string)
            // System path parameters are string
            this.databaseConfig.EnableAdvanced = true;
            this.databaseConfig.DbExtensionSize = 199;
            this.databaseConfig.MaxSessions = 500;
            this.databaseConfig.EnableIndexChecking = true;
            this.databaseConfig.CheckpointDepthMax = 10 * 1024 * 1024;

            using (var database = new Database(this.databaseConfig))
            {
                Assert.AreEqual(this.databaseConfig.EnableAdvanced, this.GetIntParam(database.InstanceHandle, VistaParam.EnableAdvanced) == 1);
                Assert.AreEqual(this.databaseConfig.DbExtensionSize, this.GetIntParam(database.InstanceHandle, JET_param.DbExtensionSize));
                Assert.AreEqual(this.databaseConfig.MaxSessions, this.GetIntParam(database.InstanceHandle, JET_param.MaxSessions));
                Assert.AreEqual(this.databaseConfig.EnableIndexChecking, this.GetIntParam(database.InstanceHandle, JET_param.EnableIndexChecking) == 1);
                Assert.AreEqual(this.databaseConfig.CheckpointDepthMax, this.GetIntParam(database.InstanceHandle, JET_param.CheckpointDepthMax));

                // Verify that Database's own config set returns the right values
                Assert.AreEqual(this.databaseConfig.EnableAdvanced, database.Config.EnableAdvanced);
                Assert.AreEqual(this.databaseConfig.DbExtensionSize, database.Config.DbExtensionSize);
                Assert.AreEqual(this.databaseConfig.MaxSessions, database.Config.MaxSessions);
                Assert.AreEqual(this.databaseConfig.EnableIndexChecking, database.Config.EnableIndexChecking);
                Assert.AreEqual(this.databaseConfig.CheckpointDepthMax, database.Config.CheckpointDepthMax);
            }
        }

        /// <summary>
        /// Verifies that the latest value of instance parameters is retrieved successfully.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that the latest value of instance parameters is retrieved successfully.")]
        public void VerifyGetInstanceParams()
        {
            // Set some random instance parameters
            // Make sure to set at least 1 of each type (IntPtr, int, bool, enum, string)
            // System path parameters are string
            this.databaseConfig.EnableAdvanced = true;

            using (var database = new Database(this.databaseConfig))
            {
                this.SetParam(database.InstanceHandle, JET_param.CacheSizeMax, 123);

                // Verify that Database's own config set returns the right values
                Assert.AreEqual(123, database.Config.CacheSizeMax);
                Assert.AreNotEqual(IntPtr.Zero, database.Config.PrintFunction); // get a readonly param
            }
        }

        /// <summary>
        /// Verifies that same global parameters can be specified on multiple config sets and Database instances.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that same global parameters can be specified on multiple config sets and Database instances.")]
        public void VerifySameGlobalParamsOnMultipleEngines()
        {
            this.databaseConfig.DatabasePageSize = 16 * 1024;

            using (var database1 = new Database(this.databaseConfig))
            {
                var databaseConfig2 = new DatabaseConfig();
                databaseConfig2.Merge(this.databaseConfig);
                databaseConfig2.DatabaseFilename = Path.Combine(databaseConfig2.SystemPath, "DatabaseConfig2.edb");
                databaseConfig2.TempPath = ".\\";
                using (var database2 = new Database(databaseConfig2))
                {
                    Assert.AreEqual(database1.Config.DatabasePageSize, database2.Config.DatabasePageSize);
                }
            }
        }

        /// <summary>
        /// Verifies that conflicting global parameters can't be set.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that conflicting global parameters can't be set.")]
        [ExpectedException(typeof(EsentAlreadyInitializedException))]
        public void VerifyConfilictingGlobalParamsFail()
        {
            this.databaseConfig.DatabasePageSize = 16 * 1024;

            using (var database1 = new Database(this.databaseConfig))
            {
                this.databaseConfig.DatabasePageSize = 8 * 1024;
                using (var database2 = new Database(this.databaseConfig))
                {
                }
            }
        }

        /// <summary>
        /// Verifies that mutable parameters can be changed after Database has been initialized.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that parameters can be changed after Database has been initialized.")]
        public void VerifyChangeParamsAtRuntime()
        {
            using (var database = new Database(this.databaseConfig))
            {
                database.Config.CacheSizeMin = 128;
                database.Config.CacheSizeMax = 256;
                database.Config.DefragmentSequentialBTrees = false;
                database.Config.ExceptionAction = JET_ExceptionAction.None;

                int cacheSize = this.GetIntParam(database.InstanceHandle, JET_param.CacheSize);
                Assert.IsTrue(cacheSize >= 128 && cacheSize <= 256);
                Assert.AreEqual(false, this.GetIntParam(database.InstanceHandle,  (JET_param)160) == 1); // JET_paramDefragmentSequentialBTrees
                Assert.AreEqual((int)JET_ExceptionAction.None, this.GetIntParam(database.InstanceHandle, JET_param.ExceptionAction));
            }
        }

        /// <summary>
        /// Verifies that mutable parameters can be changed after Database has been initialized using a merge operation.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that parameters can be changed after Database has been initialized using a merge operation.")]
        public void VerifyChangeParamsAtRuntimeWithMerge()
        {
            var newConfig = new DatabaseConfig()
            {
                CacheSizeMin = 128,
                CacheSizeMax = 256,
                DefragmentSequentialBTrees = false,
                ExceptionAction = JET_ExceptionAction.None,
            };

            using (var database = new Database(this.databaseConfig))
            {
                database.Config.Merge(newConfig, MergeRules.Overwrite);

                int cacheSize = this.GetIntParam(database.InstanceHandle, JET_param.CacheSize);
                Assert.IsTrue(cacheSize >= 128 && cacheSize <= 256);
                Assert.AreEqual(false, this.GetIntParam(database.InstanceHandle, (JET_param)160) == 1); // JET_paramDefragmentSequentialBTrees
                Assert.AreEqual((int)JET_ExceptionAction.None, this.GetIntParam(database.InstanceHandle, JET_param.ExceptionAction));
            }
        }

        /// <summary>
        /// Verifies that immutable parameters can not be changed after Database has been initialized.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that immutable parameters can not be changed after Database has been initialized.")]
        [ExpectedException(typeof(EsentAlreadyInitializedException))]
        public void TestChangeImmutableParamsAtRuntime()
        {
            using (var database = new Database(this.databaseConfig))
            {
                database.Config.SystemPath = "somePath\\";
            }
        }

        /// <summary>
        /// Verifies that Database parameters that are not Jet_param* can't be changed after Database has been initialized.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verifies that Database parameters that are not Jet_param* can't be changed after Database has been initialized.")]
        public void TestChangeEngineParamsAtRuntime()
        {
            Action<Action> test = (Action predicate) =>
            {
                try
                {
                    predicate();
                    Assert.Fail("Expected EsentAlreadyInitializedException !");
                }
                catch (EsentAlreadyInitializedException)
                {
                }
            };

            using (var database = new Database(this.databaseConfig))
            {
                test(() => database.Config.Identifier = "test");
                test(() => database.Config.DisplayName = "test");
                test(() => database.Config.EngineFlags = CreateInstanceGrbit.None);
                test(() => database.Config.DatabaseFilename = "somePath\\");
                test(() => database.Config.DatabaseCreationFlags = CreateDatabaseGrbit.OverwriteExisting);
                test(() => database.Config.DatabaseAttachFlags = AttachDatabaseGrbit.ReadOnly);
                test(() => database.Config.DatabaseMaxPages = 42);
            }
        }

        /// <summary>
        /// Calls get/set for all public properties of DatabaseConfig.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Calls get/set for all public properties of DatabaseConfig.")]
        public void GetSetAllProperties()
        {
            var databaseConfig = new DatabaseConfig();

            Type typeInfo = databaseConfig.GetType();
            foreach (var prop in typeInfo.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (prop.CanWrite)
                {
                    // Ignore indexers
                    if (prop.GetIndexParameters().Length == 0)
                    {
                        prop.SetValue(databaseConfig, this.GetDefaultValue(prop.PropertyType), null);
                    }
                }

                Assert.IsTrue(prop.CanRead);
                Assert.AreEqual(this.GetDefaultValue(prop.PropertyType), prop.GetValue(databaseConfig, null));
            }
        }

        /// <summary>
        /// Calls JetGetSystemParamter to return a JET_param value.
        /// </summary>
        /// <param name="instance">The instance to use.</param>
        /// <param name="param">The param to get.</param>
        /// <returns>The parameter value as an integer.</returns>
        private int GetIntParam(JET_INSTANCE instance, JET_param param)
        {
            int value = 0;
            string dummy;
            Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref value, out dummy, 0);
            return value;
        }

        /// <summary>
        /// Calls JetGetSystemParamter to return a JET_param value.
        /// </summary>
        /// <param name="instance">The instance to use.</param>
        /// <param name="param">The param to get.</param>
        /// <returns>The parameter value as an integer.</returns>
        private string GetStringParam(JET_INSTANCE instance, JET_param param)
        {
            int dummy = 0;
            string value;
            Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref dummy, out value, 260);
            return value;
        }

        /// <summary>
        /// Sets an integer param on an Ese instance.
        /// </summary>
        /// <param name="instance">The instance handle.</param>
        /// <param name="param">The param to set.</param>
        /// <param name="value">The param value.</param>
        private void SetParam(JET_INSTANCE instance, JET_param param, int value)
        {
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, param, value, null);
        }

        /// <summary>
        /// Sets a string param on an Ese instance.
        /// </summary>
        /// <param name="instance">The instance handle.</param>
        /// <param name="param">The param to set.</param>
        /// <param name="value">The param value.</param>
        private void SetParam(JET_INSTANCE instance, JET_param param, string value)
        {
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, param, 0, value);
        }

        /// <summary>
        /// Returns default(T) via reflection.
        /// </summary>
        /// <param name="t">Type of the default value.</param>
        /// <returns>Default value for the give type.</returns>
        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }
    }
}