//-----------------------------------------------------------------------
// <copyright file="VisualStudioUnitTest.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

// This file was copy/pasted from a reflection disassembler. Its purpose is to allow execution
// of the ESE tests on platformsthat don't have the Visual Studio test DLLs.
//
// It is supposed to be available on Metro with Win8 build 8250, but I simply
// couldn't get it to work.
//
// It is still far from perfect. For example the resources for the strings of the error messages is broken.
#if MANAGEDESENT_ON_METRO
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Text.RegularExpressions;

    /// <summary>
    /// How long to time out.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TimeoutAttribute : Attribute
    {
        private int m_timeout;

        /// <summary>
        /// Gets the time out period.
        /// </summary>
        public int Timeout
        {
            get { return this.m_timeout; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        public TimeoutAttribute(int timeout)
        {
            this.m_timeout = timeout;
        }
    }

    /// <summary>
    /// The test owner.
    /// </summary>
    public sealed class OwnerAttribute : Attribute
    {
        private string m_owner;

        /// <summary>
        /// 
        /// </summary>
        public string Owner
        {
            get
            {
                return this.m_owner;
            }
        }

        /// <summary>
        /// The test owner.
        /// </summary>
        /// <param name="owner"></param>
        public OwnerAttribute(string owner)
        {
            this.m_owner = owner;
        }
    }

    /// <summary>
    /// It's a test class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TestClassAttribute : Attribute
    {
    }

    /// <summary>
    /// It's a test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestMethodAttribute : Attribute
    {
    }

    /// <summary>
    /// It's a descripion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        private string m_description;

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get
            {
                return this.m_description;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        public DescriptionAttribute(string description)
        {
            this.m_description = description;
        }
    }

    /// <summary>
    /// The test priority.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PriorityAttribute : Attribute
    {
        private int m_priority;

        /// <summary>
        /// 
        /// </summary>
        public int Priority
        {
            get
            {
                return this.m_priority;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority"></param>
        public PriorityAttribute(int priority)
        {
            this.m_priority = priority;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExpectedExceptionAttribute : Attribute
    {
        private Type m_exceptionType;
        private string m_message;

        /// <summary>
        /// 
        /// </summary>
        public Type ExceptionType
        {
            get
            {
                return this.m_exceptionType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Message
        {
            get
            {
                return this.m_message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exceptionType"></param>
        public ExpectedExceptionAttribute(Type exceptionType)
            : this(exceptionType, string.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <param name="message"></param>
        public ExpectedExceptionAttribute(Type exceptionType, string message)
        {
            this.m_exceptionType = exceptionType;
            this.m_message = message;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestInitializeAttribute : Attribute
    {
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestCleanupAttribute : Attribute
    {
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ClassInitializeAttribute : Attribute
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class UnitTestAssertException : Exception
    {
        private EqtMessage m_message;

        /// <summary>
        /// 
        /// </summary>
        public override string Message
        {
            get
            {
                if (this.m_message != null)
                    return this.m_message.ToString();
                else
                    return base.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected UnitTestAssertException()
        {
        }

        internal UnitTestAssertException(EqtMessage message)
            : this(message, (Exception)null)
        {
        }

        internal UnitTestAssertException(EqtMessage message, Exception inner)
            : base((string)message, inner)
        {
            this.m_message = message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        protected UnitTestAssertException(string msg, Exception ex)
            : base(msg, ex)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        protected UnitTestAssertException(string msg)
            : base(msg)
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class AssertFailedException : UnitTestAssertException
    {
        internal AssertFailedException(EqtMessage message)
            : base(message)
        {
        }

        internal AssertFailedException(EqtMessage message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public AssertFailedException(string msg, Exception ex)
            : base(msg, ex)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public AssertFailedException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public AssertFailedException()
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class AssertInconclusiveException : UnitTestAssertException
    {
        internal AssertInconclusiveException(EqtMessage message)
            : base(message)
        {
        }

        internal AssertInconclusiveException(EqtMessage message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public AssertInconclusiveException(string msg, Exception ex)
            : base(msg, ex)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public AssertInconclusiveException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public AssertInconclusiveException()
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ClassCleanupAttribute : Attribute
    {
    }

    internal class EqtMessage
    {
        private static Dictionary<Type, ResourceManager> m_resourceManagers = new Dictionary<Type, ResourceManager>();
        private object[] m_array;
        private string m_name;
        private ResourceManager m_rm;
        private Type m_t;

        public ResourceManager RM
        {
            get
            {
                if (this.m_rm == null && !EqtMessage.m_resourceManagers.TryGetValue(this.m_t, out this.m_rm))
                    EqtMessage.m_resourceManagers.Add(this.m_t, this.m_rm = new ResourceManager(this.m_t));
                return this.m_rm;
            }
        }

        internal string Name
        {
            get
            {
                return this.m_name;
            }
        }

        internal object[] Params
        {
            get
            {
                return this.m_array;
            }
        }

        static EqtMessage()
        {
        }

        private EqtMessage()
        {
        }

        public EqtMessage(string name, Type type, ResourceManager resourceManager, object[] array)
        {
            this.m_name = name;
            this.m_rm = resourceManager;
            this.m_array = array;
            this.m_t = type;
        }

        public static implicit operator string(EqtMessage eqtMessage)
        {
            return eqtMessage.ToString();
        }

        public override string ToString()
        {
            string @string = this.RM.GetString(this.Name, CultureInfo.CurrentUICulture);
            object[] @params = this.Params;
            if (@params != null)
                return string.Format((IFormatProvider)CultureInfo.CurrentCulture, @string, @params);
            else
                return @string;
        }
    }

    // Type: Microsoft.VisualStudio.TestTools.UnitTesting.Assert
    // Assembly: Microsoft.VisualStudio.QualityTools.UnitTestFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
    // Assembly location: e:\src\e15\DISTRIB\PRIVATE\BIN\debug\amd64\Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll

    /// <summary>
    /// 
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.MaintainabilityRules",
        "SA1614:ElementParameterDocumentationMustHaveText",
        Justification = "This code was disassembled and should be deleted once the VS unit tests are working.")]
    public static class Assert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        public static void IsTrue(bool condition)
        {
            Assert.IsTrue(condition, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void IsTrue(bool condition, string message)
        {
            Assert.IsTrue(condition, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsTrue(bool condition, string message, params object[] parameters)
        {
            if (condition)
                return;
            Assert.HandleFail("Assert.IsTrue", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        public static void IsFalse(bool condition)
        {
            Assert.IsFalse(condition, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void IsFalse(bool condition, string message)
        {
            Assert.IsFalse(condition, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsFalse(bool condition, string message, params object[] parameters)
        {
            if (!condition)
                return;
            Assert.HandleFail("Assert.IsFalse", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void IsNull(object value)
        {
            Assert.IsNull(value, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        public static void IsNull(object value, string message)
        {
            Assert.IsNull(value, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsNull(object value, string message, params object[] parameters)
        {
            if (value == null)
                return;
            Assert.HandleFail("Assert.IsNull", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static void IsNotNull(object value)
        {
            Assert.IsNotNull(value, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        public static void IsNotNull(object value, string message)
        {
            Assert.IsNotNull(value, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsNotNull(object value, string message, params object[] parameters)
        {
            if (value != null)
                return;
            Assert.HandleFail("Assert.IsNotNull", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreSame(object expected, object actual)
        {
            Assert.AreSame(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreSame(object expected, object actual, string message)
        {
            Assert.AreSame(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreSame(object expected, object actual, string message, params object[] parameters)
        {
            if (object.ReferenceEquals(expected, actual))
                return;
            Assert.HandleFail("Assert.AreSame", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        public static void AreNotSame(object notExpected, object actual)
        {
            Assert.AreNotSame(notExpected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotSame(object notExpected, object actual, string message)
        {
            Assert.AreNotSame(notExpected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotSame(object notExpected, object actual, string message, params object[] parameters)
        {
            if (!object.ReferenceEquals(notExpected, actual))
                return;
            Assert.HandleFail("Assert.AreNotSame", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEqual<T>(T expected, T actual)
        {
            Assert.AreEqual<T>(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreEqual<T>(T expected, T actual, string message)
        {
            Assert.AreEqual<T>(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual<T>(T expected, T actual, string message, params object[] parameters)
        {
            if (object.Equals((object)expected, (object)actual))
                return;
            Assert.HandleFail("Assert.AreEqual",
                              (object)actual == null || (object)expected == null ||
                              actual.GetType().Equals(expected.GetType())
                                  ? (string)
                                    FrameworkMessages.AreEqualFailMsg(
                                        message == null ? (object)string.Empty : (object)message,
                                        (object)expected == null ? (object)"(null)" : (object)expected.ToString(),
                                        (object)actual == null ? (object)"(null)" : (object)actual.ToString())
                                  : (string)
                                    FrameworkMessages.AreEqualDifferentTypesFailMsg(
                                        message == null ? (object)string.Empty : (object)message,
                                        (object)expected.ToString(), (object)expected.GetType().FullName,
                                        (object)actual.ToString(), (object)actual.GetType().FullName), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        public static void AreNotEqual<T>(T notExpected, T actual)
        {
            Assert.AreNotEqual<T>(notExpected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotEqual<T>(T notExpected, T actual, string message)
        {
            Assert.AreNotEqual<T>(notExpected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual<T>(T notExpected, T actual, string message, params object[] parameters)
        {
            if (!object.Equals((object) notExpected, (object) actual))
                return;
            Assert.HandleFail(
                "Assert.AreNotEqual",
                (string)
                FrameworkMessages.AreNotEqualFailMsg(message == null ? (object) string.Empty : (object) message,
                                                     (object) notExpected == null
                                                         ? (object) "(null)"
                                                         : (object) notExpected.ToString(),
                                                     (object) actual == null
                                                         ? (object) "(null)"
                                                         : (object) actual.ToString()), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEqual(object expected, object actual)
        {
            Assert.AreEqual(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreEqual(object expected, object actual, string message)
        {
            Assert.AreEqual(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(object expected, object actual, string message, params object[] parameters)
        {
            if (object.Equals(expected, actual))
                return;
            Assert.HandleFail("Assert.AreEqual",
                              actual == null || expected == null || actual.GetType().Equals(expected.GetType())
                                  ? (string)
                                    FrameworkMessages.AreEqualFailMsg(
                                        message == null ? (object) string.Empty : (object) message,
                                        expected == null ? (object) "(null)" : (object) expected.ToString(),
                                        actual == null ? (object) "(null)" : (object) actual.ToString())
                                  : (string)
                                    FrameworkMessages.AreEqualDifferentTypesFailMsg(
                                        message == null ? (object) string.Empty : (object) message,
                                        (object) expected.ToString(), (object) expected.GetType().FullName,
                                        (object) actual.ToString(), (object) actual.GetType().FullName), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        public static void AreNotEqual(object notExpected, object actual)
        {
            Assert.AreNotEqual(notExpected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(object notExpected, object actual, string message)
        {
            Assert.AreNotEqual(notExpected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(object notExpected, object actual, string message, params object[] parameters)
        {
            if (!object.Equals(notExpected, actual))
                return;
            Assert.HandleFail("Assert.AreNotEqual",
                              (string)
                              FrameworkMessages.AreEqualFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  notExpected == null ? (object) "(null)" : (object) notExpected.ToString(),
                                  actual == null ? (object) "(null)" : (object) actual.ToString()), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        public static void AreEqual(float expected, float actual, float delta)
        {
            Assert.AreEqual(expected, actual, delta, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        public static void AreEqual(float expected, float actual, float delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(float expected, float actual, float delta, string message, params object[] parameters)
        {
            if ((double) Math.Abs(expected - actual) <= (double) delta)
                return;
            Assert.HandleFail("Assert.AreEqual",
                              (string)
                              FrameworkMessages.AreEqualFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  (object) expected.ToString((IFormatProvider) CultureInfo.CurrentCulture.NumberFormat),
                                  (object) actual.ToString((IFormatProvider) CultureInfo.CurrentCulture.NumberFormat)),
                              parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        public static void AreNotEqual(float notExpected, float actual, float delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(float notExpected, float actual, float delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(float notExpected, float actual, float delta, string message, params object[] parameters)
        {
            if ((double)Math.Abs(notExpected - actual) > (double)delta)
                return;
            Assert.HandleFail("Assert.AreNotEqual", (string)FrameworkMessages.AreNotEqualFailMsg(message == null ? (object)string.Empty : (object)message, (object)notExpected.ToString((IFormatProvider)CultureInfo.CurrentCulture.NumberFormat), (object)actual.ToString((IFormatProvider)CultureInfo.CurrentCulture.NumberFormat)), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        public static void AreEqual(double expected, double actual, double delta)
        {
            Assert.AreEqual(expected, actual, delta, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        public static void AreEqual(double expected, double actual, double delta, string message)
        {
            Assert.AreEqual(expected, actual, delta, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(double expected, double actual, double delta, string message, params object[] parameters)
        {
            if (Math.Abs(expected - actual) <= delta)
                return;
            Assert.HandleFail("Assert.AreEqual",
                              (string)
                              FrameworkMessages.AreEqualFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  (object) expected.ToString((IFormatProvider) CultureInfo.CurrentCulture.NumberFormat),
                                  (object) actual.ToString((IFormatProvider) CultureInfo.CurrentCulture.NumberFormat)),
                              parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        public static void AreNotEqual(double notExpected, double actual, double delta)
        {
            Assert.AreNotEqual(notExpected, actual, delta, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(double notExpected, double actual, double delta, string message)
        {
            Assert.AreNotEqual(notExpected, actual, delta, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="delta"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(double notExpected, double actual, double delta, string message, params object[] parameters)
        {
            if (Math.Abs(notExpected - actual) > delta)
                return;
            Assert.HandleFail("Assert.AreNotEqual",
                              (string)
                              FrameworkMessages.AreNotEqualFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  (object)
                                  notExpected.ToString((IFormatProvider) CultureInfo.CurrentCulture.NumberFormat),
                                  (object) actual.ToString((IFormatProvider) CultureInfo.CurrentCulture.NumberFormat)),
                              parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        public static void AreEqual(string expected, string actual, bool ignoreCase)
        {
            Assert.AreEqual(expected, actual, ignoreCase, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="message"></param>
        public static void AreEqual(string expected, string actual, bool ignoreCase, string message)
        {
            Assert.AreEqual(expected, actual, ignoreCase, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(string expected, string actual, bool ignoreCase, string message, params object[] parameters)
        {
            Assert.AreEqual(expected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="culture"></param>
        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture)
        {
            Assert.AreEqual(expected, actual, ignoreCase, culture, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="culture"></param>
        /// <param name="message"></param>
        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture, string message)
        {
            Assert.AreEqual(expected, actual, ignoreCase, culture, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="culture"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(string expected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object) culture, "Assert.AreEqual", "culture", string.Empty, new object[0]);

            StringComparison howToCompare = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (string.Compare(expected, actual, howToCompare) == 0)
                return;
            Assert.HandleFail("Assert.AreEqual",
                              (string)
                              FrameworkMessages.AreEqualFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  expected == null ? (object) "(null)" : (object) expected,
                                  actual == null ? (object) "(null)" : (object) actual), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, string message)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, string message, params object[] parameters)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="culture"></param>
        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, culture, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="culture"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message)
        {
            Assert.AreNotEqual(notExpected, actual, ignoreCase, culture, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notExpected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="culture"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object) culture, "Assert.AreNotEqual", "culture", string.Empty, new object[0]);
            StringComparison howToCompare = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (string.Compare(notExpected, actual, howToCompare) != 0)
                return;
            Assert.HandleFail("Assert.AreNotEqual",
                              (string)
                              FrameworkMessages.AreEqualFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  notExpected == null ? (object) "(null)" : (object) ((object) notExpected).ToString(),
                                  actual == null ? (object) "(null)" : (object) ((object) actual).ToString()),
                              parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expectedType"></param>
        public static void IsInstanceOfType(object value, Type expectedType)
        {
            Assert.IsInstanceOfType(value, expectedType, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expectedType"></param>
        /// <param name="message"></param>
        public static void IsInstanceOfType(object value, Type expectedType, string message)
        {
            Assert.IsInstanceOfType(value, expectedType, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expectedType"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsInstanceOfType(object value, Type expectedType, string message, params object[] parameters)
        {
            if (expectedType == null)
                Assert.HandleFail("Assert.IsInstanceOfType", message, parameters);
            
            if (expectedType.GetTypeInfo().Equals(value.GetType().GetTypeInfo()))
                return;
            Assert.HandleFail("Assert.IsInstanceOfType",
                              (string)
                              FrameworkMessages.IsInstanceOfFailMsg(
                                  message == null ? (object) string.Empty : (object) message,
                                  (object) expectedType.ToString(),
                                  value == null ? (object) "(null)" : (object) value.GetType().ToString()), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Fail()
        {
            Assert.Fail(string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void Fail(string message)
        {
            Assert.Fail(message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void Fail(string message, params object[] parameters)
        {
            Assert.HandleFail("Assert.Fail", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Inconclusive()
        {
            Assert.Inconclusive(string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void Inconclusive(string message)
        {
            Assert.Inconclusive(message, (object[])null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void Inconclusive(string message, params object[] parameters)
        {
            string str = string.Empty;
            if (!string.IsNullOrEmpty(message))
                str = parameters != null ? string.Format((IFormatProvider)CultureInfo.CurrentCulture, message, parameters) : message;
            throw new AssertInconclusiveException(FrameworkMessages.AssertionFailed((object)"Assert.Inconclusive", (object)str));
        }

        internal static void HandleFail(string assertionName, string message, params object[] parameters)
        {
            string str = string.Empty;
            if (!string.IsNullOrEmpty(message))
                str = parameters != null ? string.Format((IFormatProvider)CultureInfo.CurrentCulture, message, parameters) : message;
            throw new AssertFailedException(FrameworkMessages.AssertionFailed((object)assertionName, (object)str));
        }

        internal static void CheckParameterNotNull(object param, string assertionName, string parameterName, string message, params object[] parameters)
        {
            if (param != null)
                return;
            Assert.HandleFail(assertionName, (string)FrameworkMessages.NullParameterToAssert((object)parameterName, (object)message), parameters);
        }
    }

    internal class FrameworkMessages
    {
        internal static readonly ResourceManager ResourceManager = new ResourceManager(typeof(FrameworkMessages));

        public static EqtMessage AccessStringInvalidSyntax
        {
            get
            {
                return new EqtMessage("AccessStringInvalidSyntax", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage BothCollectionsSameElements
        {
            get
            {
                return new EqtMessage("BothCollectionsSameElements", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage Equal_1_2
        {
            get
            {
                return new EqtMessage("Equal_1_2", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage Equal_1_n
        {
            get
            {
                return new EqtMessage("Equal_1_n", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage Equal_2_1
        {
            get
            {
                return new EqtMessage("Equal_2_1", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage Equal_2_n
        {
            get
            {
                return new EqtMessage("Equal_2_n", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage Equal_ch_n
        {
            get
            {
                return new EqtMessage("Equal_ch_n", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage Equal_d_n
        {
            get
            {
                return new EqtMessage("Equal_d_n", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage EqualsTesterInvalidArgs
        {
            get
            {
                return new EqtMessage("EqualsTesterInvalidArgs", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage HashTesterHashMatch_Eq1_Eq2
        {
            get
            {
                return new EqtMessage("HashTesterHashMatch_Eq1_Eq2", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage HashTesterHashNotMatch_Eq1
        {
            get
            {
                return new EqtMessage("HashTesterHashNotMatch_Eq1", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage HashTesterHashNotMatch_Eq2
        {
            get
            {
                return new EqtMessage("HashTesterHashNotMatch_Eq2", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage HashTesterTypeMisMatch_1_2
        {
            get
            {
                return new EqtMessage("HashTesterTypeMisMatch_1_2", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage HashTesterTypeMisMatch_1_3
        {
            get
            {
                return new EqtMessage("HashTesterTypeMisMatch_1_3", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage InternalObjectNotValid
        {
            get
            {
                return new EqtMessage("InternalObjectNotValid", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage PrivateAccessorConstructorNotFound
        {
            get
            {
                return new EqtMessage("PrivateAccessorConstructorNotFound", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_1_ch
        {
            get
            {
                return new EqtMessage("NotEqual_1_ch", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_1_d
        {
            get
            {
                return new EqtMessage("NotEqual_1_d", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_2_d
        {
            get
            {
                return new EqtMessage("NotEqual_2_d", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_ch_1
        {
            get
            {
                return new EqtMessage("NotEqual_ch_1", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_ch_d
        {
            get
            {
                return new EqtMessage("NotEqual_ch_d", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_d_1
        {
            get
            {
                return new EqtMessage("NotEqual_d_1", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_d_2
        {
            get
            {
                return new EqtMessage("NotEqual_d_2", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NotEqual_d_ch
        {
            get
            {
                return new EqtMessage("NotEqual_d_ch", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        public static EqtMessage NumberOfElementsDiff
        {
            get
            {
                return new EqtMessage("NumberOfElementsDiff", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, (object[])null);
            }
        }

        static FrameworkMessages()
        {
        }

        public static EqtMessage ActualHasMismatchedElements(object param0, object param1, object param2, object param3)
        {
            object[] array = new object[4]
      {
        param0,
        param1,
        param2,
        param3
      };
            return new EqtMessage("ActualHasMismatchedElements", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage AllItemsAreUniqueFailMsg(object param0, object param1)
        {
            object[] array = new object[2]
      {
        param0,
        param1
      };
            return new EqtMessage("AllItemsAreUniqueFailMsg", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage AreEqualFailMsg(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("AreEqualFailMsg", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage AreEqualDifferentTypesFailMsg(object param0, object param1, object param2, object param3, object param4)
        {
            object[] array = new object[5]
      {
        param0,
        param1,
        param2,
        param3,
        param4
      };
            return new EqtMessage("AreEqualDifferentTypesFailMsg", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage AreNotEqualFailMsg(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("AreNotEqualFailMsg", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage BothCollectionsEmpty(object param0)
        {
            object[] array = new object[1]
      {
        param0
      };
            return new EqtMessage("BothCollectionsEmpty", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage BothCollectionsSameReference(object param0)
        {
            object[] array = new object[1]
      {
        param0
      };
            return new EqtMessage("BothCollectionsSameReference", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage BothSameElements(object param0)
        {
            object[] array = new object[1]
      {
        param0
      };
            return new EqtMessage("BothSameElements", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage CollectionEqualReason(object param0, object param1)
        {
            object[] array = new object[2]
      {
        param0,
        param1
      };
            return new EqtMessage("CollectionEqualReason", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage ContainsFail(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("ContainsFail", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage ElementNumbersDontMatch(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("ElementNumbersDontMatch", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage ElementsAtIndexDontMatch(object param0)
        {
            object[] array = new object[1]
      {
        param0
      };
            return new EqtMessage("ElementsAtIndexDontMatch", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage ElementTypesAtIndexDontMatch(object param0, object param1, object param2, object param3)
        {
            object[] array = new object[4]
      {
        param0,
        param1,
        param2,
        param3
      };
            return new EqtMessage("ElementTypesAtIndexDontMatch", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage ElementTypesAtIndexDontMatch2(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("ElementTypesAtIndexDontMatch2", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        public static EqtMessage EndsWithFail(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("EndsWithFail", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <returns></returns>
        public static EqtMessage ErrorInvalidCast(object param0, object param1)
        {
            object[] array = new object[2]
      {
        param0,
        param1
      };
            return new EqtMessage("ErrorInvalidCast", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <returns></returns>
        public static EqtMessage AssertionFailed(object param0, object param1)
        {
            object[] array = new object[2]
      {
        param0,
        param1
      };
            return new EqtMessage("AssertionFailed", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <returns></returns>
        public static EqtMessage InvalidParameterToAssert(object param0, object param1)
        {
            object[] array = new object[2]
      {
        param0,
        param1
      };
            return new EqtMessage("InvalidParameterToAssert", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static EqtMessage IsInstanceOfFailMsg(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("IsInstanceOfFailMsg", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static EqtMessage IsMatchFail(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("IsMatchFail", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static EqtMessage IsNotInstanceOfFailMsg(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("IsNotInstanceOfFailMsg", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static EqtMessage IsNotMatchFail(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("IsNotMatchFail", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <returns></returns>
        public static EqtMessage PrivateAccessorMemberNotFound(object param0)
        {
            object[] array = new object[1]
      {
        param0
      };
            return new EqtMessage("PrivateAccessorMemberNotFound", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <returns></returns>
        public static EqtMessage NullParameterToAssert(object param0, object param1)
        {
            object[] array = new object[2]
                                 {
                                     param0,
                                     param1
                                 };
            return new EqtMessage("NullParameterToAssert", typeof (FrameworkMessages), FrameworkMessages.ResourceManager,
                                  array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static EqtMessage StartsWithFail(object param0, object param1, object param2)
        {
            object[] array = new object[3]
            {
                param0,
                param1,
                param2
            };
            return new EqtMessage("StartsWithFail", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static EqtMessage InvalidPropertyType(object param0, object param1, object param2)
        {
            object[] array = new object[3]
      {
        param0,
        param1,
        param2
      };
            return new EqtMessage("InvalidPropertyType", typeof(FrameworkMessages), FrameworkMessages.ResourceManager, array);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class StringAssert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        public static void Contains(string value, string substring)
        {
            StringAssert.Contains(value, substring, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <param name="message"></param>
        public static void Contains(string value, string substring, string message)
        {
            StringAssert.Contains(value, substring, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void Contains(string value, string substring, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)value, "StringAssert.Contains", "value", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)substring, "StringAssert.Contains", "substring", string.Empty, new object[0]);
            if (0 <= value.IndexOf(substring, StringComparison.Ordinal))
                return;
            Assert.HandleFail("StringAssert.Contains", (string)FrameworkMessages.ContainsFail((object)value, (object)substring, (object)message), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        public static void StartsWith(string value, string substring)
        {
            StringAssert.StartsWith(value, substring, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <param name="message"></param>
        public static void StartsWith(string value, string substring, string message)
        {
            StringAssert.StartsWith(value, substring, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void StartsWith(string value, string substring, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)value, "StringAssert.StartsWith", "value", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)substring, "StringAssert.StartsWith", "substring", string.Empty, new object[0]);
            if (value.StartsWith(substring, StringComparison.Ordinal))
                return;
            Assert.HandleFail("StringAssert.StartsWith", (string)FrameworkMessages.StartsWithFail((object)value, (object)substring, (object)message), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        public static void EndsWith(string value, string substring)
        {
            StringAssert.EndsWith(value, substring, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <param name="message"></param>
        public static void EndsWith(string value, string substring, string message)
        {
            StringAssert.EndsWith(value, substring, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void EndsWith(string value, string substring, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)value, "StringAssert.EndsWith", "value", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)substring, "StringAssert.EndsWith", "substring", string.Empty, new object[0]);
            if (value.EndsWith(substring, StringComparison.Ordinal))
                return;
            Assert.HandleFail("StringAssert.EndsWith", (string)FrameworkMessages.EndsWithFail((object)value, (object)substring, (object)message), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        public static void Matches(string value, Regex pattern)
        {
            StringAssert.Matches(value, pattern, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        public static void Matches(string value, Regex pattern, string message)
        {
            StringAssert.Matches(value, pattern, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void Matches(string value, Regex pattern, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)value, "StringAssert.Matches", "value", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)pattern, "StringAssert.Matches", "pattern", string.Empty, new object[0]);
            if (pattern.IsMatch(value))
                return;
            Assert.HandleFail("StringAssert.Matches", (string)FrameworkMessages.IsMatchFail((object)value, (object)pattern, (object)message), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        public static void DoesNotMatch(string value, Regex pattern)
        {
            StringAssert.DoesNotMatch(value, pattern, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        public static void DoesNotMatch(string value, Regex pattern, string message)
        {
            StringAssert.DoesNotMatch(value, pattern, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void DoesNotMatch(string value, Regex pattern, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)value, "StringAssert.DoesNotMatch", "value", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)pattern, "StringAssert.DoesNotMatch", "pattern", string.Empty, new object[0]);
            if (!pattern.IsMatch(value))
                return;
            Assert.HandleFail("StringAssert.DoesNotMatch", (string)FrameworkMessages.IsNotMatchFail((object)value, (object)pattern, (object)message), parameters);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class CollectionAssert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="element"></param>
        public static void Contains(ICollection collection, object element)
        {
            CollectionAssert.Contains(collection, element, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="element"></param>
        /// <param name="message"></param>
        public static void Contains(ICollection collection, object element, string message)
        {
            CollectionAssert.Contains(collection, element, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="element"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void Contains(ICollection collection, object element, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)collection, "CollectionAssert.Contains", "collection", string.Empty, new object[0]);
            foreach (object objA in (IEnumerable)collection)
            {
                if (object.Equals(objA, element))
                    return;
            }
            Assert.HandleFail("CollectionAssert.Contains", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="element"></param>
        public static void DoesNotContain(ICollection collection, object element)
        {
            CollectionAssert.DoesNotContain(collection, element, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="element"></param>
        /// <param name="message"></param>
        public static void DoesNotContain(ICollection collection, object element, string message)
        {
            CollectionAssert.DoesNotContain(collection, element, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="element"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void DoesNotContain(ICollection collection, object element, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)collection, "CollectionAssert.DoesNotContain", "collection", string.Empty, new object[0]);
            foreach (object objA in (IEnumerable)collection)
            {
                if (object.Equals(objA, element))
                    Assert.HandleFail("CollectionAssert.DoesNotContain", message, parameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        public static void AllItemsAreNotNull(ICollection collection)
        {
            CollectionAssert.AllItemsAreNotNull(collection, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="message"></param>
        public static void AllItemsAreNotNull(ICollection collection, string message)
        {
            CollectionAssert.AllItemsAreNotNull(collection, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AllItemsAreNotNull(ICollection collection, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)collection, "CollectionAssert.AllItemsAreNotNull", "collection", string.Empty, new object[0]);
            foreach (object obj in (IEnumerable)collection)
            {
                if (obj == null)
                    Assert.HandleFail("CollectionAssert.AllItemsAreNotNull", message, parameters);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        public static void AllItemsAreUnique(ICollection collection)
        {
            CollectionAssert.AllItemsAreUnique(collection, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="message"></param>
        public static void AllItemsAreUnique(ICollection collection, string message)
        {
            CollectionAssert.AllItemsAreUnique(collection, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AllItemsAreUnique(ICollection collection, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)collection, "CollectionAssert.AllItemsAreUnique", "collection", string.Empty, new object[0]);
            bool flag = false;
            var hashtable = new Dictionary<object, bool?>();
            foreach (object key in (IEnumerable)collection)
            {
                if (key == null)
                {
                    if (!flag)
                        flag = true;
                    else
                        Assert.HandleFail("CollectionAssert.AllItemsAreUnique", (string)FrameworkMessages.AllItemsAreUniqueFailMsg(message == null ? (object)string.Empty : (object)message, (object)"(null)"), parameters);
                }
                else if (hashtable[key] != null)
                    Assert.HandleFail("CollectionAssert.AllItemsAreUnique", (string)FrameworkMessages.AllItemsAreUniqueFailMsg(message == null ? (object)string.Empty : (object)message, (object)key.ToString()), parameters);
                else
                    hashtable.Add(key, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="superset"></param>
        public static void IsSubsetOf(ICollection subset, ICollection superset)
        {
            CollectionAssert.IsSubsetOf(subset, superset, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="superset"></param>
        /// <param name="message"></param>
        public static void IsSubsetOf(ICollection subset, ICollection superset, string message)
        {
            CollectionAssert.IsSubsetOf(subset, superset, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="superset"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsSubsetOf(ICollection subset, ICollection superset, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)subset, "CollectionAssert.IsSubsetOf", "subset", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)superset, "CollectionAssert.IsSubsetOf", "superset", string.Empty, new object[0]);
            if (CollectionAssert.IsSubsetOfHelper(subset, superset))
                return;
            Assert.HandleFail("CollectionAssert.IsSubsetOf", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="superset"></param>
        public static void IsNotSubsetOf(ICollection subset, ICollection superset)
        {
            CollectionAssert.IsNotSubsetOf(subset, superset, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="superset"></param>
        /// <param name="message"></param>
        public static void IsNotSubsetOf(ICollection subset, ICollection superset, string message)
        {
            CollectionAssert.IsNotSubsetOf(subset, superset, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="superset"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void IsNotSubsetOf(ICollection subset, ICollection superset, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)subset, "CollectionAssert.IsNotSubsetOf", "subset", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)superset, "CollectionAssert.IsNotSubsetOf", "superset", string.Empty, new object[0]);
            if (!CollectionAssert.IsSubsetOfHelper(subset, superset))
                return;
            Assert.HandleFail("CollectionAssert.IsNotSubsetOf", message, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEquivalent(ICollection expected, ICollection actual)
        {
            CollectionAssert.AreEquivalent(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreEquivalent(ICollection expected, ICollection actual, string message)
        {
            CollectionAssert.AreEquivalent(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEquivalent(ICollection expected, ICollection actual, string message, params object[] parameters)
        {
            if (expected == null != (actual == null))
                Assert.HandleFail("CollectionAssert.AreEquivalent", message, parameters);
            if (object.ReferenceEquals((object)expected, (object)actual) || expected == null)
                return;
            if (expected.Count != actual.Count)
                Assert.HandleFail("CollectionAssert.AreEquivalent", (string)FrameworkMessages.ElementNumbersDontMatch(message == null ? (object)string.Empty : (object)message, (object)expected.Count, (object)actual.Count), parameters);
            int expectedCount;
            int actualCount;
            object mismatchedElement;
            if (expected.Count == 0 || !CollectionAssert.FindMismatchedElement(expected, actual, out expectedCount, out actualCount, out mismatchedElement))
                return;
            Assert.HandleFail("CollectionAssert.AreEquivalent", (string)FrameworkMessages.ActualHasMismatchedElements(message == null ? (object)string.Empty : (object)message, (object)expectedCount.ToString((IFormatProvider)CultureInfo.CurrentCulture.NumberFormat), mismatchedElement == null ? (object)"(null)" : (object)mismatchedElement.ToString(), (object)actualCount.ToString((IFormatProvider)CultureInfo.CurrentCulture.NumberFormat)), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreNotEquivalent(ICollection expected, ICollection actual)
        {
            CollectionAssert.AreNotEquivalent(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotEquivalent(ICollection expected, ICollection actual, string message)
        {
            CollectionAssert.AreNotEquivalent(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEquivalent(ICollection expected, ICollection actual, string message, params object[] parameters)
        {
            if (expected == null != (actual == null))
                return;
            if (object.ReferenceEquals((object)expected, (object)actual))
                Assert.HandleFail("CollectionAssert.AreNotEquivalent", (string)FrameworkMessages.BothCollectionsSameReference(message == null ? (object)string.Empty : (object)message), parameters);
            if (expected.Count != actual.Count)
                return;
            if (expected.Count == 0)
                Assert.HandleFail("CollectionAssert.AreNotEquivalent", (string)FrameworkMessages.BothCollectionsEmpty(message == null ? (object)string.Empty : (object)message), parameters);
            int expectedCount;
            int actualCount;
            object mismatchedElement;
            if (CollectionAssert.FindMismatchedElement(expected, actual, out expectedCount, out actualCount, out mismatchedElement))
                return;
            Assert.HandleFail("CollectionAssert.AreNotEquivalent", (string)FrameworkMessages.BothSameElements(message == null ? (object)string.Empty : (object)message), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="expectedType"></param>
        public static void AllItemsAreInstancesOfType(ICollection collection, Type expectedType)
        {
            CollectionAssert.AllItemsAreInstancesOfType(collection, expectedType, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="expectedType"></param>
        /// <param name="message"></param>
        public static void AllItemsAreInstancesOfType(ICollection collection, Type expectedType, string message)
        {
            CollectionAssert.AllItemsAreInstancesOfType(collection, expectedType, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="expectedType"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AllItemsAreInstancesOfType(ICollection collection, Type expectedType, string message, params object[] parameters)
        {
            Assert.CheckParameterNotNull((object)collection, "CollectionAssert.AllItemsAreInstancesOfType", "collection", string.Empty, new object[0]);
            Assert.CheckParameterNotNull((object)expectedType, "CollectionAssert.AllItemsAreInstancesOfType", "expectedType", string.Empty, new object[0]);
            int num = 0;
            foreach (object o in (IEnumerable)collection)
            {
                if (!expectedType.GetTypeInfo().Equals(o.GetType().GetTypeInfo()))
                    Assert.HandleFail("CollectionAssert.AllItemsAreInstancesOfType", o == null ? (string)FrameworkMessages.ElementTypesAtIndexDontMatch2(message == null ? (object)string.Empty : (object)message, (object)num, (object)expectedType.ToString()) : (string)FrameworkMessages.ElementTypesAtIndexDontMatch(message == null ? (object)string.Empty : (object)message, (object)num, (object)expectedType.ToString(), (object)o.GetType().ToString()), parameters);
                ++num;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEqual(ICollection expected, ICollection actual)
        {
            CollectionAssert.AreEqual(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreEqual(ICollection expected, ICollection actual, string message)
        {
            CollectionAssert.AreEqual(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(ICollection expected, ICollection actual, string message, params object[] parameters)
        {
            string reason = string.Empty;
            if (CollectionAssert.AreCollectionsEqual(expected, actual, (IComparer)new CollectionAssert.ObjectComparer(), ref reason))
                return;
            Assert.HandleFail("CollectionAssert.AreEqual", (string)FrameworkMessages.CollectionEqualReason((object)message, (object)reason), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreNotEqual(ICollection expected, ICollection actual)
        {
            CollectionAssert.AreNotEqual(expected, actual, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(ICollection expected, ICollection actual, string message)
        {
            CollectionAssert.AreNotEqual(expected, actual, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(ICollection expected, ICollection actual, string message, params object[] parameters)
        {
            string reason = string.Empty;
            if (!CollectionAssert.AreCollectionsEqual(expected, actual, (IComparer)new CollectionAssert.ObjectComparer(), ref reason))
                return;
            Assert.HandleFail("CollectionAssert.AreNotEqual", (string)FrameworkMessages.CollectionEqualReason((object)message, (object)reason), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        public static void AreEqual(ICollection expected, ICollection actual, IComparer comparer)
        {
            CollectionAssert.AreEqual(expected, actual, comparer, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        /// <param name="message"></param>
        public static void AreEqual(ICollection expected, ICollection actual, IComparer comparer, string message)
        {
            CollectionAssert.AreEqual(expected, actual, comparer, message, (object[])null);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreEqual(ICollection expected, ICollection actual, IComparer comparer, string message, params object[] parameters)
        {
            string reason = string.Empty;
            if (CollectionAssert.AreCollectionsEqual(expected, actual, comparer, ref reason))
                return;
            Assert.HandleFail("CollectionAssert.AreEqual", (string)FrameworkMessages.CollectionEqualReason((object)message, (object)reason), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        public static void AreNotEqual(ICollection expected, ICollection actual, IComparer comparer)
        {
            CollectionAssert.AreNotEqual(expected, actual, comparer, string.Empty, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        /// <param name="message"></param>
        public static void AreNotEqual(ICollection expected, ICollection actual, IComparer comparer, string message)
        {
            CollectionAssert.AreNotEqual(expected, actual, comparer, message, (object[])null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void AreNotEqual(ICollection expected, ICollection actual, IComparer comparer, string message, params object[] parameters)
        {
            string reason = string.Empty;
            if (!CollectionAssert.AreCollectionsEqual(expected, actual, comparer, ref reason))
                return;
            Assert.HandleFail("CollectionAssert.AreNotEqual", (string)FrameworkMessages.CollectionEqualReason((object)message, (object)reason), parameters);
        }

        private static Dictionary<object, int> GetElementCounts(ICollection collection, out int nullCount)
        {
            Dictionary<object, int> dictionary = new Dictionary<object, int>();
            nullCount = 0;
            foreach (object key in (IEnumerable)collection)
            {
                if (key == null)
                {
                    ++nullCount;
                }
                else
                {
                    int num;
                    dictionary.TryGetValue(key, out num);
                    ++num;
                    dictionary[key] = num;
                }
            }
            return dictionary;
        }

        internal static bool IsSubsetOfHelper(ICollection subset, ICollection superset)
        {
            int nullCount1;
            Dictionary<object, int> elementCounts1 = CollectionAssert.GetElementCounts(subset, out nullCount1);
            int nullCount2;
            Dictionary<object, int> elementCounts2 = CollectionAssert.GetElementCounts(superset, out nullCount2);
            if (nullCount1 > nullCount2)
                return false;
            foreach (object key in elementCounts1.Keys)
            {
                int num1;
                elementCounts1.TryGetValue(key, out num1);
                int num2;
                elementCounts2.TryGetValue(key, out num2);
                if (num1 > num2)
                    return false;
            }
            return true;
        }

        private static bool FindMismatchedElement(ICollection expected, ICollection actual, out int expectedCount, out int actualCount, out object mismatchedElement)
        {
            int nullCount1;
            Dictionary<object, int> elementCounts1 = CollectionAssert.GetElementCounts(expected, out nullCount1);
            int nullCount2;
            Dictionary<object, int> elementCounts2 = CollectionAssert.GetElementCounts(actual, out nullCount2);
            if (nullCount2 != nullCount1)
            {
                expectedCount = nullCount1;
                actualCount = nullCount2;
                mismatchedElement = (object)null;
                return true;
            }
            else
            {
                foreach (object key in elementCounts1.Keys)
                {
                    elementCounts1.TryGetValue(key, out expectedCount);
                    elementCounts2.TryGetValue(key, out actualCount);
                    if (expectedCount != actualCount)
                    {
                        mismatchedElement = key;
                        return true;
                    }
                }
                expectedCount = 0;
                actualCount = 0;
                mismatchedElement = (object)null;
                return false;
            }
        }

        private static bool AreCollectionsEqual(ICollection expected, ICollection actual, IComparer comparer, ref string reason)
        {
            Assert.CheckParameterNotNull((object)comparer, "Assert.AreCollectionsEqual", "comparer", string.Empty, new object[0]);
            if (!object.ReferenceEquals((object)expected, (object)actual))
            {
                if (expected == null || actual == null)
                    return false;
                if (expected.Count != actual.Count)
                {
                    reason = (string)FrameworkMessages.NumberOfElementsDiff;
                    return false;
                }
                else
                {
                    IEnumerator enumerator1 = expected.GetEnumerator();
                    IEnumerator enumerator2 = actual.GetEnumerator();
                    int num = 0;
                    while (enumerator1.MoveNext() && enumerator2.MoveNext())
                    {
                        if (0 != comparer.Compare(enumerator1.Current, enumerator2.Current))
                        {
                            reason = (string)FrameworkMessages.ElementsAtIndexDontMatch((object)num);
                            return false;
                        }
                        else
                            ++num;
                    }
                    reason = "(string)FrameworkMessages.BothCollectionsSameElements";
                    return true;
                }
            }
            else
            {
                reason = "(string)FrameworkMessages.BothCollectionsSameReference((object)string.Empty)";
                return true;
            }
        }

        private class ObjectComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return !object.Equals(x, y) ? -1 : 0;
            }
        }
    }
}
#endif // MANAGEDESENT_ON_METRO