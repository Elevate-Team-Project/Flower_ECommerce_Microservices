using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Auth.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.ContentType = "application/json";
                var statusCode = HttpStatusCode.InternalServerError;
                object errorResponse;

                switch (ex)
                {
                    case ValidationException validationEx:
                        statusCode = HttpStatusCode.BadRequest;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = "Validation failed.",
                            errors = validationEx.Errors
                                .Select(e => new
                                {
                                    Field = e.PropertyName.Replace("RegisterDto.", ""),
                                    e.ErrorMessage
                                })
                        };
                        break;

                    case BadHttpRequestException badReqEx:
                        statusCode = HttpStatusCode.BadRequest;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = badReqEx.Message
                        };
                        break;

                    case KeyNotFoundException keyNotFoundEx:
                        statusCode = HttpStatusCode.NotFound;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = keyNotFoundEx.Message
                        };
                        break;

                    case UnauthorizedAccessException unauthorizedEx:
                        // 401 = لم يُسجل دخول المستخدم
                        // 403 = لا يملك صلاحية (Forbidden)
                        if (context.User.Identity?.IsAuthenticated ?? false)
                        {
                            statusCode = HttpStatusCode.Forbidden; // 403
                            errorResponse = new
                            {
                                statusCode = (int)statusCode,
                                message = "You do not have permission to perform this action."
                            };
                        }
                        else
                        {
                            statusCode = HttpStatusCode.Unauthorized; // 401
                            errorResponse = new
                            {
                                statusCode = (int)statusCode,
                                message = "Invalid email or password."
                            };
                        }
                        break;

                    default:
                        statusCode = HttpStatusCode.InternalServerError;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = "An unexpected error occurred.",
                            details = _env.IsDevelopment() ? ex.Message : null
                        };
                        break;
                }

                context.Response.StatusCode = (int)statusCode;
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(errorResponse, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
