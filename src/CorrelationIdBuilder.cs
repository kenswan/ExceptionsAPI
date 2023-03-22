// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace ExceptionsAPI;

internal class CorrelationIdBuilder : ICorrelationBuilder
{
    private readonly Func<HttpContext, IServiceProvider, string> buildAction;

    public CorrelationIdBuilder(Func<HttpContext, IServiceProvider, string> buildAction)
    {
        this.buildAction = buildAction;
    }

    public string BuildCorrelationId(HttpContext httpContext, IServiceProvider serviceProvider) =>
        buildAction(httpContext, serviceProvider);
}
