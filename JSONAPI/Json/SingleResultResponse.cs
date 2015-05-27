using System.Threading.Tasks;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// A response for a single resource object
    /// </summary>
    /// <typeparam name="T">The type of resource object.</typeparam>
    public sealed class SingleResultResponse<T> : GenericJsonApiResponse<T>
    {
        /// <summary>
        /// Creates a new SingleResultResponse
        /// </summary>
        public SingleResultResponse(T result, JObject metadata, IModelManager modelManager)
            : base(modelManager)
        {
            _result = result;
            _metadata = metadata;
        }

        private readonly T _result;
        private readonly JObject _metadata;

        /// <inheritdoc />
        protected override IPayload CreatePayload()
        {
            return new SingleResourcePayload(_metadata);
        }

        /// <inheritdoc />
        protected override Task<T[]> EnumeratePrimaryData()
        {
            return Task.FromResult(new [] { _result });
        }
    }
}
