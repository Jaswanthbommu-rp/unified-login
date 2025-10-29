using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnifiedLogin.SharedObjects.Extensions;

namespace UnifiedLogin.BusinessLogic.Logic
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private List<string> _serializableProperties = new List<string>();

        public DynamicContractResolver(string serializableProperties)
        {
            if (!string.IsNullOrWhiteSpace(serializableProperties))
            {
                string[] returnFields = serializableProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                _serializableProperties = returnFields.ToList();
                if (!_serializableProperties.Any(p => p.Equals("id", StringComparison.OrdinalIgnoreCase)))
                {
                    _serializableProperties.Add("id");
                }
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (_serializableProperties.Count > 0)
            {
                bool serialize = _serializableProperties.Any(sp => sp.TrimStart().TrimEnd().Equals(property.PropertyName, StringComparison.OrdinalIgnoreCase));
                property.ShouldSerialize = i => serialize;
                property.Ignored = !serialize;
            }
            return property;
        }
    }
}
