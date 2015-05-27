using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Base class for json-api responses.
    /// </summary>
    public abstract class GenericJsonApiResponse : IJsonApiResponse
    {
        /// <summary>
        /// Creates a JsonApiResponseBase
        /// </summary>
        protected GenericJsonApiResponse()
        {
            InclusionPathExpressions = new List<string>();
        }

        protected readonly IList<string> InclusionPathExpressions;

        /// <inheritdoc />
        public abstract Task<IPayload> Resolve();

        internal static bool ShouldExpand(string currentPath, string inclusionPath)
        {
            if (String.IsNullOrEmpty(currentPath)) return false;
            if (String.IsNullOrEmpty(inclusionPath)) return false;

            var currentPathSplit = currentPath.Split('.');
            var inclusionPathSplit = inclusionPath.Split('.');
            if (currentPathSplit.Length > inclusionPathSplit.Length) return false;

            var iterations = Math.Min(currentPathSplit.Length, inclusionPathSplit.Length);
            for (var i = 0; i < iterations; i++)
            {
                if (currentPathSplit[i] != inclusionPathSplit[i]) return false;
            }
            return true;
        }

        private class InclusionPathVisitor : ExpressionVisitor
        {
            private readonly Stack<string> _stack = new Stack<string>();

            public string Path
            {
                get
                {
                    return string.Join(".", _stack.ToArray());
                }
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                _stack.Push(node.Member.Name);
                return Visit(node.Expression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType != typeof(Enumerable) || node.Method.Name != "Select")
                    throw new ArgumentException("Method calls must be for Enumerable.Select().", "node");

                var lambda = (LambdaExpression)node.Arguments[1];
                Visit(lambda.Body);
                return Visit(node.Arguments[0]);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Visit(node.Body);
            }
        }

        internal protected static string ConvertInclusionExpression<T>(Expression<Func<T, object>> inclusionPathExpression)
        {
            var visitor = new InclusionPathVisitor();
            visitor.Visit(inclusionPathExpression);
            return visitor.Path;
        }
    }

    /// <summary>
    /// Base generic class for json-api responses.
    /// </summary>
    public abstract class GenericJsonApiResponse<T> : GenericJsonApiResponse
    {
        /// <summary>
        /// Creates a JsonApiResponseBase
        /// </summary>
        protected GenericJsonApiResponse(IModelManager modelManager)
        {
            _modelManager = modelManager;
            _related = new Dictionary<object, IResourceObject>();
        }

        private readonly IModelManager _modelManager;
        private readonly IDictionary<object, IResourceObject> _related;

        /// <summary>
        /// Child classes must implement this to provide a payload 
        /// </summary>
        /// <returns></returns>
        protected abstract IPayload CreatePayload();

        /// <summary>
        /// Asynchronously loads the primary data
        /// </summary>
        /// <returns>An task resolving to the results of the query</returns>
        protected abstract Task<T[]> EnumeratePrimaryData();

        /// <inheritdoc />
        public override async Task<IPayload> Resolve()
        {
            var primaryData = await EnumeratePrimaryData();

            var payload = CreatePayload();

            foreach (var primaryDataItem in primaryData)
            {
                AddPrimary(payload, primaryDataItem);
            }

            return payload;
        }

        /// <summary>
        /// Adds a path to include in the response document.
        /// </summary>
        /// <param name="pathExpression">An expression pointing to the property to include.</param>
        /// <typeparam name="TResource">The resource type. Any resources of this type will use this path</typeparam>
        public void Include<TResource>(Expression<Func<TResource, object>> pathExpression)
        {
            var convertedPath = ConvertInclusionExpression(pathExpression);
            InclusionPathExpressions.Add(convertedPath);
        }

        private void AddPrimary(IPayload payload, T primary)
        {
            var primaryResourceObject = FormatResourceObject(payload, primary, "");
            payload.PrimaryData.Add(primaryResourceObject);
        }

        private IResourceObject FormatResourceObject(IPayload payload, object obj, string currentPath)
        {
            var resourceType = obj.GetType();
            var idProp = _modelManager.GetIdProperty(resourceType);
            var objectId = (string) idProp.GetValue(obj);
            var properties = _modelManager.GetProperties(resourceType);

            var dataAttributes = new Dictionary<string, JToken>();
            var relationships = new Dictionary<string, IRelationship>();

            foreach (var prop in properties)
            {
                if (prop is FieldModelProperty)
                {
                    var propertyValue = prop.Property.GetValue(obj);
                    dataAttributes.Add(prop.JsonKey, JToken.FromObject(propertyValue));
                    continue;
                }

                var relationshipModelProp = prop as RelationshipModelProperty;
                if (relationshipModelProp == null) continue;

                var relatedResourceUrl = relationshipModelProp.GetRelatedResourceUrl(objectId);
                var relationshipUrl = relationshipModelProp.GetRelationshipUrl(objectId);

                var propertyPath = (!String.IsNullOrEmpty(currentPath) ? currentPath + "." : "") + prop.Property.Name;
                var shouldExpand = InclusionPathExpressions.Any(inclusionPath => ShouldExpand(propertyPath, inclusionPath));

                IRelationship relationship;
                if (shouldExpand)
                {
                    var propertyValue = relationshipModelProp.Property.GetValue(obj);
                    if (relationshipModelProp.IsToMany)
                    {
                        var relatedResourceObjects = new List<IResourceObject>();
                        foreach (var element in (IEnumerable<object>)propertyValue)
                        {
                            IResourceObject relatedResourceObject;
                            if (!_related.TryGetValue(element, out relatedResourceObject))
                            {
                                // TODO: if the same object is included through different paths, and there are properties that aren't included
                                // on the first path, but ARE included on a later path, those properties won't be included. We need to re-visit
                                // this object on the later path and include its sub-properties.
                                relatedResourceObject = FormatResourceObject(payload, propertyValue, propertyPath);
                            }   
                        }
                        var linkage = relatedResourceObjects
                            .Select(r => Tuple.Create(r.ResourceType, r.Id))
                            .ToArray();

                        relationship = new ToManyRelationship(linkage, relatedResourceUrl, relationshipUrl);
                    }
                    else
                    {
                        IResourceObject relatedResourceObject;
                        if (!_related.TryGetValue(propertyValue, out relatedResourceObject))
                        {
                            // TODO: if the same object is included through different paths, and there are properties that aren't included
                            // on the first path, but ARE included on a later path, those properties won't be included. We need to re-visit
                            // this object on the later path and include its sub-properties.
                            relatedResourceObject = FormatResourceObject(payload, propertyValue, propertyPath);
                        }

                        relationship = new ToOneRelationship(relatedResourceObject.ResourceType,
                            relatedResourceObject.Id, relatedResourceUrl, relationshipUrl);
                    }
                }
                else
                {
                    relationship = new UnlinkedRelationship(relatedResourceUrl, relationshipUrl);
                }

                relationships.Add(prop.JsonKey, relationship);
            }

            return new ResourceObject
            {
                Id = objectId,
                ResourceType = resourceType,
                DataAttributes = dataAttributes,
                Relationships = relationships,
                Metadata = null // TODO: allow setting this
            };
        }
    }
}
