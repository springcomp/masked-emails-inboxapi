using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebApi.Services;

namespace Utils.Owin
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next_;
        private readonly ExceptionHandlingOptions options_;

        private const string HandledException = "x-owin/exception-handling/exception";

        public ExceptionHandlingMiddleware(RequestDelegate next, ExceptionHandlingOptions options)
        {
            next_ = next ?? throw new ArgumentNullException(nameof(next));
            options_ = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBody = context.Response.Body;

            using (var newBody = new MemoryStream())
            {
                byte[] content = null;

                try
                {
                    context.Response.Body = newBody;
                    await next_(context);
                }
                catch (Exception e)
                {
                    var (statusCode, buffer) = RecordException(context, e);

                    // record the exception reason
                    // that will be written in the finally block

                    content = buffer;

                    var contentLength = buffer?.Length;
                    WriteCommonResponseHeaders(context, statusCode, contentLength);
                }
                finally
                {
                    context.Response.Body = Stream.Null;
                    newBody.Seek(0, SeekOrigin.Begin);
                    context.Response.Body = originalBody;

                    await WriteResponseBody(context, content ?? newBody.ToArray());
                }
            }
        }

        private (HttpStatusCode, byte[]) RecordException(HttpContext context, Exception e)
        {
            byte[] buffer = null;
            var statusCode = HttpStatusCode.InternalServerError;

            context.Response.Clear();
            context.Items.Add(HandledException, e);

            var typeOfException = e.GetType();

            if (options_.StatusCodes.ContainsKey(typeOfException))
            {
                statusCode = options_.StatusCodes[typeOfException];
                buffer = MakeException(e);

            }

            return (statusCode, buffer);
        }

        private static void WriteCommonResponseHeaders(HttpContext context, HttpStatusCode statusCode, int? contentLength = null)
        {
            context.Response.StatusCode = (int)statusCode;

            if (contentLength != null)
            {
                context.Response.ContentLength = contentLength;
                context.Response.ContentType = "application/json";
            }
        }

        private static async Task WriteResponseBody(HttpContext context, byte[] content)
        {
            await context.Response.WriteAsync(Encoding.UTF8.GetString(content));
        }

        private static byte[] MakeException(Exception exception)
        {
            if (exception.Message == null)
                return new byte[] { };

            var error = new Error
            {
                Reason = exception.Message,
            };
            var content = JsonConvert.SerializeObject(error);
            return Encoding.UTF8.GetBytes(content);
        }
    }
}