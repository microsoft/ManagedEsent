//-----------------------------------------------------------------------
// <copyright file="InstanceTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace InteropApiTests
{
    /// <summary>
    /// Test the disposable Instance class, which wraps a JET_INSTANCE.
    /// </summary>
    [TestClass]
    public class InstanceTests
    {
        /// <summary>
        /// Allocate an instance, but don't initialize it.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void CreateInstanceNoInit()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                Assert.AreNotEqual(JET_INSTANCE.Nil, instance.JetInstance);
                Assert.IsNotNull(instance.Parameters);

                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Test implicit conversion to a JET_INSTANCE
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void InstanceCanConvertToJetInstance()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                JET_INSTANCE jetinstance = instance;
                Assert.AreEqual(jetinstance, instance.JetInstance);
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// When JetCreateInstance2 fails the instance isn't initialized
        /// so it shouldn't be freed.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyInstanceDoesNotCallJetTermWhenCreateInstanceFails()
        {
            var mocks = new MockRepository();
            var mockApi = mocks.StrictMock<IJetApi>();
            using (new ApiTestHook(mockApi))
            {
                Expect.Call(
                    mockApi.JetCreateInstance2(
                        out Arg<JET_INSTANCE>.Out(JET_INSTANCE.Nil).Dummy,
                        Arg<string>.Is.Anything,
                        Arg<string>.Is.Anything,
                        Arg<CreateInstanceGrbit>.Is.Anything))
                    .Return((int) JET_err.InvalidName);
                mocks.ReplayAll();

                try
                {
                    using (var instance = new Instance("test"))
                    {
                        Assert.Fail("Expected an EsentErrorException");
                    }
                }
                catch (EsentErrorException)
                {
                    // expected
                }

                mocks.VerifyAll();
            }
        }

        /// <summary>
        /// When JetCreateInstance2 fails the instance isn't initialized
        /// so it shouldn't be freed.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyInstanceDoesNotCallJetTermWhenJetInitFails()
        {
            var mocks = new MockRepository();
            var mockApi = mocks.StrictMock<IJetApi>();
            using (new ApiTestHook(mockApi))
            {
                var jetInstance = new JET_INSTANCE { Value = (IntPtr) 0x1 };

                Expect.Call(
                    mockApi.JetCreateInstance2(
                        out Arg<JET_INSTANCE>.Out(jetInstance).Dummy,
                        Arg<string>.Is.Anything,
                        Arg<string>.Is.Anything,
                        Arg<CreateInstanceGrbit>.Is.Anything))
                    .Return((int) JET_err.Success);
                Expect.Call(
                    mockApi.JetInit(ref Arg<JET_INSTANCE>.Ref(Is.Equal(jetInstance), JET_INSTANCE.Nil).Dummy))
                    .Return((int) JET_err.OutOfMemory);
                mocks.ReplayAll();

                try
                {
                    using (var instance = new Instance("test"))
                    {
                        instance.Init();
                        Assert.Fail("Expected an EsentErrorException");
                    }
                }
                catch (EsentErrorException)
                {
                    // expected
                }

                mocks.VerifyAll();
            }
        }

        /// <summary>
        /// Allocate an instance and initialize it.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateInstanceInit()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
                instance.Parameters.NoInformationEvent = true;
                instance.Init();
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Allocate an instance with a display name.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void CreateInstanceWithDisplayName()
        {
            using (var instance = new Instance(Guid.NewGuid().ToString(), "Friendly Display Name"))
            {
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.Recovery = false;
                instance.Init();
            }
        }

        /// <summary>
        /// Allocate an instance and initialize it and then terminate.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateInstanceInitTerm()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            using (var instance = new Instance("theinstance"))
            {
                instance.Parameters.LogFileDirectory = dir;
                instance.Parameters.SystemDirectory = dir;
                instance.Parameters.TempDirectory = dir;
                instance.Parameters.NoInformationEvent = true;
                instance.Init();
                instance.Term();
                Directory.Delete(dir, true);    // only works if the instance is terminated
            }
        }

        /// <summary>
        /// Make sure that garbage collection can close an instance
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyInstanceCanBeFinalized()
        {
            for (int i = 0; i < 3; ++i)
            {
                // If finalization doesn't close the instance then subseqent 
                // creation attempts will fail
                CreateOneInstance();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Make sure that accessing the instance of a closed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetInstanceThrowsExceptionWhenInstanceIsClosed()
        {
            var instance = new Instance("theinstance");
            instance.Parameters.NoInformationEvent = true;
            instance.Parameters.Recovery = false;
            instance.Parameters.MaxTemporaryTables = 0;
            instance.Init();
            instance.Term();
            JET_INSTANCE x = instance.JetInstance;
        }

        /// <summary>
        /// Make sure that accessing the instance of a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetInstanceThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            JET_INSTANCE x = instance.JetInstance;
        }

        /// <summary>
        /// Make sure that accessing the parameters of a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ParametersThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            InstanceParameters x = instance.Parameters;
        }

        /// <summary>
        /// Make sure that calling Init on a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void InitThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            instance.Init();
        }

        /// <summary>
        /// Make sure that calling Term on a disposed object throws an
        /// exception.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TermThrowsExceptionWhenInstanceIsDisposed()
        {
            var instance = new Instance("theinstance");
            instance.Dispose();
            instance.Term();
        }

        /// <summary>
        /// Create an instance and abandon it. Garbage collection should
        /// be able to finalize the instance.
        /// </summary>
        private static void CreateOneInstance()
        {
            var instance = new Instance("finalize_me");
            instance.Parameters.NoInformationEvent = true;
            instance.Parameters.Recovery = false;
            instance.Parameters.MaxTemporaryTables = 0;
            instance.Init();
        }
    }
}