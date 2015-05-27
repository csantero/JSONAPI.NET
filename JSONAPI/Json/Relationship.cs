using System;

namespace JSONAPI.Json
{
    internal abstract class Relationship : IRelationship
    {
        protected Relationship(string relatedResourceUrl, string relationshipUrl)
        {
            RelatedResourceUrl = relatedResourceUrl;
            RelationshipUrl = relationshipUrl;
        }
        
        public string RelationshipUrl { get; set; }
        public string RelatedResourceUrl { get; set; }
        public abstract Tuple<Type, string>[] Linkage { get; }
    }

    internal class UnlinkedRelationship : Relationship
    {
        public UnlinkedRelationship(string relatedResourceUrl, string relationshipUrl) : base(relatedResourceUrl, relationshipUrl)
        {
        }

        public override Tuple<Type, string>[] Linkage
        {
            get { return null; }
        }
    }
}