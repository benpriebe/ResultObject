using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ResultObject.Core.Http.Middleware
{
    public class ResultMessageLevelMiddleware
    {
        private const string ResultMessageLevelsHeader = "Result-Message-Levels";
        private readonly RequestDelegate next;

        public ResultMessageLevelMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            ResultMessageLevelOptions options;
            if (context.Request.Headers.ContainsKey(ResultMessageLevelsHeader))
            {
                var includeOptions = context.Request.Headers[ResultMessageLevelsHeader]
                    .First()
                    .Split(",").Select(option => option.ToLowerInvariant());
                options = new ResultMessageLevelOptions
                {
                    Code = includeOptions.Contains("code"),
                    LanguageCode = includeOptions.Contains("languagecode"),
                    Template = includeOptions.Contains("template"),
                    Tokens = includeOptions.Contains("tokens")
                };
            }
            else
            {
                options = ((IOptions<ResultMessageLevelOptions>) context.RequestServices
                    .GetService(typeof(IOptions<ResultMessageLevelOptions>)))?.Value;
            }

            Result.MessageLevelOptions.Value = options;
            
            await next.Invoke(context);
        }
    }

    public static class ResultMessageLevelMiddlewareExtensions
    {
        public static IApplicationBuilder UseResultMessageLevelMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResultMessageLevelMiddleware>();
        }
    }
}