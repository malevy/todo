using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Todo.Infrastructure.ProblemJson
{
    public static class ProblemJsonControllerExtensions
    {
        public static ProblemJsonObjectResult Problem(
            this Controller @this, 
            int httpStatusCode = 400,
            Uri type = null,
            string title = null,
            string detail = null,
            Uri instance = null,
            IDictionary<string, object> extensions = null)
        {
            return new ProblemJsonObjectResult(httpStatusCode, type, title, detail, instance, extensions);
        }

        public static ProblemJsonObjectResult Problem(
            this Controller @this,
            int httpStatusCode = 400,
            Uri type = null,
            string title = null,
            string detail = null,
            Uri instance = null,
            ModelStateDictionary modelState = null)
        {

            return new ProblemJsonObjectResult(httpStatusCode, type, title, detail, instance, modelState);
        }

    }

    /// <summary>
    /// Problem details for HTTP APIs [RFC7807]
    /// https://tools.ietf.org/html/rfc7807
    /// </summary>
    public class ProblemJsonObjectResult : ObjectResult
    {
        /// <summary>
        /// Instantiates an instance of <see cref="ProblemJsonObjectResult"/> 
        /// </summary>
        /// <param name="httpStatusCode">the HTTP status code</param>
        /// <param name="type">A URI reference that identifies the problem type</param>
        /// <param name="title">A short, human-readable summary of the problem</param>
        /// <param name="detail">A human-readable explanation specific to this occurrence, 
        /// including information to help resolve the problem </param>
        /// <param name="instance">A URI reference that identifies the specific occurence of the problem </param>
        /// <param name="extensions"></param>
        public ProblemJsonObjectResult(
            int httpStatusCode = 400,
            Uri type = null,
            string title = null,
            string detail = null,
            Uri instance = null,
            ModelStateDictionary modelState = null) : this(httpStatusCode, type, title, detail, instance, (IDictionary<string, object>) null)
        {
            if (modelState?.IsValid ?? true) return;

            var violations = new Dictionary<string, object>();

            foreach (var state in modelState)
            {
                var errors = state.Value.Errors;
                if (null == errors || !errors.Any()) continue;

                var messages = errors.
                    Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "the input is not valid" : e.ErrorMessage)
                    .ToArray();

                violations.Add(state.Key, messages);
            }

            this.AddExtension("invalid-parameters", violations);
        }

        /// <summary>
        /// Instantiates an instance of <see cref="ProblemJsonObjectResult"/> 
        /// </summary>
        /// <param name="httpStatusCode">the HTTP status code</param>
        /// <param name="type">A URI reference that identifies the problem type</param>
        /// <param name="title">A short, human-readable summary of the problem</param>
        /// <param name="detail">A human-readable explanation specific to this occurrence, 
        /// including information to help resolve the problem </param>
        /// <param name="instance">A URI reference that identifies the specific occurence of the problem </param>
        /// <param name="extensions"></param>
        public ProblemJsonObjectResult(
            int httpStatusCode = 400,
            Uri type = null,
            string title = null,
            string detail = null,
            Uri instance = null,
            IDictionary<string, object> extensions = null) : base(null)
        {
            if (! Enum.IsDefined(typeof(HttpStatusCode), httpStatusCode)) 
                throw new ArgumentOutOfRangeException(nameof(httpStatusCode), "not a valid http status code");
            this.StatusCode = httpStatusCode;


            var result = new Dictionary<string, object>
            {
                {"status", httpStatusCode},
                {"type", type?.ToString() ?? "about:blank"}
            };

            if (!string.IsNullOrWhiteSpace(title)) result.Add("title", title.Trim());
            if (!string.IsNullOrWhiteSpace(detail)) result.Add("detail", detail);
            if (null != instance) result.Add("instane", instance.ToString());

            if (null != extensions)
            {
                foreach (var pair in extensions)
                {
                    result.Add(pair.Key, pair.Value);
                }
                
            }

            this.Value = result;

        }

        public ProblemJsonObjectResult AddExtension(string key, object value)
        {
            var state = this.Value as Dictionary<string, object>;
            state[key] = value;
            return this;
        }
    }

}