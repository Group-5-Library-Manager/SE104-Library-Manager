using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Data;
using SE104_Library_Manager.Services;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE104_Library_Manager.Interfaces;
using Moq;
using SE104_Library_Manager;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SE104_Library_Manager.Tests
{
    public abstract class TestBase
    {
        protected ServiceProvider ServiceProvider { get; private set; }
        protected static IConfiguration Configuration { get; private set; }
        protected DatabaseContext DbContext { get; private set; }

        protected TestBase()
        {
            var services = new ServiceCollection();

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("app_settings.json", optional: true, reloadOnChange: true)
                .Build();

            // Register database context with InMemory provider
            // This creates a unique database name for each test class to ensure isolation
            services.AddDbContext<DatabaseContext>(options =>
                options
                    .UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString())
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            );

            // Register all repositories
            RegisterRepositories(services);

            // Register services and repositories needed for tests
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Set up App.ServiceProvider for repositories that use it
            var serviceProviderProperty = typeof(App).GetProperty("ServiceProvider");
            serviceProviderProperty?.SetValue(null, ServiceProvider);

            // Get DbContext for direct use
            DbContext = ServiceProvider.GetRequiredService<DatabaseContext>();

            // Create the database in memory
            DbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Register all repositories with their dependencies
        /// </summary>
        private void RegisterRepositories(IServiceCollection services)
        {
            // Core repositories
            services.AddScoped<IQuyDinhRepository, QuyDinhRepository>();
            services.AddScoped<IVaiTroRepository, VaiTroRepository>();
            services.AddScoped<IBangCapRepository, BangCapRepository>();
            services.AddScoped<IBoPhanRepository, BoPhanRepository>();
            services.AddScoped<IChucVuRepository, ChucVuRepository>();
            services.AddScoped<ILoaiDocGiaRepository, LoaiDocGiaRepository>();
            services.AddScoped<ITheLoaiRepository, TheLoaiRepository>();
            services.AddScoped<ITacGiaRepository, TacGiaRepository>();
            services.AddScoped<INhaXuatBanRepository, NhaXuatBanRepository>();
            services.AddScoped<INhanVienRepository, NhanVienRepository>();
            services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
            services.AddScoped<IDocGiaRepository, DocGiaRepository>();
            services.AddScoped<ISachRepository, SachRepository>();
            services.AddScoped<IPhieuMuonRepository, PhieuMuonRepository>();
            services.AddScoped<IPhieuTraRepository, PhieuTraRepository>();
            services.AddScoped<IChiTietPhieuTraRepository, ChiTietPhieuTraRepository>();
            services.AddScoped<IPhieuPhatRepository, PhieuPhatRepository>();

            // Register DatabaseService for repositories that need it
            services.AddScoped<DatabaseService>(sp =>
            {
                var dbContext = sp.GetRequiredService<DatabaseContext>();
                var dbService = new DatabaseService();
                // Use reflection to set the private _dbContext field
                var dbContextField = typeof(DatabaseService).GetField("_dbContext", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                dbContextField?.SetValue(dbService, dbContext);
                return dbService;
            });
        }

        /// <summary>
        /// Method to register services needed for tests
        /// </summary>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Register AuthService
            services.AddScoped<IAuthService, AuthService>();

            var mockStaffSession = new Mock<IStaffSessionReader>();
            mockStaffSession.Setup(x => x.CurrentStaffId).Returns(999); // Use a high ID to avoid conflicts
            mockStaffSession.Setup(x => x.GetCurrentStaffRole()).Returns("Quản trị viên");

            services.AddSingleton<IStaffSessionReader>(mockStaffSession.Object);
            // Child classes will override this method to register necessary services
        }

        /// <summary>
        /// Method to seed test data
        /// </summary>
        protected virtual void SeedData()
        {
            // Child classes will override this method to seed test data
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clear all data from InMemory database
            DbContext?.Database.EnsureDeleted();
            
            // Dispose DbContext to release resources
            DbContext?.Dispose();
            ServiceProvider?.Dispose();
        }

        /// <summary>
        /// Clear all data from the database
        /// </summary>
        protected void ClearDatabase()
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Database.EnsureCreated();
            DbContext?.SaveChanges();
        }

        /// <summary>
        /// Reset database to initial state
        /// </summary>
        protected void ResetDatabase()
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Database.EnsureCreated();
            SeedData();
            DbContext?.SaveChanges();
        }

        /// <summary>
        /// Seed basic data required for most tests
        /// </summary>
        protected void SeedBasicData()
        {
            // Add QuyDinh
            var quyDinh = new Entities.QuyDinh
            {
                TuoiDocGiaToiThieu = 18,
                TuoiDocGiaToiDa = 55,
                ThoiHanTheDocGia = 6,
                SoNamXuatBanToiDa = 8,
                SoSachMuonToiDa = 5,
                SoNgayMuonToiDa = 4,
                TienPhatQuaHanMoiNgay = 1000
            };
            DbContext.Add(quyDinh);

            // Add VaiTro
            var vaiTro1 = new Entities.VaiTro { TenVaiTro = "Quản trị viên" };
            var vaiTro2 = new Entities.VaiTro { TenVaiTro = "Thủ thư" };
            DbContext.AddRange(vaiTro1, vaiTro2);

            // Add BangCap
            var bangCap1 = new Entities.BangCap { TenBangCap = "Tú tài" };
            var bangCap2 = new Entities.BangCap { TenBangCap = "Đại học" };
            DbContext.AddRange(bangCap1, bangCap2);

            // Add BoPhan
            var boPhan1 = new Entities.BoPhan { TenBoPhan = "Thủ thư" };
            var boPhan2 = new Entities.BoPhan { TenBoPhan = "Ban giám đốc" };
            DbContext.AddRange(boPhan1, boPhan2);

            // Add ChucVu
            var chucVu1 = new Entities.ChucVu { TenChucVu = "Nhân viên" };
            var chucVu2 = new Entities.ChucVu { TenChucVu = "Giám đốc" };
            DbContext.AddRange(chucVu1, chucVu2);

            // Add LoaiDocGia
            var loaiDocGia1 = new Entities.LoaiDocGia { TenLoaiDocGia = "Sinh viên" };
            var loaiDocGia2 = new Entities.LoaiDocGia { TenLoaiDocGia = "Giảng viên" };
            DbContext.AddRange(loaiDocGia1, loaiDocGia2);

            // Add TheLoai
            var theLoai1 = new Entities.TheLoai { TenTheLoai = "Văn học" };
            var theLoai2 = new Entities.TheLoai { TenTheLoai = "Khoa học" };
            DbContext.AddRange(theLoai1, theLoai2);

            // Add TacGia
            var tacGia1 = new Entities.TacGia { TenTacGia = "Nguyễn Du" };
            var tacGia2 = new Entities.TacGia { TenTacGia = "Nam Cao" };
            DbContext.AddRange(tacGia1, tacGia2);

            // Add NhaXuatBan
            var nhaXuatBan1 = new Entities.NhaXuatBan { TenNhaXuatBan = "NXB Giáo dục" };
            var nhaXuatBan2 = new Entities.NhaXuatBan { TenNhaXuatBan = "NXB Văn học" };
            DbContext.AddRange(nhaXuatBan1, nhaXuatBan2);

            DbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Assembly-level test cleanup to ensure database is deleted after all tests
    /// </summary>
    [TestClass]
    public class GlobalTestCleanup
    {
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // This will run after all tests in the assembly have completed
            Console.WriteLine("All tests completed. Cleaning up global resources...");
            
            // Force garbage collection to ensure all database connections are properly disposed
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            Console.WriteLine("Global cleanup completed.");
        }
    }
}
