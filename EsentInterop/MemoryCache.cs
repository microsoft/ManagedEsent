//-----------------------------------------------------------------------
// <copyright file="MemoryCache.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System.Threading;

    /// <summary>
    /// Cache allocated chunks of memory that are needed for very short periods
    /// of time. The memory is not zeroed on allocation.
    /// </summary>
    internal sealed class MemoryCache
    {
        /// <summary>
        /// Default size for newly allocated buffers.
        /// </summary>
        private const int DefaultBufferSize = 32 * 1024;

        /// <summary>
        /// Maximum buffer size to cache.
        /// </summary>
        private const int MaxBufferSize = 64 * 1024;

        /// <summary>
        /// Currently cached buffer.
        /// </summary>
        private byte[] cachedBuffer;

        /// <summary>
        /// Allocates a chunk of memory. If memory is cached it is returned. If no memory
        /// is cached then it is allocated. Check the size of the returned buffer to determine
        /// how much memory was allocated.
        /// </summary>
        /// <returns>A new memory buffer.</returns>
        public byte[] Allocate()
        {
            return Interlocked.Exchange(ref this.cachedBuffer, null) ?? new byte[DefaultBufferSize];
        }

        /// <summary>
        /// Frees an unused buffer. This may be added to the cache.
        /// </summary>
        /// <param name="data">The memory to free.</param>
        public void Free(byte[] data)
        {
            if (data.Length >= DefaultBufferSize && data.Length <= MaxBufferSize)
            {
                Interlocked.CompareExchange(ref this.cachedBuffer, data, null);                
            }
        }
    }
}