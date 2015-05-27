using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Contains a deserialized representation of a json-api resource object.
    /// </summary>
    public interface IResourceObject
    {
        /// <summary>
        /// The id of the resource
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The resource's .NET type
        /// </summary>
        Type ResourceType{ get; }

        /// <summary>
        /// Metadata associated with the payload, serialized in the `meta` key
        /// </summary>
        JObject Metadata { get; }

        /// <summary>
        /// The data items belonging to this resource, keyed by name.
        /// </summary>
        IDictionary<string, JToken> DataAttributes { get; }

        /// <summary>
        /// Relationships to other resource types.
        /// </summary>
        IDictionary<string, IRelationship> Relationships { get; } 
    }
}