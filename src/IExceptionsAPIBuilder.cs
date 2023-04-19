// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Net;

namespace ExceptionsAPI;

public interface IExceptionsAPIBuilder
{
    IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode)
        where TException : Exception;

    IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode, string clientErrorMessage)
        where TException : Exception;

    IExceptionsAPIBuilder AddException<TException>(Func<HttpContext, TException, ExceptionsAPIResponse> buildExceptionResponse)
        where TException : Exception;
}
