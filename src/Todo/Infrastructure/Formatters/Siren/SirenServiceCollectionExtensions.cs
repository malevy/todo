using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Todo.Infrastructure.Formatters.Siren
{
    public static class SirenServiceCollectionExtensions
    {
        public static IServiceCollection AddSirenFormatters(this IServiceCollection @this)
        {
            @this.AddTransient<SirenTransformerProvider>();
            @this.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, SirenFormatterSetup>());

            return @this;
        }
    }
}