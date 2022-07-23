using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

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
        IServiceProvider serviceProvider,
        ILogger<ExceptionsMiddleware> logger)
    {
        bool correlationIdExists =
            httpContext.Request.Headers.TryGetValue(options.Value.CorrelationIdHeader, out StringValues correlationIds);

        // TODO: Check for correlation ID builder if correlation ID does not exist
        // If correlation ID builder has registered action, use that action
        // Otherwise use below logic, which should be placed in ICorrelationBuilder default behavior

        // TODO: Place default correlation ID composition in ICorrelationBuilder
        string correlationId = correlationIdExists ?
            correlationIds.First() : Activity.Current.Id ?? httpContext.TraceIdentifier;

        try
        {
            httpContext.Response.Headers[CORRELATION_ID_HEADER] = correlationId;

            var scope = new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            };

            using var _ = logger.BeginScope(scope);

            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Found Error: {Message} (correlation Id: {Id}", ex.Message, correlationId);
        }
    }
}

