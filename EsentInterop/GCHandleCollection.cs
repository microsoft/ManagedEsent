//-----------------------------------------------------------------------
// <copyright file="GCHandleCollection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Isam.Esent.Interop.Implementation
{
    /// <summary>
    /// A collection of GCHandles for pinned objects. The handles
    /// are freed when this object is disposed.
    /// </summary>
    internal class GCHandleCollection : IDisposable
    {
        /// <summary>
        /// The handles of the objects being pinned.
        /// </summary>
        private readonly List<GCHandle> handles;

        /// <summary>
        /// Initializes a new instance of the GCHandleCollection class.
        /// </summary>
        public GCHandleCollection()
        {
            this.handles = new List<GCHandle>();
        }

        /// <summary>
        /// Finalizes an instance of the GCHandleCollection class.
        /// </summary>
        ~GCHandleCollection()
        {
            this.Dispose(false);    
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Add an object to the handle collection. This automatically
        /// pins the object.
        /// </summary>
        /// <param name="value">The object to pin.</param>
        /// <returns>
        /// The address of the pinned object. This is valid until the
        /// GCHandleCollection is disposed.
        /// </returns>
        public IntPtr Add(object value)
        {
            if (null == value)
            {
                return IntPtr.Zero;
            }

            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            return handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Frees the allocated handles.
        /// </summary>
        /// <param name="disposing">True if called from dispose.</param>
        protected void Dispose(bool disposing)
        {
            foreach (GCHandle handle in this.handles)
            {
                handle.Free();
            }
        }
    }
}
