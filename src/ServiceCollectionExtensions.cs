using Microsoft.Extensions.DependencyInjection;

namespace ExceptionsAPI;
public static class ServiceCollectionExtensions
{
    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection)
    {
        // TODO: Add Options
        return new ExceptionsAPIBuilder();
    }

    public static IExceptionsAPIBuilder AddExceptionsAPI(this IServiceCollection serviceCollection, string correlationId)
    {
        // TODO: Add Options
        return new ExceptionsAPIBuilder();
    }

    public static void UseExcpetionsAPI()
    {
        // TODO: Add .UseMiddleware<ExceptionsMiddleware>();
    }
}
