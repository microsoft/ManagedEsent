//-----------------------------------------------------------------------
// <copyright file="OperationContextConversionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.Runtime.InteropServices;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_OPERATIONCONTEXT conversion tests.
    /// </summary>
    [TestClass]
    public class OperationContextConversionTests
    {
        /// <summary>
        /// The native OPERATIONCONTEXT that will be converted to managed.
        /// </summary>
        private NATIVE_OPERATIONCONTEXT native;

        /// <summary>
        /// The managed OPERATIONCONTEXT created from the native.
        /// </summary>
        private JET_OPERATIONCONTEXT managed;

        /// <summary>
        /// The native OPERATIONCONTEXT that was reconstituted from managed.
        /// </summary>
        private NATIVE_OPERATIONCONTEXT reconstituted;

        /// <summary>
        /// Create a native OPERATIONCONTEXT and convert it to managed.
        /// </summary>
        [TestInitialize]
        [Description("Setup the OperationContextConversionTests test fixture")]
        public void Setup()
        {
            this.native = new NATIVE_OPERATIONCONTEXT()
            {
                ulUserID = Any.Int32,
                nOperationID = Any.Byte,
                nOperationType = Any.Byte,
                nClientType = Any.Byte,
                fFlags = Any.Byte,
            };

            this.managed = new JET_OPERATIONCONTEXT(ref this.native);
            this.reconstituted = this.managed.GetNativeOperationContext();
        }

        /// <summary>
        /// Test conversion from the native struct sets ulUserID.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation from NATIVE_OPERATIONCONTEXT to JET_OPERATIONCONTEXT sets ulUserID")]
        public void ConvertOperationContextFromNativeSetsUlUserID()
        {
            Assert.AreEqual(this.native.ulUserID, this.managed.UserID);
        }

        /// <summary>
        /// Test conversion to the native struct sets ulUserID.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation to NATIVE_OPERATIONCONTEXT from JET_OPERATIONCONTEXT sets ulUserID")]
        public void ConvertOperationContextToNativeSetsUlUserID()
        {
            Assert.AreEqual(this.managed.UserID, this.reconstituted.ulUserID);
        }

        /// <summary>
        /// Test conversion from the native struct sets nOperationID.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation from NATIVE_OPERATIONCONTEXT to JET_OPERATIONCONTEXT sets nOperationID")]
        public void ConvertOperationContextFromNativeSetsnOperationID()
        {
            Assert.AreEqual(this.native.nOperationID, this.managed.OperationID);
        }

        /// <summary>
        /// Test conversion to the native struct sets nOperationID.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation to NATIVE_OPERATIONCONTEXT from JET_OPERATIONCONTEXT sets nOperationID")]
        public void ConvertOperationContextToNativeSetsnOperationID()
        {
            Assert.AreEqual(this.managed.OperationID, this.reconstituted.nOperationID);
        }

        /// <summary>
        /// Test conversion from the native struct sets nOperationType.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation from NATIVE_OPERATIONCONTEXT to JET_OPERATIONCONTEXT sets nOperationType")]
        public void ConvertOperationContextFromNativeSetsnOperationType()
        {
            Assert.AreEqual(this.native.nOperationType, this.managed.OperationType);
        }

        /// <summary>
        /// Test conversion to the native struct sets nOperationType.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation to NATIVE_OPERATIONCONTEXT from JET_OPERATIONCONTEXT sets nOperationType")]
        public void ConvertOperationContextToNativeSetsnOperationType()
        {
            Assert.AreEqual(this.managed.OperationType, this.reconstituted.nOperationType);
        }

        /// <summary>
        /// Test conversion from the native struct sets nClientType.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation from NATIVE_OPERATIONCONTEXT to JET_OPERATIONCONTEXT sets nClientType")]
        public void ConvertOperationContextFromNativeSetnClientType()
        {
            Assert.AreEqual(this.native.nClientType, this.managed.ClientType);
        }

        /// <summary>
        /// Test conversion to the native struct sets nClientType.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation to NATIVE_OPERATIONCONTEXT from JET_OPERATIONCONTEXT sets nClientType")]
        public void ConvertOperationContextToNativeSetnClientType()
        {
            Assert.AreEqual(this.managed.ClientType, this.reconstituted.nClientType);
        }

        /// <summary>
        /// Test conversion from the native struct sets fFlags.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation from NATIVE_OPERATIONCONTEXT to JET_OPERATIONCONTEXT sets fFlags")]
        public void ConvertOperationContextFromNativeSetsfFlags()
        {
            Assert.AreEqual(this.native.fFlags, this.managed.Flags);
        }

        /// <summary>
        /// Test conversion to the native struct sets fFlags.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation to NATIVE_OPERATIONCONTEXT from JET_OPERATIONCONTEXT sets fFlags")]
        public void ConvertOperationContextToNativeSetsfFlags()
        {
            Assert.AreEqual(this.managed.Flags, this.reconstituted.fFlags);
        }
    }
}