using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JSONAPI.Json
{
    /// <summary>
    /// ApiController methods must return this in order to return a json-api .document in the response 
    /// </summary>
    public interface IJsonApiResponse
    {
        /// <summary>
        /// Returns a task to get a payload object.
        /// </summary>
        /// <returns>A task that will return a payload object when resolved</returns>
        Task<IPayload> Resolve();
    }
}
