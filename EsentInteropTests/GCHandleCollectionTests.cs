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
        /// Adding null should return <see cref="IntPtr.Zero"/>.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify adding null to a GCHandleCollection returns IntPtr.Zero")]
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
        [Description("Verify adding two different objects to a GCHandleCollection returns different pointers")]
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
        /// Adding the same object twice returns the same pointer.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Adding the same object twice returns the same pointer")]
        public void AddingSameObjectTwiceReturnsSamePointer()
        {
            using (var handles = new GCHandleCollection())
            {
                string obj = "foo";
                IntPtr p1 = handles.Add(obj);
                IntPtr p2 = handles.Add(obj);
                Assert.AreEqual(p1, p2);
            }
        }

        /// <summary>
        /// Add should give a pointer to the added object.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify adding an object to a GCHandleCollection returns a pointer to the object")]
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
        /// Adding an object should pin it.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify adding an object to a GCHandleCollection prevents it from being collected")]
        public void VerifyAddPinsObject()
        {
            var expected = new string('x', 5);
            var weakref = new WeakReference(expected);
            using (var handles = new GCHandleCollection())
            {
                handles.Add(expected);
                expected = null;
                RunFullGarbageCollection();
                Assert.IsTrue(weakref.IsAlive);
            }
        }

        /// <summary>
        /// Disposing of the handle collection should free the memory.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify disposing of a GCHandleCollection allows the objects to be collected")]
        public void VerifyDisposeUnpinsObjects()
        {
            var expected = new string('x', 5);
            var weakref = new WeakReference(expected);
            using (var handles = new GCHandleCollection())
            {
                handles.Add(expected);
                expected = null; // needed to allow GC to work
            }

            RunFullGarbageCollection();

            // In DEBUG test code, the objects remain alive for an indeterminate amount of time, for some reason.
            // Note that they do get collected if a RETAIL test code is used, even if the product code is DEBUG
            // so it must be something to do with assigning the local variable 'expected' to null and the effect that
            // it has on garbage collecting weak references to it.
#if !DEBUG
            Assert.IsFalse(weakref.IsAlive);
#endif
        }

        /// <summary>
        /// Stress test for the GCHandleCollection.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Stress test for the GCHandleCollection")]
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

        /// <summary>
        /// Run a full garbage collection.
        /// </summary>
        private static void RunFullGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}