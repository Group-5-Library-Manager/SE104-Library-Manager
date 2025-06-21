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
        await EnsureCreateTacGiaAsync(context);
        await EnsureCreateNhaXuatBanAsync(context);
        await context.SaveChangesAsync(); // Ensure the above entities are saved before creating employees

        await EnsureCreateNhanVienAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateAdminAccountAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateDocGiaAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateSachAsync(context);
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

    private async Task EnsureCreateDocGiaAsync(DatabaseContext context)
    {
        if (!await context.DsDocGia.AnyAsync())
        {
            context.DsDocGia.AddRange(
                new DocGia
                {
                    TenDocGia = "Nguyễn Minh Hùng",
                    DiaChi = "123 Lê Lợi, Q.1, TP.HCM",
                    Email = "hung@example.com",
                    MaLoaiDocGia = 1, // Loại X
                    NgaySinh = new DateOnly(2000, 5, 20),
                    NgayLapThe = new DateOnly(2024, 6, 1),
                    TongNo = 0
                },
                new DocGia
                {
                    TenDocGia = "Trần Thị Mai",
                    DiaChi = "456 Nguyễn Trãi, Q.5, TP.HCM",
                    Email = "mai@example.com",
                    MaLoaiDocGia = 2, // Loại Y
                    NgaySinh = new DateOnly(1998, 9, 10),
                    NgayLapThe = new DateOnly(2024, 6, 2),
                    TongNo = 5000
                },
                new DocGia
                {
                    TenDocGia = "Lê Văn Tuấn",
                    DiaChi = "789 Cách Mạng Tháng 8, Q.10, TP.HCM",
                    Email = "tuan@example.com",
                    MaLoaiDocGia = 1,
                    NgaySinh = new DateOnly(2001, 2, 15),
                    NgayLapThe = new DateOnly(2024, 6, 3),
                    TongNo = 0
                }
            );
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

    private async Task EnsureCreateTacGiaAsync(DatabaseContext context)
    {
        if (!await context.DsTacGia.AnyAsync())
        {
            context.DsTacGia.AddRange(
                new TacGia { TenTacGia = "Nguyễn Văn A" },
                new TacGia { TenTacGia = "Trần Thị B" },
                new TacGia { TenTacGia = "Lê Văn C" }
            );
        }
    }

    private async Task EnsureCreateNhaXuatBanAsync(DatabaseContext context)
    {
        if (!await context.DsNhaXuatBan.AnyAsync())
        {
            context.DsNhaXuatBan.AddRange(
                new NhaXuatBan { TenNhaXuatBan = "NXB Giáo Dục" },
                new NhaXuatBan { TenNhaXuatBan = "NXB Trẻ" },
                new NhaXuatBan { TenNhaXuatBan = "NXB Kim Đồng" }
            );
        }
    }

    private async Task EnsureCreateSachAsync(DatabaseContext context)
    {
        if (!await context.DsSach.AnyAsync())
        {
            context.DsSach.AddRange(
                new Sach { TenSach = "Lập Trình C# Cơ Bản", MaTheLoai = 1, MaTacGia = 1, NamXuatBan = "2020", MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 1), TriGia = 120000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Giải Tích 1", MaTheLoai = 2, MaTacGia = 2, NamXuatBan = "2019", MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 2), TriGia = 95000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Kỹ Năng Sống", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = "2021", MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 3), TriGia = 75000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Toán Cao Cấp", MaTheLoai = 1, MaTacGia = 1, NamXuatBan = "2018", MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 4), TriGia = 110000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Hóa Đại Cương", MaTheLoai = 2, MaTacGia = 2, NamXuatBan = "2022", MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 5), TriGia = 89000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Văn Học Việt Nam", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = "2020", MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 6), TriGia = 78000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Lịch Sử Thế Giới", MaTheLoai = 1, MaTacGia = 2, NamXuatBan = "2017", MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 7), TriGia = 67000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Kinh Tế Vĩ Mô", MaTheLoai = 2, MaTacGia = 1, NamXuatBan = "2023", MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 8), TriGia = 112000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Kỹ Thuật Lập Trình", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = "2021", MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 9), TriGia = 134000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Giáo Dục Công Dân", MaTheLoai = 1, MaTacGia = 2, NamXuatBan = "2019", MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 10), TriGia = 56000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Sinh Học 12", MaTheLoai = 2, MaTacGia = 1, NamXuatBan = "2020", MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 11), TriGia = 87000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Cấu Trúc Dữ Liệu", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = "2022", MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 12), TriGia = 125000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Lập Trình Java", MaTheLoai = 1, MaTacGia = 1, NamXuatBan = "2021", MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 13), TriGia = 132000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Kỹ Năng Mềm", MaTheLoai = 2, MaTacGia = 2, NamXuatBan = "2018", MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 14), TriGia = 49000, TrangThai = "Có sẵn" },
                new Sach { TenSach = "Tâm Lý Học Đại Cương", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = "2020", MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 15), TriGia = 99000, TrangThai = "Có sẵn" }
            );
        }
    }


}
