//-----------------------------------------------------------------------
// <copyright file="GCHandleCollectionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Isam.Esent.Interop.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing the GCHandleCollection class
    /// </summary>
    [TestClass]
    public class GCHandleCollectionTests
    {
        /// <summary>
        /// Adding null should return IntPtr.Zero
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAddingNullReturnsIntPtrZero()
        {
            using (var handles = new GCHandleCollection())
            {
                IntPtr p = handles.Add(null);
                Assert.AreEqual(IntPtr.Zero, p);
            }
        }

        /// <summary>
        /// Adding two different objects should give different pointers.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAddGivesDifferentPointers()
        {
            using (var handles = new GCHandleCollection())
            {
                IntPtr p1 = handles.Add("foo");
                IntPtr p2 = handles.Add("bar");
                Assert.AreNotEqual(p1, p2);
            }
        }

        /// <summary>
        /// Add should give a pointer to the added object
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAddGivesPointerToObject()
        {
            using (var handles = new GCHandleCollection())
            {
                IntPtr p = handles.Add("expected");
                string actual = Marshal.PtrToStringUni(p);
                Assert.AreEqual("expected", actual);
            }
        }

        /// <summary>
        /// Adding an object should pin it
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyAddPinsObject()
        {
            var expected = new string('x', 5);
            var weakref = new WeakReference(expected);
            using (var handles = new GCHandleCollection())
            {
                handles.Add(expected);
                expected = null;
                GC.Collect();
                Assert.IsTrue(weakref.IsAlive);
            }
        }

        /// <summary>
        /// Disposing of the handle collection should free the memory
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyDisposeUnpinsObjects()
        {
            var expected = new string('x', 5);
            var weakref = new WeakReference(expected);
            using (var handles = new GCHandleCollection())
            {
                handles.Add(expected);
                expected = null; // needed to allow GC to work
            }

            GC.Collect();
            Assert.IsFalse(weakref.IsAlive);
        }

        /// <summary>
        /// Add should give a pointer to the added object
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void GCHandleCollectionStress()
        {
            for (int i = 0; i < 1000; i++)
            {
                using (var handles = new GCHandleCollection())
                {
                    for (int j = 0; j < 100; j++)
                    {
                        IntPtr p = handles.Add(new byte[1]);
                        Assert.AreNotEqual(IntPtr.Zero, p);
                    }
                }
            }
        }
    }
}