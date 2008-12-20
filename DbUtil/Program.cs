//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Exchange.Isam.Utilities
{
    /// <summary>
    /// Contains the static method that starts the program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method, called when the program starts.
        /// </summary>
        /// <param name="args">Arguments to the program</param>
        public static void Main(string[] args)
        {
            var dbutil = new Dbutil();
            dbutil.Execute(args);
        }
    }
}
