//-----------------------------------------------------------------------
// <copyright file="Instance.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;

    /// <summary>
    /// A class that encapsulated a JET_INSTANCE in a disposable object.
    /// </summary>
    public class Instance : EsentResource
    {
        /// <summary>
        /// The underlying JET_INSTANCE.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// Parameters for the instance.
        /// </summary>
        private InstanceParameters parameters;

        /// <summary>
        /// Initializes a new instance of the Instance class. The underlying
        /// JET_INSTANCE is allocated, but not initialized.
        /// </summary>
        /// <param name="name">The name of the instance.</param>
        public Instance(string name)
        {
            Api.JetCreateInstance(out this.instance, name);
            this.ResourceWasAllocated();
            this.Parameters = new InstanceParameters(this.instance);
        }

        /// <summary>
        /// Gets the JET_INSTANCE that this instance contains.
        /// </summary>
        public JET_INSTANCE JetInstance
        {
            get
            {
                this.CheckObjectIsNotDisposed();
                return this.instance;
            }

            private set
            {
                this.CheckObjectIsNotDisposed();
                this.instance = value;
            }
        }

        /// <summary>
        /// Gets the InstanceParameters for this instance. 
        /// </summary>
        public InstanceParameters Parameters
        {
            get
            {
                this.CheckObjectIsNotDisposed();
                return this.parameters;
            }

            private set
            {
                this.CheckObjectIsNotDisposed();
                this.parameters = value;
            }
        }

        /// <summary>
        /// Initialize the JET_INSTANCE.
        /// </summary>
        public void Init()
        {
            this.CheckObjectIsNotDisposed();
            Api.JetInit(ref this.instance);
        }

        /// <summary>
        /// Terminate the JET_INSTANCE.
        /// </summary>
        public void Term()
        {
            this.CheckObjectIsNotDisposed();
            this.ReleaseResource();
        }

        /// <summary>
        /// Free the underlying JET_INSTANCE
        /// </summary>
        protected override void ReleaseResource()
        {
            Api.JetTerm(this.instance);
            this.ResourceWasReleased();
        }
    }
}