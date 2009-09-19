//-----------------------------------------------------------------------
// <copyright file="jet_retrievecolumn.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The native version of the <see cref="JET_RETRIEVECOLUMN"/> structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_RETRIEVECOLUMN
    {
    }

    /// <summary>
    /// Contains input and output parameters for <see cref="Api.JetRetrieveColumns"/>.
    /// Fields in the structure describe what column value to retrieve, how to
    /// retrieve it, and where to save results.
    /// </summary>
    public class JET_RETRIEVECOLUMN
    {
    }
}