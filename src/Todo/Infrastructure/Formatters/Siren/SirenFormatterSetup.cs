using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Todo.Infrastructure.Formatters.Siren
{
    /// <summary>
    /// setup object for Siren Formatters
    /// </summary>
    /// <remarks>
    /// using this object allows us to delay the creation of the formatters until later in the 
    /// pipeline creation, when any setting overrides are present.
    /// </remarks>
    public class SirenFormatterSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public SirenFormatterSetup(ILoggerFactory loggerFactory, IOptions<MvcJsonOptions> jsonOptions)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (jsonOptions == null) throw new ArgumentNullException(nameof(jsonOptions));
            _loggerFactory = loggerFactory;
            _jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
        }

        public void Configure(MvcOptions options)
        {
            var outputFormatterLogger = this._loggerFactory.CreateLogger<SirenOutputFormatter>();
            options.OutputFormatters.Add(new SirenOutputFormatter(_jsonSerializerSettings, outputFormatterLogger));

            // add a mapping for the FormatFilter
            options.FormatterMappings.SetMediaTypeMappingForFormat("siren", MediaTypeHeaderValue.Parse(SirenOutputFormatter.MediaType));
        }
    }
}