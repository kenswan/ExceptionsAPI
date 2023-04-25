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

namespace ExceptionsAPI.Middleware;

public partial class ExceptionsAPIMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyDuringError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedCorrelationHeader = new Faker().Internet.UserAgent();
        var thrownException = new Exception("Internal Error Message");

        var exceptionAPIOptions = new ExceptionAPIOptions
        {
            CorrelationKey = expectedCorrelationHeader
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
                    Options.Create(exceptionAPIOptions),
                    emptyOptionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Key and default Guid Value
        Assert.True(httpContext.Response.Headers.ContainsKey(expectedCorrelationHeader));
        Assert.True(Guid.TryParse(httpContext.Response.Headers[expectedCorrelationHeader], out Guid _));

        Assert.Equal(exceptionAPIOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(500, actualErrorResponse.Status);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyWithoutError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedCorrelationHeader = new Faker().Internet.UserAgent();

        var exceptionAPIOptions = new ExceptionAPIOptions
        {
            CorrelationKey = expectedCorrelationHeader
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionOptions> emptyOptionsMonitor = default;

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionAPIOptions),
                    emptyOptionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Key and default Guid Value
        Assert.True(httpContext.Response.Headers.ContainsKey(expectedCorrelationHeader));
        Assert.True(Guid.TryParse(httpContext.Response.Headers[expectedCorrelationHeader], out Guid _));
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationValueDuringError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();
        var thrownException = new Exception("Internal Error Message");

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionAPIOptions = new ExceptionAPIOptions
        {
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
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
                    Options.Create(exceptionAPIOptions),
                    emptyOptionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[exceptionAPIOptions.CorrelationKey]);

        Assert.Equal(exceptionAPIOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(500, actualErrorResponse.Status);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationValueWithoutError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionAPIOptions = new ExceptionAPIOptions
        {
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionOptions> emptyOptionsMonitor = default;

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionAPIOptions),
                    emptyOptionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[exceptionAPIOptions.CorrelationKey]);
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyAndValueDuringError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedCorrelationHeader = new Faker().Internet.UserAgent();
        var expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();
        var thrownException = new Exception("Internal Error Message");

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionAPIOptions = new ExceptionAPIOptions
        {
            CorrelationKey = expectedCorrelationHeader,
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
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
                    Options.Create(exceptionAPIOptions),
                    emptyOptionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[expectedCorrelationHeader]);

        Assert.Equal(exceptionAPIOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(500, actualErrorResponse.Status);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyAndValueWithoutError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var expectedCorrelationHeader = new Faker().Internet.UserAgent();
        var expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionAPIOptions = new ExceptionAPIOptions
        {
            CorrelationKey = expectedCorrelationHeader,
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionOptions> emptyOptionsMonitor = default;

        Exception actualException =
            await Record.ExceptionAsync(() =>
                exceptionsAPIMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionAPIOptions),
                    emptyOptionsMonitor,
                    NullLogger<ExceptionsAPIMiddleware>.Instance));

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[expectedCorrelationHeader]);
        Assert.Equal(200, httpContext.Response.StatusCode);
    }
}
