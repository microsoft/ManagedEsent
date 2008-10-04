//-----------------------------------------------------------------------
// <copyright file="types.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Esent.Interop
{
    using System;

    /// <summary>
    /// A JET_INSTANCE contains a handle to the instance of the database to use for calls to the JET API.
    /// </summary>
    public struct JET_INSTANCE
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal IntPtr Value;

        /// <summary>
        /// Gets a null JET_INSTANCE.
        /// </summary>
        public static JET_INSTANCE Nil
        {
            get { return new JET_INSTANCE(); }
        }
    }

    /// <summary>
    /// A JET_SESID contains a handle to the session to use for calls to the JET API.
    /// </summary>
    public struct JET_SESID
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal IntPtr Value;

        /// <summary>
        /// Gets a null JET_SESID.
        /// </summary>
        public static JET_SESID Nil
        {
            get { return new JET_SESID(); }
        }
    }

    /// <summary>
    /// A JET_TABLEID contains a handle to the database cursor to use for a call to the JET API.
    /// A cursor can only be used with the session that was used to open that cursor.
    /// </summary>
    public struct JET_TABLEID
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal IntPtr Value;

        /// <summary>
        /// Gets a null JET_TABLEID.
        /// </summary>
        public static JET_TABLEID Nil
        {
            get { return new JET_TABLEID(); }
        }
    }

    /// <summary>
    /// A JET_DBID contains the handle to the database. A database handle is used to manage the
    /// schema of a database. It can also be used to manage the tables inside of that database.
    /// </summary>
    public struct JET_DBID
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal uint Value;

        /// <summary>
        /// Gets a null JET_DBID.
        /// </summary>
        public static JET_DBID Nil
        {
            get { return new JET_DBID(); }
        }
    }

    /// <summary>
    /// A JET_COLUMNID identifies a column within a table.
    /// </summary>
    public struct JET_COLUMNID
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal uint Value;

        /// <summary>
        /// Gets a null JET_COLUMNID.
        /// </summary>
        public static JET_COLUMNID Nil
        { 
            get { return new JET_COLUMNID(); }
        }
    }
}
