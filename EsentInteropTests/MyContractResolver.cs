//-----------------------------------------------------------------------
// <copyright file="MyContractResolver.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Allow serialization/deserialization of all properties/fields on object using newtonsoft.json serializer.
    /// </summary>
    public class MyContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Mark all properties/fields as needing serialization.
        /// </summary>
        /// <param name="objectType">
        /// type of the object.
        /// </param>
        /// <returns>
        /// List of all members to serialize
        /// </returns>
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MemberInfo[] fields = objectType.GetFields(flags);
            return fields
                .Concat(objectType.GetProperties(flags).Where(propInfo => propInfo.CanWrite))
                .ToList();
        }

        /// <summary>
        /// Mark all properties/fields as needing serialization.
        /// </summary>
        /// <param name="type">
        /// type of the object.
        /// </param>
        /// <param name="memberSerialization">
        /// unused parameter.
        /// </param>
        /// <returns>
        /// List of all members to serialize
        /// </returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
               return base.CreateProperties(type, MemberSerialization.Fields);
        }
    }
}
