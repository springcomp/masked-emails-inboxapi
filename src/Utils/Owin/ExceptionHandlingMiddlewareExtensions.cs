using System;
using Microsoft.AspNetCore.Builder;

namespace Utils.Owin
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder HandleExceptions(this IApplicationBuilder app, Action<ExceptionHandlingOptions> options = null)
        {
            var arguments = new ExceptionHandlingOptions();
            options?.Invoke(arguments);
            return app.UseMiddleware<ExceptionHandlingMiddleware>(arguments);
        }
    }
}