using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Todo.Filters;
using Todo.Models;
using Todo.ViewModels.Out;

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [AddHeader("cache-control", "no-cache")]
    public class TodosController : Controller
    {
        private readonly TodoRepository _repository;
        private const string NotFoundMessage = "that task does not exist. it may have been removed";

        public TodosController(TodoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetCollection([FromQuery] int skip = 0, [FromQuery] int take = 2)
        {
           var items = _repository.Items
                .OrderBy(i => i.Id)
                .Skip(skip)
                .Take(take)
                .Select(ToVm)
                .ToArray();

            var pages = this._repository.BuildPagingFor(skip, take);
            pages.Add(new PagingInfo("this", skip, take));

            return this.Ok(this.ToVm(items, pages));
        }

        [HttpPost]
        public IActionResult Create([FromBody] ViewModels.In.TodoVM vmIn)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(ModelState);
            }

            if (!bool.TryParse(vmIn.Important, out bool important))
            {
                return this.BadRequest("could not convert value 'imporant' to a boolean (true/false).");
            }

            var t = this._repository.Add(vmIn.Description, important);
            return this.CreatedAtAction(nameof(GetOne), new {id = t.Id}, this.ToVm(t));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var todo = this._repository.Get(id);
            return todo.Match<IActionResult>(
                some: t => this.Ok(ToVm(t)),
                none: () => this.NotFound(NotFoundMessage)
            );
        }

        [HttpPost("{id}/complete")]
        public IActionResult Complete([FromQuery] int id)
        {
            var todo = this._repository.Get(id);
            return todo.Match<IActionResult>(
                some: t =>
                {
                    t.MarkComplete();
                    return this.Ok();
                },
                none: () => this.NotFound(NotFoundMessage)
            );
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]ViewModels.In.TodoVM vmIn)
        {
            var todo = this._repository.Get(id);
            if (!todo.HasValue) return NotFound(NotFoundMessage);

            if (string.IsNullOrWhiteSpace(vmIn.Description)) return BadRequest("missing description");
            if (!bool.TryParse(vmIn.Important, out bool important))
            {
                return this.BadRequest("could not convert value 'imporant' to a boolean (true/false).");
            }

            todo.MatchSome(t => t.Change(vmIn.Description, important));
            return this.Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return this._repository.Remove(id)
                ? (IActionResult)Ok()
                : NotFound(NotFoundMessage);
        }

        private TodoVM ToVm (Models.Todo todo)
        {
            var vm = TodoVM.From(todo);
    
            vm.Links.Add(new LinkVM("this", this.Url.Action(nameof(GetOne), new {id = todo.Id})));

            vm.Actions.Add(new ActionVM()
            {
                Name = "complete",
                Title = "mark complete",
                Href = this.Url.Action(nameof(Complete), new {id = todo.Id}),
                Method = "POST"
            });

            vm.Actions.Add(new ActionVM()
            {
                Name = "delete",
                Title = "delete",
                Href = this.Url.Action(nameof(Delete), new { id = todo.Id }),
                Method = "DELETE"
            });

            return vm;
        }

        private TodoCollectionVm ToVm(IEnumerable<TodoVM> items, IList<PagingInfo> pages)
        {
            var vm = new TodoCollectionVm(items);

            foreach (var pi in pages)
            {
                vm.Links.Add(new LinkVM(pi.Direction, 
                    this.Url.Action(nameof(GetCollection), new { skip = pi.Offset, take = pi.PageSize }))
                    );
            }

            vm.Actions.Add(new ActionVM()
            {
                Name = "create",
                Title = "new task",
                Href = this.Url.Action(nameof(Create)),
                Method = "POST",
                Fields =
                {
                    new FieldVM(nameof(ViewModels.In.TodoVM.Description).ToLower(), FieldType.Text),
                    new FieldVM(nameof(ViewModels.In.TodoVM.Important).ToLower(), FieldType.Text),
                }
            });

            return vm;
        }

    }
}
