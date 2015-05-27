using System;

namespace JSONAPI.Json
{
    internal class ToOneRelationship : Relationship
    {
        public ToOneRelationship(string relatedResourceUrl, string relationshipUrl)
            : base(relatedResourceUrl, relationshipUrl)
        {
        }

        public ToOneRelationship(Type relatedResourceType, string relatedResourceId, string relatedResourceUrl, string relationshipUrl)
            : this(relatedResourceUrl, relationshipUrl)
        {
            RelatedResourceType = relatedResourceType;
            RelatedResourceId = relatedResourceId;
        }

        public Type RelatedResourceType { get; private set; }
        public string RelatedResourceId { get; private set; }

        public override Tuple<Type, string>[] Linkage
        {
            get
            {
                if (RelatedResourceType == null)
                    return null;
                return new []
                {
                    Tuple.Create(RelatedResourceType, RelatedResourceId)
                };
            }
        }
    }
}