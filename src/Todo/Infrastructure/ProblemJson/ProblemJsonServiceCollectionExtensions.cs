using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Todo.Infrastructure.ProblemJson
{
    public static class ProblemJsonServiceCollectionExtensions
    {
        public static IServiceCollection AddProblemJson(this IServiceCollection @this)
        {
            @this.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ProblemJsonSetup>());
            return @this;
        }

        public static IServiceCollection ConfigureExceptionHandlerForProblemJson(this IServiceCollection @this)
        {
            @this.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<ExceptionHandlerOptions>, ProblemJsonExceptionHandlerSetup>());
            return @this;
        }
    }

    public class ProblemJsonExceptionHandlerSetup : IConfigureOptions<ExceptionHandlerOptions>
    {
        public void Configure(ExceptionHandlerOptions options)
        {
            options.ExceptionHandler = this.Invoke;
        }

        protected virtual Task Invoke(HttpContext context)
        {
            var genericProblem = new ProblemJsonObjectResult(
                500,
                new Uri("/docs/errors/internal", UriKind.Relative),
                "unable to process your request",
                "an error has occurred. it has been logged and will be investigated",
                null, (IDictionary<string, object>)null);

            var response = context.Response;
            response.ContentType = ProblemJsonSetup.ProblemJsonMediaType;

            var serializer = JsonSerializer.CreateDefault();

            using (var writer = new StreamWriter(response.Body))
            {
                serializer.Serialize(writer, genericProblem.Value);
                writer.Flush();
            }

            return Task.CompletedTask;
        }

    }

    public class ProblemJsonSetup : IConfigureOptions<MvcOptions>
    {
        public const string ProblemJsonMediaType = "application/problem+json";
        private const string JsonFormatterNotFoundMessage = "could not find the an instance of JsonOutputFormatter. " +
                                                            "make sure that json formatting is added to the pipeline prior to ProblemJson";

        public void Configure(MvcOptions options)
        {
            var jsonFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
            if (null == jsonFormatter) throw new InvalidOperationException(JsonFormatterNotFoundMessage);

            // configure the existing json output formatter to also handle problem+json
            jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(ProblemJsonMediaType));

        }
    }

}