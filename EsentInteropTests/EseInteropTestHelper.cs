//-----------------------------------------------------------------------
// <copyright file="EseInteropTestHelper.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
#if MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA
    using System.Security.Cryptography;
#endif

    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Server2003;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Contains several helper functions that are useful in the test binary.
    /// In particular, it contains functionality that is not available in
    /// reduced-functionality environments (such as CoreClr).
    /// </summary>
    internal static class EseInteropTestHelper
    {
        /// <summary>
        /// The characters used for random file names.
        /// </summary>
        private static readonly char[] StaticBase32Char = new char[32]
                                                              {
                                                                  'a',
                                                                  'b',
                                                                  'c',
                                                                  'd',
                                                                  'e',
                                                                  'f',
                                                                  'g',
                                                                  'h',
                                                                  'i',
                                                                  'j',
                                                                  'k',
                                                                  'l',
                                                                  'm',
                                                                  'n',
                                                                  'o',
                                                                  'p',
                                                                  'q',
                                                                  'r',
                                                                  's',
                                                                  't',
                                                                  'u',
                                                                  'v',
                                                                  'w',
                                                                  'x',
                                                                  'y',
                                                                  'z',
                                                                  '0',
                                                                  '1',
                                                                  '2',
                                                                  '3',
                                                                  '4',
                                                                  '5'
                                                              };

        /// <summary>
        /// Sets the Thread.Priority, if available.
        /// </summary>
        public static ThreadPriority CurrentThreadPriority
        {
            set
            {
#if MANAGEDESENT_ON_CORECLR
                // Ignored on CoreClr.
#else
                Thread.CurrentThread.Priority = value;
#endif
            }
        }

        /// <summary>
        /// Provides a wrapper to retrieve the Assembly from the given Type.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> for which the containing Assembly is desired.</param>
        /// <returns>The containing Assembly.</returns>
        public static Assembly GetAssembly(Type type)
        {
#if MANAGEDESENT_ON_WSA
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        /// <summary>
        /// Writes the text representation of the specified array of objects, followed by the current line terminator, to the standard output stream using the specified format information.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An array of objects to write using <paramref name="format"/>.
        /// </param><exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null. </exception>
        /// <exception cref="T:System.FormatException">The format specification in <paramref name="format"/> is invalid. </exception><filterpriority>1</filterpriority>
        public static void ConsoleWriteLine(string format, params object[] args)
        {
#if MANAGEDESENT_ON_WSA
            Debug.WriteLine(format, args);
#else
            System.Console.WriteLine(format, args);
#endif
        }

        /// <summary>
        /// Creates the specified directory.
        /// </summary>
        /// <param name="newDirectory">The name of the directory to create.</param>
        public static void DirectoryCreateDirectory(string newDirectory)
        {
#if MANAGEDESENT_ON_WSA
            string realPath = StringToPath(newDirectory);

            if (!CreateDirectoryW(realPath, IntPtr.Zero))
            {
                // Not sure what to do here...
            }
#else
            Directory.CreateDirectory(newDirectory);
#endif
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="dirName">The file path to check.</param>
        /// <returns>True if the file exists.</returns>
        public static bool DirectoryDelete(string dirName)
        {
#if MANAGEDESENT_ON_WSA
            string realPath = StringToPath(dirName);

            return RemoveDirectoryW(realPath);
#else
            Directory.Delete(dirName);
            return true;
#endif
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="dirName">The file path to check.</param>
        /// <param name="recursive"><c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>.</param>
        /// <returns><c>true</c> if the file exists.</returns>
        public static bool DirectoryDelete(
            string dirName,
            bool recursive)
        {
#if MANAGEDESENT_ON_WSA
            string realPath = StringToPath(dirName);

            return RemoveDirectoryW(realPath);
#else
            Directory.Delete(dirName, recursive);
            return true;
#endif
        }

        /// <summary>
        /// Checks for the existence of a file.
        /// </summary>
        /// <param name="dirName">The file path to check.</param>
        /// <returns>True if the file exists.</returns>
        public static bool DirectoryExists(string dirName)
        {
#if MANAGEDESENT_ON_WSA
            string realPath = StringToPath(dirName);

            bool dirExists = false;
            WIN32_FILE_ATTRIBUTE_DATA fileData;
            bool successful = GetFileAttributesExW(realPath, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out fileData);
            if (successful)
            {
                if ((fileData.dwFileAttributes & FileAttributes.Directory) != 0)
                {
                    dirExists = true;
                }
            }

            return dirExists;
#else
            return Directory.Exists(dirName);
#endif
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="filename">The file path to check.</param>
        /// <returns>True if the file exists.</returns>
        public static bool FileDelete(string filename)
        {
#if MANAGEDESENT_ON_WSA
            string realPath = StringToPath(filename);

            return DeleteFileW(realPath);
#else
            File.Delete(filename);
            return true;
#endif
        }

        /// <summary>
        /// Checks for the existence of a file.
        /// </summary>
        /// <param name="filename">The file path to check.</param>
        /// <returns>True if the file exists.</returns>
        public static bool FileExists(string filename)
        {
#if MANAGEDESENT_ON_WSA
            string realPath = StringToPath(filename);

            bool fileExists = false;

            WIN32_FILE_ATTRIBUTE_DATA fileData;
            bool successful = GetFileAttributesExW(realPath, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out fileData);
            if (successful)
            {
                if ((fileData.dwFileAttributes & FileAttributes.Directory) == 0)
                {
                    fileExists = true;
                }
            }

            return fileExists;
#else
            return File.Exists(filename);
#endif
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file
        /// already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to. </param>
        /// <param name="contents">The string to write to the file.</param>
        public static void FileWriteAllText(
            string path,
            string contents)
        {
#if MANAGEDESENT_ON_CORECLR
            string realPath = StringToPath(path);

            using (SafeFileHandle handle = CreateFileW(
                realPath,
                EFileAccess.FILE_GENERIC_WRITE,
                EFileShare.Read,
                IntPtr.Zero,
                ECreationDisposition.CreateAlways,
                EFileAttributes.Normal, 
                IntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    throw new Exception("Failed to create file: " + realPath + "with error: " + Marshal.GetLastWin32Error());
                }

                // FUTURE: Actually write the data to the stream.
            }
#else
            File.WriteAllText(path, contents);
#endif
        }

        /// <summary>
        /// Returns the number of times garbage collection has occurred for the specified generation of objects.
        /// </summary>
        /// <param name="generation">The generation of objects for which the garbage collection count is to be determined.
        /// </param>
        /// <returns>The number of times garbage collection has occurred for the specified generation since the process was started.</returns>
        public static int GCCollectionCount(int generation)
        {
#if MANAGEDESENT_ON_CORECLR
            return 0;
#else
            return GC.CollectionCount(generation);
#endif
        }

        /// <summary>
        /// Retrieves the number of bytes currently thought to be allocated. A parameter indicates whether this method can wait a short interval before returning, to allow the system to collect garbage and finalize objects.
        /// </summary>
        /// <returns>
        /// A number that is the best available approximation of the number of bytes currently allocated in managed memory.
        /// </returns>
        /// <param name="forceFullCollection"><c>true</c> to indicate that this method can wait for garbage collection to occur before returning; otherwise, <c>false</c>.</param><filterpriority>1</filterpriority>
        public static long GCGetTotalMemory(bool forceFullCollection)
        {
#if MANAGEDESENT_ON_WSA
            return 1024;
#else
            return GC.GetTotalMemory(forceFullCollection);
#endif
        }

        /// <summary>
        /// Returns a random folder name or file name.
        /// </summary>
        /// <returns>
        /// A random folder name or file name.
        /// </returns>
        public static string PathGetRandomFileName()
        {
#if MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA
            byte[] numArray = new byte[10];
            RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();
            cryptoServiceProvider.GetBytes(numArray);
            char[] charArray = PathToBase32StringSuitableForDirName(numArray).ToCharArray();
            charArray[8] = '.';
            return new string(charArray, 0, 12);
#else
            return Path.GetRandomFileName();
#endif
        }

        /// <summary>
        /// A wrapper around Marshal.AllocHGlobal(), if available.
        /// </summary>
        /// <param name="size">The size of the buffer desired.</param>
        /// <returns>A pointer to the native memory.</returns>
        public static IntPtr MarshalAllocHGlobal(IntPtr size)
        {
#if MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA
            return MarshalEx.AllocHGlobal(size);
#else
            return Marshal.AllocHGlobal(size);
#endif
        }

        /// <summary>
        /// Frees memory that was allocated on the native heap.
        /// </summary>
        /// <param name="buffer">A pointer to native memory.</param>
        public static void MarshalFreeHGlobal(IntPtr buffer)
        {
#if MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA
            MarshalEx.FreeHGlobal(buffer);
#else
            Marshal.FreeHGlobal(buffer);
#endif
        }

        /// <summary>
        /// Resizes a block of memory previously allocated with <see cref="M:System.Runtime.InteropServices.Marshal.AllocHGlobal(System.IntPtr)"/>.
        /// </summary>
        /// <returns>
        /// A pointer to the reallocated memory. This memory must be released using <see cref="M:System.Runtime.InteropServices.Marshal.FreeHGlobal(System.IntPtr)"/>.
        /// </returns>
        /// <param name="presentValue">A pointer to memory allocated with
        ///  <see cref="M:System.Runtime.InteropServices.Marshal.AllocHGlobal(System.IntPtr)"/>.</param>
        /// <param name="newSize">The new size of the allocated block.</param><exception cref="T:System.OutOfMemoryException">
        /// There is insufficient memory to satisfy the request.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode"/></PermissionSet>
        public static IntPtr MarshalReAllocHGlobal(IntPtr presentValue, IntPtr newSize)
        {
#if MANAGEDESENT_ON_CORECLR
            IntPtr num = LocalReAlloc(
                presentValue,
                newSize,
                2); // LMEM_MOVEABLE

            if (num == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            else
            {
                return num;
            }
#else
            return Marshal.ReAllocHGlobal(presentValue, newSize);
#endif
        }

        /// <summary>Copies the contents of a managed <see cref="T:System.String" /> into unmanaged memory.</summary>
        /// <returns>The address, in unmanaged memory, to where the <paramref name="managedString" /> was copied, or 0 if <paramref name="managedString" /> is null.</returns>
        /// <param name="managedString">A managed string to be copied.</param>
        /// <exception cref="T:System.OutOfMemoryException">The method could not allocate enough native heap memory.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="managedString" /> parameter exceeds the maximum length allowed by the operating system.</exception>
        public static IntPtr MarshalStringToHGlobalAnsi(string managedString)
        {
#if MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA
            return MyStringToHGlobalAnsi(managedString);
#else
            return Marshal.StringToHGlobalAnsi(managedString);
#endif
        }

        /// <summary>Copies the contents of a managed <see cref="T:System.String" /> into unmanaged memory.</summary>
        /// <returns>The address, in unmanaged memory, to where the <paramref name="managedString" /> was copied, or 0 if <paramref name="managedString" /> is null.</returns>
        /// <param name="managedString">A managed string to be copied.</param>
        /// <exception cref="T:System.OutOfMemoryException">The method could not allocate enough native heap memory.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="managedString" /> parameter exceeds the maximum length allowed by the operating system.</exception>
        public static IntPtr MarshalStringToHGlobalUni(string managedString)
        {
#if MANAGEDESENT_ON_CORECLR
            return LibraryHelpers.MarshalStringToHGlobalUni(managedString);
#else
            return Marshal.StringToHGlobalUni(managedString);
#endif
        }

        /// <summary>
        /// A wrapper around Thread.BeginThreadAffinity(), if available.
        /// </summary>
        public static void ThreadBeginThreadAffinity()
        {
#if !MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA // Thread model has changed in Windows Store Apps.
            Thread.BeginThreadAffinity();
#endif
        }

        /// <summary>
        /// A wrapper around Thread.EndThreadAffinity(), if available.
        /// </summary>
        public static void ThreadEndThreadAffinity()
        {
#if !MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA // Thread model has changed in Windows Store Apps.
            Thread.EndThreadAffinity();
#endif
        }

        /// <summary>
        /// A wrapper around Thread.Sleep(), if available.
        /// </summary>
        /// <param name="ms">The time to sleep, in millisends.</param>
        public static void ThreadSleep(int ms)
        {
#if MANAGEDESENT_ON_WSA
            new System.Threading.ManualResetEvent(false).WaitOne(ms);
#else
            Thread.Sleep(ms);
#endif
        }

        /// <summary>
        /// A wrapper around Thread.Sleep(), if available.
        /// </summary>
        /// <param name="timeToSleep">The time to sleep.</param>
        public static void ThreadSleep(TimeSpan timeToSleep)
        {
#if MANAGEDESENT_ON_WSA
            new System.Threading.ManualResetEvent(false).WaitOne(timeToSleep);
#else
            Thread.Sleep(timeToSleep);
#endif
        }

        /// <summary>
        /// A wrapper around Culture.InfoGetLcid(), if available.
        /// </summary>
        /// <param name="culture">The culture of interest.</param>
        /// <returns>The LCID of the culture.</returns>
        public static int CultureInfoGetLcid(CultureInfo culture)
        {
#if MANAGEDESENT_ON_WSA
            // Always returning 1033 (en-us) is egregiously wrong, but this is
            // only used in a couple of spots in test code, so we'll live with it.
            return 1033;
#else
  #if MANAGEDESENT_ON_CORECLR
            return CultureInfoEx.GetLCID(culture);
  #else
            return culture.LCID;
  #endif // MANAGEDESENT_ON_CORECLR
#endif // MANAGEDESENT_ON_WSA
        }

        /// <summary>
        /// Converts from bytes to a random file name.
        /// </summary>
        /// <param name="buff">The random bytes to convert to a string.</param>
        /// <returns>A string that could be a file name.</returns>
        internal static string PathToBase32StringSuitableForDirName(byte[] buff)
        {
            StringBuilder sb = new StringBuilder(16);
            int length = buff.Length;
            int num1 = 0;
            do
            {
                byte num2 = num1 < length ? buff[num1++] : (byte)0;
                byte num3 = num1 < length ? buff[num1++] : (byte)0;
                byte num4 = num1 < length ? buff[num1++] : (byte)0;
                byte num5 = num1 < length ? buff[num1++] : (byte)0;
                byte num6 = num1 < length ? buff[num1++] : (byte)0;
                sb.Append(StaticBase32Char[(int)num2 & 31]);
                sb.Append(StaticBase32Char[(int)num3 & 31]);
                sb.Append(StaticBase32Char[(int)num4 & 31]);
                sb.Append(StaticBase32Char[(int)num5 & 31]);
                sb.Append(StaticBase32Char[(int)num6 & 31]);
                sb.Append(StaticBase32Char[((int)num2 & 224) >> 5 | ((int)num5 & 96) >> 2]);
                sb.Append(StaticBase32Char[((int)num3 & 224) >> 5 | ((int)num6 & 96) >> 2]);
                byte num7 = (byte)((uint)num4 >> 5);
                if (((int)num5 & 128) != 0)
                {
                    num7 |= (byte)8;
                }

                if (((int)num6 & 128) != 0)
                {
                    num7 |= (byte)16;
                }

                sb.Append(StaticBase32Char[(int)num7]);
            }
            while (num1 < length);
            return sb.ToString();
        }

        /// <summary>
        /// Converts a raw string to a meaningful path.
        /// </summary>
        /// <param name="rawPath">A non-fully-qualified path.</param>
        /// <returns>In Windows Store Apps programs the ApplicationData
        /// directory will be prefixed. Otherwise the original string is returned.
        /// </returns>
        private static string StringToPath(string rawPath)
        {
#if MANAGEDESENT_ON_WSA
            // Store information in the ApplicationData location. Note that we don't actually clean up after ourselves.
            var applicationData = Windows.Storage.ApplicationData.Current;
            var writeablePath = applicationData.LocalFolder.Path;

            return writeablePath + "\\" + rawPath;
#else
            return rawPath;
#endif
        }

#if MANAGEDESENT_ON_CORECLR && !MANAGEDESENT_ON_WSA
        // System.Runtime.InteropServices.Marshal

        /// <summary>Copies the contents of a managed <see cref="T:System.String" /> into unmanaged memory.</summary>
        /// <returns>The address, in unmanaged memory, to where the <paramref name="managedString" /> was copied, or 0 if <paramref name="managedString" /> is null.</returns>
        /// <param name="managedString">A managed string to be copied.</param>
        /// <exception cref="T:System.OutOfMemoryException">The method could not allocate enough native heap memory.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="managedString" /> parameter exceeds the maximum length allowed by the operating system.</exception>
        [SecurityCritical]
        private static unsafe IntPtr MyStringToHGlobalAnsi(string managedString)
        {
            if (managedString == null)
            {
                return IntPtr.Zero;
            }

            int charCountWithNull = managedString.Length + 1;
            int byteCount = charCountWithNull;

            if (byteCount < managedString.Length)
            {
                throw new ArgumentOutOfRangeException("managedString");
            }

            UIntPtr sizetdwBytes = new UIntPtr((uint)byteCount);
            IntPtr rawBuffer = Win32.NativeMethods.LocalAlloc(0, sizetdwBytes);
            if (rawBuffer == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            fixed (char* sourcePointer = managedString)
            {
                byte* destPointer = (byte*)rawBuffer;
                var utf8Encoding = new SlowAsciiEncoding();
                int bytesWritten = utf8Encoding.GetBytes(sourcePointer, charCountWithNull, destPointer, byteCount);
            }

            return rawBuffer;
        }
#endif

#if MANAGEDESENT_ON_CORECLR
        [DllImport("api-ms-win-core-heap-obsolete-l1-1-0.dll")]
        private static extern IntPtr LocalReAlloc(IntPtr hmem, IntPtr byteCount, uint flags);

        [DllImport("api-ms-win-core-file-l1-1-1.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFileW([MarshalAs(UnmanagedType.LPWStr)]string fileName);

        /// <summary>
        /// The C# version of the file access flags.
        /// </summary>
        [Flags]
        private enum EFileAccess : uint
        {
            //
            // Standard Section
            //

            AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
            MaximumAllowed = 0x2000000,     // MaximumAllowed access type

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,        // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,       // directory
            FILE_WRITE_DATA = 0x0002,       // file & pipe
            FILE_ADD_FILE = 0x0002,         // directory
            FILE_APPEND_DATA = 0x0004,      // file
            FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
            FILE_READ_EA = 0x0008,          // file & directory
            FILE_WRITE_EA = 0x0010,         // file & directory
            FILE_EXECUTE = 0x0020,          // file
            FILE_TRAVERSE = 0x0020,         // directory
            FILE_DELETE_CHILD = 0x0040,     // directory
            FILE_READ_ATTRIBUTES = 0x0080,      // all
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all

            //
            // Generic Section
            //

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS = StandardRightsRequired | Synchronize | 0x1FF,

            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            FILE_GENERIC_EXECUTE =
           StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
        }

        /// <summary>
        /// The C# equivalent for the sharing flags.
        /// </summary>
        [Flags]
        public enum EFileShare : uint
        {
            /// <summary>
            /// No options.
            /// </summary>
            None = 0x00000000,

            /// <summary>
            /// Enables subsequent open operations on an object to request read access. 
            /// Otherwise, other processes cannot open the object if they request read access. 
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>

            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access. 
            /// Otherwise, other processes cannot open the object if they request write access. 
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,

            /// <summary>
            /// Enables subsequent open operations on an object to request delete access. 
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }

        /// <summary>
        /// The CLR equivalent of the creation disposition.
        /// </summary>
        public enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,

            /// <summary>
            /// Creates a new file, always. 
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,

            /// <summary>
            /// Opens a file. The function fails if the file does not exist. 
            /// </summary>
            OpenExisting = 3,

            /// <summary>
            /// Opens a file, always. 
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,

            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right. 
            /// </summary>
            TruncateExisting = 5
        }

        /// <summary>
        /// The CLR equivalent of the file attributes.
        /// </summary>
        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        [DllImport("api-ms-win-core-file-l1-1-1.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFileW(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        /// <summary>
        /// File attributes are metadata values stored by the file system on disk and are used by the system and are available to developers via various file I/O APIs.
        /// </summary>
        [Flags]
////        [CLSCompliant(false)]
        internal enum FileAttributes : uint
        {
            /// <summary>
            /// A file that is read-only. Applications can read the file, but cannot write to it or delete it. This attribute is not honored on directories. For more information, see "You cannot view or change the Read-only or the System attributes of folders in Windows Server 2003, in Windows XP, or in Windows Vista".
            /// </summary>
            Readonly = 0x00000001,

            /// <summary>
            /// The file or directory is hidden. It is not included in an ordinary directory listing.
            /// </summary>
            Hidden = 0x00000002,

            /// <summary>
            /// A file or directory that the operating system uses a part of, or uses exclusively.
            /// </summary>
            System = 0x00000004,

            /// <summary>
            /// The handle that identifies a directory.
            /// </summary>
            Directory = 0x00000010,

            /// <summary>
            /// A file or directory that is an archive file or directory. Applications typically use this attribute to mark files for backup or removal.
            /// </summary>
            Archive = 0x00000020,

            /// <summary>
            /// This value is reserved for system use.
            /// </summary>
            Device = 0x00000040,

            /// <summary>
            /// A file that does not have other attributes set. This attribute is valid only when used alone.
            /// </summary>
            Normal = 0x00000080,

            /// <summary>
            /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage if sufficient cache memory is available, because typically, an application deletes a temporary file after the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.
            /// </summary>
            Temporary = 0x00000100,

            /// <summary>
            /// A file that is a sparse file.
            /// </summary>
            SparseFile = 0x00000200,

            /// <summary>
            /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
            /// </summary>
            ReparsePoint = 0x00000400,

            /// <summary>
            /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a directory, compression is the default for newly created files and subdirectories.
            /// </summary>
            Compressed = 0x00000800,

            /// <summary>
            /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute.
            /// </summary>
            Offline = 0x00001000,

            /// <summary>
            /// The file or directory is not to be indexed by the content indexing service.
            /// </summary>
            NotContentIndexed = 0x00002000,

            /// <summary>
            /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory, encryption is the default for newly created files and subdirectories.
            /// </summary>
            Encrypted = 0x00004000,

            /// <summary>
            /// This value is reserved for system use.
            /// </summary>
            Virtual = 0x00010000
        }

        /// <summary>
        /// The CLR equivalent for GetFielAttributesExW's info level parameter.
        /// </summary>
        public enum GET_FILEEX_INFO_LEVELS
        {
            GetFileExInfoStandard,
            GetFileExMaxInfoLevel
        }

        /// <summary>
        /// A filetime structure.
        /// </summary>
        public struct FileTime
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        /// <summary>
        /// The information returned by GetFileAttributesExW().
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes dwFileAttributes;
            public FileTime ftCreationTime;
            public FileTime ftLastAccessTime;
            public FileTime ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [DllImport("api-ms-win-core-file-l1-1-1.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileAttributesExW(
            string lpFileName,
            GET_FILEEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FILE_ATTRIBUTE_DATA fileData);


        [DllImport("api-ms-win-core-handle-l1-1-0.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("api-ms-win-core-file-l1-1-1.dll", CharSet = CharSet.Unicode)]
        private static extern bool CreateDirectoryW(string lpPathName, IntPtr lpSecurityAttributes);
        
        [DllImport("api-ms-win-core-file-l1-1-1.dll", CharSet = CharSet.Unicode)]
        private static extern bool RemoveDirectoryW(string lpPathName);
#endif // MANAGEDESENT_ON_WSA
    }

#if MANAGEDESENT_ON_CORECLR
    /// <summary>
    /// Represents a wrapper class for a file handle.
    /// 
    /// </summary>
////    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public sealed class SafeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeFileHandle()
            : base(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFileHandle"/> class. 
        /// </summary>
        /// <param name="preexistingHandle">An <see cref="T:System.IntPtr"/> object that represents the pre-existing handle to use.
        ///                 </param><param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).
        ///                 </param>
        public SafeFileHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            this.SetHandle(preexistingHandle);
        }

        /// <summary>
        /// Releases the underlying resource.
        /// </summary>
        /// <returns>Whether the resource was released.</returns>
        protected override bool ReleaseHandle()
        {
            return EseInteropTestHelper.CloseHandle(this.handle);
        }
    }

    /// <summary>
    /// Provides a base class for Win32 safe handle implementations in which the value of either 0 or -1 indicates an invalid handle.
    /// 
    /// </summary>
////    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
////    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    public abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
    {
        /// <summary>
        /// Gets a value that indicates whether the handle is invalid.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// true if the handle is not valid; otherwise, false.
        /// 
        /// </returns>
        public override bool IsInvalid
        {
            get
            {
                long rawValue = this.handle.ToInt64();
                if ((rawValue == 0) || (rawValue == -1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeHandleZeroOrMinusOneIsInvalid"/> class, specifying whether the handle is to be reliably released.
        /// 
        /// </summary>
        /// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent reliable release (not recommended).
        ///                 </param>
        protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle)
        {
        }
    }
#endif // MANAGEDESENT_ON_CORECLR
}

#if MANAGEDESENT_ON_WSA
namespace System.Threading
{
    /// <summary>
    /// Describes the priority of a thread.
    /// </summary>
    public enum ThreadPriority
    {
        /// <summary>
        /// The lowest priority.
        /// </summary>
        Lowest,

        /// <summary>
        /// A prioirty below normal.
        /// </summary>
        BelowNormal,

        /// <summary>
        /// The regular priority of a thread.
        /// </summary>
        Normal,

        /// <summary>
        /// Higher priority.
        /// </summary>
        AboveNormal,

        /// <summary>
        /// The highest prioirty of a thread.
        /// </summary>
        Highest,
    }
}
#endif // MANAGEDESENT_ON_WSA

#if MANAGEDESENT_ON_CORECLR
namespace TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test runner class.
    /// </summary>
    internal class TestRunner
    {
        /// <summary>
        /// The assembly to load and run tests from.
        /// </summary>
        private readonly Assembly assembly;

        /// <summary>
        /// Number of test failures.
        /// </summary>
        private int failureCount;

        /// <summary>
        /// Initializes a new instance of the TestRunner class.
        /// </summary>
        /// <param name="assemblyPath">
        /// The path to the assembly to run tests in.
        /// </param>
        public TestRunner(string assemblyPath)
        {
            this.ShuffleTests = true;
            this.CatchExceptions = true;
            // that can cause trouble in resolving type references between the default load context
            var myAssembly = new AssemblyName(typeof(TestRunner).GetTypeInfo().Assembly.FullName);
            var assemblyName = new AssemblyName();
            assemblyName.Name = assemblyPath;
            assemblyName.Version = myAssembly.Version;
            assemblyName.Flags = myAssembly.Flags;
            assemblyName.ContentType = myAssembly.ContentType;
            assemblyName.SetPublicKey(myAssembly.GetPublicKey());
            assemblyName.SetPublicKeyToken(myAssembly.GetPublicKeyToken());

            this.assembly = Assembly.Load(assemblyName);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this TestRunner should shuffle test execution order.
        /// </summary>
        public bool ShuffleTests
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this TestRunner should catch exceptions and increment the failure
        /// count or not cath them and fail right away.
        /// </summary>
        public bool CatchExceptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this TestRunner has encountered failures.
        /// </summary>
        public bool HasFailures
        {
            get
            {
                return 0 != this.failureCount;
            }
        }

        /// <summary>
        /// Run all the tests in the current assembly.
        /// </summary>
        public void RunTests()
        {
            this.RunTests(m => true);
        }

        /// <summary>
        /// Run all the tests in the current assembly that have
        /// the specified priority.
        /// </summary>
        /// <param name="priority">The priority of the tests to run.</param>
        public void RunTestsWithPriority(int priority)
        {
            this.RunTests(m => priority == GetTestMethodPriority(m));
        }

        /// <summary>
        /// Run all the tests in the current assembly that are in
        /// the specified class.
        /// </summary>
        /// <param name="className">The name of the class containing the tests to run.</param>
        /// <returns>The number of tests that ran.</returns>
        public int RunTestsInClass(string className)
        {
            return this.RunTests(m => -1 != m.DeclaringType.Name.IndexOf(className, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Run all the tests in the current assembly that have the
        /// specified name.
        /// </summary>
        /// <param name="name">The name of the tests to run.</param>
        /// <returns>The number of tests that ran.</returns>
        public int RunTestsWithName(string name)
        {
            return this.RunTests(m =>
            {
                string methodName = string.Format("{0}.{1}", m.DeclaringType.Name, m.Name);
                return -1 != methodName.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>
        /// List the tests in the current assembly.
        /// </summary>
        public void ListTests()
        {
            foreach (TypeInfo type in this.assembly.DefinedTypes)
            {
                if (MemberHasAttribute<TestClassAttribute>(type))
                {
                    foreach (MethodInfo method in type.DeclaredMethods)
                    {
                        bool methodIsTestMethod = MemberHasAttribute<TestMethodAttribute>(method);
                        string description = GetTestMethodDescription(method);

                        if (methodIsTestMethod)
                        {
                            Debug.WriteLine("\t{0}.{1}: {2}", type.Name, method.Name, description);
                            CheckMethod(type, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the first method on the type with the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute.</typeparam>
        /// <param name="type">The type to check.</param>
        /// <returns>The MethodInfo for the first method with the attribute, or null if no method exists.</returns>
        private static MethodInfo GetFirstMethodWithAttribute<T>(Type type) where T : Attribute
        {
            return type.GetTypeInfo().DeclaredMethods.FirstOrDefault(MemberHasAttribute<T>);
        }

        /// <summary>
        /// Determine if the member has an attribute of the specified type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="method">The member to check.</param>
        /// <returns>True if there is an attribute of the specified type, false otherwise.</returns>
        private static bool MemberHasAttribute<T>(MethodInfo method) where T : Attribute
        {
            return method.GetCustomAttributes(true).OfType<T>().Any();
        }

        /// <summary>
        /// Determine if the member has an attribute of the specified type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="typeInfo">The member to check.</param>
        /// <returns>True if there is an attribute of the specified type, false otherwise.</returns>
        private static bool MemberHasAttribute<T>(TypeInfo typeInfo) where T : Attribute
        {
            return typeInfo.GetCustomAttributes(typeof(T), true).Any();
        }

        /// <summary>
        /// Get the description of a test method.
        /// </summary>
        /// <param name="method">The method to get the description for.</param>
        /// <returns>The description of the method.</returns>
        private static string GetTestMethodDescription(MethodInfo method)
        {
            string description = string.Empty;

            foreach (Attribute methodAttribute in method.GetCustomAttributes(typeof(DescriptionAttribute), true))
            {
                description += ((DescriptionAttribute) methodAttribute).Description;
            }

            return description;
        }

        /// <summary>
        /// Get the priority of a test method.
        /// </summary>
        /// <param name="method">The method to get the priority for.</param>
        /// <returns>The priority of the method.</returns>
        private static int GetTestMethodPriority(MethodInfo method)
        {
            foreach (Attribute methodAttribute in method.GetCustomAttributes(typeof(PriorityAttribute), true))
            {
                return ((PriorityAttribute) methodAttribute).Priority;
            }
            
            return -1;
        }

        /// <summary>
        /// Get the expected exception for the method.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>The type of the expected exception, or null.</returns>
        private static Type GetExpectedException(MethodInfo method)
        {
            var allAttributes = method.GetCustomAttributes(typeof(ExpectedExceptionAttribute), true);
            
            foreach (Attribute attribute in allAttributes)
            {
                return ((ExpectedExceptionAttribute) attribute).ExceptionType;
            }

            return null;
        }

        /// <summary>
        /// Check the method to see if it call be called as a test. Print an error
        /// if it cannot.
        /// </summary>
        /// <param name="type">The type containing the method.</param>
        /// <param name="method">The method.</param>
        private static void CheckMethod(TypeInfo type, MethodInfo method)
        {
            if (method.IsStatic)
            {
                Debug.WriteLine("\t\tERROR: {0}.{1} is static", type.FullName, method.Name);
            }

            if (typeof(void) != method.ReturnType)
            {
                Debug.WriteLine("\t\tERROR: {0}.{1} returns should return void", type.FullName, method.Name);
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (0 != parameters.Length)
            {
                Debug.WriteLine("\t\tERROR: {0}.{1} should take void", type.FullName, method.Name);
                foreach (ParameterInfo parameter in parameters)
                {
                    Debug.WriteLine("\t\t\t{0}", parameter.ParameterType);
                }
            }
        }

        /// <summary>
        /// Invoke the given method.
        /// </summary>
        /// <param name="obj">The object to invoke the method on.</param>
        /// <param name="method">The method to invoke.</param>
        private static void InvokeMethod(object obj, MethodInfo method)
        {
            if (null != method)
            {
                if (null == obj && !method.IsStatic)
                {
                    throw new ArgumentNullException("obj", "attempting to invoke a non-static method on a null object");
                }

                int parameterCount = method.GetParameters().Length;
                object[] parameters = (parameterCount > 0) ? new object[parameterCount] : null;


                method.Invoke(obj, parameters);
            }
        }

        /// <summary>
        /// Shuffle an array.
        /// </summary>
        /// <param name="arrayToShuffle">
        /// The array to shuffle.
        /// </param>
        /// <typeparam name="T">
        /// Type of object in the array.
        /// </typeparam>
        private static void Shuffle<T>(IList<T> arrayToShuffle)
        {
            Random rand = new Random();
            for (int i = 0; i < arrayToShuffle.Count; ++i)
            {
                int swap = rand.Next(i, arrayToShuffle.Count);
                T temp = arrayToShuffle[i];
                arrayToShuffle[i] = arrayToShuffle[swap];
                arrayToShuffle[swap] = temp;
            }
        }

        /// <summary>
        /// Run the tests in the current assembly that match the selector.
        /// </summary>
        /// <param name="selector">
        /// A selector method that can choose a particular method.
        /// </param>
        /// <returns>The number of tests that ran.</returns>
        private int RunTests(Func<MethodInfo, bool> selector)
        {
            int totalTests = 0;
            int failures = 0;
            var totalStopwatch = Stopwatch.StartNew();

            TypeInfo[] typeinfos = this.assembly.DefinedTypes.Where(MemberHasAttribute<TestClassAttribute>).ToArray();
            Type[] types = new Type[typeinfos.Length];

            for (int i = 0; i < typeinfos.Length; ++i)
            {
                types[i] = typeinfos[i].AsType();
            }

            if (this.ShuffleTests)
            {
                Shuffle(types);
            }

            foreach (Type type in types)
            {
                bool failed = false;

                // ConstructorInfo constructor = System.Reflection.Emit.TypeBuilder.GetConstructor(type, null);
                ConstructorInfo[] constructors = type.GetTypeInfo().DeclaredConstructors.ToArray();
                if (null == constructors || constructors.Length == 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Type {0} does not have a constructor", type.FullName));
                }

                // ".cctor' also matches constructors with empty parameter lists.
                ConstructorInfo[] emptyConstructors = constructors
                    .Where(con => (con.GetParameters().Length == 0 && con.Name == ".ctor"))
                    .ToArray();
                if (emptyConstructors.Length > 1)
                {
                    throw new InvalidOperationException(
                        string.Format("Type {0} has too many constructors: {1}.", type.FullName, emptyConstructors.Length));
                }

                ConstructorInfo constructor = emptyConstructors[0];
                if (constructor.GetParameters().Length > 0)
                {
                    throw new InvalidOperationException(
                        string.Format("Type {0} does not have a default constructor.", type.FullName));
                }

                MethodInfo initialize = GetFirstMethodWithAttribute<TestInitializeAttribute>(type);
                MethodInfo cleanup = GetFirstMethodWithAttribute<TestCleanupAttribute>(type);

                MethodInfo[] methods = type.GetTypeInfo().DeclaredMethods.Where(MemberHasAttribute<TestMethodAttribute>).Where(selector).ToArray();

                if (this.ShuffleTests)
                {
                    Shuffle(methods);
                }

                InvokeMethod(null, GetFirstMethodWithAttribute<ClassInitializeAttribute>(type));
                try
                {
                    foreach (MethodInfo method in methods)
                    {
                        string description = GetTestMethodDescription(method);
                        Type expectedException = GetExpectedException(method);

                        Debug.WriteLine("{0}.{1}: ", type.Name, method.Name);

                        Debug.WriteLine(".......................................................................................");
                        Stopwatch stopwatch = Stopwatch.StartNew();

                        object obj = constructor.Invoke(null);
                        InvokeMethod(obj, initialize);
                        try
                        {
                            totalTests++;
                            InvokeMethod(obj, method);
                            if (null != expectedException)
                            {
                                failures++;
                                failed = true;
                                Debug.WriteLine(String.Empty);
                                Debug.WriteLine("Failure in {0}.{1}: {2}", type.Name, method.Name, description);
                                Debug.WriteLine("Expected exception {0} was not thrown", expectedException);

                                if (!this.CatchExceptions)
                                {
                                    Assert.Fail(String.Format("Expected exception {0} was not thrown", expectedException));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is TargetInvocationException)
                            {
                                ex = ex.InnerException;
                            }

                            if (ex is AssertInconclusiveException)
                            {
                                Debug.WriteLine(String.Empty);
                                Debug.WriteLine("Assert.Inconclusive in {0}.{1}: {2}", type.Name, method.Name, description);
                                Debug.WriteLine("{0}", ex);
                            }
                            else if (null == expectedException)
                            {
                                failures++;
                                failed = true;
                                Debug.WriteLine(String.Empty);
                                Debug.WriteLine("Failure in {0}.{1}: {2}", type.Name, method.Name, description);
                                Debug.WriteLine("Unexpected exception {0}", ex);

                                if (!this.CatchExceptions)
                                {
                                    throw;
                                }
                            }
                            else if (!ex.GetType().Equals(expectedException) && !ex.GetType().GetTypeInfo().IsSubclassOf(expectedException))
                            {
                                failures++;
                                failed = true;
                                Debug.WriteLine(String.Empty);
                                Debug.WriteLine("Failure in {0}.{1}: {2}", type.Name, method.Name, description);
                                Debug.WriteLine("Unexpected exception {0}, expected {1}", ex, expectedException);

                                if (!this.CatchExceptions)
                                {
                                    throw;
                                }
                            }
                            else if (ex.GetType().Equals(expectedException))
                            {
                                Debug.WriteLine(String.Empty);
                                Debug.WriteLine("Exception {0} caught, and it was expected", ex);
                            }
                            else if (ex.GetType().GetTypeInfo().IsSubclassOf(expectedException))
                            {
                                Debug.WriteLine(String.Empty);
                                Debug.WriteLine("Exception {0} caught, subclass of expected {1}", ex, expectedException);
                            }
                        }
                        finally
                        {
                            if (!failed || this.CatchExceptions)
                            {
                                InvokeMethod(obj, cleanup);
                            }
                        }

                        stopwatch.Stop();
                        Debug.WriteLine("\tFinishes in {0}", stopwatch.Elapsed);
                    }
                }
                finally
                {
                    if (!failed || this.CatchExceptions)
                    {
                        InvokeMethod(null, GetFirstMethodWithAttribute<ClassCleanupAttribute>(type));
                    }
                }
            }

            totalStopwatch.Stop();
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("{0} tests", totalTests);
            Debug.WriteLine("\t{0} failures", failures);
            Debug.WriteLine("\t{0}", totalStopwatch.Elapsed);

            this.failureCount += failures;

            return totalTests;
        }
    }
}

namespace TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    //    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test runner class.
    /// </summary>
    public static class MainClass
    {
        /// <summary>
        /// Main method. Executed when the program runs.
        /// </summary>
        /// <param name="args">Commandline arguments.</param>
        /// <returns>Zero if all tests pass, non-zero otherwise.</returns>
        public static int TestRunnerMain(params string[] args)
        {
            string testDllPath = null;
            Dictionary<string, string[]> arguments = new Dictionary<string, string[]>(args.Length - 1);

            char[] argumentSeparators = new[] { ':' };
            char[] valueSeparators = new[] { ',' };
            string[] emptyValues = new string[0];
            string previousArg = null;

            foreach (string s in args)
            {
                if (s.StartsWith("-") || s.StartsWith("/"))
                {
                    bool needToSetPreviousArg = false;
                    string[] parsed = s.Substring(1).Split(argumentSeparators, 2);
                    if (parsed.Length > 1)
                    {
                        arguments[parsed[0].ToLower()] = parsed[1].Split(valueSeparators);
                        if (string.IsNullOrEmpty(parsed[1]))
                        {
                            needToSetPreviousArg = true;
                        }
                    }
                    else
                    {
                        needToSetPreviousArg = true;
                    }

                    if (needToSetPreviousArg)
                    {
                        previousArg = parsed[0].ToLower();
                        arguments[previousArg] = emptyValues;
                        continue;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(previousArg))
                    {
                        if (null != testDllPath)
                        {
                            Debug.WriteLine("Cannot specify multiple test DLLs");
                            return -1;
                        }

                        testDllPath = s;
                    }
                    else
                    {
                        arguments[previousArg] = s.Split(valueSeparators);
                    }
                }
                previousArg = null;
            }

            if (null == testDllPath)
            {
                PrintHelp();
                return -1;
            }

            try
            {
                var testRunner = new TestRunner(testDllPath);

                if (arguments.ContainsKey("help"))
                {
                    PrintHelp();
                }

                if (arguments.ContainsKey("list"))
                {
                    testRunner.ListTests();
                }

                if (arguments.ContainsKey("donotshuffle"))
                {
                    testRunner.ShuffleTests = false;
                }

                if (arguments.ContainsKey("donotcatch"))
                {
                    testRunner.CatchExceptions = false;
                }

                if (arguments.ContainsKey("priority"))
                {
                    foreach (string p in arguments["priority"])
                    {
                        int priority;
                        if (int.TryParse(p, out priority))
                        {
                            testRunner.RunTestsWithPriority(priority);
                        }
                        else
                        {
                            Debug.WriteLine("Unknown priority {0}", p);
                            return -1;
                        }
                    }
                }

                if (arguments.ContainsKey("suite"))
                {
                    foreach (string suite in arguments["suite"])
                    {
                        if (testRunner.RunTestsInClass(suite) < 1)
                        {
                            Debug.WriteLine("No tests found in suite {0} or suite not found", suite);
                            return -1;
                        }
                    }
                }

                if (arguments.ContainsKey("test"))
                {
                    foreach (string test in arguments["test"])
                    {
                        if (testRunner.RunTestsWithName(test) < 1)
                        {
                            Debug.WriteLine("Test {0} not found", test);
                            return -1;
                        }
                    }
                }

                if (arguments.ContainsKey("all"))
                {
                    testRunner.RunTests();
                }

                if (testRunner.HasFailures)
                {
                    return -1;
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    Debug.WriteLine(loaderException.Message);
                }

                throw;
            }

            return 0;
        }

        /// <summary>
        /// Print command line argument help.
        /// </summary>
        private static void PrintHelp()
        {
            string program = "testrunner.exe";
            Debug.WriteLine("{0} <test DLL name> [-Priority:<priorities>] [-Suite:<suites>] [-Test:<tests>] [-Help] [-All] [-DoNotShuffle] [-DoNotCatch]", program);
            Debug.WriteLine("\t-Priority: run tests with the given priority.");
            Debug.WriteLine("\t-Suite: run tests in the matching suite.");
            Debug.WriteLine("\t-Test: run tests that match the name.");
            Debug.WriteLine("\t-Help: print this help.");
            Debug.WriteLine("\t-All: run all tests.");
            Debug.WriteLine("\t-DoNotShuffle: do not shuffle tests, will always be executed in the same order in which they were enumerated.");
            Debug.WriteLine("\t-DoNotCatch: do not catch failures. The runner will crash with an unhandled exception on failure.");
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("Values for the priority, suite and tests arguments can be comma separated (no spaces).");
            Debug.WriteLine("Suite and test names are matched with case insensitive substring matching.");
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("Examples:");
            Debug.WriteLine("\t{0} InteropApiTests.dll -Priority:0,1", program);
            Debug.WriteLine("\t{0} InteropApiTests.dll -Suite:JetThreadStats,JetTracing", program);
            Debug.WriteLine("\t{0} InteropApiTests.dll -Priority:0 -Test:RetrieveColumnsPerfTest.TestJetGetBookmarkPerf", program);
        }
    }
}
#endif // MANAGEDESENT_ON_CORECLR
