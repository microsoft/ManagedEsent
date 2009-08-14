//-----------------------------------------------------------------------
// <copyright file="ExceptionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Isam.Esent;
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
        /// Verify that creating an EsentException with a message sets the message.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyEsentExceptionConstructorSetsMessage()
        {
            var ex = new EsentException("hello");
            Assert.AreEqual("hello", ex.Message);
        }

        /// <summary>
        /// Verify that creating an EsentException with an innner exception sets
        /// the inner exception property.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyEsentExceptionConstructorSetsInnerException()
        {
            var ex = new EsentException("foo", new OutOfMemoryException("InnerException"));
            Assert.AreEqual("InnerException", ex.InnerException.Message);
        }

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
        /// Verify that the error passed into the constructor is set in the error
        /// property.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyEsentErrorExceptionHasMessage()
        {
            var ex = new EsentErrorException(JET_err.AccessDenied);
            Assert.IsNotNull(ex.Message);
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

        /// <summary>
        /// Verify that an EsentInvalidColumnException can be serialized and deserialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyEsentInvalidColumnExceptionSerializationPreservesMessage()
        {
            var originalException = new EsentInvalidColumnException();

            var stream = new MemoryStream();

            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, originalException);

            stream.Position = 0; // rewind

            var deserializedException = (EsentInvalidColumnException)formatter.Deserialize(stream);
            Assert.AreEqual(originalException.Message, deserializedException.Message);
        }
    }
}