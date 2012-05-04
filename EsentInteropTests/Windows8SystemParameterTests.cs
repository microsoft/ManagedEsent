//-----------------------------------------------------------------------
// <copyright file="Windows8SystemParameterTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#if !MANAGEDESENT_ON_CORECLR
    using Rhino.Mocks;
#endif

    /// <summary>
    /// Test the SystemParameters class. To avoid changing global parameters
    /// this is tested with a mock IJetApi.
    /// </summary>
    public partial class SystemParameterTests
    {
#if !MANAGEDESENT_ON_CORECLR
        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify SystemParameters.MinDataForXpress sets Windows8Param.MinDataForXpress")]
        public void VerifySettingMinDataForXpress()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, Windows8Param.MinDataForXpress, new IntPtr(70), null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.MinDataForXpress = 70;
            this.repository.VerifyAll();
        }        
        
        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify SystemParameters.HungIOThreshold sets Windows8Param.HungIOThreshold")]
        public void VerifySettingHungIOThreshold()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, Windows8Param.HungIOThreshold, new IntPtr(71), null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.HungIOThreshold = 71;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify SystemParameters.HungIOActions sets Windows8Param.HungIOActions")]
        public void VerifySettingHungIOActions()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, Windows8Param.HungIOActions, new IntPtr(72), null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.HungIOActions = 72;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify SystemParameters.ProcessFriendlyName sets Windows8Param.ProcessFriendlyName")]
        public void VerifySettingProcessFriendlyName()
        {
            string processFriendlyName = "AProcessFriendlyName";

            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, Windows8Param.ProcessFriendlyName, new IntPtr(0), processFriendlyName)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.ProcessFriendlyName = processFriendlyName;
            this.repository.VerifyAll();
        }
#endif // !MANAGEDESENT_ON_CORECLR
    }
}