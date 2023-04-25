// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using BlazorFocused;
using BlazorFocused.Extensions;
using Bogus;
using ExceptionsAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Samples.ExceptionsAPI.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using Xunit.Abstractions;

namespace Samples.ExceptionsAPI.Test;

[Collection(nameof(SampleExceptionsAPITestCollection))]
public class ExceptionsAPITests
{
    private readonly IRestClient restClient;
    private readonly ITestOutputHelper testOutputHelper;

    public ExceptionsAPITests(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper testOutputHelper)
    {
        restClient = RestClientExtensions.CreateRestClient(webApplicationFactory.CreateClient());

        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task ShouldReturnProperStatusCodes()
    {
        HttpStatusCode expectedStatusCode = GenerateNon300LevelStatusCode();

        testOutputHelper.WriteLine("Expected Status Code: {0}", expectedStatusCode);

        var url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        testOutputHelper.WriteLine("Expected Status Code: {0}", response.StatusCode.Value);

        Assert.Equal(expectedStatusCode, response.StatusCode.Value);

        ValidateHeaders(response.Headers);
    }

    [Fact]
    public async Task ShouldReturnAmbiguousForRandomException()
    {
        var url = "/ThrowRandomException";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        ProblemDetails problemDetails = GetProblemDetails(response);

        Assert.Equal(nameof(RandomException), problemDetails.Type);
        Assert.Equal((int)HttpStatusCode.FailedDependency, problemDetails.Status);
        Assert.Equal(HttpStatusCode.FailedDependency.ToString(), problemDetails.Title);
        Assert.Equal(new ExceptionAPIOptions().DefaultErrorMessage, problemDetails.Detail);
        Assert.Equal(url, problemDetails.Instance);

        ValidateHeaders(response.Headers);
    }

    [Fact]
    public async Task ShouldReturnRuntimeExceptionStatusCodeException()
    {
        HttpStatusCode expectedStatusCode = GenerateNon300LevelStatusCode();
        var expectedMessage = new Faker().Lorem.Sentence();

        var url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}&message={expectedMessage}";

        RestClientTask response = await restClient.SendAsync(HttpMethod.Get, url);

        ProblemDetails problemDetails = GetProblemDetails(response);

        Assert.Equal(nameof(CustomClientException), problemDetails.Type);
        Assert.Equal((int)expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedStatusCode.ToString(), problemDetails.Title);
        Assert.Equal(expectedMessage, problemDetails.Detail);
        Assert.Equal(url, HttpUtility.UrlDecode(problemDetails.Instance));

        ValidateHeaders(response.Headers);
    }

    [Fact]
    public async Task ShouldReturnClientSpecificExceptionMessage()
    {
        HttpStatusCode expectedStatusCode = GenerateNon300LevelStatusCode();
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

        ValidateHeaders(response.Headers);
    }

    private static ProblemDetails GetProblemDetails(RestClientTask restClientTask) =>
        JsonSerializer.Deserialize<ProblemDetails>(restClientTask.Content);

    private static void ValidateHeaders(HttpResponseHeaders httpResponseHeaders)
    {
        Assert.True(httpResponseHeaders.TryGetValues(Program.CORRELATION_HEADER_KEY, out IEnumerable<string> headers));
        Assert.Single(headers);
        Assert.False(string.IsNullOrWhiteSpace(headers.First()));
    }

    // When running responses from WebApplication Factory, 300 level errors are
    // causing unpredicted behavior
    private static HttpStatusCode GenerateNon300LevelStatusCode()
    {
        HttpStatusCode? expectedStatusCodeGenerated;

        do
        {
            expectedStatusCodeGenerated =
                new Faker().PickRandomWithout<HttpStatusCode>(
                    HttpStatusCode.Redirect,
                    HttpStatusCode.MovedPermanently,
                    HttpStatusCode.Moved,
                    HttpStatusCode.SeeOther,
                    HttpStatusCode.NotModified,
                    HttpStatusCode.UseProxy,
                    HttpStatusCode.TemporaryRedirect,
                    HttpStatusCode.PermanentRedirect);
        }
        while (!expectedStatusCodeGenerated.HasValue ||
            (int)expectedStatusCodeGenerated.Value > 299 && (int)expectedStatusCodeGenerated.Value < 400);

        return expectedStatusCodeGenerated.Value;
    }
}
