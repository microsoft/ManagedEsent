//-----------------------------------------------------------------------
// <copyright file="Cleanup.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace IsamUnitTests
{
    using System;
    using System.IO;

    /// <summary>
    /// Methods for test cleanup.
    /// </summary>
    internal static class Cleanup
    {
        /// <summary>
        /// The maximum number of attempts for the cleanup.
        /// </summary>
        private const int MaxAttempts = 3;

        /// <summary>
        /// Delete a directory, retrying the operation if the delete fails.
        /// </summary>
        /// <param name="directory">
        /// The directory to delete.
        /// </param>
        public static void DeleteDirectoryWithRetry(string directory)
        {
            PerformActionWithRetry(
                () =>
                {
                    if (IsamTestHelper.DirectoryExists(directory))
                    {
                        IsamTestHelper.DirectoryDelete(directory, true);
                    }
                });
        }

        /// <summary>
        /// Delete a file, retrying the operation if the delete fails.
        /// </summary>
        /// <param name="file">
        /// The file to delete.
        /// </param>
        public static void DeleteFileWithRetry(string file)
        {
            PerformActionWithRetry(() => IsamTestHelper.FileDelete(file));
        }

        /// <summary>
        /// Perform an action and retry on I/O failure, with a 1 second
        /// sleep between retries.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        private static void PerformActionWithRetry(Action action)
        {
            for (int attempt = 1; attempt <= MaxAttempts; ++attempt)
            {
                try
                {
                    action();
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    if (MaxAttempts == attempt)
                    {
                        throw;
                    }
                }
                catch (IOException)
                {
                    if (MaxAttempts == attempt)
                    {
                        throw;
                    }
                }

                IsamTestHelper.ThreadSleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}