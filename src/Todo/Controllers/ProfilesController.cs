using Microsoft.AspNetCore.Mvc;
using Todo.Filters;
using Todo.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [AddHeader("cache-control", "max-age=86400")]
    public class ProfilesController : Controller
    {
        [HttpGet("todo", Name = "TodoProfile")]
        public IActionResult Todo()
        {
            var document = this.View();
            document.ContentType = "application/alps+xml";
            return document;
        }
    }
}
