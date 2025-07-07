using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Tests.Repositories
{
    [TestClass]
    public class PhieuTraRepositoryTests : TestBase
    {
        private IPhieuTraRepository _phieuTraRepository;
        private IPhieuMuonRepository _phieuMuonRepository;
        private IDocGiaRepository _docGiaRepository;
        private INhanVienRepository _nhanVienRepository;
        private ISachRepository _sachRepository;
        private IQuyDinhRepository _quyDinhRepository;
        private ITheLoaiRepository _theLoaiRepository;
        private ITacGiaRepository _tacGiaRepository;
        private INhaXuatBanRepository _nhaXuatBanRepository;
        private ILoaiDocGiaRepository _loaiDocGiaRepository;
        private IBangCapRepository _bangCapRepository;
        private IBoPhanRepository _boPhanRepository;
        private IChucVuRepository _chucVuRepository;
        private IChiTietPhieuTraRepository _chiTietPhieuTraRepository;

        [TestInitialize]
        public void Initialize()
        {
            _phieuTraRepository = ServiceProvider.GetRequiredService<IPhieuTraRepository>();
            _phieuMuonRepository = ServiceProvider.GetRequiredService<IPhieuMuonRepository>();
            _docGiaRepository = ServiceProvider.GetRequiredService<IDocGiaRepository>();
            _nhanVienRepository = ServiceProvider.GetRequiredService<INhanVienRepository>();
            _sachRepository = ServiceProvider.GetRequiredService<ISachRepository>();
            _quyDinhRepository = ServiceProvider.GetRequiredService<IQuyDinhRepository>();
            _theLoaiRepository = ServiceProvider.GetRequiredService<ITheLoaiRepository>();
            _tacGiaRepository = ServiceProvider.GetRequiredService<ITacGiaRepository>();
            _nhaXuatBanRepository = ServiceProvider.GetRequiredService<INhaXuatBanRepository>();
            _loaiDocGiaRepository = ServiceProvider.GetRequiredService<ILoaiDocGiaRepository>();
            _bangCapRepository = ServiceProvider.GetRequiredService<IBangCapRepository>();
            _boPhanRepository = ServiceProvider.GetRequiredService<IBoPhanRepository>();
            _chucVuRepository = ServiceProvider.GetRequiredService<IChucVuRepository>();
            _chiTietPhieuTraRepository = ServiceProvider.GetRequiredService<IChiTietPhieuTraRepository>();

            // Seed basic data
            SeedBasicData();
        }

        [TestMethod]
        public async Task AddAsync_ValidPhieuTra_ShouldAddSuccessfully()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            // Act
            await _phieuTraRepository.AddAsync(phieuTra);

            // Assert
            var result = await _phieuTraRepository.GetByIdAsync(phieuTra.MaPhieuTra);
            result.Should().NotBeNull();
            result.MaDocGia.Should().Be(docGia.MaDocGia);
            result.MaNhanVien.Should().Be(nhanVien.MaNhanVien);
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingPhieuTra_ShouldReturnPhieuTra()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            await _phieuTraRepository.AddAsync(phieuTra);

            // Act
            var result = await _phieuTraRepository.GetByIdAsync(phieuTra.MaPhieuTra);

            // Assert
            result.Should().NotBeNull();
            result.MaPhieuTra.Should().Be(phieuTra.MaPhieuTra);
            result.DocGia.Should().NotBeNull();
            result.NhanVien.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistentPhieuTra_ShouldReturnNull()
        {
            // Act
            var result = await _phieuTraRepository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllNonDeletedPhieuTra()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            // Create borrow receipts
            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };

            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach2.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTietPhieuMuon1);
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTietPhieuMuon2);

            // Create return receipts
            var phieuTra1 = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            var phieuTra2 = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 1000
            };

            await _phieuTraRepository.AddAsync(phieuTra1);
            await _phieuTraRepository.AddAsync(phieuTra2);

            // Act
            var result = await _phieuTraRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(pt => !pt.DaXoa);
        }

        [TestMethod]
        public async Task UpdateAsync_ExistingPhieuTra_ShouldUpdateSuccessfully()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            await _phieuTraRepository.AddAsync(phieuTra);

            // Update the return receipt
            phieuTra.TienPhatKyNay = 1500;
            phieuTra.NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

            // Act
            await _phieuTraRepository.UpdateAsync(phieuTra);

            // Assert
            var result = await _phieuTraRepository.GetByIdAsync(phieuTra.MaPhieuTra);
            result.Should().NotBeNull();
            result.TienPhatKyNay.Should().Be(1500);
            result.NgayTra.Should().Be(DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
        }

        [TestMethod]
        public async Task UpdateAsync_NonExistentPhieuTra_ShouldThrowException()
        {
            // Arrange
            var phieuTra = new PhieuTra
            {
                MaPhieuTra = 999,
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = 1,
                MaNhanVien = 1,
                TienPhatKyNay = 0
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _phieuTraRepository.UpdateAsync(phieuTra));
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingPhieuTra_ShouldMarkAsDeleted()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            await _phieuTraRepository.AddAsync(phieuTra);

            // Act
            await _phieuTraRepository.DeleteAsync(phieuTra.MaPhieuTra);

            // Assert
            var result = await _phieuTraRepository.GetByIdAsync(phieuTra.MaPhieuTra);
            result.Should().BeNull(); // Should be null because it's marked as deleted
        }

        [TestMethod]
        public async Task GetDocGiaDangCoSachMuonAsync_ShouldReturnReadersWithBorrowedBooks()
        {
            // Arrange
            var docGia1 = await CreateTestDocGia();
            var docGia2 = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            // Create borrow receipts for both readers
            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                MaDocGia = docGia1.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };

            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                MaDocGia = docGia2.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach2.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTietPhieuMuon1);
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTietPhieuMuon2);

            // Act
            var result = await _phieuTraRepository.GetDocGiaDangCoSachMuonAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(dg => dg.MaDocGia == docGia1.MaDocGia);
            result.Should().Contain(dg => dg.MaDocGia == docGia2.MaDocGia);
        }

        [TestMethod]
        public async Task GetSachDangMuonByDocGiaAsync_ValidDocGia_ShouldReturnBorrowedBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            // Create first borrow
            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };
            var chiTietPhieuMuon1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };
            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTietPhieuMuon1);

            // Return the first book
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };
            await _phieuTraRepository.AddAsync(phieuTra);
            var chiTietPhieuTra = new ChiTietPhieuTra
            {
                MaSach = sach1.MaSach,
                MaPhieuTra = phieuTra.MaPhieuTra,
                MaPhieuMuon = phieuMuon1.MaPhieuMuon
            };
            DbContext.Add(chiTietPhieuTra);
            await DbContext.SaveChangesAsync();

            // Now create the second borrow
            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };
            var chiTietPhieuMuon2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach2.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTietPhieuMuon2);

            // Act
            var result = await _phieuTraRepository.GetSachDangMuonByDocGiaAsync(docGia.MaDocGia);

            // Assert
            result.Should().HaveCount(1);
            result.Should().OnlyContain(ct => ct.PhieuMuon.MaDocGia == docGia.MaDocGia);
        }

        [TestMethod]
        public async Task GetSachDangMuonByDocGiaAsync_NonExistentDocGia_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                _phieuTraRepository.GetSachDangMuonByDocGiaAsync(999));
        }

        [TestMethod]
        public async Task GetChiTietMuonMoiNhatChuaTraAsync_BookWithBorrowHistory_ShouldReturnLatestBorrow()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Create first borrow
            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };
            var chiTietPhieuMuon1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };
            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTietPhieuMuon1);

            // Return the first borrow
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };
            await _phieuTraRepository.AddAsync(phieuTra);
            var chiTietPhieuTra = new ChiTietPhieuTra
            {
                MaSach = sach.MaSach,
                MaPhieuTra = phieuTra.MaPhieuTra,
                MaPhieuMuon = phieuMuon1.MaPhieuMuon
            };
            
            // Use the repository to add the return detail which will update book status
            await _chiTietPhieuTraRepository.AddAsync(chiTietPhieuTra);

            // Now create the second borrow
            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };
            var chiTietPhieuMuon2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTietPhieuMuon2);

            // Act
            var result = await _phieuTraRepository.GetChiTietMuonMoiNhatChuaTraAsync(sach.MaSach);

            // Assert
            result.Should().NotBeNull();
            result.MaSach.Should().Be(sach.MaSach);
            result.PhieuMuon.NgayMuon.Should().Be(DateOnly.FromDateTime(DateTime.Now)); // Latest borrow
        }

        [TestMethod]
        public async Task GetChiTietMuonMoiNhatChuaTraAsync_BookNotBorrowed_ShouldReturnNull()
        {
            // Arrange
            var sach = await CreateTestSach();

            // Act
            var result = await _phieuTraRepository.GetChiTietMuonMoiNhatChuaTraAsync(sach.MaSach);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetDocGiaDangCoSachMuonAsync_WithReturnedBooks_ShouldExcludeReturnedBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            // Create first borrow
            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };
            var chiTietPhieuMuon1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };
            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTietPhieuMuon1);

            // Return the first book
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };
            await _phieuTraRepository.AddAsync(phieuTra);
            var chiTietPhieuTra = new ChiTietPhieuTra
            {
                MaSach = sach1.MaSach,
                MaPhieuTra = phieuTra.MaPhieuTra,
                MaPhieuMuon = phieuMuon1.MaPhieuMuon
            };
            // Use the repository to add the return detail which will update book status
            await _chiTietPhieuTraRepository.AddAsync(chiTietPhieuTra);

            // Now create the second borrow
            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };
            var chiTietPhieuMuon2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach2.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTietPhieuMuon2);

            // Act
            var result = await _phieuTraRepository.GetDocGiaDangCoSachMuonAsync();

            // Assert
            result.Should().HaveCount(1); // Should still return the reader because they still have one book borrowed
            result.Should().Contain(dg => dg.MaDocGia == docGia.MaDocGia);
        }

        // Helper methods to create test data
        private async Task<DocGia> CreateTestDocGia()
        {
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = $"Test Reader {Guid.NewGuid().ToString().Substring(0, 8)}",
                DiaChi = "Test Address",
                Email = $"test{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(1990, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            await _docGiaRepository.AddAsync(docGia);
            return docGia;
        }

        private async Task<NhanVien> CreateTestNhanVien()
        {
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Test Staff",
                DiaChi = "Test Address",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1985, 1, 1),
                MaChucVu = chucVu[0].MaChucVu,
                MaBangCap = bangCap[0].MaBangCap,
                MaBoPhan = boPhan[0].MaBoPhan
            };

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = "testuser",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = nhanVien.MaNhanVien,
                MaVaiTro = 1
            };

            await _nhanVienRepository.AddAsync(nhanVien, taiKhoan);
            return nhanVien;
        }

        private async Task<Sach> CreateTestSach()
        {
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Test Book",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            await _sachRepository.AddAsync(sach);
            return sach;
        }
    }
} 