using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment if used
// using Microsoft.Extensions.Hosting; // For IHostEnvironment and IsDevelopment() extension

namespace ParkShare.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Default

        // Customize status code based on exception type
        object responsePayload;
        switch (exception)
        {
            case ArgumentException argEx: // Covers ArgumentNullException, ArgumentOutOfRangeException
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                responsePayload = new { statusCode = context.Response.StatusCode, message = argEx.Message };
                break;
            case InvalidOperationException opEx:
                 context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // Or Conflict (409) depending on context
                 responsePayload = new { statusCode = context.Response.StatusCode, message = opEx.Message };
                break;
            case UnauthorizedAccessException authEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                 responsePayload = new { statusCode = context.Response.StatusCode, message = "Unauthorized access." , detailed = authEx.Message};
                break;
            // Add more specific exception types as needed
            // case MyCustomNotFoundException notFoundEx:
            //     context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            //     responsePayload = new { statusCode = context.Response.StatusCode, message = notFoundEx.Message };
            //     break;
            default:
                responsePayload = new
                {
                    statusCode = context.Response.StatusCode,
                    message = "An internal server error occurred. Please try again later.",
                    // detailed = exception.Message // Only include detailed message in Development
                };
                break;
        }

        // Example of providing more details in development (requires IWebHostEnvironment)
        // var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        // if (env.IsDevelopment() && !(exception is ArgumentException || exception is InvalidOperationException))
        // {
        //     // For general errors in dev, show more details. For specific handled ones, the message above is likely enough.
        //     responsePayload = new { statusCode = context.Response.StatusCode, message = exception.Message, details = exception.StackTrace };
        // }


        var jsonResponse = JsonSerializer.Serialize(responsePayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(jsonResponse);
    }
}
