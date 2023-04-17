// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text.Json;

namespace ExceptionsAPI.Middleware;

public partial class ExceptionsAPIMiddlewareTests
{
    private readonly Mock<RequestDelegate> requestDelegateMock;

    private readonly ExceptionsAPIMiddleware exceptionsAPIMiddleware;

    public ExceptionsAPIMiddlewareTests()
    {
        requestDelegateMock = new();

        exceptionsAPIMiddleware = new ExceptionsAPIMiddleware(requestDelegateMock.Object);
    }

    private static async Task<T> GetErrorResponseFromBody<T>(MemoryStream stream)
    {
        stream.Position = 0;
        using var streamReader = new StreamReader(stream);
        var errorResponseString = await streamReader.ReadToEndAsync();

        return JsonSerializer.Deserialize<T>(errorResponseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // Adding real options monitor due to inability to Moq
    private static IOptionsMonitor<ExceptionOptions> GenerateOptionsMonitor(string name, ExceptionOptions options)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection
            .AddOptions<ExceptionOptions>(name)
            .Configure(internalOptions =>
        {
            internalOptions.ExceptionType = options.ExceptionType;
            internalOptions.HttpStatusCode = options.HttpStatusCode;
            internalOptions.DefaultMessage = options.DefaultMessage;
            internalOptions.ExceptionMapping = options.ExceptionMapping;
        });

        return serviceCollection
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<ExceptionOptions>>();
    }

    public static TheoryData<Exception, HttpStatusCode> ExceptionsWithStatusCode => new()
    {
        {
            new Exception("Exception Message Test"),
            HttpStatusCode.InternalServerError
        },
        {
            new ApplicationException("Exception Message Test 2"),
            HttpStatusCode.BadGateway
        },
        {
            new DirectoryNotFoundException(),
            HttpStatusCode.Locked
        }
    };

    private class TestException : ExceptionsAPIException
    {
        private const HttpStatusCode defaultStatusCode = HttpStatusCode.Unused;

        public TestException(string message) :
            base(defaultStatusCode, message)
        { }

        public TestException(HttpStatusCode httpStatusCode, string message) :
            base(httpStatusCode, message)
        { }

        public TestException(string message, Exception innerException) :
            base(defaultStatusCode, message, innerException)
        { }

        public TestException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
            base(httpStatusCode, message, innerException)
        { }
    }
}
