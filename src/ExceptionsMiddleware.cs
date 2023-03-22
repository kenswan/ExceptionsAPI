// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ExceptionsAPI;

internal class ExceptionsMiddleware
{
    private readonly RequestDelegate next;

    // TODO: Remove this value and pull from configured options
    private const string CORRELATION_ID_HEADER = "X-Correlation-Id";

    public ExceptionsMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(
        HttpContext httpContext,
        IOptions<ExceptionsOptions> options,
        // IServiceProvider serviceProvider,
        ILogger<ExceptionsMiddleware> logger)
    {
        var correlationIdExists =
            httpContext.Request.Headers.TryGetValue(options.Value.CorrelationIdHeader, out StringValues correlationIds);

        // TODO: Check for correlation ID builder if correlation ID does not exist
        // If correlation ID builder has registered action, use that action
        // Otherwise use below logic, which should be placed in ICorrelationBuilder default behavior

        // TODO: Place default correlation ID composition in ICorrelationBuilder
        var correlationId = correlationIdExists ?
            correlationIds.First() : Activity.Current.Id ?? httpContext.TraceIdentifier;

        try
        {
            httpContext.Response.Headers[CORRELATION_ID_HEADER] = correlationId;

            var scope = new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            };

            using IDisposable _ = logger.BeginScope(scope);

            await next(httpContext);
        }
        catch (Exception exception)
        {
            await LogAndWriteExceptionAsync(HttpStatusCode.InternalServerError, exception, "An internal error has occurred");
        }

        async Task LogAndWriteExceptionAsync(HttpStatusCode httpStatusCode, Exception exception, string messageOverride = default)
        {
            if (exception.Data.Count == 0)
            {
                ProblemDetails problemDetails =
                    InitializeProblemDetails<ProblemDetails>(httpStatusCode, exception, messageOverride);

                await LogAndWriteProblemDetailsExceptionAsync(problemDetails, exception);
            }
            else
            {
                ValidationProblemDetails problemDetails =
                    InitializeProblemDetails<ValidationProblemDetails>(httpStatusCode, exception, messageOverride);

                await LogAndWriteValidationProblemDetailsExceptionAsync(problemDetails, exception);
            }
        }

        T InitializeProblemDetails<T>(HttpStatusCode httpStatusCode, Exception exception, string messageOverride = default)
            where T : ProblemDetails, new()
        {
            return new T
            {
                Instance = !string.IsNullOrWhiteSpace(httpContext.Request.QueryString.ToString()) ?
                    string.Concat(httpContext.Request.Path, httpContext.Request.QueryString) :
                    httpContext.Request.Path,
                Detail = messageOverride ?? exception.Message,
                Status = (int)httpStatusCode,
                Title = httpStatusCode.ToString(),
                Type = exception.GetType().Name
            };
        }

        async Task LogAndWriteValidationProblemDetailsExceptionAsync(ValidationProblemDetails problemDetails, Exception exception)
        {
            foreach (var key in exception.Data.Keys)
            {
                problemDetails.Errors.TryAdd(key.ToString(), new string[] { exception.Data[key].ToString() });
            }

            await LogAndWriteProblemDetailsExceptionAsync(problemDetails, exception);
        }

        async Task LogAndWriteProblemDetailsExceptionAsync(ProblemDetails problemDetails, Exception exception)
        {
            logger.LogError(exception, "{Status}({Title}) - {Message}", problemDetails.Status, problemDetails.Title, exception.Message);

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

            var errorResponseString = JsonSerializer.Serialize(problemDetails);

            await httpContext.Response.WriteAsync(errorResponseString);

            // TODO: Add package needed for WriteAsJsonAsync
            /*
            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                problemDetails.GetType(), // WriteAsJson needs type to add additional fields provided in ValidationProblemDetails
                new JsonSerializerOptions { WriteIndented = true },
                "application/json"); */
        }
    }
}

