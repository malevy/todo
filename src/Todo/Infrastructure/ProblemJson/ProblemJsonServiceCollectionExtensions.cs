using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Todo.Infrastructure.ProblemJson
{
    public static class ProblemJsonServiceCollectionExtensions
    {
        public static IServiceCollection AddProblemJson(this IServiceCollection @this)
        {
            @this.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ProblemJsonSetup>());
            return @this;
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