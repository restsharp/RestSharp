using System;

namespace RestSharp.Deserializers
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AsDateTimeFormatAttribute : Attribute
    {
        public string Format { get; set; }

        public AsDateTimeFormatAttribute(string format)
        {
            Format = format;
        }
    }
}
