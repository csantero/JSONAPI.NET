using System;
using System.Linq;
using System.Reflection;

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
        }
    }

    /// <summary>
    /// A ModelProperty representing a relationship to another resource
    /// </summary>
    public class RelationshipModelProperty : ModelProperty
    {
        private readonly string _objectType;

        internal RelationshipModelProperty(PropertyInfo property, string objectType, string jsonKey, bool ignoreByDefault, Type relatedType, bool isToMany)
            : base(property, jsonKey, ignoreByDefault)
        {
            _objectType = objectType;
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

        /// <summary>
        /// Creates a related resource URL for this relationship. For example, if this relationship
        /// represents the `authors` property on a `posts` object that has id `4`, the default implementation
        /// will yield a related resource URL of `/posts/4/authors`.
        /// </summary>
        /// <param name="objectId">The ID of the object on the left-hand side of the relationship</param>
        /// <returns>The related resource URL</returns>
        public virtual string GetRelatedResourceUrl(string objectId)
        {
            return String.Format("/{0}/{1}/{2}", _objectType, objectId, JsonKey);
        }

        /// <summary>
        /// Creates a relationship URL for this relationship. For example, if this relationship
        /// represents the `authors` property on a `posts` object that has id `4`, the default implementation
        /// will yield a relationship URL of `/posts/4/links/authors`.
        /// </summary>
        /// <param name="objectId">The ID of the object on the left-hand side of the relationship</param>
        /// <returns>The relationship URL</returns>
        public virtual string GetRelationshipUrl(string objectId)
        {
            return String.Format("/{0}/{1}/links/{2}", _objectType, objectId, JsonKey);
        }

        /// <summary>
        /// Child classes should override this method if they want to allow expansion of the property in the compound document
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldExpand()
        {
            return false;
        }
    }
}
