﻿using System.IO;

namespace RestSharp.Serializers
{
	/// <summary>
	/// Default JSON serializer for request bodies
	/// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
	/// </summary>
	public class JsonSerializer : ISerializer
	{
		/// <summary>
		/// Default serializer
		/// </summary>
		public JsonSerializer()
		{
			ContentType = "application/json";
		}

		public string Serialize(object obj)
		{
			return Serialize(obj, Options);
		}
		
		/// <summary>
		/// Serialize the object as JSON
		/// </summary>
		/// <param name="obj">Object to serialize</param>
		/// <returns>JSON as String</returns>
		public string Serialize(object obj, SerializerOptions options) 
		{
			return SimpleJson.SerializeObject(obj, options);
		}

		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string DateFormat { get; set; }
		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string RootElement { get; set; }
		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string Namespace { get; set; }
		/// <summary>
		/// Content type for serialized content
		/// </summary>
		public string ContentType { get; set; }
		/// <summary>
		/// Current serialization options
		/// </summary>
		public SerializerOptions Options { get; set; }
	}
}