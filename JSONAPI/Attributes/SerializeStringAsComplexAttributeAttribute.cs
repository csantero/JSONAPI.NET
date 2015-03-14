using System;

namespace JSONAPI.Attributes
{
    /// <summary>
    /// String properties decorated with this attribute will be serialized as
    /// json-api "complex attributes". The value of such a property will be
    /// a json object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializeStringAsComplexAttributeAttribute : Attribute
    {
    }
}
