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
    [MemberData(nameof(Exceptions))]
    public async Task Invoke_ShouldReturnProblemDetailsNoConfiguration(Exception thrownException)
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        IOptionsMonitor<ExceptionOptions> optionsMonitor = default;
        var exceptionAPIOptions = new ExceptionAPIOptions();

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
                    .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionAPIOptions),
                    optionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(exceptionAPIOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)exceptionAPIOptions.DefaultErrorStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)exceptionAPIOptions.DefaultErrorStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Theory]
    [MemberData(nameof(ExceptionsWithStatusCode))]
    public async Task Invoke_ShouldReturnProblemDetailsWithDefaultMessage(Exception thrownException, HttpStatusCode expectedStatusCode)
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedMessage = new Faker().Lorem.Sentence();

        var configuredExceptionOptions = new ExceptionOptions
        {
            ExceptionType = thrownException.GetType(),
            HttpStatusCode = expectedStatusCode,
            DefaultMessage = expectedMessage
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

    [Fact]
    public async Task Invoke_ShouldReturnProblemDetailsWithConfiguredMessage()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedMessage = new Faker().Lorem.Sentence();
        var exceptionMessage = new Faker().Lorem.Sentence();
        var thrownException = new AccessViolationException(exceptionMessage);
        HttpStatusCode expectedStatusCode = HttpStatusCode.SwitchingProtocols;

        var configuredExceptionOptions = new ExceptionOptions
        {
            ExceptionType = thrownException.GetType(),
            ExceptionResponseResolver =
                new ExceptionResponseResolver<Exception>((httpContext, exception) =>
                {
                    return new ExceptionsAPIResponse
                    {
                        ErrorMessage = expectedMessage,
                        HttpStatusCode = expectedStatusCode
                    };
                })
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
        Assert.NotEqual(exceptionMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }
}
