// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;

namespace ExceptionsAPI.Middleware;

internal class ExceptionsAPIMiddleware
{
    private readonly RequestDelegate next;

    // TODO: Remove this value and pull from configured options
    private const string CORRELATION_ID_HEADER = "X-Correlation-Id";

    public ExceptionsAPIMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(
        HttpContext httpContext,
        IOptions<ExceptionAPIOptions> exceptionAPIOptions,
        IOptionsMonitor<ExceptionOptions> exceptionOptionsMonitor,
        ILogger<ExceptionsAPIMiddleware> logger)
    {
        var correlationIdExists =
            httpContext.Request.Headers.TryGetValue(exceptionAPIOptions.Value.CorrelationIdHeader, out StringValues correlationIds);

        // TODO: Check for correlation ID builder if correlation ID does not exist
        // If correlation ID builder has registered action, use that action
        // Otherwise use below logic, which should be placed in ICorrelationBuilder default behavior

        // TODO: Place default correlation ID composition in ICorrelationBuilder
        var correlationId = correlationIdExists ? correlationIds.First() : Guid.NewGuid().ToString();

        var scope = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        };

        using (logger.BeginScope(scope))
        {
            try
            {
                httpContext.Response.Headers[CORRELATION_ID_HEADER] = correlationId;

                await next(httpContext);
            }
            catch (ExceptionsAPIException exception)
            {
                if (string.IsNullOrEmpty(exception.ClientErrorMessage))
                {
                    await LogAndWriteExceptionAsync(exception.StatusCode, exception);
                }
                else
                {
                    await LogAndWriteExceptionAsync(exception.StatusCode, exception, exception.ClientErrorMessage);
                }
            }
            catch (Exception exception)
            {
                ExceptionOptions exceptionOptions =
                    exceptionOptionsMonitor.Get(exception.GetType().AssemblyQualifiedName);

                if (exceptionOptions is not null)
                {
                    await LogAndWriteExceptionAsync(
                        exceptionOptions.HttpStatusCode,
                        exception,
                        exceptionOptions.Message);
                }
                else
                {
                    await LogAndWriteExceptionAsync(HttpStatusCode.InternalServerError, exception, "An internal error has occurred");
                }
            }
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
            ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

            await LogAndWriteProblemDetailsExceptionAsync(problemDetails, exception);
        }

        async Task LogAndWriteProblemDetailsExceptionAsync(ProblemDetails problemDetails, Exception exception)
        {
            logger.LogError(exception, "{Status}({Title}) - {Message}", problemDetails.Status, problemDetails.Title, exception.Message);

            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                problemDetails.GetType(), // WriteAsJson needs type to add additional fields provided in ValidationProblemDetails
                new JsonSerializerOptions { WriteIndented = true },
                "application/json");
        }
    }
}

