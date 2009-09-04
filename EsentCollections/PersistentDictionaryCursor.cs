//-----------------------------------------------------------------------
// <copyright file="PersistentDictionaryCursor.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Isam.Esent.Interop;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Combines a JET_SESID and JET_TABLEID into a cursor which can
    /// retrieve data from and update a PersistentDictionary database.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal class PersistentDictionaryCursor<TKey, TValue> : IDisposable where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The ESENT instance the cursor is opened against.
        /// </summary>
        private readonly Instance instance;

        /// <summary>
        /// The ESENT session the cursor is using.
        /// </summary>
        private readonly Session session;

        /// <summary>
        /// Converters used to interact with ESENT.
        /// </summary>
        private readonly PersistentDictionaryConverters<TKey, TValue> converters;

        /// <summary>
        /// Database meta-data configuration.
        /// </summary>
        private readonly PersistentDictionaryConfig config;

        /// <summary>
        /// The database to use.
        /// </summary>
        private readonly string database;

        /// <summary>
        /// ID of the opened database.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// ID of the opened globals table.
        /// </summary>
        private JET_TABLEID globalsTable;

        /// <summary>
        /// ID of the count column in the globals table. This stores the
        /// number of items in the collection.
        /// </summary>
        private JET_COLUMNID countColumn;

        /// <summary>
        /// ID of the flush column in the globals table. This is updated
        /// when we want to flush the database.
        /// </summary>
        private JET_COLUMNID flushColumn;

        /// <summary>
        /// ID of the opened data table.
        /// </summary>
        private JET_TABLEID dataTable;

        /// <summary>
        /// ID of the key column in the data table. This stores the keys of
        /// the items.
        /// </summary>
        private JET_COLUMNID keyColumn;

        /// <summary>
        /// ID of the value column in the data table. This stores the values
        /// of the items.
        /// </summary>
        private JET_COLUMNID valueColumn;

        /// <summary>
        /// Initializes a new instance of the PersistentDictionaryCursor class.
        /// </summary>
        /// <param name="instance">The instance to use.</param>
        /// <param name="database">The database to open.</param>
        /// <param name="converters">ESENT conversion functions.</param>
        /// <param name="config">The database meta-data config.</param>
        public PersistentDictionaryCursor(
            Instance instance,
            string database,
            PersistentDictionaryConverters<TKey, TValue> converters,
            PersistentDictionaryConfig config)
        {
            this.instance = instance;
            this.converters = converters;
            this.config = config;
            this.database = database;
            this.session = new Session(this.instance);
            this.AttachDatabase();
        }

        /// <summary>
        /// Begin a new transaction for this cursor.
        /// </summary>
        /// <returns>The new transaction.</returns>
        public Transaction BeginTransaction()
        {
            return new Transaction(this.session);
        }

        #region Navigation

        /// <summary>
        /// Try to find the specified key. If the key is found
        /// the cursor will be positioned on the record with the
        /// key.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool TrySeek(TKey key)
        {
            this.MakeKey(key);
            return Api.TrySeek(this.session, this.dataTable, SeekGrbit.SeekEQ);
        }

        /// <summary>
        /// Seek for the specified key. If the key is found the
        /// cursor will be positioned with the record on the key.
        /// If the key is not found then an exception will be thrown.
        /// </summary>
        /// <param name="key">The key to find.</param>
        /// <exception cref="KeyNotFoundException">
        /// The key wasn't found.
        /// </exception>
        public void SeekWithKeyNotFoundException(TKey key)
        {
            if (!this.TrySeek(key))
            {
                throw new KeyNotFoundException(String.Format(CultureInfo.InvariantCulture, "{0} was not found", key));
            }
        }

        /// <summary>
        /// Position the cursor before the first record in the table.
        /// A <see cref="TryMoveNext"/> will then position the cursor
        /// on the first record.
        /// </summary>
        public void MoveBeforeFirst()
        {
            Api.MoveBeforeFirst(this.session, this.dataTable);
        }

        /// <summary>
        /// Try to move to the next record.
        /// </summary>
        /// <returns>
        /// True if the move was successful, false if there are no more records.
        /// </returns>
        public bool TryMoveNext()
        {
            return Api.TryMoveNext(this.session, this.dataTable);
        }

        /// <summary>
        /// Create an index range on the cursor, controlling which records will be enumerated.
        /// </summary>
        /// <param name="range">The range to set.</param>
        /// <returns>False if the range is empty.</returns>
        public bool SetIndexRange(KeyRange<TKey> range)
        {
            if (null != range.Min
                && null != range.Max
                && range.Min.Value.CompareTo(range.Max.Value) > 0)
            {
                return false;
            }

            if (null != range.Min)
            {
                this.MakeKey(range.Min.Value);
                SeekGrbit grbit = range.Min.IsInclusive ? SeekGrbit.SeekGE : SeekGrbit.SeekGT;
                if (!Api.TrySeek(this.session, this.dataTable, grbit))
                {
                    return false;
                }
            }
            else
            {
                Api.MoveBeforeFirst(this.session, this.dataTable);
                if (!Api.TryMoveNext(this.session, this.dataTable))
                {
                    return false;
                }
            }

            if (null != range.Max)
            {
                this.MakeKey(range.Max.Value);
                SetIndexRangeGrbit grbit = SetIndexRangeGrbit.RangeUpperLimit | (range.Max.IsInclusive
                                                                                     ? SetIndexRangeGrbit.RangeInclusive
                                                                                     : SetIndexRangeGrbit.None);
                return Api.TrySetIndexRange(this.session, this.dataTable, grbit);
            }

            return true;
        }

        #endregion

        #region Data Retrieval

        /// <summary>
        /// Retrieve the key column of the record the cursor is currently positioned on.
        /// </summary>
        /// <returns>The key of the record.</returns>
        public TKey RetrieveCurrentKey()
        {
            return (TKey) this.converters.RetrieveKeyColumn(this.session, this.dataTable, this.keyColumn);
        }

        /// <summary>
        /// Retrieve the value column of the record the cursor is currently positioned on.
        /// </summary>
        /// <returns>The value of the record.</returns>
        public TValue RetrieveCurrentValue()
        {
            return (TValue)this.converters.RetrieveValueColumn(this.session, this.dataTable, this.valueColumn);
        }

        /// <summary>
        /// Retrieve the key and value of record the cursor is currently positioned on.
        /// </summary>
        /// <returns>The key and value of the record as a KeyValuePair.</returns>
        public KeyValuePair<TKey, TValue> RetrieveCurrent()
        {
            TKey key = this.RetrieveCurrentKey();
            TValue value = this.RetrieveCurrentValue();
            return new KeyValuePair<TKey, TValue>(key, value);    
        }

        /// <summary>
        /// Retrieve the count of items in the database from the globals table.
        /// </summary>
        /// <returns>The number of items in the database.</returns>
        public int RetrieveCount()
        {
            return (int)Api.RetrieveColumnAsInt32(this.session, this.globalsTable, this.countColumn);
        }

        #endregion

        #region DML

        /// <summary>
        /// Insert data into the data table. No record with the same key
        /// should exist.
        /// </summary>
        /// <param name="data">The data to add.</param>
        public void Insert(KeyValuePair<TKey, TValue> data)
        {
            using (var update = new Update(this.session, this.dataTable, JET_prep.Insert))
            {
                this.SetKeyColumn(data.Key);
                this.SetValue(data.Value);
                update.Save();
            }

            this.UpdateCount(1);
        }

        /// <summary>
        /// Delete the record the cursor is currently positioned on.
        /// </summary>
        public void DeleteCurrent()
        {
            Api.JetDelete(this.session, this.dataTable);
            this.UpdateCount(-1);
        }

        /// <summary>
        /// Replace the value column of the record the cursor is currently on.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void ReplaceCurrentValue(TValue value)
        {
            using (var update = new Update(this.session, this.dataTable, JET_prep.Replace))
            {
                this.SetValue(value);
                update.Save();
            }
        }

        /// <summary>
        /// Generate a null database update that we can wrap in a non-lazy transaction.
        /// </summary>
        public void Flush()
        {
            using (var transaction = this.BeginTransaction())
            {
                Api.EscrowUpdate(this.session, this.globalsTable, this.flushColumn, 1);
                transaction.Commit(CommitTransactionGrbit.None);
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.session.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Calls JetMakeKey.
        /// </summary>
        /// <param name="key">The value of the key column.</param>
        private void MakeKey(TKey key)
        {
            // Casts TKey to object
            this.converters.MakeKey(this.session, this.dataTable, key, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Sets the key column.
        /// </summary>
        /// <param name="key">The value of the key column.</param>
        private void SetKeyColumn(TKey key)
        {
            // Casts TKey to object
            this.converters.SetKeyColumn(this.session, this.dataTable, this.keyColumn, key);
        }

        /// <summary>
        /// Sets the value column.
        /// </summary>
        /// <param name="value">The value of the value column.</param>
        private void SetValue(TValue value)
        {
            // Casts TValue to object
            this.converters.SetValueColumn(this.session, this.dataTable, this.valueColumn, value);
        }

        /// <summary>
        /// Update the count in the globals table. This is done with EscrowUpdate
        /// so that there won't be any write conflicts.
        /// </summary>
        /// <param name="delta">The delta to apply to the count.</param>
        private void UpdateCount(int delta)
        {
            Api.EscrowUpdate(this.session, this.globalsTable, this.countColumn, delta);
        }

        /// <summary>
        /// Attach the database, open the global and data tables and get the required columnids.
        /// </summary>
        private void AttachDatabase()
        {
            Api.JetAttachDatabase(this.session, this.database, AttachDatabaseGrbit.None);
            Api.JetOpenDatabase(this.session, this.database, String.Empty, out this.dbid, OpenDatabaseGrbit.None);
            Api.JetOpenTable(
                this.session, this.dbid, this.config.GlobalsTableName, null, 0, OpenTableGrbit.None, out this.globalsTable);
            this.countColumn = Api.GetTableColumnid(this.session, this.globalsTable, this.config.CountColumnName);
            this.flushColumn = Api.GetTableColumnid(this.session, this.globalsTable, this.config.FlushColumnName);

            Api.JetOpenTable(
                this.session, this.dbid, this.config.DataTableName, null, 0, OpenTableGrbit.None, out this.dataTable);
            this.keyColumn = Api.GetTableColumnid(this.session, this.dataTable, this.config.KeyColumnName);
            this.valueColumn = Api.GetTableColumnid(this.session, this.dataTable, this.config.ValueColumnName);
        }
    }
}