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
        ExceptionAPIOptions exceptionAPIOptionsValue = exceptionAPIOptions.Value ??
            throw new ArgumentNullException(nameof(exceptionAPIOptions));

        var correlationKey = exceptionAPIOptionsValue.CorrelationKey;

        var correlationKeyExists =
            httpContext.Request.Headers.TryGetValue(exceptionAPIOptionsValue.CorrelationKey, out StringValues correlationValues);

        var correlationValue = correlationKeyExists switch
        {
            { } when correlationKeyExists => correlationValues.First(),

            { } when exceptionAPIOptionsValue.ConfigureCorrelationValue is not null =>
                    exceptionAPIOptionsValue.ConfigureCorrelationValue(httpContext),

            _ => Guid.NewGuid().ToString()
        };

        var scope = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationValue
        };

        using (logger.BeginScope(scope))
        {
            try
            {
                httpContext.Response.Headers[correlationKey] = correlationValue;

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
                    exceptionOptionsMonitor?.Get(exception.GetType().AssemblyQualifiedName);

                Task logAndWriteResponseTask = exceptionOptions switch
                {
                    // Leverage Message composition
                    // If both default message and message composition exists,
                    // message composition will take precedence
                    { } when exceptionOptions is not null && exceptionOptions.ExceptionResponseResolver is not null =>
                        LogAndWriteExceptionsResponseResolverAsync(exceptionOptions.ExceptionResponseResolver, exception),

                    // Leverage Default Message configured (and status code)
                    { } when exceptionOptions is not null &&
                            exceptionOptions.DefaultMessage is not null &&
                            exceptionOptions.HttpStatusCode.HasValue =>
                        LogAndWriteExceptionAsync(
                            exceptionOptions.HttpStatusCode.Value,
                            exception,
                            exceptionOptions.DefaultMessage),

                    // Leverage Status Code configured
                    { } when exceptionOptions is not null && exceptionOptions.HttpStatusCode.HasValue =>
                        LogAndWriteExceptionAsync(
                            exceptionOptions.HttpStatusCode.Value,
                            exception,
                            exceptionAPIOptionsValue.DefaultErrorMessage),

                    _ => LogAndWriteExceptionAsync(
                            exceptionAPIOptionsValue.DefaultErrorStatusCode,
                            exception,
                            exceptionAPIOptionsValue.DefaultErrorMessage)
                }; ;

                await logAndWriteResponseTask;
            }
        }

        async Task LogAndWriteExceptionsResponseResolverAsync(ExceptionResponseResolver exceptionResponseResolver, Exception exception)
        {
            ExceptionsAPIResponse exceptionsAPIResponse =
                            exceptionResponseResolver.Resolve(httpContext, exception);

            await LogAndWriteExceptionAsync(
                exceptionsAPIResponse.HttpStatusCode,
                exception,
                exceptionsAPIResponse.ErrorMessage);
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

