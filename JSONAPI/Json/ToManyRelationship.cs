using System;

namespace JSONAPI.Json
{
    internal class ToManyRelationship : Relationship
    {
        private readonly Tuple<Type, string>[] _linkage;

        public ToManyRelationship(string relatedResourceUrl, string relationshipUrl)
            : base(relatedResourceUrl, relationshipUrl)
        {
        }

        public ToManyRelationship(Tuple<Type, string>[] linkage, string relatedResourceUrl, string relationshipUrl)
            : this(relatedResourceUrl, relationshipUrl)
        {
            _linkage = linkage;
        }

        public override Tuple<Type, string>[] Linkage { get { return _linkage; } }
    }
}