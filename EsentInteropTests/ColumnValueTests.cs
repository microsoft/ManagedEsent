//-----------------------------------------------------------------------
// <copyright file="ColumnValueTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing ColumnValue objects.
    /// </summary>
    [TestClass]
    public class ColumnValueTests
    {
        /// <summary>
        /// Test the ValuesAsObject method of a Bool column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test BoolColumnValue.ValueAsObject")]
        public void TestBoolValueAsObject()
        {
            TestValueAsObjectForStruct(new BoolColumnValue(), true);
            TestValueAsObjectForStruct(new BoolColumnValue(), false);
        }

        /// <summary>
        /// Test the ValuesAsObject method of a Byte column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test ByteColumnValue.ValueAsObject")]
        public void TestByteValueAsObject()
        {
            var value = new ByteColumnValue();
            for (byte i = Byte.MinValue; i < Byte.MaxValue; ++i)
            {
                TestValueAsObjectForStruct(value, i);
            }
        }

        /// <summary>
        /// Test the ValuesAsObject method of an Int16 column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Int16ColumnValue.ValueAsObject")]
        public void TestInt16ValueAsObject()
        {
            var value = new Int16ColumnValue();
            for (short i = Int16.MinValue; i < Int16.MaxValue; ++i)
            {
                TestValueAsObjectForStruct(value, i);
            }
        }

        /// <summary>
        /// Test the ValuesAsObject method of an UInt16 column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test UInt16ColumnValue.ValueAsObject")]
        public void TestUInt16ValueAsObject()
        {
            var value = new UInt16ColumnValue();
            for (ushort i = UInt16.MinValue; i < Int16.MaxValue; ++i)
            {
                TestValueAsObjectForStruct(value, i);
            }
        }

        /// <summary>
        /// Test the ValuesAsObject method of an Int64 column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Int64ColumnValue.ValueAsObject")]
        public void TestInt64ValueAsObject()
        {
            var instance = new Int64ColumnValue { Value = 99 };
            Assert.AreEqual(99L, instance.ValueAsObject);
        }

        /// <summary>
        /// Test the ValuesAsObject method of an string column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test StringColumnValue.ValueAsObject")]
        public void TestStringValueAsObject()
        {
            var instance = new StringColumnValue { Value = "hello" };
            Assert.AreEqual("hello", instance.ValueAsObject);
        }

        /// <summary>
        /// Test the ValuesAsObject method of an bytes column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test BytesColumnValue.ValueAsObject")]
        public void TestBytesValueAsObject()
        {
            byte[] data = Any.Bytes;
            var instance = new BytesColumnValue { Value = data };
            Assert.AreEqual(data, instance.ValueAsObject);
        }

        /// <summary>
        /// Test the ToString() method of an Int32 column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Int32ColumnValue.ToString()")]
        public void TestInt32ColumnValueToString()
        {
            var instance = new Int32ColumnValue { Value = 5 };
            Assert.AreEqual("5", instance.ToString());
        }

        /// <summary>
        /// Test the ToString() method of a string column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test StringColumnValue.ToString()")]
        public void TestStringColumnValueToString()
        {
            var instance = new StringColumnValue { Value = "foo" };
            Assert.AreEqual("foo", instance.ToString());
        }

        /// <summary>
        /// Test the ToString() method of a GUID column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test GuidColumnValue.ToString()")]
        public void TestGuidColumnValueToString()
        {
            Guid guid = Guid.NewGuid();
            var instance = new GuidColumnValue { Value = guid };
            Assert.AreEqual(guid.ToString(), instance.ToString());
        }

        /// <summary>
        /// Test the ToString() method of a Bytes column value with a null value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test BytesColumnValue.ToString() with a null value")]
        public void TestNullBytesColumnValueToString()
        {
            var instance = new BytesColumnValue { Value = null };
            Assert.AreEqual(String.Empty, instance.ToString());
        }

        /// <summary>
        /// Test the ToString() method of a Bytes column value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test BytesColumnValue.ToString()")]
        public void TestBytesColumnValueToString()
        {
            var instance = new BytesColumnValue { Value = BitConverter.GetBytes(0x1122334455667788UL) };
            Assert.AreEqual("88-77-66-55-44-33-22-11", instance.ToString());
        }

        /// <summary>
        /// Test the ValueAsObject method for structure types.
        /// </summary>
        /// <typeparam name="T">The structure type.</typeparam>
        /// <param name="columnValue">The column value.</param>
        /// <param name="value">The value to set.</param>
        private static void TestValueAsObjectForStruct<T>(ColumnValueOfStruct<T> columnValue, T value) where T : struct
        {
            columnValue.Value = value;
            object o1 = columnValue.ValueAsObject;
            Assert.AreEqual(o1, value);
            columnValue.Value = null;
            Assert.IsNull(columnValue.ValueAsObject);
            columnValue.Value = default(T);
            Assert.AreEqual(columnValue.ValueAsObject, default(T));
            columnValue.Value = value;
            object o2 = columnValue.ValueAsObject;
            Assert.AreEqual(o1, o2);
            Assert.AreSame(o1, o2);
        }
    }
}