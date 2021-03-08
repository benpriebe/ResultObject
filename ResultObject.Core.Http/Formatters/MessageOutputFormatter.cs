using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace ResultObject.Core.Http.Formatters
{
    /// <summary>
    /// Processes request ResultMessageLevel options and serializes/ignore Message properties.
    /// If there is no request level ResultMessageLevel option it falls back to server-side configuration. 
    /// </summary>
    public class MessageOutputFormatter : TextOutputFormatter
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public MessageOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        }

        protected override bool CanWriteType(Type type)
        {
            return type.IsAssignableTo(typeof(ResultBase));
        }

        public override async Task WriteResponseBodyAsync(
            OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;
            if (!(context.Object is ResultBase result))
            {
                await httpContext.Response.WriteAsync(string.Empty, selectedEncoding);
                return;
            }

            result.Messages = result.Messages?.Select(msg => new Message
            {
                Type = msg.Type,
                Content = msg.Content,

                Code = Result.MessageLevelOptions.Value.Code ? msg.Code : null,
                Template = Result.MessageLevelOptions.Value.Template ? msg.Template : null,
                Tokens = Result.MessageLevelOptions.Value.Tokens ? msg.Tokens : null,
                LanguageCode = Result.MessageLevelOptions.Value.LanguageCode ? msg.LanguageCode : null,
            }).ToArray();

            var response = JsonSerializer.Serialize(context.Object, jsonSerializerOptions);
            await httpContext.Response.WriteAsync(response, selectedEncoding);
        }
    }
}