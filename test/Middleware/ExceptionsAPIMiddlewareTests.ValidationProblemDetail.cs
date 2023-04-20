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
    public async Task Invoke_ShouldReturnValidationProblemDetails(Exception thrownException, HttpStatusCode expectedStatusCode)
    {
        // Adding data to exception should signal use of Problem Details
        thrownException.Data.Add("TestField1", "TestValue1");
        thrownException.Data.Add("TestField2", "TestValue2");
        thrownException.Data.Add("TestField3", "TestValue3");

        using MemoryStream memoryStream =
            GenerateHttpContext(out var expectedInstance, out HttpContext httpContext);

        var correlationIdKey = new Faker().Internet.UserAgent();
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

        ValidationProblemDetails actualErrorResponse =
            await GetErrorResponseFromBody<ValidationProblemDetails>(memoryStream);

        Assert.Equal(3, actualErrorResponse.Errors.Count);
        Assert.Equal("TestValue1", actualErrorResponse.Errors["TestField1"].First());
        Assert.Equal("TestValue2", actualErrorResponse.Errors["TestField2"].First());
        Assert.Equal("TestValue3", actualErrorResponse.Errors["TestField3"].First());

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(expectedMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }
}
