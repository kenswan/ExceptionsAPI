// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace ExceptionsAPI;
internal interface ICorrelationBuilder
{
    string BuildCorrelationId(HttpContext httpContext, IServiceProvider serviceProvider);
}
