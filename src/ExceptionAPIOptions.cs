// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Net;

namespace ExceptionsAPI;

public class ExceptionAPIOptions
{
    public string CorrelationId { get; set; } = "X-Correlation-Id";

    public string DefaultErrorMessage { get; set; } = "An internal error has occurred";

    public HttpStatusCode DefaultErrorStatusCode { get; set; } = HttpStatusCode.InternalServerError;

    public Func<HttpContext, IServiceProvider, string> ConfigureCorrelationValue { get; set; }
}
