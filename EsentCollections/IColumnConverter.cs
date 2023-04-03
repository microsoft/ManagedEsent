// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IColumnConverter.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Contains methods to set and get data from the ESENT database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using Microsoft.Isam.Esent.Interop;
    
    /// <summary>
    /// Represents a SetColumn operation.
    /// </summary>
    /// <param name="sesid">The session to use.</param>
    /// <param name="tableid">The cursor to set the value in. An update should be prepared.</param>
    /// <param name="columnid">The column to set.</param>
    /// <param name="value">The value to set.</param>
    public delegate void SetColumnDelegate<TColumn>(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, TColumn value);

    /// <summary>
    /// Represents a RetrieveColumn operation.
    /// </summary>
    /// <param name="sesid">The session to use.</param>
    /// <param name="tableid">The cursor to retrieve the value from.</param>
    /// <param name="columnid">The column to retrieve.</param>
    /// <returns>The retrieved value.</returns>
    public delegate TColumn RetrieveColumnDelegate<TColumn>(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid);
    
    /// <summary>
    /// Contains methods to set and get data from the ESENT database.
    /// </summary>
    public interface IColumnConverter<TColumn>
    {
        /// <summary>
        /// Gets a delegate that can be used to set the Key column with an object of
        /// </summary>
        SetColumnDelegate<TColumn> ColumnSetter { get; }

        /// <summary>
        /// Gets a delegate that can be used to retrieve the Key column, returning
        /// </summary>
        RetrieveColumnDelegate<TColumn> ColumnRetriever { get; }

        /// <summary>
        /// Gets the type of database column the value should be stored in.
        /// </summary>
        JET_coltyp Coltyp { get; }
    }
}