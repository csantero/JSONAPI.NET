using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Reflection;
using JSONAPI.Core;
using JSONAPI.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Http
{
    //TODO: Authorization checking framework, maybe?
    public class ApiController<T> : System.Web.Http.ApiController
        where T : class
    {
        private readonly IModelManager _modelManager;

        /// <param name="modelManager">The model manager used to look up registered types</param>
        public ApiController(IModelManager modelManager)
        {
            _modelManager = modelManager;
        }

        protected virtual IMaterializer MaterializerFactory()
        {
            return null;
        }

        protected virtual TM MaterializerFactory<TM>()
            where TM : IMaterializer
        {
            return (TM)this.MaterializerFactory();
        }

        /// <summary>
        /// Override this method to provide an IQueryable set of objects of type T. If this
        /// method is not overridden, an empty List&lt;T&gt; will be returned.
        /// </summary>
        /// <param name="materializer"></param>
        /// <returns></returns>
        protected virtual IQueryable<T> QueryableFactory(IMaterializer materializer = null)
        {
            return (new List<T>()).AsQueryable<T>();
        }

        //[System.Web.OData.EnableQuery] // Do this yourself!
        /// <summary>
        /// Default Get method implementation. Returns the result of
        /// Note: You can easily add OData query support by overriding this method and decorating
        /// it with the [System.Web.OData.EnableQuery] attribute.
        /// </summary>
        /// <returns></returns>
        public virtual IJsonApiResponse Get()
        {
            IMaterializer materializer = MaterializerFactory();

            IQueryable<T> es = QueryableFactory(materializer);

            return CreateQueryableResponse(es);
        }

        public virtual async Task<IJsonApiResponse> Get(string id)
        {
            IMaterializer materializer = MaterializerFactory();

            List<T> results = new List<T>();
            string[] arrIds;
            if (id.Contains(","))
            {
                 arrIds = id.Split(',');
            }
            else
            {
                arrIds = new string[] { id };
            }
            foreach (string singleid in arrIds)
            {
                T hit = await materializer.GetByIdAsync<T>(singleid);
                if (hit != null)
                {
                    results.Add(hit);
                }
            }
            return CreateCollectionResponse(results);
        }

        /// <summary>
        /// In this base class, the Post operation is essentially a no-op. It returns a materialized
        /// copy of the object (which is meaningless unless the materializer implements
        /// some logic that does something to it), but fulfills the JSONAPI requirement
        /// that the POST operation return the POSTed object. It should probably be
        /// overridden in any implementation.
        /// </summary>
        /// <param name="payload">The payload object</param>
        /// <returns></returns>
        public virtual Task<IJsonApiResponse> Post(IPayload payload)
        {
            var primaryData = ExtractFromPayload(payload);

            var returnPayload = CreateSingleResultResponse(primaryData);
            return Task.FromResult(returnPayload);
        }

        /// <summary>
        /// Similar to Post, this method doesn't do much. It calls MaterializeUpdateAsync() on the
        /// input and returns it. It should probably always be overridden.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public virtual async Task<IJsonApiResponse> Patch(string id, IPayload payload)
        {
            var primaryData = ExtractFromPayload(payload);

            IMaterializer materializer = MaterializerFactory();
            var materialized = await materializer.MaterializeUpdateAsync(primaryData);

            return CreateSingleResultResponse(materialized);
        }

        /// <summary>
        /// A no-op method. This should be overriden in subclasses if Delete is to be supported.
        /// </summary>
        /// <param name="id"></param>
        public virtual Task Delete(string id)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Creates a .NET object from a json-api payload
        /// </summary>
        /// <param name="payload">The request payload</param>
        /// <returns>A .NET object of type T</returns>
        protected virtual T ExtractFromPayload(IPayload payload)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a json-api response for a single result.
        /// </summary>
        /// <param name="result">The result</param>
        /// <returns>A response for this result</returns>
        protected IJsonApiResponse CreateSingleResultResponse(T result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a json-api response for an IQueryable.
        /// </summary>
        /// <param name="result">The queryable result</param>
        /// <returns>A response for this result</returns>
        protected IJsonApiResponse CreateQueryableResponse(IQueryable<T> result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a json-api response for a result collection. Note: If returning
        /// an IQueryable, you should use CreateQueryableResponse instead.
        /// </summary>
        /// <param name="result">The collection result</param>
        /// <returns>A response for this result</returns>
        protected IJsonApiResponse CreateCollectionResponse(IEnumerable<T> result)
        {
            throw new NotImplementedException();
        }
    }
}
