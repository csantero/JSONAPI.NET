using System;
using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Contains an IQueryable that can be enumerated later after transformations.
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    public class QueryableResponse<T> : GenericJsonApiResponse<T>
    {
        /// <summary>
        /// Creates a new QueryablePayload
        /// </summary>
        public QueryableResponse(IQueryable<T> queryable, JObject metadata, IModelManager modelManager)
            : base(modelManager)
        {
            Queryable = queryable;
            _metadata = metadata;
        }

        /// <summary>
        /// The queryable primary data source
        /// </summary>
        public readonly IQueryable<T> Queryable;

        private readonly JObject _metadata;

        protected override IPayload CreatePayload()
        {
            return new ResourceCollectionPayload(_metadata);
        }

        protected override Task<T[]> EnumeratePrimaryData()
        {
            return Task.FromResult(Queryable.ToArray());
        }
    }
}
