using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Intercepts errors that occur in the Web API pipeline and prepares them for formatting.
    /// </summary>
    public class ErrorHandlingFilter : IActionFilter
    {
        public bool AllowMultiple { get { return false; } }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var response = await continuation();
            return response;
        }
    }
}
