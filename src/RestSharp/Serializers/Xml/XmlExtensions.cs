using System.Xml.Linq;
using RestSharp.Extensions;

namespace RestSharp.Serializers.Xml; 

/// <summary>
/// XML Extension Methods
/// </summary>
public static class XmlExtensions {
    /// <summary>
    /// Returns the name of an element with the namespace if specified
    /// </summary>
    /// <param name="name">Element name</param>
    /// <param name="namespace">XML Namespace</param>
    /// <returns></returns>
    public static XName AsNamespaced(this string name, string @namespace) {
        XName xName = name;

        if (@namespace.HasValue()) xName = XName.Get(name, @namespace);

        return xName;
    }
}