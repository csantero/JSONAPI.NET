using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Json;

namespace JSONAPI.EntityFramework.Http
{
    public class ApiController<T,TC> : JSONAPI.Http.ApiController<T>
        where T : class // hmm...see http://stackoverflow.com/a/6451237/489116
        where TC : DbContext
    {
        public ApiController(IModelManager modelManager)
            : base(modelManager)
        {
            
        }

        private EntityFrameworkMaterializer _materializer = null;

        protected override JSONAPI.Core.IMaterializer MaterializerFactory()
        {
            if (_materializer == null)
            {
                DbContext context = (DbContext)Activator.CreateInstance(typeof(TC));
                var metadataManager = MetadataManager.Instance;
                _materializer = new JSONAPI.EntityFramework.EntityFrameworkMaterializer(context, metadataManager);
            }
            return _materializer;
        }

        protected override TM MaterializerFactory<TM>()
        {
            return base.MaterializerFactory<TM>();
        }

        protected override IQueryable<T> QueryableFactory(Core.IMaterializer materializer = null)
        {
            if (materializer == null)
            {
                materializer = MaterializerFactory();
            }
            return ((EntityFrameworkMaterializer)materializer).DbContext.Set<T>();
        }

        public override async Task<IJsonApiResponse> Post(IPayload payload)
        {
            var materializer = this.MaterializerFactory<EntityFrameworkMaterializer>();

            var primaryData = ExtractFromPayload(payload);
            DbContext context = materializer.DbContext;
            var material = await materializer.MaterializeUpdateAsync(primaryData);
            if (context.Entry<T>(material).State == EntityState.Added)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                // POST should only create an object--if the EntityState is Unchanged or Modified, this is an illegal operation.
                var e = new System.Web.Http.HttpResponseException(System.Net.HttpStatusCode.BadRequest);
                //e.InnerException = new ArgumentException("The POSTed object already exists!"); // Can't do this, I guess...
                throw e;
            }

            return CreateSingleResultResponse(material);
        }

        public override async Task<IJsonApiResponse> Patch(string id, IPayload payload)
        {
            var materializer = this.MaterializerFactory<EntityFrameworkMaterializer>();
            var primaryData = ExtractFromPayload(payload);
            DbContext context = materializer.DbContext;
            var material = await materializer.MaterializeUpdateAsync(primaryData);
            await context.SaveChangesAsync();

            return CreateSingleResultResponse(material);
        }

        public override async Task Delete(string id)
        {
            var materializer = this.MaterializerFactory<EntityFrameworkMaterializer>();
            DbContext context = materializer.DbContext;
            T target = await materializer.GetByIdAsync<T>(id);
            context.Set<T>().Remove(target);
            await context.SaveChangesAsync();
            await base.Delete(id);
        }

        protected override void Dispose(bool disposing)
        {
            //FIXME: Unsure what to do with the "disposing" parameter here...what does it mean??
            if (_materializer != null)
            {
                _materializer.DbContext.Dispose();
            }
            _materializer = null;
            base.Dispose(disposing);
        }
    }
}
