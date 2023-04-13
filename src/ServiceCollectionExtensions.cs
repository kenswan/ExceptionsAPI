// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using ExceptionsAPI.Builder;
using ExceptionsAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ExceptionsAPI;

public static class ServiceCollectionExtensions
{
    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<ExceptionAPIOptions>();

        return new ExceptionsAPIBuilder(serviceCollection);
    }

    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection, Action<ExceptionAPIOptions> modifyOptions)
    {
        serviceCollection.Configure<ExceptionAPIOptions>(options => modifyOptions(options));

        return new ExceptionsAPIBuilder(serviceCollection);
    }

    public static IExceptionsAPIBuilder AddExceptionsAPI(
        this IServiceCollection serviceCollection,
        Action<ExceptionAPIOptions> modifyOptions,
        Func<HttpContext, IServiceProvider, string> defaultCorrelationIdBuilder)
    {
        serviceCollection.Configure<ExceptionAPIOptions>(options => modifyOptions(options));

        serviceCollection.AddTransient<ICorrelationBuilder>(_ => new CorrelationIdBuilder(defaultCorrelationIdBuilder));

        return new ExceptionsAPIBuilder(serviceCollection);
    }

    public static IApplicationBuilder UseExceptionsAPI(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionsAPIMiddleware>();
}
