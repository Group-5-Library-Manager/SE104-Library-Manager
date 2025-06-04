using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Repositories;
using SE104_Library_Manager.Services;
using SE104_Library_Manager.ViewModels;
using SE104_Library_Manager.Views;

namespace SE104_Library_Manager.Extensions;

public static class AppServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Register database context and services
        services.AddSingleton<DatabaseService>();

        services.AddSingleton<UserSessionManager>();
        services.AddSingleton<IUserSessionReader>(sp => sp.GetRequiredService<UserSessionManager>());
        services.AddSingleton<IUserSessionManager>(sp => sp.GetRequiredService<UserSessionManager>());
        services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();

        return services;
    }
}