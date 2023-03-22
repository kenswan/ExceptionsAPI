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

    public IExceptionsAPIBuilder AddException(Exception exception, HttpStatusCode httpStatusCode);

    public IExceptionsAPIBuilder AddException(Exception exception, Action<ValidationProblemDetails> action);
}
