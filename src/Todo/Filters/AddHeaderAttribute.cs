using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Todo.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AddHeaderAttribute : ResultFilterAttribute
    {
        private readonly string _key;
        private readonly string[] _values;

        public AddHeaderAttribute(string key, string value) : this(key, new []{value})
        {
        }

        public AddHeaderAttribute(string key, string[] values)
        {
            _key = key;
            _values = values;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add(_key, _values);
        }

    }
}