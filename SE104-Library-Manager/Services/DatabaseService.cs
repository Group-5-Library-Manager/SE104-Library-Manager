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

        await EnsureCreatePhieuMuonAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateChiTietPhieuMuonAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreatePhieuTraAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateChiTietPhieuTraAsync(context);
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
                    NgayLapThe = new DateOnly(2025, 6, 1),
                    TongNo = 0
                },
                new DocGia
                {
                    TenDocGia = "Trần Thị Mai",
                    DiaChi = "456 Nguyễn Trãi, Q.5, TP.HCM",
                    Email = "mai@example.com",
                    MaLoaiDocGia = 2, // Loại Y
                    NgaySinh = new DateOnly(1998, 9, 10),
                    NgayLapThe = new DateOnly(2025, 6, 2),
                    TongNo = 5000
                },
                new DocGia
                {
                    TenDocGia = "Lê Văn Tuấn",
                    DiaChi = "789 Cách Mạng Tháng 8, Q.10, TP.HCM",
                    Email = "tuan@example.com",
                    MaLoaiDocGia = 1,
                    NgaySinh = new DateOnly(2001, 2, 15),
                    NgayLapThe = new DateOnly(2025, 6, 3),
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

    private async Task EnsureCreatePhieuMuonAsync(DatabaseContext context)
    {
        if (!await context.DsPhieuMuon.AnyAsync())
        {
            var phieuMuonList = new List<PhieuMuon>
        {
            // Recent borrowing records (last 3 months)
            new PhieuMuon { MaDocGia = 1, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-190)), DaXoa = false },
                new PhieuMuon { MaDocGia = 2, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-185)), DaXoa = false },
                new PhieuMuon { MaDocGia = 3, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-180)), DaXoa = false },
                new PhieuMuon { MaDocGia = 1, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-175)), DaXoa = false },
                new PhieuMuon { MaDocGia = 2, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-170)), DaXoa = false },
                new PhieuMuon { MaDocGia = 3, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-165)), DaXoa = false },
                new PhieuMuon { MaDocGia = 1, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-60)), DaXoa = false },
                new PhieuMuon { MaDocGia = 2, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-55)), DaXoa = false },
                new PhieuMuon { MaDocGia = 3, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-150)), DaXoa = false },
                new PhieuMuon { MaDocGia = 1, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)), DaXoa = false },
                new PhieuMuon { MaDocGia = 2, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)), DaXoa = false },
                new PhieuMuon { MaDocGia = 3, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-135)), DaXoa = false },
                new PhieuMuon { MaDocGia = 1, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), DaXoa = false },
                new PhieuMuon { MaDocGia = 2, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)), DaXoa = false },
                new PhieuMuon { MaDocGia = 3, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-120)), DaXoa = false },
                new PhieuMuon { MaDocGia = 1, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)), DaXoa = false },
                new PhieuMuon { MaDocGia = 2, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), DaXoa = false },
                new PhieuMuon { MaDocGia = 3, MaNhanVien = 1, NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)), DaXoa = false },
        };

            context.DsPhieuMuon.AddRange(phieuMuonList);
        }
    }

    private async Task EnsureCreateChiTietPhieuMuonAsync(DatabaseContext context)
    {
        if (!await context.DsChiTietPhieuMuon.AnyAsync())
        {
            var chiTietPhieuMuonList = new List<ChiTietPhieuMuon>
        {
            // Details for PhieuMuon 1 - Mix of genres to show borrowing statistics
            new ChiTietPhieuMuon { MaPhieuMuon = 1, MaSach = 1 }, // Genre A
            new ChiTietPhieuMuon { MaPhieuMuon = 1, MaSach = 4 }, // Genre A
            
            // Details for PhieuMuon 2
            new ChiTietPhieuMuon { MaPhieuMuon = 2, MaSach = 2 }, // Genre B
            new ChiTietPhieuMuon { MaPhieuMuon = 2, MaSach = 5 }, // Genre B
            
            // Details for PhieuMuon 3
            new ChiTietPhieuMuon { MaPhieuMuon = 3, MaSach = 3 }, // Genre C
            new ChiTietPhieuMuon { MaPhieuMuon = 3, MaSach = 6 }, // Genre C
            
            // Details for PhieuMuon 4
            new ChiTietPhieuMuon { MaPhieuMuon = 4, MaSach = 7 }, // Genre A
            new ChiTietPhieuMuon { MaPhieuMuon = 4, MaSach = 10 }, // Genre A
            
            // Details for PhieuMuon 5
            new ChiTietPhieuMuon { MaPhieuMuon = 5, MaSach = 8 }, // Genre B
            new ChiTietPhieuMuon { MaPhieuMuon = 5, MaSach = 11 }, // Genre B
            
            // Details for PhieuMuon 6
            new ChiTietPhieuMuon { MaPhieuMuon = 6, MaSach = 9 }, // Genre C
            new ChiTietPhieuMuon { MaPhieuMuon = 6, MaSach = 12 }, // Genre C
            
            // Details for PhieuMuon 7
            new ChiTietPhieuMuon { MaPhieuMuon = 7, MaSach = 13 }, // Genre A
            new ChiTietPhieuMuon { MaPhieuMuon = 7, MaSach = 1 }, // Genre A
            
            // Details for PhieuMuon 8
            new ChiTietPhieuMuon { MaPhieuMuon = 8, MaSach = 14 }, // Genre B
            new ChiTietPhieuMuon { MaPhieuMuon = 8, MaSach = 2 }, // Genre B
            
            // Details for PhieuMuon 9
            new ChiTietPhieuMuon { MaPhieuMuon = 9, MaSach = 15 }, // Genre C
            new ChiTietPhieuMuon { MaPhieuMuon = 9, MaSach = 3 }, // Genre C
            
            // Details for remaining PhieuMuon records
            new ChiTietPhieuMuon { MaPhieuMuon = 10, MaSach = 4 }, // Genre A
            new ChiTietPhieuMuon { MaPhieuMuon = 11, MaSach = 5 }, // Genre B
            new ChiTietPhieuMuon { MaPhieuMuon = 12, MaSach = 6 }, // Genre C
            new ChiTietPhieuMuon { MaPhieuMuon = 13, MaSach = 7 }, // Genre A
            new ChiTietPhieuMuon { MaPhieuMuon = 14, MaSach = 8 }, // Genre B
            new ChiTietPhieuMuon { MaPhieuMuon = 15, MaSach = 9 }, // Genre C
            new ChiTietPhieuMuon { MaPhieuMuon = 16, MaSach = 10 }, // Genre A
            new ChiTietPhieuMuon { MaPhieuMuon = 17, MaSach = 11 }, // Genre B
            new ChiTietPhieuMuon { MaPhieuMuon = 18, MaSach = 12 }, // Genre C
        };

            context.DsChiTietPhieuMuon.AddRange(chiTietPhieuMuonList);
        }
    }

    private async Task EnsureCreatePhieuTraAsync(DatabaseContext context)
    {
        if (!await context.DsPhieuTra.AnyAsync())
        {
            var phieuTraList = new List<PhieuTra>
        {
            // Some books returned on time (no fines)
            new PhieuTra { MaDocGia = 1, MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-75)), TienPhatKyNay = 0, DaXoa = false },
            new PhieuTra {MaDocGia = 2,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-70)), TienPhatKyNay = 0, DaXoa = false },
            new PhieuTra {MaDocGia = 3, MaNhanVien = 1,NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-65)), TienPhatKyNay = 0, DaXoa = false },
            
            // Some books returned late (with fines) - for revenue statistics
            new PhieuTra {MaDocGia = 3, MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-60)), TienPhatKyNay = 15000, DaXoa = false },
            new PhieuTra {MaDocGia = 1,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-55)), TienPhatKyNay = 25000, DaXoa = false },
            new PhieuTra {MaDocGia = 2,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-50)), TienPhatKyNay = 10000, DaXoa = false },
            new PhieuTra {MaDocGia = 1,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)), TienPhatKyNay = 30000, DaXoa = false },
            new PhieuTra {MaDocGia = 2,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)), TienPhatKyNay = 20000, DaXoa = false },
            new PhieuTra {MaDocGia = 3,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-35)), TienPhatKyNay = 12000, DaXoa = false },
            new PhieuTra {MaDocGia = 2,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), TienPhatKyNay = 18000, DaXoa = false },
            new PhieuTra { MaDocGia = 1,MaNhanVien = 1,NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)), TienPhatKyNay = 22000, DaXoa = false },
            new PhieuTra {MaDocGia = 3,MaNhanVien = 1, NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)), TienPhatKyNay = 8000, DaXoa = false },
            new PhieuTra { MaDocGia = 1,MaNhanVien = 1,NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)), TienPhatKyNay = 35000, DaXoa = false },
            new PhieuTra { MaDocGia = 3,MaNhanVien = 1,NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), TienPhatKyNay = 16000, DaXoa = false },
            new PhieuTra {MaDocGia = 2, MaNhanVien = 1,NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)), TienPhatKyNay = 28000, DaXoa = false },
        };

            context.DsPhieuTra.AddRange(phieuTraList);
        }
    }

    private async Task EnsureCreateChiTietPhieuTraAsync(DatabaseContext context)
    {
        if (!await context.DsChiTietPhieuTra.AnyAsync())
        {
            var chiTietPhieuTraList = new List<ChiTietPhieuTra>
        {
            // Returns with no fines (returned on time)
            new ChiTietPhieuTra { MaPhieuTra = 1, MaPhieuMuon = 1, MaSach = 1, TienPhat = 0, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 1, MaPhieuMuon = 1, MaSach = 4, TienPhat = 0, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 2, MaPhieuMuon = 2, MaSach = 2, TienPhat = 0, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 2, MaPhieuMuon = 2, MaSach = 5, TienPhat = 0, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 3, MaPhieuMuon = 3, MaSach = 3, TienPhat = 0, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 3, MaPhieuMuon = 3, MaSach = 6, TienPhat = 0, DaXoa = false },
            
            // Returns with fines (returned late) - for late return statistics
            new ChiTietPhieuTra { MaPhieuTra = 4, MaPhieuMuon = 4, MaSach = 7, TienPhat = 15000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 5, MaPhieuMuon = 5, MaSach = 8, TienPhat = 25000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 6, MaPhieuMuon = 6, MaSach = 9, TienPhat = 10000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 7, MaPhieuMuon = 7, MaSach = 13, TienPhat = 30000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 8, MaPhieuMuon = 8, MaSach = 14, TienPhat = 20000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 9, MaPhieuMuon = 9, MaSach = 15, TienPhat = 12000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 10, MaPhieuMuon = 10, MaSach = 4, TienPhat = 18000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 11, MaPhieuMuon = 11, MaSach = 5, TienPhat = 22000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 12, MaPhieuMuon = 12, MaSach = 6, TienPhat = 8000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 13, MaPhieuMuon = 13, MaSach = 7, TienPhat = 35000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 14, MaPhieuMuon = 14, MaSach = 8, TienPhat = 16000, DaXoa = false },
            new ChiTietPhieuTra { MaPhieuTra = 15, MaPhieuMuon = 15, MaSach = 9, TienPhat = 28000, DaXoa = false },
        };

            context.DsChiTietPhieuTra.AddRange(chiTietPhieuTraList);
        }
    }
}
