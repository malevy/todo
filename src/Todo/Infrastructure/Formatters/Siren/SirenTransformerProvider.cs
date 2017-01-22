using System;
using System.Collections.Generic;
using System.Linq;
using D2L.Hypermedia.Siren;
using Todo.Formatters;
using Todo.ViewModels.Out;

namespace Todo.Infrastructure.Formatters.Siren
{
    public class SirenTransformerProvider : TransformerProviderBase<ISirenEntity>
    {
        protected override ISirenEntity FromItem(TodoVM todo, string[] additionalRels)
        {

            string[] rels = null;
            if (null != additionalRels)
            {
                rels = new string[additionalRels.Length];
                Array.Copy(additionalRels, rels, rels.Length);
            }

            var entity = new SirenEntity(
                @class: new[] { "todo" },
                rel: rels,
                properties: new
                {
                    completedOn = todo.CompletedOn,
                    description = todo.Description,
                    important = todo.Important
                },
                links: todo.ToSirenLinks(),
                actions: todo.ToSirenAction()
            );

            return entity;
        }

        protected override ISirenEntity FromCollection(TodoCollectionVm collection)
        {
            var itemRel = new[] { "item" };

            var entity = new SirenEntity(
                @class: new[] { "collection" },
                entities: collection.Items.Select(i => FromItem(i, itemRel)),
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