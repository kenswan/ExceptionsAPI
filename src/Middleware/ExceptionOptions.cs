// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace ExceptionsAPI.Middleware;

internal class ExceptionOptions
{
    public Type ExceptionType { get; set; }

    public HttpStatusCode? HttpStatusCode { get; set; }

    public string DefaultMessage { get; set; }

    public ExceptionResponseResolver ExceptionResponseResolver { get; set; }
}
