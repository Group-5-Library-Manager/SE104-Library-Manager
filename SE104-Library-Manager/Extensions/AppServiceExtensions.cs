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

        services.AddSingleton<StaffSessionManager>();
        services.AddSingleton<IStaffSessionReader>(sp => sp.GetRequiredService<StaffSessionManager>());
        services.AddSingleton<IStaffSessionManager>(sp => sp.GetRequiredService<StaffSessionManager>());
        services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
        services.AddScoped<IDocGiaRepository, DocGiaRepository>();
        services.AddScoped<ILoaiDocGiaRepository, LoaiDocGiaRepository>();
        services.AddScoped<INhanVienRepository, NhanVienRepository>();
        services.AddScoped<IChucVuRepository, ChucVuRepository>();
        services.AddScoped<IBangCapRepository, BangCapRepository>();
        services.AddScoped<IBoPhanRepository, BoPhanRepository>();
        services.AddScoped<IVaiTroRepository, VaiTroRepository>();
        services.AddScoped<IQuyDinhRepository, QuyDinhRepository>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();

        services.AddTransient<ReaderView>();
        services.AddTransient<ReaderViewModel>();
        services.AddTransient<AddReaderWindow>();
        services.AddTransient<AddReaderViewModel>();
        services.AddTransient<AddReaderTypeWindow>();
        services.AddTransient<AddReaderTypeViewModel>();


        services.AddTransient<StaffView>();
        services.AddTransient<StaffViewModel>();
        services.AddTransient<AddStaffWindow>();
        services.AddTransient<AddStaffViewModel>();
        services.AddTransient<AddPositionWindow>();
        services.AddTransient<AddPositionViewModel>();
        services.AddTransient<AddDegreeWindow>();
        services.AddTransient<AddDegreeViewModel>();
        services.AddTransient<AddDepartmentWindow>();
        services.AddTransient<AddDepartmentViewModel>();

        return services;
    }
}