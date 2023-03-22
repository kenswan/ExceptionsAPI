using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExceptionsAPI;
internal class ExceptionsAPIBuilder : IExceptionsAPIBuilder
{
    public IExceptionsAPIBuilder AddCorrelation(Func<HttpContext, IServiceProvider, string> correlationBuilder)
    {
        throw new NotImplementedException();
    }

    public IExceptionsAPIBuilder AddException(Exception exception, HttpStatusCode httpStatusCode)
    {
        throw new NotImplementedException();
    }

    public IExceptionsAPIBuilder AddException(Exception exception, Action<ValidationProblemDetails> action)
    {
        throw new NotImplementedException();
    }
}
