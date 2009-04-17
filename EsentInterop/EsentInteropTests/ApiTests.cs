//-----------------------------------------------------------------------
// <copyright file="ApiTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace InteropApiTests
{
    /// <summary>
    /// Test the Api class functionality which wraps the IJetApi
    /// implementation.
    /// </summary>
    [TestClass]
    public class ApiTests
    {
        /// <summary>
        /// Mock object repository.
        /// </summary>
        private MockRepository mocks;

        /// <summary>
        /// The saved API, replaced when finished.
        /// </summary>
        private IJetApi savedImpl;

        /// <summary>
        /// Setup the mock object repository.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.savedImpl = Api.Impl;
            this.mocks = new MockRepository();
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.Impl = this.savedImpl;
        }

        /// <summary>
        /// Verify that the internal IJetApi has default implementation.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyApiHasDefaultImplementation()
        {
            Assert.IsNotNull(Api.Impl);
            Assert.IsInstanceOfType(Api.Impl, typeof(JetApi));
        }

        /// <summary>
        /// Verify that the internal IJetApi can be substituted with a different
        /// object.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyJetApiImplementationCanBeChanged()
        {
            var jetApi = this.mocks.StrictMock<IJetApi>();
            Api.Impl = jetApi;

            Expect.Call(
                jetApi.JetSetCurrentIndex(JET_SESID.Nil, JET_TABLEID.Nil, String.Empty))
                .IgnoreArguments()
                .Return(0);
            this.mocks.ReplayAll();

            Api.JetSetCurrentIndex(JET_SESID.Nil, JET_TABLEID.Nil, Any.String);

            this.mocks.VerifyAll();
        }

        /// <summary>
        /// Verify that an error returned from the IJetApi implementation
        /// causes an exception to be thrown.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(EsentErrorException))]
        public void VerifyErrorFromJetApiImplementationGeneratesException()
        {
            var jetApi = this.mocks.Stub<IJetApi>();
            Api.Impl = jetApi;

            SetupResult.For(
                jetApi.JetTerm(JET_INSTANCE.Nil))
                .IgnoreArguments()
                .Return((int)JET_err.OutOfMemory);
            this.mocks.ReplayAll();

            Api.JetTerm(JET_INSTANCE.Nil);
        }

        /// <summary>
        /// Verify that the ExceptionHandler event is invoked when an exception is
        /// generated.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyExceptionHandlerIsInvokedOnException()
        {
            var jetApi = this.mocks.Stub<IJetApi>();
            Api.Impl = jetApi;

            SetupResult.For(
                jetApi.JetBeginTransaction(JET_SESID.Nil))
                .IgnoreArguments()
                .Return((int)JET_err.TransTooDeep);
            this.mocks.ReplayAll();

            bool eventWasCalled = false;
            Api.ExceptionHandler handler = ex =>
                {
                    eventWasCalled = true;
                    return ex;
                };

            try
            {
                Api.HandleException += handler;
                Api.JetBeginTransaction(JET_SESID.Nil);
            }
            catch (EsentErrorException)
            {
            }
            Api.HandleException -= handler;
            Assert.IsTrue(eventWasCalled);
        }

        /// <summary>
        /// Verify that the ExceptionHandler event can wrap exceptions.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyExceptionHandlerCanWrapExceptions()
        {
            var jetApi = this.mocks.Stub<IJetApi>();
            Api.Impl = jetApi;

            SetupResult.For(
                jetApi.JetBeginTransaction(JET_SESID.Nil))
                .IgnoreArguments()
                .Return((int)JET_err.TransTooDeep);
            this.mocks.ReplayAll();

            Api.ExceptionHandler handler = ex =>
                {
                    throw new InvalidOperationException("test", ex);
                };

            try
            {
                Api.HandleException += handler;
                Api.JetBeginTransaction(JET_SESID.Nil);
                Assert.Fail("Expected an invalid operation exception");
            }
            catch (InvalidOperationException)
            {
            }
            Api.HandleException -= handler;
        }
    }
}