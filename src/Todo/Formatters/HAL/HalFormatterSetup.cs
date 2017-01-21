using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Todo.Formatters.HAL
{
    /// <summary>
    /// setup object for Siren Formatters
    /// </summary>
    /// <remarks>
    /// using this object allows us to delay the creation of the formatters until later in the 
    /// pipeline creation, when any setting overrides are present.
    /// </remarks>
    public class HalFormatterSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public HalFormatterSetup(ILoggerFactory loggerFactory, IOptions<MvcJsonOptions> jsonOptions)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (jsonOptions == null) throw new ArgumentNullException(nameof(jsonOptions));
            _loggerFactory = loggerFactory;
            _jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
        }

        public void Configure(MvcOptions options)
        {
            var outputFormatterLogger = this._loggerFactory.CreateLogger<HalOutputFormatter>();
            options.OutputFormatters.Add(new HalOutputFormatter(_jsonSerializerSettings, outputFormatterLogger));

            // add a mapping for the FormatFilter
            options.FormatterMappings.SetMediaTypeMappingForFormat("hal", MediaTypeHeaderValue.Parse(HalOutputFormatter.MediaType));
        }
    }
}