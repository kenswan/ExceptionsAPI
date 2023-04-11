// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExceptionsAPI;

public interface IExceptionsAPIBuilder
{
    public IExceptionsAPIBuilder AddCorrelation(Func<HttpContext, IServiceProvider, string> correlationBuilder);

    public IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode)
        where TException : Exception;

    public IExceptionsAPIBuilder AddException<TException>(Func<TException, ProblemDetails> action)
        where TException : Exception;
}
