using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using D2L.Hypermedia.Siren;
using Optional;
using Optional.Linq;
using Todo.ViewModels.Out;

namespace Todo.Formatters.Siren
{
    public class SirenTransformerProvider
    {

        private readonly List<Tuple<Type, Func<object, ISirenEntity>>> _transformers 
            = new List<Tuple<Type, Func<object, ISirenEntity>>>();

        public SirenTransformerProvider()
        {

            this._transformers.Add(Tuple.Create<Type, Func<object, ISirenEntity>>(typeof(TodoVM), FromItem));
            this._transformers.Add(Tuple.Create<Type, Func<object, ISirenEntity>>(typeof(TodoCollectionVm), FromCollection));
        }

        public Option<ISirenEntity, string> From(object vm)
        {
            var transformer = this._transformers
                .Where(t => t.Item1.IsInstanceOfType(vm))
                .Select(t => t.Item2)
                .FirstOrDefault()
                .SomeNotNull($"no Siren transformer for {vm.GetType()}");

            return transformer.Map(x => x(vm));
        }

        private ISirenEntity FromItem(object vm)
        {
            var todo = vm as TodoVM;
            if (null == todo)
                throw new ArgumentException($"expected '{typeof(TodoVM)}' but received '{vm.GetType()}'", nameof(vm));

            var entity = new SirenEntity(
                @class: new []{"todo"},
                properties: new
                {
                    completedOn = todo.CompletedOn,
                    description = todo.Description,
                    important = todo.Important
                },
                links: todo.ToSirenLinks(),
                actions: todo.ToSirenAction()
                );

            return entity ;
        }

        private ISirenEntity FromCollection(object vm)
        {
            var collection = vm as TodoCollectionVm;
            if (null == collection)
                throw new ArgumentException($"expected '{typeof(TodoCollectionVm)}' but received '{vm.GetType()}'", nameof(vm));

            var entity = new SirenEntity(
                @class: new[] {"collection"},
                entities: collection.Items.Select(FromItem),
                links: collection.ToSirenLinks(),
                actions: collection.ToSirenAction()
                );

            return entity;
        }

    }

    static class SirenComponentExtensions
    {
        public static IEnumerable<SirenLink> ToSirenLinks(this IHasLinks @this)
        {
            if (null == @this.Links) yield break;

            foreach (var link in @this.Links)
            {
                yield return new SirenLink(
                    rel: link.Rel.ToArray(),
                    href: link.Href,
                    title: link.Title);
            }
        }

        public static IEnumerable<SirenAction> ToSirenAction(this IHasActions @this)
        {
            if (null == @this.Actions) yield break;

            foreach (var action in @this.Actions)
            {
                IEnumerable<SirenField> fields = null;
                if (action.Fields.Any())
                {
                    fields = action.Fields.Select(f => new SirenField(
                        name: f.Name,
                        type: f.Type.ToString(),
                        value: f.Value,
                        title: f.Title
                    ));
                }

                var sirenAction = new SirenAction(
                    name: action.Name,
                    href: action.Href,
                    method: action.Method,
                    title: action.Title,
                    type: action.Accepts,
                    fields: fields
                );

                yield return sirenAction;
            }
        }
    }



}