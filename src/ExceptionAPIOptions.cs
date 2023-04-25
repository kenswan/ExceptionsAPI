// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Net;

namespace ExceptionsAPI;

public class ExceptionAPIOptions
{
    public string CorrelationKey { get; set; } = "X-Correlation-Id";

    public Func<HttpContext, string> ConfigureCorrelationValue { get; set; }

    public string DefaultErrorMessage { get; set; } = "An internal error has occurred";

    public HttpStatusCode DefaultErrorStatusCode { get; set; } = HttpStatusCode.InternalServerError;
}
