using Application.Exceptions;
using Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace WebApi.Middleware
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            object response;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    response = new
                    {
                        title = "Validation Error",
                        status = (int)statusCode,
                        errors = validationException.Errors
                                    .GroupBy(e => e.PropertyName)
                                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                    };
                    break;

                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    response = new
                    {
                        title = "Resource Not Found",
                        status = (int)statusCode,
                        detail = notFoundException.Message
                    };
                    break;

                case DomainException domainException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    response = new
                    {
                        title = "Domain Rule Violation",
                        status = (int)statusCode,
                        detail = domainException.Message
                    };
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError; // 500
                    response = new
                    {
                        title = "An unexpected error occurred.",
                        status = (int)statusCode,
                        detail = exception.Message // Sadece Development'ta gösterilmeli
                    };
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
