// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnConverter.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Contains methods to set and get data from the ESENT database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;

    /// <summary>
    /// Contains methods to set and get data from the ESENT
    /// database.
    /// </summary>
    /// <typeparam name="TColumn">The type of the column.</typeparam>
    internal class ColumnConverter<TColumn>
    {
        /// <summary>
        /// A mapping of types to RetrieveColumn function names.
        /// </summary>
        private static readonly IDictionary<Type, string> RetrieveColumnMethodNamesMap = new Dictionary<Type, string>
        {
            { typeof(bool), "RetrieveColumnAsBoolean" },
            { typeof(byte), "RetrieveColumnAsByte" },
            { typeof(short), "RetrieveColumnAsInt16" },
            { typeof(ushort), "RetrieveColumnAsUInt16" },
            { typeof(int), "RetrieveColumnAsInt32" },
            { typeof(uint), "RetrieveColumnAsUInt32" },
            { typeof(long), "RetrieveColumnAsInt64" },
            { typeof(ulong), "RetrieveColumnAsUInt64" },
            { typeof(float), "RetrieveColumnAsFloat" },
            { typeof(double), "RetrieveColumnAsDouble" },
            { typeof(Guid), "RetrieveColumnAsGuid" },
            { typeof(string), "RetrieveColumnAsString" },
            { typeof(DateTime), "RetrieveColumnAsDateTime" },
            { typeof(TimeSpan), "RetrieveColumnAsTimeSpan" },
            { typeof(PersistentBlob), "RetrieveColumnAsPersistentBlob" },
        };

        /// <summary>
        /// A mapping of types to RetrieveColumn function names for non-nullable versions.
        /// </summary>
        private static readonly IDictionary<Type, string> RetrieveColumnMethodNonNullableNamesMap = new Dictionary<Type, string>
        {
            { typeof(bool), "RetrieveColumnAsNonNullableBoolean" },
            { typeof(byte), "RetrieveColumnAsNonNullableByte" },
            { typeof(short), "RetrieveColumnAsNonNullableInt16" },
            { typeof(ushort), "RetrieveColumnAsNonNullableUInt16" },
            { typeof(int), "RetrieveColumnAsNonNullableInt32" },
            { typeof(uint), "RetrieveColumnAsNonNullableUInt32" },
            { typeof(long), "RetrieveColumnAsNonNullableInt64" },
            { typeof(ulong), "RetrieveColumnAsNonNullableUInt64" },
            { typeof(float), "RetrieveColumnAsNonNullableFloat" },
            { typeof(double), "RetrieveColumnAsNonNullableDouble" },
            { typeof(Guid), "RetrieveColumnAsNonNullableGuid" },
            { typeof(string), "RetrieveColumnAsString" },
            { typeof(DateTime), "RetrieveColumnAsNonNullableDateTime" },
            { typeof(TimeSpan), "RetrieveColumnAsNonNullableTimeSpan" },
            { typeof(PersistentBlob), "RetrieveColumnAsPersistentBlob" },
        };

        /// <summary>
        /// A mapping of types to RetrieveColumn function names for non-nullable versions.
        /// </summary>
        private static readonly IDictionary<Type, string> RetrieveColumnMethodNonNullableNamesMap = new Dictionary<Type, string>
        {
            { typeof(bool), "RetrieveColumnAsNonNullableBoolean" },
            { typeof(byte), "RetrieveColumnAsNonNullableByte" },
            { typeof(short), "RetrieveColumnAsNonNullableInt16" },
            { typeof(ushort), "RetrieveColumnAsNonNullableUInt16" },
            { typeof(int), "RetrieveColumnAsNonNullableInt32" },
            { typeof(uint), "RetrieveColumnAsNonNullableUInt32" },
            { typeof(long), "RetrieveColumnAsNonNullableInt64" },
            { typeof(ulong), "RetrieveColumnAsNonNullableUInt64" },
            { typeof(float), "RetrieveColumnAsNonNullableFloat" },
            { typeof(double), "RetrieveColumnAsNonNullableDouble" },
            { typeof(Guid), "RetrieveColumnAsNonNullableGuid" },
            { typeof(string), "RetrieveColumnAsString" },
            { typeof(DateTime), "RetrieveColumnAsNonNullableDateTime" },
            { typeof(TimeSpan), "RetrieveColumnAsNonNullableTimeSpan" },
            { typeof(PersistentBlob), "RetrieveColumnAsPersistentBlob" },
        };

        /// <summary>
        /// A mapping of types to ESENT column types.
        /// </summary>
        private static readonly IDictionary<Type, JET_coltyp> ColumnTypeMap = new Dictionary<Type, JET_coltyp>
        {
            { typeof(bool), JET_coltyp.Bit },
            { typeof(byte), JET_coltyp.UnsignedByte },
            { typeof(short), JET_coltyp.Short },
            { typeof(ushort), JET_coltyp.Binary },
            { typeof(int), JET_coltyp.Long },
            { typeof(uint), JET_coltyp.Binary },
            { typeof(long), JET_coltyp.Currency },
            { typeof(ulong), JET_coltyp.Binary },
            { typeof(float), JET_coltyp.IEEESingle },
            { typeof(double), JET_coltyp.IEEEDouble },
            { typeof(Guid), JET_coltyp.Binary },
            { typeof(string), JET_coltyp.LongText },
            { typeof(DateTime), JET_coltyp.Currency },
            { typeof(TimeSpan), JET_coltyp.Currency },
            { typeof(PersistentBlob), JET_coltyp.LongBinary },
        };

        /// <summary>
        /// The SetColumn delegate for this object.
        /// </summary>
        private readonly SetColumnDelegate columnSetter;

        /// <summary>
        /// The RetrieveColumn delegate for this object.
        /// </summary>
        private readonly RetrieveColumnDelegate columnRetriever;

        /// <summary>
        /// The column type for this object.
        /// </summary>
        private readonly JET_coltyp coltyp;

        /// <summary>
        /// Initializes a new instance of the ColumnConverter class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage",
                         "CA2208:InstantiateArgumentExceptionsCorrectly",
                         Justification = "ArgumentOutOfRangeException wants a local argument, not a generic type-argument.")]
        public ColumnConverter()
        {
            Type underlyingType = IsNullableType(typeof(TColumn)) ? GetUnderlyingType(typeof(TColumn)) : typeof(TColumn);
            if (RetrieveColumnMethodNamesMap.ContainsKey(underlyingType))
            {
                this.columnSetter = CreateSetColumnDelegate();
                this.columnRetriever = CreateRetrieveColumnDelegate();
                this.coltyp = ColumnTypeMap[underlyingType];
            }
#if ESENTCOLLECTIONS_SUPPORTS_SERIALIZATION
            else if (IsSerializable(typeof(TColumn)))
            {
                this.columnSetter = (s, t, c, o) => Api.SerializeObjectToColumn(s, t, c, o);
                this.columnRetriever = (s, t, c) => (TColumn)Api.DeserializeObjectFromColumn(s, t, c);
                this.coltyp = JET_coltyp.LongBinary;
            }
#endif
            else
            {
                throw new ArgumentOutOfRangeException("TColumn", typeof(TColumn), "Not supported for SetColumn");                    
            }

            // Compile the new delegates.
            RuntimeHelpers.PrepareDelegate(this.columnSetter);
            RuntimeHelpers.PrepareDelegate(this.columnRetriever);
        }

        /// <summary>
        /// Represents a SetColumn operation.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the value in. An update should be prepared.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        public delegate void SetColumnDelegate(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, TColumn value);

        /// <summary>
        /// Represents a RetrieveColumn operation.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the value from.</param>
        /// <param name="columnid">The column to retrieve.</param>
        /// <returns>The retrieved value.</returns>
        public delegate TColumn RetrieveColumnDelegate(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid);

        /// <summary>
        /// Gets the type of database column the value should be stored in.
        /// </summary>
        public JET_coltyp Coltyp
        {
            get
            {
                return this.coltyp;
            }
        }

        /// <summary>
        /// Gets a delegate that can be used to set the Key column with an object of
        /// type <see cref="Type"/>.
        /// </summary>
        public SetColumnDelegate ColumnSetter
        {
            get
            {
                return this.columnSetter;
            }
        }

        /// <summary>
        /// Gets a delegate that can be used to retrieve the Key column, returning
        /// type <see cref="Type"/>.
        /// </summary>
        public RetrieveColumnDelegate ColumnRetriever
        {
            get
            {
                return this.columnRetriever;
            }
        }

        /// <summary>
        /// Determine if the given type is a nullable type.
        /// </summary>
        /// <param name="t">The type to check.</param>
        /// <returns>True if the type is nullable.</returns>
        private static bool IsNullableType(Type t)
        {
#if MANAGEDESENT_ON_WSA
            return t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
#else
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
#endif
        }

        /// <summary>
        /// Get the type that underlies the nullable type.
        /// </summary>
        /// <param name="t">The nullable type.</param>
        /// <returns>The type that underlies the nullable type.</returns>
        private static Type GetUnderlyingType(Type t)
        {
            Debug.Assert(IsNullableType(t), "Type should be nullable");
#if MANAGEDESENT_ON_WSA
            return t.GetTypeInfo().GetGenericArguments()[0];
#else
            return t.GetGenericArguments()[0];
#endif
        }

        /// <summary>
        /// Determine if the given type is a serializable structure.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// True if the type (and the types it contains) are all serializable structures.
        /// </returns>
        private static bool IsSerializable(Type type)
        {
            // Strings are fine (they are immutable)
            if (typeof(string) == type)
            {
                return true;
            }

            // Immutable serializable classes from .NET framework.
#if MANAGEDESENT_ON_WSA
            // IPAddress not available in Metro.
            if (typeof(Uri) == type)
#else
            if (typeof(Uri) == type
                || typeof(IPAddress) == type)
#endif
            {
                return true;
            }

#if MANAGEDESENT_ON_WSA
            TypeInfo typeinfo = type.GetTypeInfo();

            // A primitive serializable type is fine
            if (typeinfo.IsPrimitive && typeinfo.IsSerializable)
            {
                return true;
            }

            // If this isn't a serializable struct, the type definitely isn't serializable
            if (!(typeinfo.IsValueType && typeinfo.IsSerializable))
            {
                return false;
            }

            // This is a serializable struct. Recursively check that all members are serializable.
            // Unlike classes, structs cannot have cycles in their definitions so a simple enumeration
            // will work.
            MemberInfo[] members = typeinfo.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
#else
            // A primitive serializable type is fine
            if (type.IsPrimitive && type.IsSerializable)
            {
                return true;
            }

            // If this isn't a serializable struct, the type definitely isn't serializable
            if (!(type.IsValueType && type.IsSerializable))
            {
                return false;
            }

            // This is a serializable struct. Recursively check that all members are serializable.
            // Unlike classes, structs cannot have cycles in their definitions so a simple enumeration
            // will work.
            MemberInfo[] members = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
#endif
            return members.Cast<FieldInfo>().All(fieldinfo => IsSerializable(fieldinfo.FieldType));
        }

        /// <summary>
        /// Set a string.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, string value)
        {
            Api.SetColumn(sesid, tableid, columnid, value, Encoding.Unicode, SetColumnGrbit.IntrinsicLV);
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, bool? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, short? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, ushort? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, int? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, uint? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, long? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, ulong? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, float? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, double? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable value.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, Guid? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                Api.SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a nullable date time.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, DateTime? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a date time.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, DateTime value)
        {            
            Api.SetColumn(sesid, tableid, columnid, value.Ticks);
        }
        
        /// <summary>
        /// Set a nullable timespan.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, TimeSpan? value)
        {
            if (!SetColumnIfNull(sesid, tableid, columnid, value))
            {
                SetColumn(sesid, tableid, columnid, value.Value);
            }
        }

        /// <summary>
        /// Set a timespan.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, TimeSpan value)
        {
            Api.SetColumn(sesid, tableid, columnid, value.Ticks);
        }

        /// <summary>
        /// Set a PersistentBlob.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the value in.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, PersistentBlob value)
        {
            value.CheckImmutability();
            Api.SetColumn(sesid, tableid, columnid, value.GetBytes());
        }

        /// <summary>
        /// Retrieve a nullable date time. We do not use Api.RetrieveColumnAsDateTime because
        /// that stores the value in OADate format, which is less accurate than System.DateTime.
        /// Instead we store a DateTime as its Tick value in an Int64 column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A nullable DateTime constructed from the column.</returns>
        private static DateTime? RetrieveColumnAsDateTime(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            long? ticks = Api.RetrieveColumnAsInt64(sesid, tableid, columnid);
            if (ticks.HasValue)
            {
                return new DateTime(ticks.Value);
            }

            return null;
        }

        /// <summary>
        /// Retrieve a nullable TimeSpan.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A nullable TimeSpan constructed from the column.</returns>
        private static TimeSpan? RetrieveColumnAsTimeSpan(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            long? ticks = Api.RetrieveColumnAsInt64(sesid, tableid, columnid);
            if (ticks.HasValue)
            {
                return new TimeSpan(ticks.Value);
            }

            return null;
        }

        /// <summary>
        /// Retrieve a PersistentBlob.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A PersistentBlob constructed from the column.</returns>
        private static PersistentBlob RetrieveColumnAsPersistentBlob(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            byte[] bytes = Api.RetrieveColumn(sesid, tableid, columnid);
            return new PersistentBlob(bytes);
        }

        /// <summary>
        /// Retrieve a non-nullable Boolean
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Boolean constructed from the column.</returns>
        private static bool RetrieveColumnAsNonNullableBoolean(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<bool>(Api.RetrieveColumnAsBoolean(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Byte
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Byte constructed from the column.</returns>
        private static byte RetrieveColumnAsNonNullableByte(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<byte>(Api.RetrieveColumnAsByte(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Int16
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Int16 constructed from the column.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Matches EsentInterop style.")]
        private static Int16 RetrieveColumnAsNonNullableInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<Int16>(Api.RetrieveColumnAsInt16(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable UInt16
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable UInt16 constructed from the column.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Matches EsentInterop style.")]
        private static UInt16 RetrieveColumnAsNonNullableUInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<UInt16>(Api.RetrieveColumnAsUInt16(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Int32
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Int32 constructed from the column.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Matches EsentInterop style.")]
        private static Int32 RetrieveColumnAsNonNullableInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<Int32>(Api.RetrieveColumnAsInt32(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable UInt32
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable UInt32 constructed from the column.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Matches EsentInterop style.")]
        private static UInt32 RetrieveColumnAsNonNullableUInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<UInt32>(Api.RetrieveColumnAsUInt32(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Int64
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Int64 constructed from the column.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Matches EsentInterop style.")]
        private static Int64 RetrieveColumnAsNonNullableInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<Int64>(Api.RetrieveColumnAsInt64(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable UInt64
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable UInt64 constructed from the column.</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Matches EsentInterop style.")]
        private static UInt64 RetrieveColumnAsNonNullableUInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<UInt64>(Api.RetrieveColumnAsUInt64(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Float
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Float constructed from the column.</returns>
        private static float RetrieveColumnAsNonNullableFloat(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<float>(Api.RetrieveColumnAsFloat(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Double
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Double constructed from the column.</returns>
        private static double RetrieveColumnAsNonNullableDouble(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<double>(Api.RetrieveColumnAsDouble(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable Guid
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Guid constructed from the column.</returns>
        private static Guid RetrieveColumnAsNonNullableGuid(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<Guid>(Api.RetrieveColumnAsGuid(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable DateTime
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable DateTime constructed from the column.</returns>
        private static DateTime RetrieveColumnAsNonNullableDateTime(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<DateTime>(ColumnConverter<TColumn>.RetrieveColumnAsDateTime(sesid, tableid, columnid));
        }

        /// <summary>
        /// Retrieve a non-nullable TimeSpan
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve the value from.</param>
        /// <param name="columnid">The column containing the value.</param>
        /// <returns>A non-nullable Float constructed from the column.</returns>
        private static TimeSpan RetrieveColumnAsNonNullableTimeSpan(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return UnwrapNullableColumnValue<TimeSpan>(ColumnConverter<TColumn>.RetrieveColumnAsTimeSpan(sesid, tableid, columnid));
        }

        /// <summary>
        /// Unwraps a nullable column value when used with a dictionary that is not
        /// using nullable types
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="val">Value to unwrap</param>
        /// <returns>Unwrapped value</returns>
        private static T UnwrapNullableColumnValue<T>(T? val)
            where T : struct
        {
            return val.Value;
        }

        /// <summary>
        /// Set the column to null, if the nullable value is null.
        /// </summary>
        /// <typeparam name="T">The underlying type.</typeparam>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The tableid to set.</param>
        /// <param name="columnid">The column to set.</param>
        /// <param name="value">The nullable value to set.</param>
        /// <returns>
        /// True if the value was null and the column was set to null
        /// .</returns>
        private static bool SetColumnIfNull<T>(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, T? value) where T : struct
        {
            if (!value.HasValue)
            {
                Api.SetColumn(sesid, tableid, columnid, null);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the retrieve column delegate for the type.
        /// </summary>
        /// <returns>The retrieve column delegate for the type.</returns>
        private static RetrieveColumnDelegate CreateRetrieveColumnDelegate()
        {
            // Look for a method called "RetrieveColumnAs{Type}", which will return a
            // nullable version of the type (except for strings, which are are ready 
            // reference types). First look for a private method in this class that
            // takes the appropriate arguments, otherwise a method on the Api class.
            Type underlyingType = IsNullableType(typeof(TColumn)) ? GetUnderlyingType(typeof(TColumn)) : typeof(TColumn);
            string retrieveColumnMethodName = RetrieveColumnMethodNamesMap[underlyingType];
            string retrieveColumnNonNullableMethodName = RetrieveColumnMethodNonNullableNamesMap[underlyingType];
            Type[] retrieveColumnArguments = new[] { typeof(JET_SESID), typeof(JET_TABLEID), typeof(JET_COLUMNID) };
            
            if (typeof(TColumn).IsClass || IsNullableType(typeof(TColumn)))
            {
                MethodInfo retrieveColumnMethod = typeof(ColumnConverter<TColumn>).GetMethod(
                                                  retrieveColumnMethodName,
                                                  BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.ExactBinding,
                                                  null,
                                                  retrieveColumnArguments,
                                                  null) ?? typeof(Api).GetMethod(
                                                               retrieveColumnMethodName,
                                                               BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding,
                                                               null,
                                                               retrieveColumnArguments,
                                                               null);

                // Return the string/nullable type.
                return (RetrieveColumnDelegate)Delegate.CreateDelegate(typeof(RetrieveColumnDelegate), retrieveColumnMethod);
            }
            else
            {
                MethodInfo retrieveNonNullableColumnMethod = typeof(ColumnConverter<TColumn>).GetMethod(
                                                  retrieveColumnNonNullableMethodName,
                                                  BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.ExactBinding,
                                                  null,
                                                  retrieveColumnArguments,
                                                  null) ?? typeof(Api).GetMethod(
                                                               retrieveColumnNonNullableMethodName,
                                                               BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding,
                                                               null,
                                                               retrieveColumnArguments,
                                                               null);

                return (RetrieveColumnDelegate)Delegate.CreateDelegate(typeof(RetrieveColumnDelegate), retrieveNonNullableColumnMethod);
            }
        }

        /// <summary>
        /// Create the set column delegate.
        /// </summary>
        /// <returns>The set column delegate.</returns>
        private static SetColumnDelegate CreateSetColumnDelegate()
        {
            // Look for a method called "SetColumn", which takes a TColumn.
            // First look for a private method in this class that takes the
            // appropriate arguments, otherwise a method on the Api class.
            const string SetColumnMethodName = "SetColumn";
            Type[] setColumnArguments = new[] { typeof(JET_SESID), typeof(JET_TABLEID), typeof(JET_COLUMNID), typeof(TColumn) };
            MethodInfo setColumnMethod = typeof(ColumnConverter<TColumn>).GetMethod(
                                             SetColumnMethodName,
                                             BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.ExactBinding,
                                             null,
                                             setColumnArguments,
                                             null) ?? typeof(Api).GetMethod(
                                                          SetColumnMethodName,
                                                          BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding,
                                                          null,
                                                          setColumnArguments,
                                                          null);
            return (SetColumnDelegate)Delegate.CreateDelegate(typeof(SetColumnDelegate), setColumnMethod);
        }
    }
}
