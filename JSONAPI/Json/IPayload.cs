using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Contains a deserialized representation of a json-api payload.
    /// </summary>
    public interface IPayload
    {
        /// <summary>
        /// Metadata associated with the payload, serialized in the `meta` key.
        /// </summary>
        JObject Metadata { get; }

        /// <summary>
        /// The primary data for this payload, serialized in the `data` key.
        /// If IsCollection is false then there should be zero or 1 elements.
        /// </summary>
        IList<IResourceObject> PrimaryData { get; }

        /// <summary>
        /// Whether the primary data is a collection of resource objects or just a single resource object
        /// </summary>
        bool IsCollection { get; }

        /// <summary>
        /// Resource objects to include in the `related` key of the payload.
        /// </summary>
        IList<IResourceObject> RelatedData { get; }
    }
}