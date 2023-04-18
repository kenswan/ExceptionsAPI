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

    public ExceptionsAPIResponse Resolve(HttpContext httpContext, T exception) =>
        exceptionMapping(httpContext, exception);
}

public class ExceptionResponseResolver
{
}
