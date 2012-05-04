//-----------------------------------------------------------------------
// <copyright file="EsentVersionTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Server2003;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows7;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the static version class
    /// </summary>
    [TestClass]
    public class EsentVersionTests
    {
        /// <summary>
        /// Print the current version of Esent (for debugging).
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Print the current version of Esent (for debugging)")]
        public void PrintVersion()
        {
            if (EsentVersion.SupportsServer2003Features)
            {
                EseInteropTestHelper.ConsoleWriteLine("SupportsServer2003Features");    
            }

            if (EsentVersion.SupportsVistaFeatures)
            {
                EseInteropTestHelper.ConsoleWriteLine("SupportsVistaFeatures");
            }

            if (EsentVersion.SupportsWindows7Features)
            {
                EseInteropTestHelper.ConsoleWriteLine("SupportsWindows7Features");
            }

            if (EsentVersion.SupportsUnicodePaths)
            {
                EseInteropTestHelper.ConsoleWriteLine("SupportsUnicodePaths");
            }

            if (EsentVersion.SupportsLargeKeys)
            {
                EseInteropTestHelper.ConsoleWriteLine("SupportsLargeKeys");
            }
        }

        /// <summary>
        /// If Windows 8 is supported then older features must be 
        /// supported too.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("If Windows 8 is supported then older features must be supported too")]
        public void VerifyWindows8FeaturesIncludesOlderFeatures()
        {
            if (EsentVersion.SupportsWindows8Features)
            {
                Assert.IsTrue(EsentVersion.SupportsWindows7Features);
                Assert.IsTrue(EsentVersion.SupportsServer2003Features);
                Assert.IsTrue(EsentVersion.SupportsVistaFeatures);
                Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
                Assert.IsTrue(EsentVersion.SupportsLargeKeys);
            }
        }

        /// <summary>
        /// If Windows 7 is supported then older features must be 
        /// supported too.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("If Windows 7 is supported then older features must be supported too")]
        public void VerifyWindows7FeaturesIncludesOlderFeatures()
        {
            if (EsentVersion.SupportsWindows7Features)
            {
                Assert.IsTrue(EsentVersion.SupportsServer2003Features);
                Assert.IsTrue(EsentVersion.SupportsVistaFeatures);
                Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
                Assert.IsTrue(EsentVersion.SupportsLargeKeys);
            }
        }

        /// <summary>
        /// If Windows Vista is supported then older features must be 
        /// supported too.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("If Windows Vista is supported then older features must be supported too")]
        public void VerifyWindowsVistaFeaturesIncludesOlderFeatures()
        {
            if (EsentVersion.SupportsVistaFeatures)
            {
                Assert.IsTrue(EsentVersion.SupportsServer2003Features);
                Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
                Assert.IsTrue(EsentVersion.SupportsLargeKeys);
            }
        }

        /// <summary>
        /// Prints a list of all the Jet APIs.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Prints a list of all the Jet APIs")]
        public void ListAllApis()
        {
            EseInteropTestHelper.ConsoleWriteLine("Api");
            int totalApis = PrintJetApiNames(typeof(Api));
            EseInteropTestHelper.ConsoleWriteLine("Server2003Api");
            totalApis += PrintJetApiNames(typeof(Server2003Api));
            EseInteropTestHelper.ConsoleWriteLine("VistaApi");
            totalApis += PrintJetApiNames(typeof(VistaApi));
            EseInteropTestHelper.ConsoleWriteLine("Windows7Api");
            totalApis += PrintJetApiNames(typeof(Windows7Api));
            EseInteropTestHelper.ConsoleWriteLine("Windows8Api");
            totalApis += PrintJetApiNames(typeof(Windows8Api));
            EseInteropTestHelper.ConsoleWriteLine("Total APIs: {0}", totalApis);
        }

        /// <summary>
        /// Prints a sorted list of the Jet apis in the given type.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>The number of APIs found in the type.</returns>
        private static int PrintJetApiNames(Type type)
        {
            int numApisFound = 0;
            foreach (string method in GetJetApiNames(type).OrderBy(x => x).Distinct())
            {
                EseInteropTestHelper.ConsoleWriteLine("\t{0}", method);
                numApisFound++;
            }

            return numApisFound;
        }

        /// <summary>
        /// Returns the names of all the static methods in the given type
        /// that start with 'Jet'.
        /// </summary>
        /// <param name="type">The type to look at.</param>
        /// <returns>
        /// An enumeration of all the static methods in the type that
        /// start with 'Jet'.
        /// </returns>
        private static IEnumerable<string> GetJetApiNames(Type type)
        {
#if MANAGEDESENT_ON_METRO
            foreach (MemberInfo member in type.GetTypeInfo().DeclaredMethods)
            {
                if (member.Name.StartsWith("Jet"))
                {
                    yield return member.Name;
                }
            }
#else
            foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                if (member.Name.StartsWith("Jet") && (member.MemberType == MemberTypes.Method))
                {
                    yield return member.Name;
                }
            }
#endif
        }
    }
}