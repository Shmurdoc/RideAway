using System.Net;
using System.Text.Json;
using RideAway.Domain.Exceptions;

namespace RideAway.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            RideNotFoundException => (HttpStatusCode.NotFound, "Ride not found."),
            InvalidRideStatusException => (HttpStatusCode.BadRequest, ex.Message),
            RideAlreadyCompletedException => (HttpStatusCode.BadRequest, "Ride is already completed."),
            PaymentProcessingException => (HttpStatusCode.InternalServerError, "Payment processing encountered an error."),
            InvalidGeoLocationException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidCredentialsException => (HttpStatusCode.Unauthorized, "Invalid email or password."),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "You are not authorized to perform this action."),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            System.Data.Common.DbException => (HttpStatusCode.ServiceUnavailable, "The database is currently unavailable. Please try again later."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Success = false,
            StatusCode = context.Response.StatusCode,
            Message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
