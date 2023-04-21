// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace ExceptionsAPI.Middleware;

public class ExceptionResponseResolver<T> : ExceptionResponseResolver where T : Exception
{
    private readonly Func<HttpContext, T, ExceptionsAPIResponse> exceptionMapping;

    public ExceptionResponseResolver(Func<HttpContext, T, ExceptionsAPIResponse> exceptionMapping)
    {
        this.exceptionMapping = exceptionMapping;
    }

    public override ExceptionsAPIResponse Resolve(HttpContext httpContext, Exception exception) =>
        exception is not T castedException
            ? throw new ArgumentException($"Exception of type {exception.GetType().FullName} does not have configured response")
            : exceptionMapping(httpContext, castedException);
}

public abstract class ExceptionResponseResolver
{
    public abstract ExceptionsAPIResponse Resolve(HttpContext httpContext, Exception exception);
}
