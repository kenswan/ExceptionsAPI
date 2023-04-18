// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using ExceptionsAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace ExceptionsAPI.Builder;

public class ExceptionsAPIBuilderTests
{
    private readonly IServiceCollection serviceCollection;

    private readonly IExceptionsAPIBuilder exceptionsAPIBuilder;

    public ExceptionsAPIBuilderTests()
    {
        serviceCollection = new ServiceCollection();

        exceptionsAPIBuilder = new ExceptionsAPIBuilder(serviceCollection);
    }

    [Fact]
    public void AddException_ShouldRegisterExceptionOptions()
    {
        HttpStatusCode expectedStatusCode = GenerateRandomStatusCode();
        var exceptionKey = typeof(ArgumentOutOfRangeException).AssemblyQualifiedName;

        // Under Test
        exceptionsAPIBuilder.AddException<ArgumentOutOfRangeException>(expectedStatusCode);

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        IOptionsMonitor<ExceptionOptions> optionsMonitor = serviceProvider.GetService<IOptionsMonitor<ExceptionOptions>>();
        ExceptionOptions actualOptions = optionsMonitor.Get(exceptionKey);

        Assert.NotNull(actualOptions);

        Assert.Equal(expectedStatusCode, actualOptions.HttpStatusCode);
        Assert.Equal(exceptionKey, actualOptions.ExceptionType.AssemblyQualifiedName);

        Assert.Null(actualOptions.DefaultMessage);
        Assert.Null(actualOptions.ExceptionResponseResolver);
    }

    [Fact]
    public void AddException_ShouldRegisterExceptionOptionsWithDefaultMessage()
    {
        HttpStatusCode expectedStatusCode = GenerateRandomStatusCode();
        var exceptionKey = typeof(ArgumentException).AssemblyQualifiedName;
        var expectedResponseMessage = new Faker().Lorem.Sentence();

        // Under Test
        exceptionsAPIBuilder.AddException<ArgumentException>(expectedStatusCode, expectedResponseMessage);

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        IOptionsMonitor<ExceptionOptions> optionsMonitor = serviceProvider.GetService<IOptionsMonitor<ExceptionOptions>>();
        ExceptionOptions actualOptions = optionsMonitor.Get(exceptionKey);

        Assert.NotNull(actualOptions);

        Assert.Equal(expectedStatusCode, actualOptions.HttpStatusCode);
        Assert.Equal(expectedResponseMessage, actualOptions.DefaultMessage);
        Assert.Equal(exceptionKey, actualOptions.ExceptionType.AssemblyQualifiedName);

        Assert.Null(actualOptions.ExceptionResponseResolver);
    }

    [Fact]
    public void AddException_ShouldRegisterExceptionOptionsWithResponseCompilation()
    {
        HttpStatusCode expectedStatusCode = GenerateRandomStatusCode();
        Type exceptionType = typeof(AppDomainUnloadedException);
        var exceptionKey = exceptionType.AssemblyQualifiedName;
        var expectedException = new AppDomainUnloadedException("This is a test");
        Mock<HttpContext> httpContextMock = new();
        Mock<HttpResponse> httpResponseMock = new();

        Func<HttpContext, AppDomainUnloadedException, ExceptionsAPIResponse> responseMapping =
            GenerateAPIResponse<AppDomainUnloadedException>;

        httpContextMock.SetupGet(context => context.Response).Returns(httpResponseMock.Object);
        httpResponseMock.SetupGet(request => request.StatusCode).Returns((int)expectedStatusCode);

        // Under Test
        exceptionsAPIBuilder.AddException<AppDomainUnloadedException>(responseMapping);

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        IOptionsMonitor<ExceptionOptions> optionsMonitor = serviceProvider.GetService<IOptionsMonitor<ExceptionOptions>>();
        ExceptionOptions actualOptions = optionsMonitor.Get(exceptionKey);

        Assert.NotNull(actualOptions);

        Assert.NotNull(actualOptions.ExceptionResponseResolver);
        Assert.Equal(exceptionKey, actualOptions.ExceptionType.AssemblyQualifiedName);

        ExceptionsAPIResponse expectedExceptionsResponse =
            GenerateAPIResponse(httpContextMock.Object, expectedException);

        var resolver = actualOptions.ExceptionResponseResolver as ExceptionResponseResolver<AppDomainUnloadedException>;

        ExceptionsAPIResponse actualExceptionsResponse =
            resolver.Resolve(httpContextMock.Object, expectedException);

        Assert.Equal(expectedExceptionsResponse.StatusCode, actualExceptionsResponse.StatusCode);
        Assert.Equal(expectedExceptionsResponse.ErrorMessage, actualExceptionsResponse.ErrorMessage);
    }

    private static HttpStatusCode GenerateRandomStatusCode() =>
        new Faker().PickRandom<HttpStatusCode>();

    private static ExceptionsAPIResponse GenerateAPIResponse<T>(HttpContext httpContext, T exception)
        where T : Exception =>
            new()
            {
                StatusCode = (HttpStatusCode)httpContext.Response.StatusCode,
                ErrorMessage = exception.Message
            };
}
