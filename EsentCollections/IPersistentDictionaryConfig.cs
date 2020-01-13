// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPersistentDictionaryConfig.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   An interface for meta-data configuration for the dictionary database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// An interface for meta-data configuration for the dictionary database.
    /// </summary>
    public interface IPersistentDictionaryConfig
    {
        /// <summary>
        /// Gets a string describing the current version of the 
        /// PersistentDictionary.
        /// </summary>
        /// <value>
        /// A string describing the current version of the 
        /// PersistentDictionary.
        /// </value>
        string Version { get; }

        /// <summary>
        /// Gets the name of the globals table.
        /// </summary>
        /// <value>
        /// The name of the globals table.
        /// </value>
        string GlobalsTableName { get; }

        /// <summary>
        /// Gets the name of the version column in the globals table.
        /// </summary>
        /// <value>
        /// The name of the version column in the globals table.
        /// </value>
        string VersionColumnName { get; }

        /// <summary>
        /// Gets the name of the count column in the globals table.
        /// </summary>
        /// <value>
        /// The name of the count column in the globals table.
        /// This column tracks the number of items in the collection.
        /// </value>
        string CountColumnName { get; }

        /// <summary>
        /// Gets the name of the flush column in the globals table.
        /// </summary>
        /// <value>
        /// The name of the flush column in the globals table.
        /// This column is updated when a Flush operation is performed.
        /// </value>
        string FlushColumnName { get; }

        /// <summary>
        /// Gets the name of the key type column in the globals table.
        /// </summary>
        /// <value>
        /// The name of the key type column in the globals table.
        /// This column stores the type of the key in the dictionary.
        /// </value>
        string KeyTypeColumnName { get; }

        /// <summary>
        /// Gets the name of the key type column in the globals table.
        /// This column stores the type of the key in the dictionary.
        /// </summary>
        string KeyTypeNameColumnName { get; }

        /// <summary>
        /// Gets the name of the value type column in the globals table.
        /// </summary>
        /// <value>
        /// The name of the value type column in the globals table.
        /// This column stores the type of the value in the dictionary.
        /// </value>
        string ValueTypeColumnName { get; }

        /// <summary>
        /// Gets the name of the value type name column in the globals table.
        /// This column stores the name of the type of the value in the dictionary.
        /// </summary>
        string ValueTypeNameColumnName { get; }

        /// <summary>
        /// Gets the name of the data table.
        /// </summary>
        /// <value>
        /// The name of the data table.
        /// </value>
        string DataTableName { get; }

        /// <summary>
        /// Gets the name of the key column in the data table.
        /// </summary>
        /// <value>
        /// The name of the key column in the data table.
        /// This column stores the key of the item.
        /// </value>
        string KeyColumnName { get; }

        /// <summary>
        /// Gets the name of the value column in the data table.
        /// </summary>
        /// <value>
        /// The name of the value column in the data table.
        /// This column stores the value of the item.
        /// </value>
        string ValueColumnName { get; }
    }
}