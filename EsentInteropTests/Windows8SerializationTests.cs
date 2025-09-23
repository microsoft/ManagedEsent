//-----------------------------------------------------------------------
// <copyright file="Windows8SerializationTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for serialization/deserialization of objects.
    /// </summary>
    public partial class SerializationTests
    {
#if !MANAGEDESENT_ON_CORECLR
       /// <summary>
        /// Verify that an ErrorInfo can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an ErrorInfo can be serialized")]
        public void VerifyErrorInfoCanBeSerialized()
        {
            var expected = new JET_ERRINFOBASIC()
            {
                errValue = JET_err.ReadVerifyFailure,
                errcat = JET_ERRCAT.Corruption,
                rgCategoricalHierarchy = new JET_ERRCAT[] { JET_ERRCAT.Data, 0, 0, 0, 0, 0, 0, 0 },
                lSourceLine = 42,
                rgszSourceFile = "sourcefile.cxx",
            };

            var actual = SerializeDeserialize(expected);
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected.errValue, actual.errValue);
            Assert.AreEqual(expected.errcat, actual.errcat);
            Assert.AreEqual(expected.rgCategoricalHierarchy[0], actual.rgCategoricalHierarchy[0]);
            Assert.AreEqual(expected.rgCategoricalHierarchy[1], actual.rgCategoricalHierarchy[1]);
            Assert.AreEqual(expected.lSourceLine, actual.lSourceLine);
            Assert.AreEqual(expected.rgszSourceFile, actual.rgszSourceFile);
        }
#endif // !MANAGEDESENT_ON_CORECLR
    }
}