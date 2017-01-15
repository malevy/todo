using System;
using System.Text;
using System.Threading.Tasks;
using D2L.Hypermedia.Siren;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Todo.ViewModels.Out;

namespace Todo.Formatters.Siren
{
    internal class SirenOutputFormatter : TextOutputFormatter
    {
        private ILogger<SirenOutputFormatter> _outputFormatterLogger;
        private JsonSerializerSettings _jsonSerializerSettings;

        public const string MediaType = "application/vnd.siren+json";

        // too bad that Lazy<> won't work here
        private JsonSerializer _jsonSerializer;
        private JsonSerializer Serializer => 
            _jsonSerializer 
            ?? (_jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings));

        public SirenOutputFormatter(JsonSerializerSettings jsonSerializerSettings, ILogger<SirenOutputFormatter> outputFormatterLogger)
        {
            if (jsonSerializerSettings == null) throw new ArgumentNullException(nameof(jsonSerializerSettings));
            _jsonSerializerSettings = jsonSerializerSettings;
            this._outputFormatterLogger = outputFormatterLogger;

            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
            this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(MediaType));

        }

        // tell the pipeline which types can be handled by this formatter
        protected override bool CanWriteType(Type type)
        {
            return type == typeof(TodoVM) || type == typeof(TodoCollectionVm);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (selectedEncoding == null) throw new ArgumentNullException(nameof(selectedEncoding));

            var transformerProvider = new SirenTransformerProvider();
            var formattedObject = transformerProvider.From(context.Object);

            await formattedObject.Match(
                some: async se => await WriteObject(se, context, selectedEncoding),
                none: async msg => await HandleTransformError(msg)
            );
        }

        private Task HandleTransformError(string problem)
        {
            throw new FormatException($"Unable to transform viewmodel to Siren ({problem})");
        }

        private async Task WriteObject(ISirenEntity sirenEntity, OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            using (var jw = new JsonTextWriter(writer) { CloseOutput = false })
            {
                this.Serializer.Serialize(jw, sirenEntity);
                await writer.FlushAsync();
            }
        }
    }
}