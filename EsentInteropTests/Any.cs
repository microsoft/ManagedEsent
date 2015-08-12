//-----------------------------------------------------------------------
// <copyright file="Any.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;

    /// <summary>
    /// Generate arbitrary (random) values
    /// </summary>
    internal static partial class Any
    {
        /// <summary>
        /// Random object used to generate the values.
        /// </summary>
        private static readonly Random MyRandom = new Random();

        /// <summary>
        /// Gets a value indicating whether the random boolean value
        /// is true or false.
        /// </summary>
        public static bool Boolean
        {
            get
            {
                return 0 == Any.MyRandom.Next() % 2;
            }
        }

        /// <summary>
        /// Gets a random byte.
        /// </summary>
        public static byte Byte
        {
            get
            {
                var data = new byte[1];
                Any.MyRandom.NextBytes(data);
                return data[0];
            }
        }

        /// <summary>
        /// Gets a random short.
        /// </summary>
        public static short Int16
        {
            get
            {
                var data = new byte[2];
                Any.MyRandom.NextBytes(data);
                return BitConverter.ToInt16(data, 0);
            }
        }

        /// <summary>
        /// Gets a random ushort.
        /// </summary>
        public static ushort UInt16
        {
            get
            {
                var data = new byte[2];
                Any.MyRandom.NextBytes(data);
                return BitConverter.ToUInt16(data, 0);
            }
        }

        /// <summary>
        /// Gets a random int.
        /// </summary>
        public static int Int32
        {
            get
            {
                var data = new byte[4];
                Any.MyRandom.NextBytes(data);
                return BitConverter.ToInt32(data, 0);
            }
        }

        /// <summary>
        /// Gets a random positive int.
        /// </summary>
        public static int PositiveInt32
        {
            get
            {
                var temp = Any.Int32;
                return temp >= 0 ? temp : (temp - int.MinValue);
            }
        }

        /// <summary>
        /// Gets a random uint.
        /// </summary>
        public static uint UInt32
        {
            get
            {
                var data = new byte[4];
                Any.MyRandom.NextBytes(data);
                return BitConverter.ToUInt32(data, 0);
            }
        }

        /// <summary>
        /// Gets a random Int64 (long).
        /// </summary>
        public static long Int64
        {
            get
            {
                var data = new byte[8];
                Any.MyRandom.NextBytes(data);
                return BitConverter.ToInt64(data, 0);
            }
        }

        /// <summary>
        /// Gets a random positive Int64 (long).
        /// </summary>
        public static long PositiveInt64
        {
            get
            {
                var temp = Any.Int64;
                return temp >= 0 ? temp : (temp - long.MinValue);
            }
        }

        /// <summary>
        /// Gets a random UInt64 (ulong).
        /// </summary>
        public static ulong UInt64
        {
            get
            {
                var data = new byte[8];
                Any.MyRandom.NextBytes(data);
                return BitConverter.ToUInt64(data, 0);
            }
        }

        /// <summary>
        /// Gets a random IntPtr.
        /// </summary>
        public static IntPtr IntPtr
        {
            get
            {
                return new IntPtr(Any.Int32);
            }
        }

        /// <summary>
        /// Gets a random float.
        /// </summary>
        public static float Float
        {
            get
            {
                return (float)Any.MyRandom.NextDouble();
            }
        }

        /// <summary>
        /// Gets a random double.
        /// </summary>
        public static double Double
        {
            get
            {
                return Any.MyRandom.NextDouble();
            }
        }

        /// <summary>
        /// Gets a random Guid.
        /// </summary>
        public static Guid Guid
        {
            get
            {
                return Guid.NewGuid();
            }
        }

        /// <summary>
        /// Gets a random DateTime.
        /// </summary>
        public static DateTime DateTime
        {
            get
            {
                // MSDN says: d must be a value between -657435.0 through positive 2958466.0.
                double d = Any.MyRandom.Next(-657435, 2958466);
                return LibraryHelpers.FromOADate(d);
            }
        }

        /// <summary>
        /// Gets a random DateTime than fits in the range 1900 to 2155 (Stored as a byte offset from 1900).
        /// </summary>
        public static DateTime DateTimeLimited
        {
            get
            {
                const int MaximumDays = 255 * 365;
                double d = Any.MyRandom.Next(2, MaximumDays);

                // FromOADate takes the number of days from 30 Dec 1899.
                return LibraryHelpers.FromOADate(d);
            }
        }

        /// <summary>
        /// Gets a random string. The string will only
        /// contain ASCII characters and will be 1 to
        /// 120 characters long.
        /// </summary>
        public static string String
        {
            get
            {
                int length = Any.MyRandom.Next(1, 120);
                return StringOfLength(length);
            }
        }

#if !MANAGEDESENT_ON_WSA // String interning is not supported.
        /// <summary>
        /// Gets a random string with the same characteristics of Any.String,
        /// but the string is guaranteed not to be interned.
        /// </summary>
        public static string NonInternedString
        {
            get
            {
                while (true)
                {
                    string s = Any.String;
                    if (string.IsInterned(s) == null)
                    {
                        return s;
                    }
                }
            }
        }
#endif // !MANAGEDESENT_ON_WSA

        /// <summary>
        /// Gets a random array of bytes. The array will
        /// be from 1 to 255 bytes.
        /// </summary>
        public static byte[] Bytes
        {
            get
            {
                int length = Any.MyRandom.Next(1, 255);
                return BytesOfLength(length);
            }
        }

        /// <summary>
        /// Gets a random JET_LOGTIME.
        /// </summary>
        public static JET_LOGTIME Logtime
        {
            get
            {
                return new JET_LOGTIME(DateTime.Now - TimeSpan.FromSeconds(Any.Int16));
            }
        }

        /// <summary>
        /// Gets a random JET_SIGNATURE.
        /// </summary>
        public static Microsoft.Isam.Esent.Interop.JET_SIGNATURE Signature
        {
            get
            {
                return new Microsoft.Isam.Esent.Interop.JET_SIGNATURE(Any.Int32, Any.DateTimeLimited, Any.String);
            }
        }

        /// <summary>
        /// Gets a random JET_LGPOS.
        /// </summary>
        public static JET_LGPOS Lgpos
        {
            get { return new JET_LGPOS { ib = Any.UInt16, isec = Any.UInt16, lGeneration = Any.UInt16 }; }
        }

        /// <summary>
        /// Gets a random JET_BKINFO.
        /// </summary>
        public static JET_BKINFO Bkinfo
        {
            get
            {
                return new JET_BKINFO
                {
                    bklogtimeMark = new JET_BKLOGTIME(DateTime.UtcNow, Any.Boolean),
                    genHigh = Any.UInt16,
                    genLow = Any.UInt16,
                    lgposMark = Any.Lgpos,
                };
            }
        }

        /// <summary>
        /// Gets a random string of the specified length.
        /// </summary>
        /// <param name="numChars">Number of chars to be in the string.</param>
        /// <returns>A random ASCII string of the specified length.</returns>
        public static string StringOfLength(int numChars)
        {
            var sb = new StringBuilder(numChars);
            for (int i = 0; i < numChars; ++i)
            {
                var c = (char)Any.MyRandom.Next(32, 127);
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a random array of bytes of the specified length.
        /// </summary>
        /// <param name="numBytes">Number of bytes to be returned.</param>
        /// <returns>An array of random bytes of the specified length.</returns>
        public static byte[] BytesOfLength(int numBytes)
        {
            var data = new byte[numBytes];
            Any.MyRandom.NextBytes(data);
            return data;            
        }

        /// <summary>
        /// Gets a random int within the given range.
        /// </summary>
        /// <param name="minValue">Minimum possible value to return.</param>
        /// <param name="maxValue">Maximum possible value to return.</param>
        /// <returns>An integer value in the given range.</returns>
        public static int Int32Range(int minValue, int maxValue)
        {
            return Any.MyRandom.Next(minValue, maxValue + 1);
        }
      }
}
