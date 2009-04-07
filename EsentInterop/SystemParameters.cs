//-----------------------------------------------------------------------
// <copyright file="SystemParameters.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// This class provides static properties to set and get
    /// global ESENT system parameters
    /// </summary>
    public static class SystemParameters
    {
        /// <summary>
        /// Gets or sets the maximum size of the database page cache. The size
        /// is in database pages. If this parameter is left to its default value, then the
        /// maximum size of the cache will be set to the size of physical memory when JetInit
        /// is called.
        /// </summary>
        public static int CacheSizeMax
        {
            get
            {
                return SystemParameters.GetIntegerParameter(JET_param.CacheSizeMax);
            }

            set
            {
                SystemParameters.SetIntegerParameter(JET_param.CacheSizeMax, value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the database cache in pages. By default the
        /// database cache will automatically tune its size, setting this property
        /// to a non-zero value will cause the cache to adjust itself to the target
        /// size.
        /// </summary>
        public static int CacheSize
        {
            get
            {
                return SystemParameters.GetIntegerParameter(JET_param.CacheSize);
            }

            set
            {
                SystemParameters.SetIntegerParameter(JET_param.CacheSize, value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the database pages, in bytes.
        /// </summary>
        public static int DatabasePageSize
        {
            get
            {
                return SystemParameters.GetIntegerParameter(JET_param.DatabasePageSize);
            }

            set
            {
                SystemParameters.SetIntegerParameter(JET_param.DatabasePageSize, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum size of the database page cache, in database pages.
        /// </summary>
        public static int CacheSizeMin
        {
            get
            {
                return SystemParameters.GetIntegerParameter(JET_param.CacheSizeMin);
            }

            set
            {
                SystemParameters.SetIntegerParameter(JET_param.CacheSizeMin, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of instances that can be created.
        /// </summary>
        public static int MaxInstances
        {
            get
            {
                return SystemParameters.GetIntegerParameter(JET_param.MaxInstances);
            }

            set
            {
                SystemParameters.SetIntegerParameter(JET_param.MaxInstances, value);
            }
        }

        /// <summary>
        /// Set a system parameter which is a string.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetStringParameter(JET_param param, string value)
        {
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, param, 0, value);
        }

        /// <summary>
        /// Get a system parameter which is a string.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private static string GetStringParameter(JET_param param)
        {
            int ignored = 0;
            string value;
            Api.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, param, ref ignored, out value, 1024);
            return value;
        }

        /// <summary>
        /// Set a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetIntegerParameter(JET_param param, int value)
        {
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, param, value, null);
        }

        /// <summary>
        /// Get a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private static int GetIntegerParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            Api.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, param, ref value, out ignored, 0);
            return value;
        }
    }
}