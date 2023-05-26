// -------------------------------------------------------
// Copyright (c) Ken Swan. All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using ExceptionsAPI.Builder;
using ExceptionsAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ExceptionsAPI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Exception API Services to current collection of service descriptors
    /// </summary>
    /// <param name="serviceCollection">Current collection of services registered for dependency injection</param>
    /// <returns><see cref="IExceptionsAPIBuilder"/> used for further middleware extension/customization</returns>
    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<ExceptionAPIOptions>();

        return new ExceptionsAPIBuilder(serviceCollection);
    }

    /// <summary>
    /// Add Exception API Services to current collection of service descriptors
    /// </summary>
    /// <param name="serviceCollection">Current collection of services registered for dependency injection</param>
    /// <param name="modifyOptions"></param>
    /// <returns><see cref="IExceptionsAPIBuilder"/> used for further middleware extension/customization</returns>
    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection, Action<ExceptionAPIOptions> modifyOptions)
    {
        serviceCollection.Configure<ExceptionAPIOptions>(options => modifyOptions(options));

        return new ExceptionsAPIBuilder(serviceCollection);
    }

    /// <summary>
    /// Add Exception API Middleware to application request/response pipeline
    /// </summary>
    /// <param name="builder">Current application request pipeline configuration to append Exception API middleware activation</param>
    /// <returns>Current application request pipeline configuration</returns>
    public static IApplicationBuilder UseExceptionsAPI(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionsAPIMiddleware>();
}
