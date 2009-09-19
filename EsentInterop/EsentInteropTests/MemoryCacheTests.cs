//-----------------------------------------------------------------------
// <copyright file="MemoryCacheTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the methods of the MemoryCache class
    /// </summary>
    [TestClass]
    public class MemoryCacheTests
    {
        /// <summary>
        /// The MemoryCache object being tested.
        /// </summary>
        private MemoryCache memoryCache;

        /// <summary>
        /// Initializes the fixture by creating a MemoryCache object.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.memoryCache = new MemoryCache();           
        }

        /// <summary>
        /// Allocating a buffer should give a non-null result.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAllocateDoesNotReturnNull()
        {
            Assert.IsNotNull(this.memoryCache.Allocate());
        }

        /// <summary>
        /// An allocated buffer must be a reasonable size
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAllocatedBufferIsLargeEnough()
        {
            byte[] buffer = this.memoryCache.Allocate();
            Assert.IsTrue(buffer.Length > 32);
        }

        /// <summary>
        /// Allocating a buffer, freeing it and reallocating should
        /// give back the same buffer.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAllocationLocality()
        {
            byte[] buffer = this.memoryCache.Allocate();
            this.memoryCache.Free(buffer);
            Assert.AreEqual(buffer, this.memoryCache.Allocate());
        }

        /// <summary>
        /// A short (in this case zero-length) buffer should not be cached.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyZeroLengthBufferIsNotCached()
        {
            this.memoryCache.Free(new byte[0]);
            byte[] buffer = this.memoryCache.Allocate();
            Assert.IsTrue(buffer.Length > 0);
        }

        /// <summary>
        /// We don't want to keep very large buffers alive. Make sure a
        /// huge buffer isn't cached.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyHugeBufferIsNotCached()
        {
            var hugeBuffer = new byte[10 * 1024 * 1024];
            this.memoryCache.Free(hugeBuffer);
            byte[] buffer = this.memoryCache.Allocate();
            Assert.AreNotEqual(buffer, hugeBuffer);
        }
    }
}