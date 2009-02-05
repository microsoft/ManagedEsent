//-----------------------------------------------------------------------
// <copyright file="VistaParam.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Vista
{
    /// <summary>
    /// System parameters that have been added to the Vista version of ESENT.
    /// </summary>
    public static class VistaParam
    {
        /// <summary>
        /// This parameter exposes multiple sets of default values for the
        /// entire set of system parameters. When this parameter is set to
        /// a specific configuration, all system parameter values are reset
        /// to their default values for that configuration. If the
        /// configuration is set for a specific instance then global system
        /// parameters will not be reset to their default values.
        /// Small Configuration (0): The database engine is optimized for memory use. 
        /// Legacy Configuration (1): The database engine has its traditional defaults.
        /// </summary>
        public const JET_param Configuration = (JET_param)129;

        /// <summary>
        /// This parameter is used to control when the database engine accepts
        /// or rejects changes to a subset of the system parameters. This
        /// parameter is used in conjunction with JET_paramConfiguration to
        /// prevent some system parameters from being set away from the selected
        /// configuration's defaults.
        /// </summary>
        public const JET_param EnabledAdvanced = (JET_param)130;
    }
}