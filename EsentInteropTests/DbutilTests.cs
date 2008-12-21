//-----------------------------------------------------------------------
// <copyright file="DbutilTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Exchange.Isam.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
        
    /// <summary>
    /// Test Dbutil methods.
    /// </summary>
    [TestClass()]
    public class DbutilTests
    {
        /// <summary>
        /// Test FormatBytes
        /// </summary>
        [TestMethod()]
        public void FormatBytes()
        {
            byte[] data = new byte[] { 0x1, 0xf, 0x22, 0xee };
            string expected = "010f22ee";
            string actual = Dbutil.FormatBytes(data);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test FormatBytes will null data
        /// </summary>
        [TestMethod()]
        public void FormatBytesWithNullData()
        {
            Assert.IsNull(Dbutil.FormatBytes(null));
        }

        /// <summary>
        /// Test QuoteForCsv with text that is null
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvNullText()
        {
            Assert.IsNull(Dbutil.QuoteForCsv(null));
        }

        /// <summary>
        /// Test QuoteForCsv with text that doesn't need quoting
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvNoQuote()
        {
            Assert.AreEqual("100", Dbutil.QuoteForCsv("100"));
        }

        /// <summary>
        /// Test QuoteForCsv with text that contains quotes
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvContainsQuotes()
        {
            // The quote should be doubled and the text surround with quotes
            Assert.AreEqual("\"xx\"\"xx\"", Dbutil.QuoteForCsv("xx\"xx"));
        }

        /// <summary>
        /// Test QuoteForCsv with text that contains a comma
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvContainsComma()
        {
            Assert.AreEqual("\",\"", Dbutil.QuoteForCsv(","));
        }

        /// <summary>
        /// Test QuoteForCsv with text that contains a newline
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvContainsNewline()
        {
            Assert.AreEqual("\"\r\n\"", Dbutil.QuoteForCsv("\r\n"));
        }

        /// <summary>
        /// Test QuoteForCsv with text that starts with a space
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvStartsWithSpace()
        {
            Assert.AreEqual("\" hello\"", Dbutil.QuoteForCsv(" hello"));
        }

        /// <summary>
        /// Test QuoteForCsv with text that starts with a space
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvStartsWithTab()
        {
            Assert.AreEqual("\"\t$$\"", Dbutil.QuoteForCsv("\t$$"));
        }

        /// <summary>
        /// Test QuoteForCsv with text that ends with a space
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvEndsWithSpace()
        {
            Assert.AreEqual("\"1.2 \"", Dbutil.QuoteForCsv("1.2 "));
        }

        /// <summary>
        /// Test QuoteForCsv with text that ends with a space
        /// </summary>
        [TestMethod()]
        public void QuoteForCsvEndsWithTab()
        {
            Assert.AreEqual("\"__\t\"", Dbutil.QuoteForCsv("__\t"));
        }
    }
}
