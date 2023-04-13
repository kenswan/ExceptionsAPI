// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace ExceptionsAPI.Middleware;

public partial class ExceptionsAPIMiddlewareTests
{
    [Theory]
    [MemberData(nameof(ExceptionsWithStatusCode))]
    public async Task ShouldReturnProblemDetails(Exception thrownException, HttpStatusCode expectedStatusCode)
    {
        var url = new Faker().Internet.UrlRootedPath();
        var queryString = "?testParam=testValue&testParam2=testValue2";
        var expectedInstance = string.Concat(url, queryString);
        var expectedMessage = new Faker().Lorem.Sentence();

        using var memoryStream = new MemoryStream();
        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = memoryStream;
        httpContext.Request.Path = url;
        httpContext.Request.QueryString = new QueryString(queryString);

        var configuredExceptionOptions = new ExceptionOptions
        {
            ExceptionType = thrownException.GetType(),
            HttpStatusCode = expectedStatusCode,
            Message = expectedMessage
        };

        IOptionsMonitor<ExceptionOptions> optionsMonitor =
            GenerateOptionsMonitor(thrownException.GetType().AssemblyQualifiedName, configuredExceptionOptions);

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
                    .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionAPIOptions()),
                    optionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(expectedMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }
}
