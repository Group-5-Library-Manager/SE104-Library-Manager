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
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            // Act
            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

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
            var banSaoList = await CreateMultipleTestBanSao(6); // More than allowed (5)

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _phieuMuonRepository.AddAsync(phieuMuon, banSaoList));
        }

        [TestMethod]
        public async Task AddAsync_UnavailableBookCopy_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Make book copy unavailable
            banSao.TinhTrang = "Đã mượn";
            DbContext.Update(banSao);
            await DbContext.SaveChangesAsync();

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies));
        }

        [TestMethod]
        public async Task AddAsync_ReaderWithOverdueBooks_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create an overdue borrow receipt
            var overduePhieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), // Overdue
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var overdueSelectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(overduePhieuMuon, overdueSelectedCopies);

            // Try to create another borrow receipt
            var newSach = await CreateTestSach();
            var newBanSao = await CreateTestBanSao(newSach.MaSach);
            var newPhieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var newSelectedCopies = new List<BanSaoSach> { newBanSao };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _phieuMuonRepository.AddAsync(newPhieuMuon, newSelectedCopies));
        }

        [TestMethod]
        public async Task DeleteAsync_PhieuMuonWithReturnedBooks_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

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
                new ChiTietPhieuTra { MaBanSao = banSao.MaBanSao, MaPhieuTra = phieuTra.MaPhieuTra, MaPhieuMuon = phieuMuon.MaPhieuMuon }
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
            var banSao1 = await CreateTestBanSao(sach1.MaSach);
            var banSao2 = await CreateTestBanSao(sach2.MaSach);

            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies1 = new List<BanSaoSach> { banSao1 };
            var selectedCopies2 = new List<BanSaoSach> { banSao2 };

            await _phieuMuonRepository.AddAsync(phieuMuon1, selectedCopies1);
            await _phieuMuonRepository.AddAsync(phieuMuon2, selectedCopies2);

            // Act
            var result = await _phieuMuonRepository.GetAllAsync();

            // Assert
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Should().OnlyContain(pm => !pm.DaXoa);
        }

        [TestMethod]
        public async Task GetByReaderIdAsync_ShouldReturnReaderPhieuMuon()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Act
            var result = await _phieuMuonRepository.GetByReaderIdAsync(docGia.MaDocGia);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(pm => pm.MaDocGia == docGia.MaDocGia);
        }

        [TestMethod]
        public async Task GetOverdueBooksAsync_ShouldReturnOverdueBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var overduePhieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), // Overdue
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(overduePhieuMuon, selectedCopies);

            // Act
            var result = await _phieuMuonRepository.GetOverdueBooksAsync(docGia.MaDocGia);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(pm => pm.MaDocGia == docGia.MaDocGia);
        }

        [TestMethod]
        public async Task HasOverdueBooksAsync_WithOverdueBooks_ShouldReturnTrue()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var overduePhieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), // Overdue
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(overduePhieuMuon, selectedCopies);

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
            var banSao = await CreateTestBanSao(sach.MaSach);

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
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

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
            var banSao1 = await CreateTestBanSao(sach1.MaSach);
            var banSao2 = await CreateTestBanSao(sach2.MaSach);

            // Borrow one book
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao1 };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Act
            var result = await _phieuMuonRepository.GetAvailableBooksAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().NotContain(s => s.MaSach == sach1.MaSach);
        }

        [TestMethod]
        public async Task UpdateAsync_ValidPhieuMuon_ShouldUpdateSuccessfully()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Update the borrow receipt
            phieuMuon.NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var newSach = await CreateTestSach();
            var newBanSao = await CreateTestBanSao(newSach.MaSach);
            var newSelectedCopies = new List<BanSaoSach> { newBanSao };

            // Act
            await _phieuMuonRepository.UpdateAsync(phieuMuon, newSelectedCopies);

            // Assert
            var result = await _phieuMuonRepository.GetByIdAsync(phieuMuon.MaPhieuMuon);
            result.Should().NotBeNull();
            result.NgayMuon.Should().Be(phieuMuon.NgayMuon);
        }

        [TestMethod]
        public async Task UpdateAsync_PhieuMuonWithReturnedBooks_ShouldThrowException()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Create a return receipt for this borrow receipt
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaNhanVien = nhanVien.MaNhanVien,
                MaDocGia = docGia.MaDocGia,
            };
            DbContext.Add(phieuTra);
            await DbContext.SaveChangesAsync();

            var chiTietPhieuTra = new List<ChiTietPhieuTra>
            {
                new ChiTietPhieuTra { MaBanSao = banSao.MaBanSao, MaPhieuTra = phieuTra.MaPhieuTra, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };
            DbContext.AddRange(chiTietPhieuTra);
            await DbContext.SaveChangesAsync();

            // Try to update the borrow receipt
            var newSach = await CreateTestSach();
            var newBanSao = await CreateTestBanSao(newSach.MaSach);
            var newSelectedCopies = new List<BanSaoSach> { newBanSao };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _phieuMuonRepository.UpdateAsync(phieuMuon, newSelectedCopies));
        }

        [TestMethod]
        public async Task GetAllOverdueBooksAsync_ShouldReturnAllOverdueBooks()
        {
            // Arrange
            var docGia1 = await CreateTestDocGia();
            var docGia2 = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();
            var banSao1 = await CreateTestBanSao(sach1.MaSach);
            var banSao2 = await CreateTestBanSao(sach2.MaSach);

            var overduePhieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                MaDocGia = docGia1.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var overduePhieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                MaDocGia = docGia2.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies1 = new List<BanSaoSach> { banSao1 };
            var selectedCopies2 = new List<BanSaoSach> { banSao2 };

            await _phieuMuonRepository.AddAsync(overduePhieuMuon1, selectedCopies1);
            await _phieuMuonRepository.AddAsync(overduePhieuMuon2, selectedCopies2);

            // Act
            var result = await _phieuMuonRepository.GetAllOverdueBooksAsync();

            // Assert
            result.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [TestMethod]
        public async Task HasReturnedBooksAsync_WithReturnedBooks_ShouldReturnTrue()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Create a return receipt
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaNhanVien = nhanVien.MaNhanVien,
                MaDocGia = docGia.MaDocGia,
            };
            DbContext.Add(phieuTra);
            await DbContext.SaveChangesAsync();

            var chiTietPhieuTra = new List<ChiTietPhieuTra>
            {
                new ChiTietPhieuTra { MaBanSao = banSao.MaBanSao, MaPhieuTra = phieuTra.MaPhieuTra, MaPhieuMuon = phieuMuon.MaPhieuMuon }
            };
            DbContext.AddRange(chiTietPhieuTra);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _phieuMuonRepository.HasReturnedBooksAsync(phieuMuon.MaPhieuMuon);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAvailableBanSaoSach_ShouldReturnAvailableCopies()
        {
            // Arrange
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Act
            var result = _phieuMuonRepository.GetAvailableBanSaoSach();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(bs => bs.TinhTrang == "Có sẵn");
        }

        [TestMethod]
        public async Task GetAllBanSaoSach_ShouldReturnAllCopies()
        {
            // Arrange
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Act
            var result = _phieuMuonRepository.GetAllBanSaoSach();

            // Assert
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task UpdateBookStatusAsync_ShouldUpdateBookStatus()
        {
            // Arrange
            var sach = await CreateTestSach();

            // Act
            await _phieuMuonRepository.UpdateBookStatusAsync(sach.MaSach, "Đã mượn");

            // Assert
            var updatedSach = await _phieuMuonRepository.GetBookByIdAsync(sach.MaSach);
            updatedSach.Should().NotBeNull();
            updatedSach.TrangThai.Should().Be("Đã mượn");
        }

        [TestMethod]
        public async Task GetBookByIdAsync_ExistingBook_ShouldReturnBook()
        {
            // Arrange
            var sach = await CreateTestSach();

            // Act
            var result = await _phieuMuonRepository.GetBookByIdAsync(sach.MaSach);

            // Assert
            result.Should().NotBeNull();
            result.MaSach.Should().Be(sach.MaSach);
        }

        [TestMethod]
        public async Task GetBookByIdAsync_NonExistentBook_ShouldReturnNull()
        {
            // Act
            var result = await _phieuMuonRepository.GetBookByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetAllBooksAsync_ShouldReturnAllBooks()
        {
            // Arrange
            var sach1 = await CreateTestSach();
            var sach2 = await CreateTestSach();

            // Act
            var result = await _phieuMuonRepository.GetAllBooksAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.MaSach == sach1.MaSach);
            result.Should().Contain(s => s.MaSach == sach2.MaSach);
        }

        // Helper methods
        private async Task<DocGia> CreateTestDocGia()
        {
            var loaiDocGia = new LoaiDocGia
            {
                TenLoaiDocGia = "Sinh viên"
            };
            DbContext.Add(loaiDocGia);
            await DbContext.SaveChangesAsync();

            var docGia = new DocGia
            {
                TenDocGia = "Nguyễn Văn A",
                DiaChi = "Hà Nội",
                MaLoaiDocGia = loaiDocGia.MaLoaiDocGia,
                NgaySinh = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };
            DbContext.Add(docGia);
            await DbContext.SaveChangesAsync();
            return docGia;
        }

        private async Task<NhanVien> CreateTestNhanVien()
        {
            var boPhan = new BoPhan
            {
                TenBoPhan = "Thư viện"
            };
            DbContext.Add(boPhan);
            await DbContext.SaveChangesAsync();

            var chucVu = new ChucVu
            {
                TenChucVu = "Nhân viên"
            };
            DbContext.Add(chucVu);
            await DbContext.SaveChangesAsync();

            var bangCap = new BangCap
            {
                TenBangCap = "Đại học"
            };
            DbContext.Add(bangCap);
            await DbContext.SaveChangesAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Nguyễn Văn B",
                DiaChi = "Hà Nội",
                DienThoai = "0987654321",
                NgaySinh = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                MaBoPhan = boPhan.MaBoPhan,
                MaChucVu = chucVu.MaChucVu,
                MaBangCap = bangCap.MaBangCap
            };
            DbContext.Add(nhanVien);
            await DbContext.SaveChangesAsync();
            return nhanVien;
        }

        private async Task<Sach> CreateTestSach()
        {
            var theLoai = new TheLoai
            {
                TenTheLoai = "Khoa học"
            };
            DbContext.Add(theLoai);
            await DbContext.SaveChangesAsync();

            var tacGia = new TacGia
            {
                TenTacGia = "Tác giả A"
            };
            DbContext.Add(tacGia);
            await DbContext.SaveChangesAsync();

            var nhaXuatBan = new NhaXuatBan
            {
                TenNhaXuatBan = "NXB A"
            };
            DbContext.Add(nhaXuatBan);
            await DbContext.SaveChangesAsync();

            var sach = new Sach
            {
                TenSach = "Sách Test",
                MaTheLoai = theLoai.MaTheLoai,
                MaTacGia = tacGia.MaTacGia,
                MaNhaXuatBan = nhaXuatBan.MaNhaXuatBan,
                NamXuatBan = 2023,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 100000,
                TrangThai = "Còn sách",
                SoLuongHienCo = 5,
                SoLuongTong = 5
            };
            DbContext.Add(sach);
            await DbContext.SaveChangesAsync();
            return sach;
        }

        private async Task<BanSaoSach> CreateTestBanSao(int maSach)
        {
            var banSao = new BanSaoSach
            {
                MaSach = maSach,
                TinhTrang = "Có sẵn"
            };
            DbContext.Add(banSao);
            await DbContext.SaveChangesAsync();
            return banSao;
        }

        private async Task<List<BanSaoSach>> CreateMultipleTestBanSao(int count)
        {
            var sach = await CreateTestSach();
            var banSaoList = new List<BanSaoSach>();

            for (int i = 0; i < count; i++)
            {
                var banSao = new BanSaoSach
                {
                    MaSach = sach.MaSach,
                    TinhTrang = "Có sẵn"
                };
                banSaoList.Add(banSao);
            }

            DbContext.AddRange(banSaoList);
            await DbContext.SaveChangesAsync();
            return banSaoList;
        }
    }
}