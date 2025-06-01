using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Windows;
using SE104_Library_Manager.Extensions;
using SE104_Library_Manager.Services;

namespace SE104_Library_Manager;

public partial class App : Application
{
    public static ServiceProvider? ServiceProvider { get; private set; }
    public static IConfiguration? Configuration { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("app_settings.json", optional: true, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddAppServices();

        ServiceProvider = services.BuildServiceProvider();

        DatabaseService? databaseService = ServiceProvider.GetRequiredService<DatabaseService>();

        string? connectionString = Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ConfigurationErrorsException("Connection string 'DefaultConnection' is not configured in app_settings.json.");
        }

        databaseService.Initialize(connectionString).GetAwaiter().GetResult();

        // !IMPORTANT: Do not use StartupUri for anything needing dependency injection (DI)
        // Due to the way WPF handles StartupUri, it does not support DI properly.
        MainWindow mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
