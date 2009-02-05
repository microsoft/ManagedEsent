//-----------------------------------------------------------------------
// <copyright file="ExceptionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the exception classes.
    /// </summary>
    [TestClass]
    public class ExceptionTests
    {
        /// <summary>
        /// Verify that the error passed into the constructor is set in the error
        /// property.
        /// </summary>
        [TestMethod]
        public void VerifyEsentErrorExceptionConstructorSetsError()
        {
            EsentErrorException ex = new EsentErrorException(JET_err.AccessDenied);

            Assert.AreEqual(JET_err.AccessDenied, ex.Error);
        }

        /// <summary>
        /// Verify that an EsentErrorException can be serialized and deserialized.
        /// </summary>
        [TestMethod]
        public void VerifyEsentErrorExceptionSerializationPreservesError()
        {
            EsentErrorException originalException = new EsentErrorException(JET_err.VersionStoreOutOfMemory);

            MemoryStream stream = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, originalException);

            stream.Position = 0; // rewind

            EsentErrorException deserializedException = (EsentErrorException)formatter.Deserialize(stream);
            Assert.AreEqual(originalException.Error, deserializedException.Error);
        }
    }
}