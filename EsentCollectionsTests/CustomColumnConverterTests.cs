// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomColumnConverterTest.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Tests for custom CustomColumnConverter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Globalization;
    using Microsoft.Database.Isam.Config;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// TODO do we need implement IEquatable<PooledPersistentBlob>?
    /// <summary>
    /// An item for storing in PersistentDictionary for these tests.
    /// </summary>
    internal class PooledPersistentBlob 
    {
        public static readonly TestArrayPool ArrayPool = new TestArrayPool();

        /// <summary>
        /// Byte array containing the Blob.
        /// </summary>
        private readonly byte[] blobData;

        /// <summary>
        /// Length of the Blob.
        /// </summary>
        private readonly int length;
        
        /// <summary>Hash code to detect illegal changes to the Blob.</summary>
        private readonly int blobHashCode;

        public PooledPersistentBlob(byte[] blobData, int length)
        {
            if (blobData != null && blobData.Length < length)
            {
                throw new ArgumentException(string.Format("length cannot be more, than the array length: blobData.length={0}, length={1}", blobData.Length, length));
            }
            
            this.blobData = blobData;
            this.length = length;
            
            if (blobData == null)
            {
                return;
            }
            this.blobHashCode = blobData.GetHashCode();
        }
        
        /// <summary>
        /// Returns the byte array representing the blob.
        /// </summary>
        /// <returns>The byte[] array.</returns>
        public byte[] GetBytes()
        {
            return this.blobData;
        }

        /// <summary>
        /// Return the length of the blob.
        /// </summary>
        /// <returns>The length of the blob.</returns>
        public int GetLength()
        {
            return this.length;
        }

        /// <summary>
        /// Get a copy of bytes of the blob.
        /// </summary>
        /// <returns>Cope of bytes of the blob.</returns>
        public byte[] ToArray()
        {
            if (this.blobData == null || this.blobData.Length == 0 || this.length == 0)
            {
                return new byte[] { };
            }

            byte[] result = new byte[this.length];
            Array.Copy(this.blobData, result, this.length);
            return result;
        }
        
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A Int32 that contains the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.blobData.GetHashCode();
        }

        /// <summary>
        /// Checks that this instance hasn't changed illegally, throws an exception if it did.
        /// </summary>
        public void CheckImmutability()
        {
            int currentHashCode = this.blobData != null ? this.blobData.GetHashCode() : 0;
            if (currentHashCode != this.blobHashCode)
            {
                throw new InvalidOperationException("A PooledPersistentBlob was changed in memory without being changed in the associated PersistentDictionary.");
            }
        }
    }
    
    /// <summary>
    /// ColumnConverter for PooledPersistentBlob
    /// </summary>
    internal class PersistentBlobCustomColumnConverter : IColumnConverter<PooledPersistentBlob>
    {
        internal static int setColumnExecutionsCounter = 0;
        internal static int retrieveColumnExecutionsCounter = 0;
        internal const int initialAllocaitonSize = 93;
        
        public SetColumnDelegate<PooledPersistentBlob> ColumnSetter
        {
            get { return SetColumn; }
        }

        public RetrieveColumnDelegate<PooledPersistentBlob> ColumnRetriever{
            get { return RetrieveColumn; }
        }

        public JET_coltyp Coltyp
        {
            get { return JET_coltyp.LongBinary; }
        }
        
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, PooledPersistentBlob value)
        {
            value.CheckImmutability();
            Api.SetColumn(sesid, tableid, columnid, value.GetBytes(), value.GetLength(), SetColumnGrbit.None);
            setColumnExecutionsCounter++;
        }
        
        private static PooledPersistentBlob RetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            retrieveColumnExecutionsCounter++;
            
            byte[] data = PooledPersistentBlob.ArrayPool.Rent(initialAllocaitonSize);
            int dataSize;
            
            JET_wrn wrn;

            try
            {
                wrn = Api.RetrieveColumn(sesid, tableid, columnid, data, data.Length, out dataSize, RetrieveColumnGrbit.None, null);
            }
            catch (Exception ex)
            {
                PooledPersistentBlob.ArrayPool.Return(data);
                throw;
            }

            if (JET_wrn.ColumnNull == wrn)
            {
                // null column
                PooledPersistentBlob.ArrayPool.Return(data);
                return new PooledPersistentBlob(null, 0);
            }

            if (JET_wrn.Success == wrn)
            {
                return new PooledPersistentBlob(data, dataSize);
            }

            // there is more data to retrieve, so we need another buffer
            PooledPersistentBlob.ArrayPool.Return(data);
           
            data = PooledPersistentBlob.ArrayPool.Rent(dataSize);
            int newDataSize;

            try
            {
                wrn = Api.RetrieveColumn(sesid, tableid, columnid, data, data.Length, out newDataSize, RetrieveColumnGrbit.None, null);
            }
            catch (Exception ex)
            {
                PooledPersistentBlob.ArrayPool.Return(data);
                throw;
            }

            if (JET_wrn.BufferTruncated == wrn)
            {
                PooledPersistentBlob.ArrayPool.Return(data);
                string error = string.Format(
                    CultureInfo.CurrentCulture,
                    "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                    data.Length,
                    newDataSize);
                throw new InvalidOperationException(error);
            }

            return new PooledPersistentBlob(data, newDataSize);
        }
    }
    
    /// <summary>
    /// Test a class that implements IColumnConverter.
    /// </summary>
    [TestClass]
    public class CustomColumnConverterTests
    {
        /// <summary>
        /// Path to put the dictionary in.
        /// </summary>
        private const string DictionaryPath = "CustomColumnConverter";
        
        /// <summary>
        /// The dictionary we are testing.
        /// </summary>
        private PersistentDictionary<string, PooledPersistentBlob> dictionary;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.dictionary = new PersistentDictionary<string, PooledPersistentBlob>(DictionaryPath, new DatabaseConfig()
            {
                DisplayName = "CustomColumnConverterTestsDb"
            }, new PersistentBlobCustomColumnConverter());
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            PooledPersistentBlob.ArrayPool.arrayAllocatedCounter = 0;
            PooledPersistentBlob.ArrayPool.arrayRentedCounter = 0;
            PooledPersistentBlob.ArrayPool.arrayReturnedCounter = 0;
            PooledPersistentBlob.ArrayPool.ClearPool();
            
            PersistentBlobCustomColumnConverter.setColumnExecutionsCounter = 0;
            PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter = 0;
            
            this.dictionary.Dispose();
            Cleanup.DeleteDirectoryWithRetry(DictionaryPath);
        }
        
        /// <summary>
        /// Can add/read an array with custom lenght.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadArrayWithLenghtTest()
        {
            byte[] expectedArray = new byte[]{1, 2, 3};
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(new byte[]{1, 2, 3, 4, 5}, 3);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Can add, and twice read an array with custom lenght without additional allocation.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadArrayWithLenghtTwiceTest()
        {
            byte[] expectedArray = new byte[]{1, 2, 3};
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(new byte[]{1, 2, 3, 4, 5}, 3);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            
            // return array to pool
            PooledPersistentBlob.ArrayPool.Return(actualBlob.GetBytes());
            CollectionAssert.AreNotEqual(expectedArray, actualBlob.ToArray());
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
            
            // read the second time
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(2, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(2, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Can add/read an array with full lenght.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadArrayWithFullLenghtTest()
        {
            byte[] expectedArray = new byte[]{1, 2, 3, 4, 5};
            byte[] storedArray = new byte[]{1, 2, 3, 4, 5};
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(storedArray, storedArray.Length);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Can add/read a big array in LOH (>85kb) with custom lenght.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadArrayInLargeHeapWithLenghtTest()
        {
            const int lenght = 100_000;
            byte[] expectedArray = new byte[lenght];
            byte[] storedArray = new byte[2 * lenght];

            for (int i = 0; i < lenght * 2; i++)
            {
                if (i < lenght)
                {
                    expectedArray[i] = (byte) (i % 256);
                }
                storedArray[i] = (byte) (i % 256);
            }
            
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(storedArray, lenght);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(2, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(2, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Can add/read a big array in LOH (>85kb) with full lenght.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadArrayInLargeHeapWithFullLenghtTest()
        {
            const int lenght = 100_000;
            byte[] expectedArray = new byte[lenght];
            byte[] storedArray = new byte[lenght];

            for (int i = 0; i < lenght; i++)
            {
                expectedArray[i] = (byte) (i % 256);
                storedArray[i] = (byte) (i % 256);
            }
            
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(storedArray, storedArray.Length);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(2, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(2, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Can add/read an empty array.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadEmptyArrayWithLenghtTest()
        {
            byte[] expectedArray = new byte[]{};
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(new byte[]{}, 0);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(0, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Can add/read an null array.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AddAndReadNullArrayWithLenghtTest()
        {
            byte[] expectedArray = new byte[]{};
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(null, 0);
            this.dictionary.Flush();

            PooledPersistentBlob actualBlob;
            Assert.IsTrue(this.dictionary.TryGetValue(key, out actualBlob));
            
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.setColumnExecutionsCounter, "setColumn should be executed once");
            Assert.AreEqual(1, PersistentBlobCustomColumnConverter.retrieveColumnExecutionsCounter, "retrieveColumn should be executed once");
            CollectionAssert.AreEqual(expectedArray, actualBlob.ToArray());
            
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayRentedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayAllocatedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.arrayReturnedCounter);
            Assert.AreEqual(1, PooledPersistentBlob.ArrayPool.GetNumberOfCurrentArraysInPool());
        }
        
        /// <summary>
        /// Cannot add an element, when data is null, but lenght is more than 0.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWhenDataIsNullAndLenghtIsNot0Test()
        {
            string key = "key";
            this.dictionary[key] = new PooledPersistentBlob(null, 1);
            this.dictionary.Flush();
        }
        
        /// <summary>
        /// Cannot add an element, when data lenght is less than dataSize.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentException))]
        public void AddWhenDataLenghtIsLessThanDataSizeTest()
        {
            string key = "key";
            byte[] data = new byte[]{1, 2, 3, 4, 5};
            this.dictionary[key] = new PooledPersistentBlob(data, data.Length + 1);
            this.dictionary.Flush();
        }
    }
}