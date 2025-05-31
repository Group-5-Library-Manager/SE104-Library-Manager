using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager.Extensions;

public static class AppServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Register database context and services
        services.AddSingleton<DatabaseService>();

        services.AddSingleton<MainWindow>();

        return services;
    }
}