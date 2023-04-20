// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using BlazorFocused.Tools;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace ExceptionsAPI.Middleware;

public partial class ExceptionsAPIMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldReturnAbstractExceptionDetails()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();
        var expectedMessage = new Faker().Lorem.Sentence();
        var thrownException = new TestException(expectedStatusCode, expectedMessage);

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionOptions> emptyOptionsMonitor = default;

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
                    .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionAPIOptions()),
                    emptyOptionsMonitor,
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

    [Fact]
    public async Task Invoke_ShouldReturnClientMessageInsteadOfExceptionMessage()
    {
        // Add Test Logger to ensure internal message is being logged
        ITestLogger<ExceptionsAPIMiddleware> loggerMock = ToolsBuilder.CreateTestLogger<ExceptionsAPIMiddleware>();

        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();
        var expectedMessage = new Faker().Lorem.Sentence();
        var internalLogMessage = new Faker().Lorem.Sentence();
        var externalClientMessage = new Faker().Lorem.Sentence();

        var thrownException = new TestException(expectedStatusCode, internalLogMessage)
        {
            ClientErrorMessage = externalClientMessage
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionOptions> emptyOptionsMonitor = default;

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
                    .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionAPIOptions()),
                    emptyOptionsMonitor,
                    loggerMock));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check only client message is seen by consumer
        Assert.Equal(externalClientMessage, actualErrorResponse.Detail);

        // Verify original exception message was logged
        loggerMock.VerifyWasCalledWith(LogLevel.Error, internalLogMessage);

        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }
}
