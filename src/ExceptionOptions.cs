// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExceptionsAPI;

internal class ExceptionOptions
{
    public Type ExceptionType { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public string Message { get; set; } = "An Error Has Occurred";

    public Func<Exception, ProblemDetails> ExceptionMapping { get; set; }
}
