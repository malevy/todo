﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Todo.Infrastructure.Formatters.HAL
{
    public static class HalServiceCollectionExtensions
    {
        public static IServiceCollection AddHalFormatters(this IServiceCollection @this)
        {
            @this.AddTransient<HalTransformerProvider>();
            @this.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, HalFormatterSetup>());

            return @this;
        }
    }
}