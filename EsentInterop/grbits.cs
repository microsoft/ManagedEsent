//-----------------------------------------------------------------------
// <copyright file="grbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Esent.Interop
{
    using System;

    /// <summary>
    /// Options for JetCreateDatabase
    /// </summary>
    [Flags]
    public enum CreateDatabaseGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// By default, if JetCreateDatabase is called and the database already exists,
        /// the API call will fail and the original database will not be overwritten.
        /// OverwriteExisting changes this behavior, and the old database
        /// will be overwritten with a new one.
        /// </summary>
        /// <remarks>
        /// Windows XP and later.
        /// </remarks>
        OverwriteExisting = 0x200,

        /// <summary>
        /// Turns off logging. Setting this bit loses the ability to replay log files
        /// and recover the database to a consistent usable state after a crash.
        /// </summary>
        RecoveryOff = 0x8,
    }

    /// <summary>
    /// Options for JetCommitTransaction
    /// </summary>
    [Flags]
    public enum CommitTransactionGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// The transaction is committed normally but this API does not wait for
        /// the transaction to be flushed to the transaction log file before returning
        /// to the caller. This drastically reduces the duration of a commit operation
        /// at the cost of durability. Any transaction that is not flushed to the log
        /// before a crash will be automatically aborted during crash recovery during
        /// the next call to JetInit. If WaitLastLevel0Commit or WaitAllLevel0Commit
        /// are specified, this option is ignored.
        /// </summary>
        LazyFlush = 0x1,

        /// <summary>
        /// All transactions previously committed by any session that have not yet been
        /// flushed to the transaction log file will be flushed immediately. This API
        /// will wait until the transactions have been flushed before returning to the caller.
        /// </summary>
        /// <remarks>
        /// This option may be used even if the session is not currently in a transaction.
        /// This option cannot be used in combination with any other option.
        /// This option is only available as of Windows Server 2003.
        /// </remarks>
        WaitAllLevel0Commit = 0x8,

        /// <summary>
        ///  If the session has previously committed any transactions and they have not yet
        ///  been flushed to the transaction log file, they should be flushed immediately.
        ///  This API will wait until the transactions have been flushed before returning
        ///  to the caller. This is useful if the application has previously committed several
        ///  transactions using JET_bitCommitLazyFlush and now wants to flush all of them to disk.
        /// </summary>
        /// <remarks>
        /// This option may be used even if the session is not currently in a transaction.
        /// This option cannot be used in combination with any other option.
        /// </remarks>
        WaitLastLevel0Commit = 0x2,
    }

    /// <summary>
    /// Options for JetRollbackTransaction
    /// </summary>
    public enum RollbackTransactionGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// This option requests that all changes made to the state of the
        /// database during all save points be undone. As a result, the
        /// session will exit the transaction.
        /// </summary>
        RollbackAll = 0x1,
    }

    /// <summary>
    /// Options for JetEndSession
    /// </summary>
    public enum EndSessionGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None,
    }

    /// <summary>
    /// Options for JetOpenTable.
    /// </summary>
    [Flags]
    public enum OpenTableGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// This table cannot be opened for read access by another session.
        /// </summary>
        DenyRead,

        /// <summary>
        /// This table cannot be opened for write access by another session.
        /// </summary>
        DenyWrite,

        /// <summary>
        /// Do not cache pages for this table.
        /// </summary>
        NoCache,

        /// <summary>
        /// Allow DDL modifications to a table flagged as FixedDDL. This option
        /// must be used with DenyRead.
        /// </summary>
        PermitDDL,

        /// <summary>
        /// Provides a hint that the table is probably not in the buffer cache, and
        /// that pre-reading may be beneficial to performance.
        /// </summary>
        Preread,

        /// <summary>
        /// Request read-only access to the table.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// Assume a sequential access pattern and prefetch database pages.
        /// </summary>
        Sequential,

        /// <summary>
        /// Request write access to the table.
        /// </summary>
        Updatable,
    }

    /// <summary>
    /// Options for JetSetColumn.
    /// </summary>
    [Flags]
    public enum SetColumnGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// This option is used to append data to a column of type JET_coltypLongText
        /// or JET_coltypLongBinary. The same behavior can be achieved by determining
        /// the size of the existing long value and specifying ibLongValue in psetinfo.
        /// However, its simpler to use this grbit since knowing the size of the existing
        /// column value is not necessary.
        /// </summary>
        AppendLV = 0x1,

        /// <summary>
        /// This option is used replace the existing long value with the newly provided
        /// data. When this option is used, it is as though the existing long value has
        /// been set to 0 (zero) length prior to setting the new data.
        /// </summary>
        OverwriteLV = 0x4,

        /// <summary>
        /// This option is only applicable for tagged, sparse or multi-valued columns.
        /// It causes the column to return the default column value on subsequent retrieve
        /// column operations. All existing column values are removed.
        /// </summary>
        RevertToDefaultValue = 0x200,

        /// <summary>
        /// This option is used to force a long value, columns of type JET_coltyp.LongText
        /// or JET_coltyp.LongBinary, to be stored separately from the remainder of record
        /// data. This occurs normally when the size of the long value prevents it from being 
        /// stored with remaining record data. However, this option can be used to force the
        /// long value to be stored separately. Note that long values four bytes in size
        /// of smaller cannot be forced to be separate. In such cases, the option is ignored.
        /// </summary>
        SeparateLV = 0x40,

        /// <summary>
        /// This option is used to interpret the input buffer as a integer number of bytes
        /// to set as the length of the long value described by the given columnid and if
        /// provided, the sequence number in psetinfo->itagSequence. If the size given is
        /// larger than the existing column value, the column will be extended with 0s.
        /// If the size is smaller than the existing column value then the value will be
        /// truncated.
        /// </summary>
        SizeLV = 0x8,

        /// <summary>
        /// This option is used to enforce that all values in a multi-valued column are
        /// distinct. This option compares the source column data, without any
        /// transformations, to other existing column values and an error is returned
        /// if a duplicate is found. If this option is given, then AppendLV, OverwriteLV
        /// and SizeLV cannot also be given.
        /// </summary>
        UniqueMultiValues = 0x80,

        /// <summary>
        /// This option is used to enforce that all values in a multi-valued column are
        /// distinct. This option compares the key normalized transformation of column
        /// data, to other similarly transformed existing column values and an error is
        /// returned if a duplicate is found. If this option is given, then AppendLV, 
        /// OverwriteLV and SizeLV cannot also be given.
        /// </summary>
        UniqueNormalizedMultiValues = 0x100,

        /// <summary>
        /// This option is used to set a value to zero length. Normally, a column value
        /// is set to NULL by passing a cbMax of 0 (zero). However, for some types, like
        /// JET_coltyp.Text, a column value can be 0 (zero) length instead of NULL, and
        /// this option is used to differentiate between NULL and 0 (zero) length.
        /// </summary>
        ZeroLength = 0x20,
    }

    /// <summary>
    /// Options for JetRetrieveColumn.
    /// </summary>
    [Flags]
    public enum RetrieveColumnGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        ///  This flag causes retrieve column to retrieve the modified value instead of
        ///  the original value. If the value has not been modified, then the original
        ///  value is retrieved. In this way, a value that has not yet been inserted or
        ///  updated may be retrieved during the operation of inserting or updating a record.
        /// </summary>
        RetrieveCopy = 0x1,

        /// <summary>
        /// This option is used to retrieve column values from the index, if possible,
        /// without accessing the record. In this way, unnecessary loading of records
        /// can be avoided when needed data is available from index entries themselves.
        /// </summary>
        RetrieveFromIndex = 0x2,
        
        /// <summary>
        /// This option is used to retrieve column values from the index bookmark,
        /// and may differ from the index value when a column appears both in the
        /// primary index and the current index. This option should not be specified
        /// if the current index is the clustered, or primary, index. This bit cannot
        /// be set if RetrieveFromIndex is also set. 
        /// </summary>
        RetrieveFromPrimaryBookmark = 0x4,

        /// <summary>
        /// This option is used to retrieve the sequence number of a multi-valued
        /// column value in JET_RETINFO.itagSequence. Retrieving the sequence number
        /// can be a costly operation and should only be done if necessary. 
        /// </summary>
        RetrieveTag = 0x8,

        /// <summary>
        /// This option is used to retrieve multi-valued column NULL values. If
        /// this option is not specified, multi-valued column NULL values will
        /// automatically be skipped. 
        /// </summary>
        RetrieveNull = 0x10,

        /// <summary>
        /// This option affects only multi-valued columns and causes a NULL
        /// value to be returned when the requested sequence number is 1 and
        /// there are no set values for the column in the record. 
        /// </summary>
        RetrieveIgnoreDefault = 0x20,

        /// <summary>
        /// This flag will allow the retrieval of a tuple segment of the index.
        /// This bit must be specified with RetrieveFromIndex. 
        /// </summary>
        RetrieveTuple = 0x800,
    }

    /// <summary>
    /// Options for the JET_COLUMNDEF structure.
    /// </summary>
    [Flags]
    public enum ColumndefGrbit : int
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The column will be fixed. It will always use the same amount of space in a row,
        /// regardless of how much data is being stored in the column. ColumnFixed
        /// cannot be used with ColumnTagged. This bit cannot be used with long values
        /// (that is JET_coltyp.LongText and JET_coltyp.LongBinary).
        /// </summary>
        ColumnFixed = 0x1,

        /// <summary>
        ///  The column will be tagged. Tagged columns do not take up any space in the database
        ///  if they do not contain data. This bit cannot be used with ColumnFixed.
        /// </summary>
        ColumnTagged = 0x2,

        /// <summary>
        /// The column must never be set to a NULL value.
        /// </summary>
        ColumnNotNULL = 0x4,

        /// <summary>
        /// The column is a version column that specifies the version of the row. The value of
        /// this column starts at zero and will be automatically incremented for each update on
        /// the row. This option can only be applied to JET_coltyp.Long columns. This option cannot
        /// be used with ColumnAutoincrement, ColumnEscrowUpdate, or ColumnTagged.
        /// </summary>
        ColumnVersion = 0x8,

        /// <summary>
        /// The column will automatically be incremented. The number is an increasing number, and
        /// is guaranteed to be unique within a table. The numbers, however, might not be continuous.
        /// For example, if five rows are inserted into a table, the "autoincrement" column could
        /// contain the values { 1, 2, 6, 7, 8 }. This bit can only be used on columns of type
        /// JET_coltyp.Long or JET_coltyp.Currency.
        /// </summary>
        /// <remarks>
        /// Windows 2000:  In Windows 2000, this bit can only be used on columns of type JET_coltyp.Long. 
        /// </remarks>
        ColumnAutoincrement = 0x10,

        /// <summary>
        /// The column can be multi-valued.
        /// A multi-valued column can have zero, one, or more values
        /// associated with it. The various values in a multi-valued column are identified by a number
        /// called the itagSequence member, which belongs to various structures, including:
        /// JET_RETINFO, JET_SETINFO, JET_SETCOLUMN, JET_RETRIEVECOLUMN, and JET_ENUMCOLUMNVALUE.
        /// Multi-valued columns must be tagged columns; that is, they cannot be fixed-length or
        /// variable-length columns.
        /// </summary>
        ColumnMultiValued = 0x400,

        /// <summary>
        ///  Specifies that a column is an escrow update column. An escrow update column can be
        ///  updated concurrently by different sessions with JetEscrowUpdate and will maintain
        ///  transactional consistency. An escrow update column must also meet the following conditions:
        ///  An escrow update column can be created only when the table is empty. 
        ///  An escrow update column must be of type JET_coltypLong. 
        ///  An escrow update column must have a default value (that is cbDefault must be positive). 
        ///  JET_bitColumnEscrowUpdate cannot be used in conjunction with ColumnTagged,
        ///  ColumnVersion, or ColumnAutoincrement. 
        /// </summary>
        ColumnEscrowUpdate = 0x800,

        /// <summary>
        /// The column will be created in an without version information. This means that other
        /// transactions that attempt to add a column with the same name will fail. This bit
        /// is only useful with JetAddColumn. It cannot be used within a transaction.
        /// </summary>
        ColumnUnversioned = 0x1000,

        /// <summary>
        /// The default value for a column will be provided by a callback function. A column that
        /// has a user-defined default must be a tagged column. Specifying JET_bitColumnUserDefinedDefault
        /// means that pvDefault must point to a JET_USERDEFINEDDEFAULT structure, and cbDefault must be
        /// set to sizeof( JET_USERDEFINEDDEFAULT ).
        /// </summary>
        ColumnUserDefinedDefault = 0x8000,

        /// <summary>
        /// The column is an escrow update column, and when it reaches zero, the record will be
        /// deleted. A common use for a column that can be finalized is to use it as a reference
        /// count field, and when the field reaches zero the record gets deleted.
        /// A Delete-on-zero column must be an escrow update column. 
        /// ColumnDeleteOnZero cannot be a user defined default column.
        /// </summary>
        ColumnDeleteOnZero = 0x20000,
    }
}