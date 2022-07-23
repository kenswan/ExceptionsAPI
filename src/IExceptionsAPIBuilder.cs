using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExceptionsAPI;
public interface IExceptionsAPIBuilder
{
    public IExceptionsAPIBuilder HandleException(Exception exception, HttpStatusCode httpStatusCode);
    public IExceptionsAPIBuilder HandleException(Exception exception, Action<ValidationProblemDetails> action);
}
