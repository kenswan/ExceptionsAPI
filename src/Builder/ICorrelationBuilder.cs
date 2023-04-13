// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace ExceptionsAPI.Builder;

internal interface ICorrelationBuilder
{
    string BuildCorrelationId(HttpContext httpContext, IServiceProvider serviceProvider);
}
