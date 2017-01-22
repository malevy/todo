using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Todo.Filters;
using Todo.Infrastructure.ProblemJson;
using Todo.Models;
using Todo.ViewModels;
using Todo.ViewModels.Out;

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [AddHeader("cache-control", "no-cache")]
    [AddProfileLink("TodoProfile")]
    [FormatFilter]
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
            pages.Add(new PagingInfo("self", skip, take));

            return this.Ok(this.ToVm(items, pages));
        }

        [HttpPost]
        public IActionResult Create([FromBody] ViewModels.In.TodoVM vmIn)
        {

            if (!bool.TryParse(vmIn.Important, out bool important))
            {
                ModelState.AddModelError("important", "could not convert value to a boolean (true/false).");
            }

            if (!ModelState.IsValid)
            {
                return this.Problem(
                    400,
                    new Uri("/docs/errors/bad-input", UriKind.Relative),
                    "unable to create task",
                    "one or more inputs failed validations", modelState: ModelState);
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

            if (!bool.TryParse(vmIn.Important, out bool important))
            {
                ModelState.AddModelError("important", "could not convert value to a boolean (true/false).");
            }

            if (!ModelState.IsValid)
            {
                return this.Problem(
                    400,
                    new Uri("/docs/errors/bad-input", UriKind.Relative),
                    "unable to create task",
                    "one or more inputs failed validations", modelState: ModelState);
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
    
            vm.Links.Add(new LinkVM("self", this.Url.AbsoluteActionUri(nameof(GetOne), new {id = todo.Id})));
            vm.Links.Add(new LinkVM("collection",this.Url.AbsoluteActionUri(nameof(GetCollection))));

            vm.Actions.Add(new ActionVM()
            {
                Name = "complete",
                Title = "mark complete",
                Href = this.Url.AbsoluteActionUri(nameof(Complete), new {id = todo.Id}),
                Method = "POST"
            });

            vm.Actions.Add(new ActionVM()
            {
                Name = "delete",
                Title = "delete",
                Href = this.Url.AbsoluteActionUri(nameof(Delete), new { id = todo.Id }),
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
                    this.Url.AbsoluteActionUri(nameof(GetCollection), new { skip = pi.Offset, take = pi.PageSize }))
                    );
            }

            vm.Actions.Add(new ActionVM()
            {
                Name = "create",
                Title = "new task",
                Href = this.Url.AbsoluteActionUri(nameof(Create)),
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
