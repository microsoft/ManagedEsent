//-----------------------------------------------------------------------
// <copyright file="EsentResource.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// This is the base class for all esent resource objects.
    /// Subclasses of this class can allocate and release unmanaged
    /// resources.
    /// </summary>
    public abstract class EsentResource : IDisposable
    {
        private bool hasResource;
        private bool isDisposed;

        /// <summary>
        /// Finalizes an instance of the EsentResource class.
        /// </summary>
        ~EsentResource()
        {
            if (this.hasResource)
            {
                // We should not get to this point. The problem is that if
                // we use finalizers to free esent resources they may end
                // up being freed in the wrong order (e.g. JetEndSession is
                // called before JetCloseTable). Freeing esent resources
                // in the wrong order will generate EsentExceptions.
                throw new InvalidOperationException("EsentResource was not disposed");
            }
        }

        /// <summary>
        /// Dispose of this object, releasing the underlying
        /// Esent resource.
        /// </summary>
        public void Dispose()
        {
            if (this.hasResource)
            {
                this.ReleaseResource();
                Debug.Assert(!this.hasResource, "Resource was not freed");
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Throw an exception if this object has been disposed.
        /// </summary>
        protected void CheckObjectIsNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("EsentResource");
            }
        }

        /// <summary>
        /// Called by a subclass when a resource is allocated.
        /// </summary>
        protected void ResourceWasAllocated()
        {
            this.CheckObjectIsNotDisposed();
            this.hasResource = true;
        }

        /// <summary>
        /// Called by a subclass when a resource is freed.
        /// </summary>
        protected void ResourceWasReleased()
        {
            this.CheckObjectIsNotDisposed();
            this.hasResource = false;
        }

        /// <summary>
        /// Implemented by the subclass to release a resource.
        /// </summary>
        protected abstract void ReleaseResource();
    }
}
