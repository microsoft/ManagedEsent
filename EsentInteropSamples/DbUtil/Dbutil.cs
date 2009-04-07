//-----------------------------------------------------------------------
// <copyright file="DbUtil.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Isam.Esent.Utilities
{
    /// <summary>
    /// Database utilities
    /// </summary>
    internal partial class Dbutil
    {
        /// <summary>
        /// Maps a commad to the method that implements it.
        /// </summary>
        private readonly Dictionary<string, Action<string[]>> actions;

        /// <summary>
        /// Initializes a new instance of the Dbutil class.
        /// </summary>
        public Dbutil()
        {
            this.actions = new Dictionary<string, Action<string[]>>();
            this.actions.Add("dumpmetadata", this.DumpMetaData);
            this.actions.Add("createsample", this.CreateSampleDb);
            this.actions.Add("dumptocsv", this.DumpToCsv);
        }

        /// <summary>
        /// Execute the command given by the arguments.
        /// </summary>
        /// <param name="args">The arguments to the program.</param>
        public void Execute(string[] args)
        {
            if (null == args)
            {
                throw new ArgumentNullException("args");
            }

            if (args.Length < 1)
            {
                throw new ArgumentException("specify arguments", "args");
            }

            IEnumerable<Action<string[]>> methods = from x in this.actions
                                      where 0 == String.Compare(x.Key, args[0], true)
                                      select x.Value;
            if (methods.Count() != 1)
            {
                throw new ArgumentException("unknown command", "args");
            }

            // now shift off the first argument
            var newArgs = new string[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            methods.Single()(newArgs);
        }
    }
}
