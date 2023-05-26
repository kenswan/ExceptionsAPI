// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace ExceptionsAPI;

/// <summary>
/// Represents current context of an exception being handled in Exceptions API Middleware
/// </summary>
public class ExceptionsAPIResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string ErrorMessage { get; set; }
}
