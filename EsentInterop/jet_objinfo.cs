//-----------------------------------------------------------------------
// <copyright file="jet_objinfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Info levels for retrieving column info.
    /// </summary>
    internal enum JET_ObjInfo
    {
        /// <summary>
        /// Retrieve a JET_OBJINFOLIST containing information
        /// about all object in the table.
        /// </summary>
        ListNoStats = 1,

        /// <summary>
        /// Retrive a JET_OBJINFO containing information about
        /// the object.
        /// </summary>
        NoStats = 3,
    }
}
