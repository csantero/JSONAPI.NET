using JSONAPI.Core;
using JSONAPI.EntityFramework.Http;
using JSONAPI.TodoMVC.API.Models;

namespace JSONAPI.TodoMVC.API.Controllers
{
    public class TodosController : ApiController<Todo, TodoMvcContext>
    {
        public TodosController(IModelManager modelManager) : base(modelManager)
        {
            
        }
    }
}