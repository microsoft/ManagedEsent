//-----------------------------------------------------------------------
// <copyright file="VSTestDescriptors.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Internal.Ese.Wstf;
    using Internal.Ese.Wstf.Utils;
    using VS = Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// A custom TestListProvider for enumerating Visual studio tests.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Suppression is OK here because it's a collection of related trivial classes.")]
    public class VSTestListProvider : TestListProvider
    {
        /// <summary>
        /// Initializes a new instance of the VSTestListProvider class
        /// </summary>
        /// <param name="assembly">Associated assembly</param>
        public VSTestListProvider(Assembly assembly)
            : this(assembly, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the VSTestListProvider class
        /// </summary>
        /// <param name="assembly">Associated assembly</param>
        /// <param name="attrs">Assembly attributes</param>
        public VSTestListProvider(Assembly assembly, IEnumerable<Attribute> attrs)
            : base(assembly, attrs)
        {
        }

        /// <summary>
        /// Returns true if a type is a valid test suite.
        /// </summary>
        /// <param name="implementingType">The type to check.</param>
        /// <returns>Returns true if the type is a valid test suite.</returns>
        public override bool IsValidTestSuite(Type implementingType)
        {
            var typeInfo = implementingType.GetTypeInfo();
            VS.TestClassAttribute testClass;
            if (typeInfo.TryGetAttribute(out testClass))
            {
                return true;
            }
            else
            {
                return base.IsValidTestSuite(implementingType);
            }
        }

        /// <summary>
        /// Gets the default TestSuiteDescriptor for a type if none was specified.
        /// </summary>
        /// <param name="implementingType">Type to query</param>
        /// <returns>The default test suite descriptor for the catalog</returns>
        protected override TestSuiteDescriptor GetDefaultTestSuiteDescriptor(Type implementingType)
        {
            var typeInfo = implementingType.GetTypeInfo();
            var attrs = typeInfo.GetCustomAttributes(typeof(VS.TestClassAttribute));
            if (attrs.FirstOrDefault() == null)
            {
                throw new InvalidOperationException(implementingType.Name + " is not a valid TestClass");
            }

            return new VSTestSuiteDescriptor(this, implementingType, typeInfo.GetCustomAttributes());
        }
    }

    /// <summary>
    /// A custom TestSuiteDescriptor that provides metadata for a Visual studio test suite.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Suppression is OK here because it's a collection of related trivial classes.")]
    public class VSTestSuiteDescriptor : TestSuiteDescriptor
    {
        /// <summary>
        /// Default test owner
        /// </summary>
        public const string DefaultOwner = "esetest";

        /// <summary>
        /// Default test priority
        /// </summary>
        public const int DefaultPriority = 2;

        /// <summary>
        /// Initializes a new instance of the VSTestSuiteDescriptor class.
        /// </summary>
        /// <param name="provider">Parent test list provider.</param>
        /// <param name="implementingType">Associated type.</param>
        public VSTestSuiteDescriptor(TestListProvider provider, Type implementingType)
            : this(provider, implementingType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the VSTestSuiteDescriptor class.
        /// </summary>
        /// <param name="provider">Parent test list provider.</param>
        /// <param name="implementingType">Associated type.</param>
        /// <param name="typeAttrs">Attributes defined on the associated types.</param>
        public VSTestSuiteDescriptor(TestListProvider provider, Type implementingType, IEnumerable<Attribute> typeAttrs)
            : base(provider, implementingType, typeAttrs)
        {
            // Visual studio tests dont have a title attribute
            this.Title = implementingType.Name;

            VS.DescriptionAttribute da;
            if (this.typeAttrs.TryGetAttribute(out da))
            {
                this.Description = da.Description;
            }

            VS.OwnerAttribute oa;
            if (this.typeAttrs.TryGetAttribute(out oa))
            {
                this.Owner = oa.Owner;
            }
            else
            {
                this.Owner = DefaultOwner;
            }

            Func<Type, Attribute[], Attribute> containsAttribute = (type, attrs) =>
            {
                foreach (var attr in attrs)
                {
                    if (type == attr.GetType())
                    {
                        return attr;
                    }
                }
                
                return null;
            };

            var allMethods = this.ImplementingType.GetTypeInfo().GetMethods();
            this.TestDescriptors = new SortedDictionary<string, TestMethodDescriptor>();
            foreach (var method in allMethods)
            {
                var methodAttrs = (Attribute[])method.GetCustomAttributes(typeof(Attribute), false);
                if (methodAttrs != null)
                {
                    if (null != containsAttribute(typeof(VS.TestMethodAttribute), methodAttrs))
                    {
                        var methodDesc = this.BuildTestMethodDescriptor(method);
                        this.TestDescriptors.Add(method.Name, methodDesc);
                    }
                    else if (null != containsAttribute(typeof(VS.ClassInitializeAttribute), methodAttrs))
                    {
                        var methodDesc = this.BuildTestMethodDescriptor(method);
                        Assert.IsNull(this.SuiteInitDesc, "Duplicate method in test suite");
                        this.SuiteInitDesc = methodDesc;
                    }
                    else if (null != containsAttribute(typeof(VS.ClassCleanupAttribute), methodAttrs))
                    {
                        var methodDesc = this.BuildTestMethodDescriptor(method);
                        Assert.IsNull(this.SuiteCleanupDesc, "Duplicate method in test suite");
                        this.SuiteCleanupDesc = methodDesc;
                    }
                    else if (null != containsAttribute(typeof(VS.TestInitializeAttribute), methodAttrs))
                    {
                        var methodDesc = this.BuildTestMethodDescriptor(method);
                        Assert.IsNull(this.TestInitDesc, "Duplicate method in test suite");
                        this.TestInitDesc = methodDesc;
                    }
                    else if (null != containsAttribute(typeof(VS.TestCleanupAttribute), methodAttrs))
                    {
                        var methodDesc = this.BuildTestMethodDescriptor(method);
                        Assert.IsNull(this.TestCleanupDesc, "Duplicate method in test suite");
                        this.TestCleanupDesc = methodDesc;
                    }
                }
            }

            // Unsupported methods
            this.SetupDesc = null;
            this.TestAbortDesc = null;
        }

        /// <summary>
        /// A descriptor for the Setup method.
        /// </summary>
        public override TestMethodDescriptor SetupDesc { get; protected set; }

        /// <summary>
        /// A descriptor for the SuiteInit method.
        /// </summary>
        public override TestMethodDescriptor SuiteInitDesc { get; protected set; }

        /// <summary>
        /// A descriptor for the SuiteCleanup method.
        /// </summary>
        public override TestMethodDescriptor SuiteCleanupDesc { get; protected set; }

        /// <summary>
        /// A descriptor for the TestInit method.
        /// </summary>
        public override TestMethodDescriptor TestInitDesc { get; protected set; }

        /// <summary>
        /// A descriptor for the TestCleanup method.
        /// </summary>
        public override TestMethodDescriptor TestCleanupDesc { get; protected set; }

        /// <summary>
        /// A descriptor for the TestAbort method.
        /// </summary>
        public override TestMethodDescriptor TestAbortDesc { get; protected set; }

        /// <summary>
        /// Descriptors for the test methods for the suite.
        /// </summary>
        public override SortedDictionary<string, TestMethodDescriptor> TestDescriptors { get; protected set; }

        /// <summary>
        /// Returns the TestRunAdapter capable of running this test suite
        /// </summary>
        /// <returns>Type of the TestRunApdater associated with the test suite</returns>
        public override Type GetTestRunner()
        {
            return typeof(VSTestRunAdapter);
        }

        /// <summary>
        /// Creates an instance of the test suite
        /// </summary>
        /// <returns>A test suite instance</returns>
        public override object CreateInstance()
        {
            var testClass = Activator.CreateInstance(this.ImplementingType);
            return testClass;
        }

        /// <summary>
        /// Builds a TestMethodDescriptor from MethodInfo
        /// </summary>
        /// <param name="methodInfo">A MethodInfo to reflect on</param>
        /// <returns>A TestMethodDescriptor for the method</returns>
        private TestMethodDescriptor BuildTestMethodDescriptor(MethodInfo methodInfo)
        {
            string title = this.Title;
            string desc = this.Description;
            string owner = this.Owner;
            int id = 0;
            int priority = DefaultPriority;

            // Visual studio tests dont have a title attribute
            this.Title = methodInfo.Name;

            VS.DescriptionAttribute da;
            if (methodInfo.TryGetAttribute(out da))
            {
                desc = da.Description;
                title = desc;
            }

            VS.OwnerAttribute oa;
            if (methodInfo.TryGetAttribute(out oa))
            {
                owner = oa.Owner;
            }
            else
            {
                owner = VSTestSuiteDescriptor.DefaultOwner;
            }

            VS.PriorityAttribute pa;
            if (methodInfo.TryGetAttribute(out pa))
            {
                priority = pa.Priority;
            }
           
            // Read tcmid/wttid from AttributeStore
            id = GetTestId(methodInfo.Name);

            TimeSpan timeout = TimeSpan.Zero;
            VS.TimeoutAttribute timeoutAttr;
            if (methodInfo.TryGetAttribute(out timeoutAttr))
            {
                timeout = TimeSpan.FromMilliseconds(timeoutAttr.Timeout);
            }
            else
            {
                timeout = TimeSpan.FromMinutes(1.0);
            }

            return new TestMethodDescriptor(this, methodInfo.Name, methodInfo, id, timeout, title, desc, owner, priority);
        }
    }

    /// <summary>
    /// A custom TestRunAdapter for running Visual studio tests.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass",
        Justification = "Suppression is OK here because it's a collection of related trivial classes.")]
    public class VSTestRunAdapter : TestRunAdapter
    {
        /// <summary>
        /// Captures the StdOut output to the Wstf logger
        /// </summary>
        private LoggedTextWriter loggedTextWriter;

        /// <summary>
        /// Old text writer saved for restoring later
        /// </summary>
        private TextWriter oldTextWriter;

        /// <summary>
        /// Initializes a new instance of the VSTestRunAdapter class
        /// </summary>
        /// <param name="suiteDesc">The associated TestSuiteDescriptor</param>
        /// <param name="rc">A run context object provided by the test harness</param>
        public VSTestRunAdapter(TestSuiteDescriptor suiteDesc, IRunContext rc)
            : base(suiteDesc, rc)
        {
        }

        /// <summary>
        /// Common setup for all InteropApiTests
        /// </summary>
        /// <param name="sc">Setup context</param>
        public override void Setup(ISetupContext sc)
        {
            // Copy common binaries required for running InteropApiTests
            sc.CopyBinary("microsoft.isam.esent.interop.dll");
            sc.CopyBinary("microsoft.isam.esent.interop.types.dll");
            
            sc.CopyBinary("Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll");
            sc.CopyBinary("Microsoft.VisualStudio.QualityTools.Resource.dll");
            sc.CopyBinary("Rhino.Mocks.dll");
            sc.CopyBinary("Microsoft.Exchange.Diagnostics.dll");
        }

        /// <summary>
        /// Suite Initialize.
        /// </summary>
        public override void SuiteInit()
        {
            this.oldTextWriter = Console.Out;
            this.loggedTextWriter = new LoggedTextWriter(this.RunContext.Logger, this.oldTextWriter);
            Console.SetOut(this.loggedTextWriter);
            base.SuiteInit();
        }

        /// <summary>
        /// Suite Cleanup
        /// </summary>
        public override void SuiteCleanup()
        {
            base.SuiteCleanup();
            Console.SetOut(this.oldTextWriter);
            this.loggedTextWriter.Dispose();
        }

        /// <summary>
        /// Runs a test
        /// </summary>
        /// <param name="methodDesc">Method to run</param>
        public override void RunTest(TestMethodDescriptor methodDesc)
        {
            // Must use Console.WriteLine() for logging in this method
            // because StdOut has been redirected. Using the logger directly
            // will print each log twice.
            Console.WriteLine("{0}.{1}():", methodDesc.ImplementingType.Name, methodDesc.Name);
            var start = DateTime.Now;

            try
            {
                VS.ExpectedExceptionAttribute expectedExcepAttr;
                if (methodDesc.Method.TryGetAttribute(out expectedExcepAttr))
                {
                    // Run tests that expect an exception
                    try
                    {
                        base.RunTest(methodDesc);
                        throw new WstfTestException("Test failed to thrown expected exception: " + expectedExcepAttr.ExceptionType.Name);
                    }
                    catch (Exception ex)
                    {
                        var actualEx = ex;
                        if (ex is TargetInvocationException)
                        {
                            actualEx = ((TargetInvocationException)ex).InnerException;
                        }

                        if (actualEx.GetType() != expectedExcepAttr.ExceptionType)
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    // Run normal tests
                    base.RunTest(methodDesc);
                }
            }
            catch (VS.AssertInconclusiveException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Test will pass");
            }

            var end = DateTime.Now;
            Console.WriteLine("\tFinished in {0}", end - start);
        }
    }

    /// <summary>
    /// A TextWriter that redirects StdOut to Wstf logger
    /// </summary>
    internal class LoggedTextWriter : TextWriter
    {
        /// <summary>
        /// Flag for detecting recursive calls
        /// </summary>
        [ThreadStatic]
        private static bool isRecursive;

        /// <summary>
        /// Wstf logger
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Old TextWriter
        /// </summary>
        private TextWriter oldTextWriter;

        /// <summary>
        /// Initializes a new instance of the LoggedTextWriter class
        /// </summary>
        /// <param name="logger">Wstf logger</param>
        /// <param name="oldTextWriter">Old TextWriter</param>
        public LoggedTextWriter(ILogger logger, TextWriter oldTextWriter)
        {
            this.logger = logger;
            this.oldTextWriter = oldTextWriter;
        }

        /// <summary>
        /// Default encoding
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        /// <summary>
        /// Redirects all writes to the Wstf logger
        /// </summary>
        /// <param name="value">string to write</param>
        public override void Write(string value)
        {
            if (!isRecursive)
            {
                isRecursive = true;
                this.logger.LogInfo(value);
                isRecursive = false;
            }
            else
            {
                this.oldTextWriter.Write(value);
            }
        }

        /// <summary>
        /// Redirects all writes to the Wstf logger
        /// </summary>
        /// <param name="value">string to write</param>
        public override void Write(char value)
        {
            if (!isRecursive)
            {
                isRecursive = true;
                this.logger.LogInfo(value.ToString());
                isRecursive = false;
            }
            else
            {
                this.oldTextWriter.Write(value);
            }
        }

        /// <summary>
        /// Redirects all writes to the Wstf logger
        /// </summary>
        /// <param name="value">string to write</param>
        public override void WriteLine(string value)
        {
            if (!isRecursive)
            {
                isRecursive = true;
                this.logger.LogInfo(value);
                isRecursive = false;
            }
            else
            {
                this.oldTextWriter.WriteLine(value);
            }
        }
    }
}
