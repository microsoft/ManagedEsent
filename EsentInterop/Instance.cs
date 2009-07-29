//-----------------------------------------------------------------------
// <copyright file="Instance.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// A class that encapsulates a <see cref="JET_INSTANCE"/> in a disposable object. The
    /// instance must be closed last and closing the instance releases all other
    /// resources for the instance.
    /// </summary>
    public class Instance : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Parameters for the instance.
        /// </summary>
        private readonly InstanceParameters parameters;

        /// <summary>
        /// Initializes a new instance of the Instance class. The underlying
        /// JET_INSTANCE is allocated, but not initialized.
        /// </summary>
        /// <param name="name">
        /// The name of the instance. This string must be unique within a
        /// given process hosting the database engine
        /// </param>
        public Instance(string name) : this(name, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Instance class. The underlying
        /// JET_INSTANCE is allocated, but not initialized.
        /// </summary>
        /// <param name="name">
        /// The name of the instance. This string must be unique within a
        /// given process hosting the database engine
        /// </param>
        /// <param name="displayName">
        /// A display name for the instance. This will be used in eventlog
        /// entries.
        /// </param>
        public Instance(string name, string displayName) : base(true)
        {
            JET_INSTANCE instance;
            Api.JetCreateInstance2(out instance, name, displayName, CreateInstanceGrbit.None);
            this.SetHandle(instance.Value);
            this.parameters = new InstanceParameters(instance);
        }

        /// <summary>
        /// Gets the JET_INSTANCE that this instance contains.
        /// </summary>
        public JET_INSTANCE JetInstance
        {
            get
            {
                this.CheckObjectIsNotDisposed();
                return this.CreateInstanceFromHandle();
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
        }

        /// <summary>
        /// Provide implicit conversion of an Instance object to a JET_INSTANCE
        /// structure. This is done so that an Instance can be used anywhere a
        /// JET_INSTANCE is required.
        /// </summary>
        /// <param name="instance">The instance to convert.</param>
        /// <returns>The JET_INSTANCE wrapped by the instance.</returns>
        public static implicit operator JET_INSTANCE(Instance instance)
        {
            return instance.JetInstance;
        }

        /// <summary>
        /// Initialize the JET_INSTANCE.
        /// </summary>
        public void Init()
        {
            this.Init(InitGrbit.None);
        }

        /// <summary>
        /// Initialize the JET_INSTANCE.
        /// </summary>
        /// <param name="grbit">
        /// Initialization options.
        /// </param>
        public void Init(InitGrbit grbit)
        {
            this.CheckObjectIsNotDisposed();
            JET_INSTANCE instance = this.JetInstance;
            try
            {
                // Remember that a failure in JetInit can zero the handle
                // and that JetTerm should not be called in that case.
                Api.JetInit2(ref instance, grbit);
            }
            finally
            {
                this.SetHandle(instance.Value);
            }
        }

        /// <summary>
        /// Terminate the JET_INSTANCE.
        /// </summary>
        public void Term()
        {
            Api.JetTerm(this.JetInstance);
            this.SetHandleAsInvalid();
        }

        /// <summary>
        /// Release the handle for this instance.
        /// </summary>
        /// <returns>True if the handle could be released.</returns>
        protected override bool ReleaseHandle()
        {
            // The object is already marked as invalid so don't check
            var instance = this.CreateInstanceFromHandle();
            return ((int) JET_err.Success == Api.Impl.JetTerm(instance));
        }

        /// <summary>
        /// Create a JET_INSTANCE from the internal handle value.
        /// </summary>
        /// <returns>A JET_INSTANCE containing the internal handle</returns>
        private JET_INSTANCE CreateInstanceFromHandle()
        {
            return new JET_INSTANCE { Value = this.handle };
        }

        /// <summary>
        /// Check to see if this instance is invalid or closed.
        /// </summary>
        private void CheckObjectIsNotDisposed()
        {
            if (this.IsInvalid || this.IsClosed)
            {
                throw new ObjectDisposedException("Instance");
            }
        }
    }
}