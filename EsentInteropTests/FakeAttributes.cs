//-----------------------------------------------------------------------
// <copyright file="FakeAttributes.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Copied classes to remove dependency on Wstf
    /// Enables building under Visual Studio without Wstf present
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Suppression is OK here because it's a collection of related trivial classes.")]
    public abstract class TestIdAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the test id
        /// </summary>
        public virtual int Id { get; set; }
    }

    /// <summary>
    /// Copied classes to remove dependency on Wstf
    /// Enables building under Visual Studio without Wstf present
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Suppression is OK here because it's a collection of related trivial classes.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExTcmIdAttribute : TestIdAttribute
    {
        /// <summary>
        /// Initializes a new instance of the ExTcmIdAttribute class
        /// </summary>
        /// <param name="id">TCM Id of the test</param>
        public ExTcmIdAttribute(int id)
        {
            this.Id = id;
        }
    }

    /// <summary>
    /// Copied classes to remove dependency on Wstf
    /// Enables building under Visual Studio
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Suppression is OK here because it's a collection of related trivial classes.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class WttIdAttribute : TestIdAttribute
    {
        /// <summary>
        /// Initializes a new instance of the WttIdAttribute class
        /// </summary>
        /// <param name="id">WTT Id of the test</param>
        public WttIdAttribute(int id)
        {
            this.Id = id;
        }
    }
}
