using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Promotion_Service.MiddleWares
{
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var error = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error",
                            Detail = contextFeature.Error.Message
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
                    }
                });
            });

            return app;
        }
    }
}
