using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Todo.ViewModels;
using Todo.ViewModels.Out;

namespace Todo.Filters
{
    public class AddProfileLink : ResultFilterAttribute
    {
        private readonly string _routeName;

        public AddProfileLink(string routeName)
        {
            _routeName = routeName;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (null == objectResult) return;

            var hasLinks = objectResult.Value as IHasLinks;
            if (null == hasLinks) return;

            var controller = context.Controller as Controller;
            var urlHelper = controller?.Url;
            if (null == urlHelper) return;

            var routeContext = new UrlRouteContext()
            {
                RouteName = _routeName,
                Host = controller.Request.Host.Value,
                Protocol = controller.Request.Scheme
            };

            var uriString = urlHelper.RouteUrl(routeContext);

            hasLinks.Links.Add(new LinkVM("profile", new Uri(uriString)));
        }

    }
}