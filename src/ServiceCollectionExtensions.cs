using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ExceptionsAPI;
public static class ServiceCollectionExtensions
{
    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<ExceptionsOptions>();

        return new ExceptionsAPIBuilder();
    }

    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection, string correlationIdHeader)
    {
        serviceCollection.Configure<ExceptionsOptions>(options => options.CorrelationIdHeader = correlationIdHeader);

        return new ExceptionsAPIBuilder();
    }

    public static IExceptionsAPIBuilder AddExceptionsAPI(
        this IServiceCollection serviceCollection,
        string correlationIdHeader,
        Func<HttpContext, IServiceProvider, string> defaultCorrelationIdBuilder)
    {
        serviceCollection.AddOptions<ExceptionsOptions>();

        serviceCollection.AddTransient<ICorrelationBuilder>(_ => new CorrelationIdBuilder(defaultCorrelationIdBuilder));

        return new ExceptionsAPIBuilder();
    }

    public static IApplicationBuilder UseExcpetionsAPI(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionsMiddleware>();
}
