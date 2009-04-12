//-----------------------------------------------------------------------
// <copyright file="Instance.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// A class that encapsulates a JET_INSTANCE in a disposable object.
    /// </summary>
    public class Instance : EsentResource
    {
        /// <summary>
        /// Parameters for the instance.
        /// </summary>
        private InstanceParameters parameters;

        /// <summary>
        /// The underlying JET_INSTANCE.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// Initializes a new instance of the Instance class. The underlying
        /// JET_INSTANCE is allocated, but not initialized.
        /// </summary>
        /// <param name="name">
        /// The name of the instance. This string must be unique within a
        /// given process hosting the database engine
        /// </param>
        public Instance(string name)
        {
            Api.JetCreateInstance(out this.instance, name);
            this.ResourceWasAllocated();
            this.parameters = new InstanceParameters(this.instance);
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
        public Instance(string name, string displayName)
        {
            Api.JetCreateInstance2(out this.instance, name, displayName, CreateInstanceGrbit.None);
            this.ResourceWasAllocated();
            this.parameters = new InstanceParameters(this.instance);
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
            this.instance = JET_INSTANCE.Nil;
            this.parameters = null;
            this.ResourceWasReleased();
        }
    }
}