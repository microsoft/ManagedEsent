//-----------------------------------------------------------------------
// <copyright file="ExceptionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
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
        [Priority(0)]
        public void VerifyEsentErrorExceptionConstructorSetsError()
        {
            var ex = new EsentErrorException(JET_err.AccessDenied);

            Assert.AreEqual(JET_err.AccessDenied, ex.Error);
        }

        /// <summary>
        /// Verify that an EsentErrorException can be serialized and deserialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyEsentErrorExceptionSerializationPreservesError()
        {
            var originalException = new EsentErrorException(JET_err.VersionStoreOutOfMemory);

            var stream = new MemoryStream();

            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, originalException);

            stream.Position = 0; // rewind

            var deserializedException = (EsentErrorException)formatter.Deserialize(stream);
            Assert.AreEqual(originalException.Error, deserializedException.Error);
        }
    }
}