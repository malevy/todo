using System.Collections.Generic;
using System.Linq;
using Halcyon.HAL;
using Todo.Formatters;
using Todo.ViewModels.Out;

namespace Todo.Infrastructure.Formatters.HAL
{
    public class HalTransformerProvider : TransformerProviderBase<HALResponse>
    {
        protected override HALResponse FromItem(TodoVM vm, string[] additionalRels)
        {
            var model = new
            {
                description = vm.Description,
                completedOn = vm.CompletedOn,
                important = vm.Important
            };
            var result = new HALResponse(model)
                .AddLinks((vm as IHasLinks).ToHalLink())
                .AddLinks((vm as IHasActions).ToHalLink());

            return result;
        }

        protected override HALResponse FromCollection(TodoCollectionVm vm)
        {
            var embedded = vm.Items.Select(i => FromItem(i, null));

            var result = new HALResponse(new {})
                .AddLinks((vm as IHasLinks).ToHalLink())
                .AddLinks((vm as IHasActions).ToHalLink())
                .AddEmbeddedCollection("items", embedded);

            return result;
        }
    }

    static class HalComponentExtensions
    {
        public static IEnumerable<Link> ToHalLink(this IHasLinks @this)
        {
            if (null == @this.Links) yield break;

            foreach (var link in @this.Links)
            {
                yield return new Link(link.Rel.First(), link.Href.ToString(), title: link.Title, isRelArray: link.Rel.Count > 1);
            }
        }

        public static IEnumerable<Link> ToHalLink(this IHasActions @this) 
        {
            if (null == @this.Actions) yield break;

            foreach (var action in @this.Actions)
	        {
                yield return new Link(action.Name, action.Href.ToString(), action.Title, action.Method, false);
	        }
        }
    } 
}