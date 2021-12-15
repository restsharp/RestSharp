using System.Globalization;
using RestSharp.Extensions;

// ReSharper disable once CheckNamespace
namespace RestSharp.Serializers; 

/// <summary>
/// Allows control how class and property names and values are serialized by XmlSerializer
/// Currently not supported with the JsonSerializer
/// When specified at the property level the class-level specification is overridden
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class SerializeAsAttribute : Attribute {
    public SerializeAsAttribute() {
        NameStyle = NameStyle.AsIs;
        Index     = int.MaxValue;
        Culture   = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// The name to use for the serialized element
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Sets the value to be serialized as an Attribute instead of an Element
    /// </summary>
    public bool Attribute { get; set; }

    /// <summary>
    /// Sets the value to be serialized as text content of current Element instead of an new Element
    /// </summary>
    public bool Content { get; set; }

    /// <summary>
    /// The culture to use when serializing
    /// </summary>
    public CultureInfo Culture { get; set; }

    /// <summary>
    /// Transforms the casing of the name based on the selected value.
    /// </summary>
    public NameStyle NameStyle { get; set; }

    /// <summary>
    /// The order to serialize the element. Default is int.MaxValue.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Called by the attribute when NameStyle is speficied
    /// </summary>
    /// <param name="input">The string to transform</param>
    /// <returns>String</returns>
    public string TransformName(string input) {
        var name = Name ?? input;

        return NameStyle switch {
            NameStyle.CamelCase  => name.ToCamelCase(Culture),
            NameStyle.PascalCase => name.ToPascalCase(Culture),
            NameStyle.LowerCase  => name.ToLower(Culture),
            _                    => input
        };
    }
}

/// <summary>
/// Options for transforming casing of element names
/// </summary>
public enum NameStyle { AsIs, CamelCase, LowerCase, PascalCase }