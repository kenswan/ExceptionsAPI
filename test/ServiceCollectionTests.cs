// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace ExceptionsAPI;

public class ServiceCollectionTests
{
    [Fact]
    public void AddExceptionsAPI_ShouldEstablishBaseOptions()
    {
        var expectedApiOptions = new ExceptionAPIOptions();

        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddExceptionsAPI();

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ExceptionAPIOptions actualApiOptions =
            serviceProvider.GetRequiredService<IOptions<ExceptionAPIOptions>>().Value;

        actualApiOptions.Should().BeEquivalentTo(expectedApiOptions);
    }

    [Fact]
    public void AddExceptionsAPI_ShouldChangeBaseOptions()
    {
        var defaultErrorMessage = "This is a test error message";
        HttpStatusCode defaultErrorHttpStatusCode = HttpStatusCode.GatewayTimeout;
        var correlationHeaderKey = "X-Test-Header";

        var expectedApiOptions = new ExceptionAPIOptions
        {
            DefaultErrorMessage = defaultErrorMessage,
            DefaultErrorStatusCode = defaultErrorHttpStatusCode,
            CorrelationKey = correlationHeaderKey,
            ConfigureCorrelationValue = ConfigureCorrelation
        };

        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddExceptionsAPI(options =>
        {
            options.DefaultErrorMessage = defaultErrorMessage;
            options.DefaultErrorStatusCode = defaultErrorHttpStatusCode;
            options.CorrelationKey = correlationHeaderKey;
            options.ConfigureCorrelationValue = ConfigureCorrelation;
        });

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ExceptionAPIOptions actualApiOptions =
            serviceProvider.GetRequiredService<IOptions<ExceptionAPIOptions>>().Value;

        actualApiOptions.Should().BeEquivalentTo(expectedApiOptions);

        static string ConfigureCorrelation(HttpContext httpContext)
        {
            return "Configure This Value From HttpContext";
        }
    }
}
