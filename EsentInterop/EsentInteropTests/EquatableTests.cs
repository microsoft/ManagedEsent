//-----------------------------------------------------------------------
// <copyright file="EquatableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
	/// <summary>
	/// Tests for classes that implement IEquatable
	/// </summary>
	[TestClass]
	public class EquatableTests
	{
		/// <summary>
		/// Check that JET_INSTANCE structures can be
		/// compared for equality.
		/// </summary>
		[TestMethod]
		[Priority(0)]
		public void VerifyJetInstanceEquality()
		{
			var x = JET_INSTANCE.Nil;
			var y = JET_INSTANCE.Nil;
			TestEqualObjects(x, y);
			Assert.IsTrue(x == y);
			Assert.IsFalse(x != y);
		}

		/// <summary>
		/// Check that JET_INSTANCE structures can be
		/// compared for inequality.
		/// </summary>
		[TestMethod]
		[Priority(0)]
		public void VerifyJetInstanceInequality()
		{
			var x = JET_INSTANCE.Nil;
			var y = new JET_INSTANCE { Value = (IntPtr)0x7 };
			TestUnequalObjects(x, y);
			Assert.IsTrue(x != y);
			Assert.IsFalse(x == y);
		}

		/// <summary>
		/// Helper method to compare two equal objects.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="x">The first object.</param>
		/// <param name="y">The second object.</param>
		private static void TestEqualObjects<T>(T x, T y) where T : struct, IEquatable<T> 
		{
			Assert.IsTrue(x.Equals(y));
			Assert.AreEqual(x.GetHashCode(), y.GetHashCode());

			object objA = x;
			object objB = y;
			Assert.IsTrue(objA.Equals(objB));
			Assert.IsFalse(objA.Equals(Any.String));
		}

		/// <summary>
		/// Helper method to compare two unequal objects.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="x">The first object.</param>
		/// <param name="y">The second object.</param>		
		private static void TestUnequalObjects<T>(T x, T y) where T : struct, IEquatable<T>
		{
			Assert.IsFalse(x.Equals(y));
			Assert.AreNotEqual(x.GetHashCode(), y.GetHashCode());

			object objA = x;
			object objB = y;
			Assert.IsFalse(objA.Equals(objB));
		}
	}
}
