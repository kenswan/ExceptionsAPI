// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace ExceptionsAPI;

public abstract class ExceptionsAPIException : Exception
{
    public HttpStatusCode StatusCode { get; private set; }

    public string ClientErrorMessage { get; init; }

    public ExceptionsAPIException(HttpStatusCode httpStatusCode, string message) :
        base(message)
    {
        StatusCode = httpStatusCode;
    }

    public ExceptionsAPIException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
        base(message, innerException)
    {
        StatusCode = httpStatusCode;
    }
}
