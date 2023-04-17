// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using ExceptionsAPI;
using System.Net;

namespace Samples.ExceptionsAPI.Exceptions;

public class CustomClientException : ExceptionsAPIException
{
    private const HttpStatusCode defaultStatusCode = HttpStatusCode.Conflict;

    public CustomClientException(string message) :
        base(defaultStatusCode, message)
    { }

    public CustomClientException(HttpStatusCode httpStatusCode, string message) :
        base(httpStatusCode, message)
    { }

    public CustomClientException(string message, Exception innerException) :
        base(defaultStatusCode, message, innerException)
    { }

    public CustomClientException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
        base(httpStatusCode, message, innerException)
    { }
}
