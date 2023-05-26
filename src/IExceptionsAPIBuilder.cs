// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Net;

namespace ExceptionsAPI;

public interface IExceptionsAPIBuilder
{
    /// <summary>
    /// Configures a response status code for a given exception
    /// </summary>
    /// <typeparam name="TException">Type of exception that has being thrown</typeparam>
    /// <param name="httpStatusCode">Configures status code returned for give exception</param>
    /// <returns>Current Exception API Configuration Builder</returns>
    IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode)
        where TException : Exception;

    /// <summary>
    /// Configures a response status code and default response message returned for a given exception
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    /// <param name="httpStatusCode"></param>
    /// <param name="clientErrorMessage"></param>
    /// <returns>Current Exception API Configuration Builder</returns>
    IExceptionsAPIBuilder AddException<TException>(HttpStatusCode httpStatusCode, string clientErrorMessage)
        where TException : Exception;

    /// <summary>
    /// Establish a response status code and response message (<see cref="ExceptionsAPIResponse"/>) returned for a given exception
    /// </summary>
    /// <typeparam name="TException"></typeparam>
    /// <param name="buildExceptionResponse"></param>
    /// <returns>Current Exception API Configuration Builder</returns>
    IExceptionsAPIBuilder AddException<TException>(Func<HttpContext, TException, ExceptionsAPIResponse> buildExceptionResponse)
        where TException : Exception;
}
