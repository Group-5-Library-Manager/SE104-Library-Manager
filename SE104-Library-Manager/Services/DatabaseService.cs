using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Data;
using SE104_Library_Manager.Entities;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Services;

public class DatabaseService
{
    private DatabaseContext? _dbContext;

    public DatabaseContext DbContext => GetDatabaseContext();

    private DatabaseContext GetDatabaseContext()
    {
        if (_dbContext == null)
        {
            throw new InvalidOperationException("Database context is not initialized. Call Initialize first.");
        }

        return _dbContext;
    }

    public async Task Initialize(string connectionString)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connectionString)
            .Options;
        
        _dbContext = new DatabaseContext(options);

        bool isNewlyCreated = await _dbContext.Database.EnsureCreatedAsync();
        if (isNewlyCreated)
        {
            await EnsureDatabaseSeededAsync();
        }
    }

    private async Task EnsureDatabaseSeededAsync()
    {
        DatabaseContext context = GetDatabaseContext();

        await EnsureCreateQuyDinhAsync(context);
        await EnsureCreateVaiTroAsync(context);
        await EnsureCreateBangCapAsync(context);
        await EnsureCreateBoPhanAsync(context);
        await EnsureCreateChucVuAsync(context);
        await EnsureCreateLoaiDocGiaAsync(context);
        await EnsureCreateTheLoaiAsync(context);
        await context.SaveChangesAsync(); // Ensure the above entities are saved before creating employees


        await EnsureCreateNhanVienAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateAdminAccountAsync(context);
        await context.SaveChangesAsync();
    }

    private async Task EnsureCreateQuyDinhAsync(DatabaseContext context)
    {
        if (!await context.DsQuyDinh.AnyAsync())
        {
            context.DsQuyDinh.Add(new QuyDinh());
        }
    }

    private async Task EnsureCreateVaiTroAsync(DatabaseContext context)
    {
        if (!await context.DsVaiTro.AnyAsync())
        {
            context.DsVaiTro.Add(new VaiTro { TenVaiTro = "Quản trị viên" });
            context.DsVaiTro.Add(new VaiTro { TenVaiTro = "Thủ thư" });
        }
    }

    private async Task EnsureCreateBangCapAsync(DatabaseContext context)
    {
        if (!await context.DsBangCap.AnyAsync())
        {
            context.DsBangCap.Add(new BangCap { TenBangCap = "Tú tài" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Trung cấp" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Cao đẳng" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Đại học" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Thạc sĩ" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Tiến sĩ" });
        }
    }

    private async Task EnsureCreateBoPhanAsync(DatabaseContext context)
    {
        if (!await context.DsBoPhan.AnyAsync())
        {
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Thủ thư" });
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Thủ kho" });
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Thủ quỹ" });
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Ban giám đốc" });
        }
    }

    private async Task EnsureCreateChucVuAsync(DatabaseContext context)
    {
        if (!await context.DsChucVu.AnyAsync())
        {
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Nhân viên" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Phó phòng" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Trưởng phòng" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Phó giám đốc" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Giám đốc" });
        }
    }

    private async Task EnsureCreateNhanVienAsync(DatabaseContext context)
    {
        if (!await context.DsNhanVien.AnyAsync())
        {
            context.DsNhanVien.Add(new NhanVien
            {
                TenNhanVien = "Admin",
                DiaChi = "123 Đường ABC, Quận 1, TP.HCM",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
                MaChucVu = 5,
                MaBangCap = 6,
                MaBoPhan = 4
            });
        }
    }

    private async Task EnsureCreateAdminAccountAsync(DatabaseContext context)
    {
        if (!await context.DsTaiKhoan.AnyAsync())
        {
            var adminAccount = new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("admin"),
                MaNhanVien = 1, // Assuming the first employee is the admin
                MaVaiTro = 1 // Assuming the first role is ADMIN
            };
            context.DsTaiKhoan.Add(adminAccount);
        }
    }

    private async Task EnsureCreateLoaiDocGiaAsync(DatabaseContext context)
    {
        if (!await context.DsLoaiDocGia.AnyAsync())
        {
            await context.DsLoaiDocGia.AddAsync(new LoaiDocGia { TenLoaiDocGia = "X" });
            await context.DsLoaiDocGia.AddAsync(new LoaiDocGia { TenLoaiDocGia = "Y" });
        }
    }

    private async Task EnsureCreateTheLoaiAsync(DatabaseContext context)
    {
        if (!await context.DsTheLoai.AnyAsync())
        {
            await context.DsTheLoai.AddAsync(new TheLoai { TenTheLoai = "A" });
            await context.DsTheLoai.AddAsync(new TheLoai { TenTheLoai = "B" });
            await context.DsTheLoai.AddAsync(new TheLoai { TenTheLoai = "C" });
        }
    }
}
