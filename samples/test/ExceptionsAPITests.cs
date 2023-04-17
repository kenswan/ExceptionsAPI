// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using BlazorFocused;
using BlazorFocused.Extensions;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Samples.ExceptionsAPI.Exceptions;
using System.Net;
using System.Text.Json;
using System.Web;

namespace Samples.ExceptionsAPI.Test;

[Collection(nameof(SampleExceptionsAPITestCollection))]
public class ExceptionsAPITests
{
    private readonly IRestClient restClient;

    public ExceptionsAPITests(WebApplicationFactory<Program> webApplicationFactory)
    {
        restClient = RestClientExtensions.CreateRestClient(webApplicationFactory.CreateClient());
    }

    [Fact]
    public async Task ShouldReturnProperStatusCodes()
    {
        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();

        var url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        Assert.Equal(expectedStatusCode, response.StatusCode.Value);
    }

    [Fact]
    public async Task ShouldReturnAmbiguousForRandomException()
    {
        var url = "/ThrowRandomException";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        ProblemDetails problemDetails = GetProblemDetails(response);

        Assert.Equal(nameof(RandomException), problemDetails.Type);
        Assert.Equal((int)HttpStatusCode.Ambiguous, problemDetails.Status);
        Assert.Equal(HttpStatusCode.Ambiguous.ToString(), problemDetails.Title);
        Assert.Equal(RandomException.DEFAULT_MESSAGE, problemDetails.Detail);
        Assert.Equal(url, problemDetails.Instance);
    }

    [Fact]
    public async Task ShouldReturnRuntimeExceptionStatusCodeException()
    {
        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();
        var expectedMessage = new Faker().Lorem.Sentence();

        var url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}&message={expectedMessage}";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        ProblemDetails problemDetails = GetProblemDetails(response);

        Assert.Equal(nameof(CustomClientException), problemDetails.Type);
        Assert.Equal((int)expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedStatusCode.ToString(), problemDetails.Title);
        Assert.Equal(expectedMessage, problemDetails.Detail);
        Assert.Equal(url, HttpUtility.UrlDecode(problemDetails.Instance));
    }

    [Fact]
    public async Task ShouldReturnClientSpecificExceptionMessage()
    {
        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();
        var internalExceptionMessage = new Faker().Lorem.Sentence();
        var expectedClientMessage = new Faker().Lorem.Sentence();

        var url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}&message={internalExceptionMessage}&clientMessage={expectedClientMessage}";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        ProblemDetails problemDetails = GetProblemDetails(response);

        Assert.NotEqual(internalExceptionMessage, problemDetails.Detail);
        Assert.Equal(expectedClientMessage, problemDetails.Detail);

        Assert.Equal(nameof(CustomClientException), problemDetails.Type);
        Assert.Equal((int)expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedStatusCode.ToString(), problemDetails.Title);
        Assert.Equal(url, HttpUtility.UrlDecode(problemDetails.Instance));
    }

    private static ProblemDetails GetProblemDetails(RestClientTask restClientTask) =>
        JsonSerializer.Deserialize<ProblemDetails>(restClientTask.Content);
}
