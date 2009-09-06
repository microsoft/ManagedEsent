//-----------------------------------------------------------------------
// <copyright file="DictionaryCreationTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Isam.Esent.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsentCollectionsTests
{
    /// <summary>
    /// Test creating a PersistentDictionary.
    /// </summary>
    [TestClass]
    public class DictionaryCreationTests
    {
        /// <summary>
        /// Creating a dictionary without a directory fails.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyConstructorThrowsExceptionWhenDirectoryIsNull()
        {
            var dictionary = new PersistentDictionary<int, int>(null);
        }

        /// <summary>
        /// PersistentDictionaryFile.Exists fails when the directory is null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyExistsThrowsExceptionWhenDirectoryIsNull()
        {
            PersistentDictionaryFile.Exists(null);
        }

        /// <summary>
        /// Checking for a database returns false when the directory 
        /// doesn't even exists.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyExistsReturnsNullWhenDirectoryDoesNotExists()
        {
            const string NonExistentDirectory = "doesnotexist";
            Assert.IsFalse(Directory.Exists(NonExistentDirectory));
            Assert.IsFalse(PersistentDictionaryFile.Exists(NonExistentDirectory));
        }

        /// <summary>
        /// Checking for a database returns false when the specified
        /// directory is actually a file.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyExistsReturnsNullWhenDirectoryIsAFile()
        {
            string file = Path.GetTempFileName();
            Assert.IsFalse(PersistentDictionaryFile.Exists(file));
            File.Delete(file);
        }

        /// <summary>
        /// Checking for a database returns false when the specified
        /// directory is actually a file.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyExistsReturnsNullWhenDatabaseFileDoesNotExist()
        {
            const string TestDirectory = "testdirectory";
            Directory.CreateDirectory(TestDirectory);
            Assert.IsFalse(PersistentDictionaryFile.Exists(TestDirectory));
            Directory.Delete(TestDirectory);
        }

        /// <summary>
        /// PersistentDictionaryFile.DeleteFiles fails when the directory is null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyDeleteThrowsExceptionWhenDirectoryIsNull()
        {
            PersistentDictionaryFile.DeleteFiles(null);
        }

        /// <summary>
        /// PersistentDictionaryFile.DeleteFiles works on a non-existent directory.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyDeleteSucceedsWhenDirectoryDoesNotExist()
        {
            const string NonExistentDirectory = "doesnotexist";
            Assert.IsFalse(Directory.Exists(NonExistentDirectory));
            PersistentDictionaryFile.DeleteFiles(NonExistentDirectory);
        }

        /// <summary>
        /// PersistentDictionaryFile.DeleteFiles works on an empty directory.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyDeleteSucceedsWhenDirectoryIsEmpty()
        {
            const string TestDirectory = "testdirectory";
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory);
            }

            Directory.CreateDirectory(TestDirectory);
            PersistentDictionaryFile.DeleteFiles(TestDirectory);
            Directory.Delete(TestDirectory);
        }

        /// <summary>
        /// PersistentDictionaryFile.DeleteFiles does not delete
        /// unrelated files.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyDeleteLeavesUnrelatedFiles()
        {
            const string TestDirectory = "testdirectory";
            string testFile = Path.Combine(TestDirectory, "myfile.log");
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory);
            }

            Directory.CreateDirectory(TestDirectory);
            File.WriteAllText(testFile, "hello world");
            PersistentDictionaryFile.DeleteFiles(TestDirectory);
            Assert.IsTrue(File.Exists(testFile));
            Directory.Delete(TestDirectory, true);
        }

        /// <summary>
        /// PersistentDictionaryFile.DeleteFiles removes all database files.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyDeleteRemovesDatabaseFiles()
        {
            const string DictionaryLocation = "DictionaryToDelete";
            var dict = new PersistentDictionary<ulong, bool>(DictionaryLocation);
            dict.Dispose();
            Assert.IsTrue(PersistentDictionaryFile.Exists(DictionaryLocation));
            PersistentDictionaryFile.DeleteFiles(DictionaryLocation);
            Assert.IsFalse(PersistentDictionaryFile.Exists(DictionaryLocation));
        }
    }
}