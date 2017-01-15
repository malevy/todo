using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Todo.ViewModels
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// build an absolute URI for the given action
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string AbsoluteAction(
            this IUrlHelper @this,
            string action,
            string controller,
            object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(nameof(action)))
                throw new ArgumentNullException(nameof(action));

            return @this.Action(new UrlActionContext()
            {
                Action = action,
                Controller = controller,
                Host = @this.ActionContext.HttpContext.Request.Host.Value,
                Protocol = @this.ActionContext.HttpContext.Request.Scheme,
                Values = routeValues
            });
        }

        /// <summary>
        /// build an absolute URI for the given action
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static Uri AbsoluteActionUri(
            this IUrlHelper @this,
            string action,
            string controller,
            object routeValues = null)
        {
            var uri = AbsoluteAction(@this, action, controller, routeValues);
            return new Uri(uri);
        }

        /// <summary>
        /// build an absolute URI for the given action
        /// </summary>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static Uri AbsoluteActionUri(
            this IUrlHelper @this,
            string action,
            object routeValues = null)
        {
            return new Uri(@this.AbsoluteAction(action, null, routeValues));
        }

    }
}