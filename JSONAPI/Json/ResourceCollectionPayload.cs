using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    internal class ResourceCollectionPayload : PayloadBase
    {
        public ResourceCollectionPayload(JObject metadata) : base(metadata)
        {
        }

        public override bool IsCollection
        {
            get { return true; }
        }
    }
}