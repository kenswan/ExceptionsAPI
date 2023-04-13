// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using ExceptionsAPI.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    }

    private static HttpStatusCode GenerateRandomStatusCode() =>
        new Faker().PickRandom<HttpStatusCode>();
}
