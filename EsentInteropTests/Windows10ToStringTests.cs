//-----------------------------------------------------------------------
// <copyright file="Windows10ToStringTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Globalization;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing the ToString methods of the basic types.
    /// </summary>
    public partial class ToStringTests
    {
        /// <summary>
        /// Test JET_OPERATIONCONTEXT.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_OPERATIONCONTEXT.ToString()")]
        public void JetOperationContextToString()
        {
            var operationContext = new JET_OPERATIONCONTEXT()
            {
                UserID = 2,
                OperationID = 3,
                OperationType = 4,
                ClientType = 5,
                Flags = 6,
            };

            Assert.AreEqual("JET_OPERATIONCONTEXT(2:3:4:5:0x06)", operationContext.ToString());
        }
    }
}