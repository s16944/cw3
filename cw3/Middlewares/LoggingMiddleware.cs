using System.IO;
using System.Text;
using System.Threading.Tasks;
using cw3.Services;
using Microsoft.AspNetCore.Http;

namespace cw3.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRequestLogger logger)
        {
            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                string path = context.Request.Path;
                var method = context.Request.Method;
                var queryString = context.Request.QueryString.ToString();

                string bodyString;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyString = await reader.ReadToEndAsync();
                }

                context.Request.Body.Seek(0, SeekOrigin.Begin);

                logger.Log(method, path, queryString, bodyString);
            }

            if (_next != null) await _next(context);
        }
    }
}