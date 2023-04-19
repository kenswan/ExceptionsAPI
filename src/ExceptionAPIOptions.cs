// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace ExceptionsAPI;

public class ExceptionAPIOptions
{
    public string CorrelationId { get; set; } = "X-Correlation-Id";

    public Func<HttpContext, IServiceProvider, string> ConfigureCorrelationValue { get; set; }
}
