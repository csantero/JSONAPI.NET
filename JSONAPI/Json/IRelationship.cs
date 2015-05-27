using System;

namespace JSONAPI.Json
{
    /// <summary>
    /// Contains a deserialized representation of a relationship belonging to a json-api resource object.
    /// </summary>
    public interface IRelationship
    {
        /// <summary>
        /// The url representing the relationship itself, serialized in the `self` key.
        /// </summary>
        string RelationshipUrl { get; }

        /// <summary>
        /// The url representing the related data.
        /// </summary>
        string RelatedResourceUrl { get; }

        /// <summary>
        /// An array of tuple items referring to resources on the other side of this relationship.
        /// If IsToMany is false, this array must be either null or have one item.
        /// </summary>
        Tuple<Type, string>[] Linkage { get; }
    }
}
