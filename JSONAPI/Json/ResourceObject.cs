using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Stores an intermediate representation of a json-api resource object.
    /// </summary>
    internal class ResourceObject : IResourceObject
    {
        public string Id { get; set; }

        public Type ResourceType { get; set; }

        public JObject Metadata { get; set; }

        public IDictionary<string, JToken> DataAttributes { get; set; }

        public IDictionary<string, IRelationship> Relationships { get; set; }
    }
}