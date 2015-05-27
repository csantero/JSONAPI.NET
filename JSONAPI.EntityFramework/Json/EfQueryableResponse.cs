using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.EntityFramework.Json
{
    internal class EfQueryableResponse<T> : QueryableResponse<T>
    {
        public EfQueryableResponse(IQueryable<T> queryable, JObject metadata, IModelManager modelManager)
            : base(queryable, metadata, modelManager)
        {
        }

        protected override Task<T[]> EnumeratePrimaryData()
        {
            return Queryable.ToArrayAsync();
        }
    }
}
