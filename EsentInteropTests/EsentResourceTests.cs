//-----------------------------------------------------------------------
// <copyright file="EsentResourceTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the EsentResource class
    /// </summary>
    [TestClass]
    public class EsentResourceTests
    {
        /// <summary>
        /// Check that disposing the object frees the resource.
        /// </summary>
        [TestMethod]
        public void EsentResourceDisposeReleasesResource()
        {
            MockEsesntResource saved = null;
            using (MockEsesntResource r = new MockEsesntResource())
            {
                saved = r;
                r.Open();
            }

            Assert.IsTrue(saved.WasReleaseResourceCalled);
        }

        /// <summary>
        /// Check that disposing the object twice works.
        /// </summary>
        [TestMethod]
        public void EsentResourceDisposeTwice()
        {
            MockEsesntResource r = new MockEsesntResource();
            r.Open();
            r.Dispose();
            r.Dispose();
            Assert.IsTrue(r.WasReleaseResourceCalled);
        }

        /// <summary>
        /// Check that disposing the object does not free a
        /// resource that was never opened.
        /// </summary>
        [TestMethod]
        public void EsentResourceDisposeDoesNotFreeUnopenedResource()
        {
            MockEsesntResource saved = null;
            using (MockEsesntResource r = new MockEsesntResource())
            {
                saved = r;
            }

            Assert.IsFalse(saved.WasReleaseResourceCalled);
        }

        /// <summary>
        /// Check that disposing the object does not free a closed
        /// resource.
        /// </summary>
        [TestMethod]
        public void EsentResourceDisposeDoesNotFreeClosedResource()
        {
            MockEsesntResource saved = null;
            using (MockEsesntResource r = new MockEsesntResource())
            {
                saved = r;
                r.Open();
                r.Close();
            }

            Assert.IsFalse(saved.WasReleaseResourceCalled);
        }

        /// <summary>
        /// Check that using a disposed object generates an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void EsentResourceDisposedObjectThrowsException()
        {
            MockEsesntResource r = new MockEsesntResource();
            r.Open();
            r.Dispose();
            r.Open();
        }

        /// <summary>
        /// Mock object that inherits from the EsentResource class.
        /// </summary>
        private class MockEsesntResource : EsentResource
        {
            /// <summary>
            /// Gets a value indicating whether the internal ReleaseResource method 
            /// was called.
            /// </summary>
            public bool WasReleaseResourceCalled { get; private set; }

            /// <summary>
            /// Performs a fake resource allocation.
            /// </summary>
            public void Open()
            {
                this.CheckObjectIsNotDisposed();
                this.ResourceWasAllocated();
            }

            /// <summary>
            /// Performs a fake resource free.
            /// </summary>
            public void Close()
            {
                this.CheckObjectIsNotDisposed();
                this.ResourceWasReleased();
            }

            /// <summary>
            /// Release the underlying resource.
            /// </summary>
            protected override void ReleaseResource()
            {
                this.WasReleaseResourceCalled = true;
                this.ResourceWasReleased();
            }
        }
    }
}