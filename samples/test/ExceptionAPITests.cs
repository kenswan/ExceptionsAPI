// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Samples.ExceptionAPI.Test;

[Collection(nameof(SampleExceptionAPITestCollection))]
public class ExceptionAPITests
{
    private readonly HttpClient client;

    public ExceptionAPITests(WebApplicationFactory<Program> webApplicationFactory)
    {
        this.client = webApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task ShouldReturnAmbiguousForRandomException()
    {
        HttpResponseMessage httpResponseMessage = await client.GetAsync("/ThrowRandomException");

        Assert.Equal(HttpStatusCode.Ambiguous, httpResponseMessage.StatusCode);
    }
}
