//-----------------------------------------------------------------------
// <copyright file="SignatureTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JET_SIGNATURE
    /// </summary>
    [TestClass]
    public class SignatureTests
    {
        /// <summary>
        /// Test constructing a JET_SIGNATURE from a NATIVE_SIGNATURE.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test constructing a JET_SIGNATURE from a NATIVE_SIGNATURE")]
        public void CreateJetSignatureFromNativeSignature()
        {
            var t = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var native = new NATIVE_SIGNATURE
            {
                ulRandom = 9,
                logtimeCreate = new JET_LOGTIME(t),
            };

            native.szComputerName = "COMPUTER";

            var expected = new JET_SIGNATURE(9, t, "COMPUTER");
            var actual = new JET_SIGNATURE(native);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test constructing a NATIVE_SIGNATURE from a JET_SIGNATURE.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test constructing a NATIVE_SIGNATURE from a JET_SIGNATURE")]
        public void CreateNativeSignatureFromJetSignature()
        {
            var time = new DateTime(2037, 10, 29, 02, 00, 00, DateTimeKind.Utc);

            var expected = new JET_SIGNATURE(37, time, "retupmoc");

            var native = expected.GetNativeSignature();
            var actual = new JET_SIGNATURE(native);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(expected.Equals(actual));
        }

        /// <summary>
        /// Test JET_SIGNATURE serialization to a byte[].
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_SIGNATURE serialization to a byte[].")]
        public void JetSignatureSerialization()
        {
            var time = new DateTime(2037, 10, 29, 02, 00, 00, DateTimeKind.Utc);

            var expected = new JET_SIGNATURE(37, time, "MaxStringLength");

            var bytes = expected.ToBytes();
            var actual = new JET_SIGNATURE(bytes);

            Assert.IsTrue(expected == actual);
            Assert.IsTrue(bytes.Length == 28);
            Assert.IsTrue(bytes[bytes.Length - 1] == 0);

            for (int i = 0; i < bytes.Length * 8; i++)
            {
                bytes[i / 8] ^= (byte)(1 << (i % 8));

                Assert.IsTrue(expected != new JET_SIGNATURE(bytes) || i >= (bytes.Length - 1) * 8);

                bytes[i / 8] ^= (byte)(1 << (i % 8));
            }

            for (int i = bytes.Length; i >= 0; i--)
            {
                Array.Resize(ref bytes, i);
                try
                {
                    Assert.IsTrue(expected == new JET_SIGNATURE(bytes));
                    Assert.IsTrue(i >= 28);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    Assert.IsTrue(i < 28);
                }
            }
        }
    }
}