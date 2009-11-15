using System;
namespace RestSharp.Deserializers
{
	public interface IDeserializer
	{
		X Deserialize<X>(string content) where X : new();
		string RootElement { get; set; }
		string Namespace { get; set; }
		DateFormat DateFormat { get; set; }
	}
}
