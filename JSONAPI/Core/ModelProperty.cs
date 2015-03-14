using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JSONAPI.Attributes;
using Newtonsoft.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Stores a model's property and its usage.
    /// </summary>
    public abstract class ModelProperty
    {
        internal ModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault)
        {
            IgnoreByDefault = ignoreByDefault;
            JsonKey = jsonKey;
            Property = property;
        }

        /// <summary>
        /// The PropertyInfo backing this ModelProperty
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// The key that will be used to represent this property in JSON API documents
        /// </summary>
        public string JsonKey { get; private set; }

        /// <summary>
        /// Whether this property should be ignored by default for serialization.
        /// </summary>
        public bool IgnoreByDefault { get; private set; }
    }

    /// <summary>
    /// A ModelProperty representing a flat field on a resource object
    /// </summary>
    public sealed class FieldModelProperty : ModelProperty
    {
        internal FieldModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault)
            : base(property, jsonKey, ignoreByDefault)
        {
            var attr = property.GetCustomAttributes(false).FirstOrDefault(a => a is UsesFilterExpressionProviderAttribute) as UsesFilterExpressionProviderAttribute;
            if (attr != null)
            {
                var providerType = attr.ProviderType;
                FilterExpressionProvider = Activator.CreateInstance(providerType) as IFilterExpressionProvider;
                if (FilterExpressionProvider == null) throw new Exception("The type provided was not an IFilterExpressionProvider.");
            }
        }

        /// <summary>
        /// The provider to use when trying to build an expression for filtering by this property.
        /// </summary>
        public IFilterExpressionProvider FilterExpressionProvider { get; private set; }
    }

    /// <summary>
    /// A ModelProperty representing a relationship to another resource
    /// </summary>
    public class RelationshipModelProperty : ModelProperty
    {
        internal RelationshipModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault, Type relatedType, bool isToMany)
            : base(property, jsonKey, ignoreByDefault)
        {
            RelatedType = relatedType;
            IsToMany = isToMany;
        }

        /// <summary>
        /// The type of resource found on the other side of this relationship
        /// </summary>
        public Type RelatedType { get; private set; }

        /// <summary>
        /// Whether the property represents a to-many (true) or to-one (false) relationship
        /// </summary>
        public bool IsToMany { get; private set; }
    }
}
