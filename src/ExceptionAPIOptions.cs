﻿// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Net;

namespace ExceptionsAPI;

/// <summary>
/// Configuration settings used to override default behavior of Exception API Middleware
/// </summary>
public class ExceptionAPIOptions
{
    /// <summary>
    /// Default correlation key to use for the correlation id header
    /// </summary>
    public string CorrelationKey { get; set; } = "X-Correlation-Id";

    /// <summary>
    /// Configure the correlation value during runtime of each request
    /// </summary>
    public Func<HttpContext, string> ConfigureCorrelationValue { get; set; }

    /// <summary>
    /// Default error message if none specified for given type of exception
    /// </summary>
    public string DefaultErrorMessage { get; set; } = "An internal error has occurred";

    /// <summary>
    /// Default error status code if none specified for given type of exception
    /// </summary>
    public HttpStatusCode DefaultErrorStatusCode { get; set; } = HttpStatusCode.InternalServerError;
}
