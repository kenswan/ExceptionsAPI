using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

    public async Task Invoke(HttpContext httpContext, ILogger<ExceptionsMiddleware> logger)
    {
        bool correlationIdExists =
            httpContext.Request.Headers.TryGetValue(CORRELATION_ID_HEADER, out StringValues correlationIds);

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

