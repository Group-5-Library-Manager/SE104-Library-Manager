using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;

namespace SE104_Library_Manager.Tests.Repositories
{
    [TestClass]
    public class PhieuMuonRepositoryTests : TestBase
    {
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

        [TestInitialize]
        public void Initialize()
        {
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

            // Seed basic data
            SeedBasicData();
        }

        [TestMethod]
        public async Task AddAsync_ValidPhieuMuon_ShouldAddSuccessfully()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

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

            // Act
            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            // Assert
            var result = await _phieuMuonRepository.GetByIdAsync(phieuMuon.MaPhieuMuon);
            result.Should().NotBeNull();
            result.MaDocGia.Should().Be(docGia.MaDocGia);
            result.MaNhanVien.Should().Be(nhanVien.MaNhanVien);
            result.DsChiTietPhieuMuon.Should().HaveCount(1);
        }

        [TestMethod]
        public async Task AddAsync_TooManyBooks_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sachList = await CreateMultipleTestSach(6); // More than allowed (5)

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = sachList.Select(s => new ChiTietPhieuMuon { MaSach = s.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }).ToList();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon));
        }

        [TestMethod]
        public async Task AddAsync_UnavailableBook_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Make book unavailable
            sach.TrangThai = "Đã mượn";
            await _sachRepository.UpdateAsync(sach);

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

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon));
        }

        [TestMethod]
        public async Task AddAsync_ReaderWithOverdueBooks_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Create an overdue borrow receipt
            var overduePhieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), // Overdue
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var overdueChiTiet = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = overduePhieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(overduePhieuMuon, overdueChiTiet);

            // Try to create another borrow receipt
            var newSach = await CreateTestSach();
            var newPhieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var newChiTiet = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = newSach.MaSach, MaPhieuMuon = newPhieuMuon.MaPhieuMuon }
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _phieuMuonRepository.AddAsync(newPhieuMuon, newChiTiet));
        }

        [TestMethod]
        public async Task DeleteAsync_PhieuMuonWithReturnedBooks_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

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

            // Create a return receipt for this borrow receipt
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaNhanVien = nhanVien.MaNhanVien,
                MaDocGia = docGia.MaDocGia,
            };
            DbContext.Add(phieuTra);
            await DbContext.SaveChangesAsync();

            // detail for return receipt
            var chiTietPhieuTra = new List<ChiTietPhieuTra>
            {
                new ChiTietPhieuTra { MaSach = sach.MaSach, MaPhieuTra = phieuTra.MaPhieuTra, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };
            DbContext.AddRange(chiTietPhieuTra);
            await DbContext.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _phieuMuonRepository.DeleteAsync(phieuMuon.MaPhieuMuon));
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllNonDeletedPhieuMuon()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTiet1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };

            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTiet2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach2.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTiet1);
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTiet2);

            // Act
            var result = await _phieuMuonRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task GetByReaderIdAsync_ShouldReturnReaderPhieuMuon()
        {
            // Arrange
            var docGia1 = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia1.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTiet1 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon1.MaPhieuMuon }
            };

            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia1.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTiet2 = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach2.MaSach, MaPhieuMuon = phieuMuon2.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon1, chiTiet1);
            await _phieuMuonRepository.AddAsync(phieuMuon2, chiTiet2);

            // Act
            var result = await _phieuMuonRepository.GetByReaderIdAsync(docGia1.MaDocGia);

            // Assert
            result.Should().HaveCount(2); // Should return both borrow receipts for the reader
            result.Should().OnlyContain(pm => pm.MaDocGia == docGia1.MaDocGia);
        }

        [TestMethod]
        public async Task GetOverdueBooksAsync_ShouldReturnOverdueBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), // Overdue
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            // Act
            var result = await _phieuMuonRepository.GetOverdueBooksAsync(docGia.MaDocGia);

            // Assert
            result.Should().HaveCount(1);
            result[0].MaDocGia.Should().Be(docGia.MaDocGia);
        }

        [TestMethod]
        public async Task HasOverdueBooksAsync_WithOverdueBooks_ShouldReturnTrue()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), // Overdue
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            // Act
            var result = await _phieuMuonRepository.HasOverdueBooksAsync(docGia.MaDocGia);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task IsBookAvailableAsync_AvailableBook_ShouldReturnTrue()
        {
            // Arrange
            var sach = await CreateTestSach();

            // Act
            var result = await _phieuMuonRepository.IsBookAvailableAsync(sach.MaSach);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task IsBookAvailableAsync_BorrowedBook_ShouldReturnFalse()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();

            // Borrow all available copies
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon, SoLuongMuon = sach.SoLuongHienCo }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            // Act
            var result = await _phieuMuonRepository.IsBookAvailableAsync(sach.MaSach);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task GetAvailableBooksAsync_ShouldReturnOnlyAvailableBooks()
        {
            // Arrange
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();

            // Borrow all copies of sach1
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var chiTietPhieuMuon = new List<ChiTietPhieuMuon>
            {
                new ChiTietPhieuMuon { MaSach = sach1.MaSach, MaPhieuMuon = phieuMuon.MaPhieuMuon, SoLuongMuon = sach1.SoLuongHienCo }
            };

            await _phieuMuonRepository.AddAsync(phieuMuon, chiTietPhieuMuon);

            // Act
            var result = await _phieuMuonRepository.GetAvailableBooksAsync();

            // Assert
            result.Should().Contain(s => s.MaSach == sach2.MaSach);
            result.Should().NotContain(s => s.MaSach == sach1.MaSach);
        }

        

        // Helper methods to create test data
        private async Task<DocGia> CreateTestDocGia()
        {
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "Test Reader",
                DiaChi = "Test Address",
                Email = "test@example.com",
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
            //sample account
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = "testuser",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = nhanVien.MaNhanVien,
                MaVaiTro = 1 // Assuming 1 is the role ID for staff
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
                SoLuongHienCo = 5,
                TrangThai = "Còn sách"
            };

            await _sachRepository.AddAsync(sach);
            return sach;
        }

        private async Task<List<Sach>> CreateMultipleTestSach(int count)
        {
            var sachList = new List<Sach>();
            for (int i = 0; i < count; i++)
            {
                var sach = await CreateTestSach();
                sach.TenSach = $"Test Book {i + 1}";
                await _sachRepository.UpdateAsync(sach);
                sachList.Add(sach);
            }
            return sachList;
        }
    }
}