using Microsoft.AspNetCore.Http;

namespace ExceptionsAPI;
internal interface ICorrelationBuilder
{
    string BuildCorrelationId(HttpContext httpContext, IServiceProvider serviceProvider);
}
