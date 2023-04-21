// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace ExceptionsAPI;

public class ExceptionsAPIResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string ErrorMessage { get; set; }
}
