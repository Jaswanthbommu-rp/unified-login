using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Used by Newtonsoft.Json.JsonSerializer to resolve a Newtonsoft.Json.Serialization.JsonContract for a given System.Type.
	/// </summary>
	public class SerializerContractResolver : DefaultContractResolver
	{
		#region Private Variables
		private readonly Dictionary<Type, HashSet<string>> _ignores;
		private readonly Dictionary<Type, Dictionary<string, string>> _renames;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public SerializerContractResolver()
		{
			_ignores = new Dictionary<Type, HashSet<string>>();
			_renames = new Dictionary<Type, Dictionary<string, string>>();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Exclude property from Json Serialization
		/// </summary>
		/// <param name="type">Property datatype</param>
		/// <param name="jsonPropertyNames">Json PropertyName</param>
		public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
		{
			if (!_ignores.ContainsKey(type))
			{
				_ignores[type] = new HashSet<string>();
			}

			foreach (var prop in jsonPropertyNames)
			{
				_ignores[type].Add(prop);
			}
		}

		/// <summary>
		/// Rename Json Property before Serialization
		/// </summary>
		/// <param name="type">Property datatype</param>
		/// <param name="propertyName">Json PropertyName</param>
		/// <param name="newJsonPropertyName">New Json PropertyName</param>
		public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
		{
			if (!_renames.ContainsKey(type))
			{
				_renames[type] = new Dictionary<string, string>();
			}
			_renames[type][propertyName] = newJsonPropertyName;
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Create a Json Property
		/// </summary>
		/// <param name="member">Object to include the new property</param>
		/// <param name="memberSerialization">Object serialization options</param>
		/// <returns></returns>
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (IsIgnored(property.DeclaringType, property.PropertyName))
			{
				property.ShouldSerialize = i => false;
			}
			string newJsonPropertyName = string.Empty;
			if (IsRenamed(property.DeclaringType, property.PropertyName, out newJsonPropertyName))
			{
				property.PropertyName = newJsonPropertyName;
			}

			return property;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Is the Json property Ignored when serialized
		/// </summary>
		/// <param name="type">Property datatype</param>
		/// <param name="jsonPropertyName">Json PropertyName</param>
		/// <returns></returns>
		private bool IsIgnored(Type type, string jsonPropertyName)
		{
			if (!_ignores.ContainsKey(type))
			{
				return false;
			}
			return _ignores[type].Contains(jsonPropertyName);
		}

		/// <summary>
		/// Is the Jsin property has been renamed?
		/// </summary>
		/// <param name="type">Property datatype</param>
		/// <param name="jsonPropertyName">Json PropertyName</param>
		/// <param name="newJsonPropertyName">New Json PropertyName</param>
		/// <returns>Has the Json property been renamed?</returns>
		private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
		{
			Dictionary<string, string> renames;

			if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
			{
				newJsonPropertyName = null;
				return false;
			}
			return true;
		}
		#endregion
	}
}
