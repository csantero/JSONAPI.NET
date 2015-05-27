using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    internal abstract class PayloadBase : IPayload
    {
        protected PayloadBase(JObject metadata)
        {
            Metadata = metadata;
            PrimaryData = new List<IResourceObject>();
            RelatedData = new List<IResourceObject>();
        }

        public abstract bool IsCollection { get; }

        public JObject Metadata { get; private set; }

        public IList<IResourceObject> PrimaryData { get; private set; }

        public IList<IResourceObject> RelatedData { get; private set; }

    }
}
