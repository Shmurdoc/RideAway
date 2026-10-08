using System.Net;
using System.Text.Json;
using RideAway.Domain.Exceptions;

namespace RideAway.API.Middleware;

/// <summary>
/// Catches unhandled exceptions and converts them into a consistent JSON error
/// response with an appropriate HTTP status code.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the next middleware and handles any exception it throws.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
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
        // Only exceptions we own get a hand-written, constant message. Broad framework
        // exceptions (ArgumentException, InvalidOperationException, KeyNotFoundException)
        // are thrown from many places and quote argument names and internal values, so
        // their message is logged but never reflected to the caller.
        var (statusCode, message) = ex switch
        {
            RideNotFoundException => (HttpStatusCode.NotFound, "Ride not found."),
            InvalidRideStatusException => (HttpStatusCode.BadRequest, ex.Message),
            RideAlreadyCompletedException => (HttpStatusCode.BadRequest, "Ride is already completed."),
            PaymentProcessingException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidGeoLocationException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidCredentialsException => (HttpStatusCode.Unauthorized, "Invalid email or password."),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "You are not authorized to perform this action."),
            ArgumentException => (HttpStatusCode.BadRequest, "The request contains an invalid value."),
            InvalidOperationException => (HttpStatusCode.BadRequest, "The request could not be completed."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
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
