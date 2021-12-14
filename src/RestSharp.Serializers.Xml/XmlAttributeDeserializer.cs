using System.Reflection;
using System.Xml.Linq;
using RestSharp.Extensions;

namespace RestSharp.Serializers.Xml;

public class XmlAttributeDeserializer : XmlDeserializer {
    protected override object? GetValueFromXml(XElement? root, XName? name, PropertyInfo prop, bool useExactName) {
        var isAttribute = false;

        //Check for the DeserializeAs attribute on the property
        var options = prop.GetAttribute<DeserializeAsAttribute>();

        if (options != null) {
            name        = options.Name ?? name;
            isAttribute = options.Attribute;
        }

        if (!isAttribute) return base.GetValueFromXml(root, name, prop, useExactName);

        var attributeVal = GetAttributeByName(root!, name!, useExactName);

        return attributeVal?.Value ?? base.GetValueFromXml(root, name, prop, useExactName);
    }
}