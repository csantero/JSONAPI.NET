using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    internal class SingleResourcePayload : PayloadBase
    {
        public SingleResourcePayload(JObject metadata) : base(metadata)
        {
        }

        public override bool IsCollection
        {
            get { return false; }
        }
    }
}