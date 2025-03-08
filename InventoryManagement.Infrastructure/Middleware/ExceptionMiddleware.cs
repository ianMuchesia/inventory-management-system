


using System.Net;
using System.Text.Json;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Middleware
{

    public class ExceptionMiddleware

    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionMiddleware> _logger;


        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"An API exception occurred: {ex.Message}");
                await HandleApiExceptionAsync(httpContext, ex);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, $"A domain exception occurred: {ex.Message}");
                await HandleDomainExceptionAsync(httpContext, ex);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Unhandled Error: {ex.Message}");
                await HandleUnknownExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleApiExceptionAsync(HttpContext context, ApiException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception.StatusCode;

            var response = ApiResponse<object>.Fail(
                exception.Message,
                exception.StatusCode,
                new ErrorDetails
                {
                    Code = exception.ErrorCode,
                    Detail = exception.Message
                });

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);

        }


        private static async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = ApiResponse<object>.Fail(
                exception.Message,
                (int)HttpStatusCode.BadRequest,
                new ErrorDetails
                {
                    Code = "INVALID_REQUEST",
                    Detail = exception.Message
                });

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }


        private static async Task HandleUnknownExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ApiResponse<object>.Fail(
                "An unexpected error occurred",
                (int)HttpStatusCode.InternalServerError,
                new ErrorDetails
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Detail = exception.Message
                });

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}