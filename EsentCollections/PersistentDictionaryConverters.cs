//-----------------------------------------------------------------------
// <copyright file="PersistentDictionaryConverters.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Isam.Esent.Interop;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Contains methods to set and get data from the ESENT
    /// database.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal class PersistentDictionaryConverters<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the PersistentDictionaryConverters
        /// class. This looks up the conversion types for
        /// <typeparamref name="TKey"/> and <typeparamref name="TValue"/>.
        /// </summary>
        public PersistentDictionaryConverters()
        {
            this.MakeKey = GetMakeKeyFunction<TKey>();
            this.SetKeyColumn = GetSetColumnFunction<TKey>();
            this.SetValueColumn = GetSetColumnFunction<TValue>();
            this.RetrieveKeyColumn = GetRetrieveColumnFunction<TKey>();
            this.RetrieveValueColumn = GetRetrieveColumnFunction<TValue>();
            this.KeyColtyp = GetColtyp<TKey>();
            this.ValueColtyp = GetColtyp<TValue>();
        }

        /// <summary>
        /// Gets a delegate that can be used to call JetMakeKey with an object of
        /// type <typeparamref name="TKey"/>.
        /// </summary>
        public Action<JET_SESID, JET_TABLEID, object, MakeKeyGrbit> MakeKey { get; private set; }

        /// <summary>
        /// Gets a delegate that can be used to set the Key column with an object of
        /// type <typeparamref name="TKey"/>.
        /// </summary>
        public Action<JET_SESID, JET_TABLEID, JET_COLUMNID, object> SetKeyColumn { get; private set; }

        /// <summary>
        /// Gets a delegate that can be used to set the Value column with an object of
        /// type <typeparamref name="TValue"/>.
        /// </summary>
        public Action<JET_SESID, JET_TABLEID, JET_COLUMNID, object> SetValueColumn { get; private set; }

        /// <summary>
        /// Gets a delegate that can be used to retrieve the Key column, returning
        /// an object of type <typeparamref name="TKey"/>.
        /// </summary>
        public Func<JET_SESID, JET_TABLEID, JET_COLUMNID, object> RetrieveKeyColumn { get; private set; }

        /// <summary>
        /// Gets a delegate that can be used to retrieve the Value column, returning
        /// an object of type <typeparamref name="TValue"/>.
        /// </summary>
        public Func<JET_SESID, JET_TABLEID, JET_COLUMNID, object> RetrieveValueColumn { get; private set; }

        /// <summary>
        /// Gets the JET_coltyp that the key column should have.
        /// </summary>
        public JET_coltyp KeyColtyp { get; private set; }

        /// <summary>
        /// Gets the JET_coltyp that the value column should have.
        /// </summary>
        public JET_coltyp ValueColtyp { get; private set; }

        /// <summary>
        /// Returns a MakeKey delegate for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <returns>A MakeKey delegate.</returns>
        private static Action<JET_SESID, JET_TABLEID, object, MakeKeyGrbit> GetMakeKeyFunction<T>()
        {
            var type = typeof(T);
            if (type == typeof(bool))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (bool) o, g);
            }
            else if (type == typeof(byte))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (byte)o, g);
            }
            else if (type == typeof(short))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (short)o, g);                
            }
            else if (type == typeof(ushort))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (ushort)o, g);
            }
            else if (type == typeof(int))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (int)o, g);
            }
            else if (type == typeof(uint))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (uint)o, g);
            }
            else if (type == typeof(long))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (long)o, g);
            }
            else if (type == typeof(ulong))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (ulong)o, g);
            }
            else if (type == typeof(float))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (float)o, g);
            }
            else if (type == typeof(double))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (double)o, g);
            }
            else if (type == typeof(DateTime))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, ((DateTime)o).Ticks, g);
            }
            else if (type == typeof(Guid))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, ((Guid)o).ToByteArray(), g);
            }
            else if (type == typeof(string))
            {
                return (s, t, o, g) => Api.MakeKey(s, t, (string) o, Encoding.Unicode, g);
            }

            throw new ArgumentOutOfRangeException("T", type, "unsupported type");
        }

        /// <summary>
        /// Returns a SetColumn delegate for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <returns>A SetColumn delegate.</returns>
        private static Action<JET_SESID, JET_TABLEID, JET_COLUMNID, object> GetSetColumnFunction<T>()
        {
            var type = typeof(T);
            if (type == typeof(bool))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (bool)o);
            }
            else if (type == typeof(byte))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (byte)o);
            }
            else if (type == typeof(short))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (short)o);
            }
            else if (type == typeof(ushort))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (ushort)o);
            }
            else if (type == typeof(int))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (int)o);
            }
            else if (type == typeof(uint))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (uint)o);
            }
            else if (type == typeof(long))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (long)o);
            }
            else if (type == typeof(ulong))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (ulong)o);
            }
            else if (type == typeof(float))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (float)o);
            }
            else if (type == typeof(double))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (double)o);
            }
            else if (type == typeof(DateTime))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, ((DateTime)o).Ticks);
            }
            else if (type == typeof(Guid))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, ((Guid)o).ToByteArray());
            }
            else if (type == typeof(string))
            {
                return (s, t, c, o) => Api.SetColumn(s, t, c, (string) o, Encoding.Unicode);
            }

            throw new ArgumentOutOfRangeException("T", type, "unsupported type");
        }

        /// <summary>
        /// Returns a RetrieveColumn delegate for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <returns>A RetrieveColumn delegate.</returns>
        private static Func<JET_SESID, JET_TABLEID, JET_COLUMNID, object> GetRetrieveColumnFunction<T>()
        {
            var type = typeof(T);
            if (type == typeof(bool))
            {
                return (s, t, c) => Api.RetrieveColumnAsBoolean(s, t, c);
            }
            else if (type == typeof(byte))
            {
                return (s, t, c) => Api.RetrieveColumnAsByte(s, t, c);
            }
            else if (type == typeof(short))
            {
                return (s, t, c) => Api.RetrieveColumnAsInt16(s, t, c);
            }
            else if (type == typeof(ushort))
            {
                return (s, t, c) => Api.RetrieveColumnAsUInt16(s, t, c);
            }
            else if (type == typeof(int))
            {
                return (s, t, c) => Api.RetrieveColumnAsInt32(s, t, c);
            }
            else if (type == typeof(uint))
            {
                return (s, t, c) => Api.RetrieveColumnAsUInt32(s, t, c);
            }
            else if (type == typeof(long))
            {
                return (s, t, c) => Api.RetrieveColumnAsInt64(s, t, c);
            }
            else if (type == typeof(ulong))
            {
                return (s, t, c) => Api.RetrieveColumnAsUInt64(s, t, c);
            }
            else if (type == typeof(float))
            {
                return (s, t, c) => Api.RetrieveColumnAsFloat(s, t, c);
            }
            else if (type == typeof(double))
            {
                return (s, t, c) => Api.RetrieveColumnAsDouble(s, t, c);
            }
            else if (type == typeof(DateTime))
            {
                return (s, t, c) => new DateTime((long)Api.RetrieveColumnAsInt64(s, t, c));
            }
            else if (type == typeof(Guid))
            {
                return (s, t, c) => Api.RetrieveColumnAsGuid(s, t, c);
            }
            else if (type == typeof(string))
            {
                return (s, t, c) => Api.RetrieveColumnAsString(s, t, c);
            }

            throw new ArgumentOutOfRangeException("T", type, "unsupported type");
        }

        /// <summary>
        /// Returns a JET_coltyp for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <returns>
        /// A JET_coltyp which can contain <typeparamref name="T"/>.
        /// </returns>
        private static JET_coltyp GetColtyp<T>()
        {
            // For compatability with older versions of ESENT 
            // we use binary columns instead of the Vista specelse ific
            // types.
            var type = typeof(T);
            if (type == typeof(bool))
            {
                return JET_coltyp.Bit;
            }
            else if (type == typeof(byte))
            {
                return JET_coltyp.UnsignedByte;
            }
            else if (type == typeof(short))
            {
                return JET_coltyp.Short;
            }
            else if (type == typeof(ushort))
            {
                return JET_coltyp.Binary;
            }
            else if (type == typeof(int))
            {
                return JET_coltyp.Long;
            }
            else if (type == typeof(uint))
            {
                return JET_coltyp.Binary;
            }
            else if (type == typeof(long))
            {
                return JET_coltyp.Currency;
            }
            else if (type == typeof(ulong))
            {
                return JET_coltyp.Binary;
            }
            else if (type == typeof(float))
            {
                return JET_coltyp.IEEESingle;
            }
            else if (type == typeof(double))
            {
                return JET_coltyp.IEEEDouble;
            }
            else if (type == typeof(DateTime))
            {
                return JET_coltyp.Currency;
            }
            else if (type == typeof(Guid))
            {
                return JET_coltyp.Binary;
            }
            else if (type == typeof(string))
            {
                return JET_coltyp.LongText;
            }

            throw new ArgumentOutOfRangeException("T", type, "unsupported type");
        }
    }
}