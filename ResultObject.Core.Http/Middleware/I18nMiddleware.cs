using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ResultObject.Core.Http.Middleware
{
    public class I18NMiddleware
    {
        private readonly RequestDelegate next;

        public I18NMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        
        public async Task Invoke(HttpContext context)
        {
            string locale = null;

            if (context.Request.Query.ContainsKey("lng"))
            {
                locale = context.Request.Query["lng"];
            } 
            
            if (string.IsNullOrWhiteSpace(locale))
            {
                // Otherwise get it from the browser.
                locale = !string.IsNullOrWhiteSpace(context.Request.Headers["Accept-Language"])
                    ? context.Request.Headers["Accept-Language"].ToArray().First().Split(',')[0]
                    : "en-US";
            }

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(locale);
            
            await next.Invoke(context);
        }
    }

    public static class I18NMiddlewareExtensions
    {
        public static IApplicationBuilder UseI18NMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<I18NMiddleware>();
        }
    }}