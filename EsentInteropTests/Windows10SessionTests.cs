//-----------------------------------------------------------------------
// <copyright file="Windows10SessionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;

    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the Windows 10 extensions to <see cref="Session"/>.
    /// </summary>
    public partial class SessionTests
    {
        /// <summary>
        /// Verifies Windows10.GetTransactionLevel().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies Windows10.GetTransactionLevel().")]
        public void VerifyWindows10GetTransactionLevel()
        {
            if (EsentVersion.SupportsWindows10Features)
            {
                using (var session = new Session(this.instance))
                {
                    Assert.AreEqual(0, session.GetTransactionLevel());
                    Api.JetBeginTransaction(session.JetSesid);
                    Assert.AreEqual(1, session.GetTransactionLevel());
                    Api.JetCommitTransaction(session.JetSesid, CommitTransactionGrbit.None);
                    Assert.AreEqual(0, session.GetTransactionLevel());
                }
            }
            else
            {
                // Functionality not supported.
                if (EsentVersion.SupportsWindows8Features)
                {
                    // Win8/Win81
                    try
                    {
                        using (var session = new Session(this.instance))
                        {
                            Assert.AreEqual(0, session.GetTransactionLevel());
                        }
                    }
                    catch (EsentInvalidParameterException)
                    {
                        return;
                    }
                }
                else
                {
                    // Win7 and below
                    try
                    {
                        using (var session = new Session(this.instance))
                        {
                            Assert.AreEqual(0, session.GetTransactionLevel());
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Verifies Windows10.GetCorrelationID and Windows10.SetCorrelationID.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies Windows10.GetCorrelationID and Windows10.SetCorrelationID.")]
        public void VerifyWindows10GetCorrelationIDAndSetCorrelationID()
        {
            if (EsentVersion.SupportsWindows10Features)
            {
                using (var session = new Session(this.instance))
                {
                    Assert.AreEqual(0, session.GetCorrelationID());
                    int anyInt = Any.Int32;

                    session.SetCorrelationID(anyInt);
                    Assert.AreEqual(anyInt, session.GetCorrelationID());
                }

                using (var session = new Session(this.instance))
                {
                    // A new session shouldn't re-use the old value.
                    Assert.AreEqual(0, session.GetCorrelationID());
                }
            }
            else
            {
                // Functionality not supported.
                if (EsentVersion.SupportsWindows8Features)
                {
                    // Win8/Win81
                    try
                    {
                        using (var session = new Session(this.instance))
                        {
                            // A new session shouldn't re-use the old value.
                            Assert.AreEqual(0, session.GetCorrelationID());
                        }
                    }
                    catch (EsentInvalidParameterException)
                    {
                        return;
                    }
                }
                else
                {
                    // Win7 and below
                    try
                    {
                        using (var session = new Session(this.instance))
                        {
                            // A new session shouldn't re-use the old value.
                            Assert.AreEqual(0, session.GetCorrelationID());
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Verifies Windows10.GetOperationContext is empty.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies Windows10.GetOperationContext is empty.")]
        public void VerifyWindows10GetOperationContextIsEmpty()
        {
            if (!EsentVersion.SupportsWindows10Features)
            {
                return;
            }

            using (var session = new Session(this.instance))
            {
                JET_OPERATIONCONTEXT operationcontext = session.GetOperationContext();

                Assert.AreEqual(0, operationcontext.UserID);
                Assert.AreEqual(0, operationcontext.OperationID);
                Assert.AreEqual(0, operationcontext.OperationType);
                Assert.AreEqual(0, operationcontext.ClientType);
                Assert.AreEqual(0, operationcontext.Flags);
            }
        }

        /// <summary>
        /// Verifies Windows10.GetOperationContext and Windows10.SetOperationContext.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies Windows10.GetOperationContext and Windows10.SetOperationContext.")]
        public void VerifyWindows10GetOperationContextAndSetOperationContext()
        {
            if (!EsentVersion.SupportsWindows10Features)
            {
                return;
            }

            using (var session = new Session(this.instance))
            {
                JET_OPERATIONCONTEXT operationcontext = new JET_OPERATIONCONTEXT()
                {
                    UserID = 2,
                    OperationID = 3,
                    OperationType = 4,
                    ClientType = 5,
                    Flags = 6,
                };

                session.SetOperationContext(operationcontext);
                var retrieved = session.GetOperationContext();

                Assert.AreEqual(operationcontext.UserID, retrieved.UserID);
                Assert.AreEqual(operationcontext.OperationID, retrieved.OperationID);
                Assert.AreEqual(operationcontext.OperationType, retrieved.OperationType);
                Assert.AreEqual(operationcontext.ClientType, retrieved.ClientType);
                Assert.AreEqual(operationcontext.Flags, retrieved.Flags);
            }

            using (var session = new Session(this.instance))
            {
                // A new session shouldn't re-use the old value.
                var retrieved = session.GetOperationContext();

                Assert.AreEqual(0, retrieved.UserID);
                Assert.AreEqual(0, retrieved.OperationID);
                Assert.AreEqual(0, retrieved.OperationType);
                Assert.AreEqual(0, retrieved.ClientType);
                Assert.AreEqual(0, retrieved.Flags);
            }
        }
    }
}