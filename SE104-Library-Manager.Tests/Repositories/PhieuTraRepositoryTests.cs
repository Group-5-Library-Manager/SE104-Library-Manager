using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Repositories;
using SE104_Library_Manager.ViewModels.Return;
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
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            var chiTietPhieuTra = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaBanSao = banSao.MaBanSao,
                    TienPhat = 0
                }
            };

            // Act
            await _phieuTraRepository.AddAsync(phieuTra, chiTietPhieuTra);

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
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            var chiTietPhieuTra = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaBanSao = banSao.MaBanSao,
                    TienPhat = 0
                }
            };

            await _phieuTraRepository.AddAsync(phieuTra, chiTietPhieuTra);

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
            var banSao1 = await CreateTestBanSao(sach1.MaSach);
            var banSao2 = await CreateTestBanSao(sach2.MaSach);

            // Create borrow receipts
            var phieuMuon1 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies1 = new List<BanSaoSach> { banSao1 };

            var phieuMuon2 = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies2 = new List<BanSaoSach> { banSao2 };

            await _phieuMuonRepository.AddAsync(phieuMuon1, selectedCopies1);
            await _phieuMuonRepository.AddAsync(phieuMuon2, selectedCopies2);

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
                TienPhatKyNay = 0
            };

            var chiTietPhieuTra1 = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon1.MaPhieuMuon,
                    MaBanSao = banSao1.MaBanSao,
                    TienPhat = 0
                }
            };

            var chiTietPhieuTra2 = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon2.MaPhieuMuon,
                    MaBanSao = banSao2.MaBanSao,
                    TienPhat = 0
                }
            };

            await _phieuTraRepository.AddAsync(phieuTra1, chiTietPhieuTra1);
            await _phieuTraRepository.AddAsync(phieuTra2, chiTietPhieuTra2);

            // Act
            var result = await _phieuTraRepository.GetAllAsync();

            // Assert
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Should().OnlyContain(pt => !pt.DaXoa);
        }

        [TestMethod]
        public async Task UpdateAsync_ExistingPhieuTra_ShouldUpdateSuccessfully()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            var chiTietPhieuTra = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaBanSao = banSao.MaBanSao,
                    TienPhat = 0
                }
            };

            await _phieuTraRepository.AddAsync(phieuTra, chiTietPhieuTra);

            // Update the return receipt
            phieuTra.NgayTra = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            phieuTra.TienPhatKyNay = 50000;

            var updatedChiTietPhieuTra = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaBanSao = banSao.MaBanSao,
                    TienPhat = 50000
                }
            };

            // Act
            await _phieuTraRepository.UpdateAsync(phieuTra, updatedChiTietPhieuTra);

            // Assert
            var result = await _phieuTraRepository.GetByIdAsync(phieuTra.MaPhieuTra);
            result.Should().NotBeNull();
            result.NgayTra.Should().Be(phieuTra.NgayTra);
            result.TienPhatKyNay.Should().Be(phieuTra.TienPhatKyNay);
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

            var chiTietPhieuTra = new List<ChiTietPhieuTraInfo>();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                _phieuTraRepository.UpdateAsync(phieuTra, chiTietPhieuTra));
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingPhieuTra_ShouldMarkAsDeleted()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt first
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            var chiTietPhieuTra = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaBanSao = banSao.MaBanSao,
                    TienPhat = 0
                }
            };

            await _phieuTraRepository.AddAsync(phieuTra, chiTietPhieuTra);

            // Act
            await _phieuTraRepository.DeleteAsync(phieuTra.MaPhieuTra);

            // Assert
            var result = await _phieuTraRepository.GetByIdAsync(phieuTra.MaPhieuTra);
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetDocGiaDangCoSachMuonAsync_ShouldReturnReadersWithBorrowedBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Act
            var result = await _phieuTraRepository.GetDocGiaDangCoSachMuonAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(dg => dg.MaDocGia == docGia.MaDocGia);
        }

        [TestMethod]
        public async Task GetBanSaoDangMuonByDocGiaAsync_ValidDocGia_ShouldReturnBorrowedBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Act
            var result = await _phieuTraRepository.GetBanSaoDangMuonByDocGiaAsync(docGia.MaDocGia);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(ct => ct.PhieuMuon.MaDocGia == docGia.MaDocGia);
        }

        [TestMethod]
        public async Task GetBanSaoDangMuonByDocGiaAsync_NonExistentDocGia_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
                _phieuTraRepository.GetBanSaoDangMuonByDocGiaAsync(999));
        }

        [TestMethod]
        public async Task GetChiTietMuonMoiNhatChuaTraAsync_BookWithBorrowHistory_ShouldReturnLatestBorrow()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Act
            var result = await _phieuTraRepository.GetChiTietMuonMoiNhatChuaTraAsync(banSao.MaBanSao);

            // Assert
            result.Should().NotBeNull();
            result.MaBanSao.Should().Be(banSao.MaBanSao);
            result.MaPhieuMuon.Should().Be(phieuMuon.MaPhieuMuon);
        }

        [TestMethod]
        public async Task GetChiTietMuonMoiNhatChuaTraAsync_BookNotBorrowed_ShouldReturnNull()
        {
            // Arrange
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Act
            var result = await _phieuTraRepository.GetChiTietMuonMoiNhatChuaTraAsync(banSao.MaBanSao);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetDocGiaDangCoSachMuonAsync_WithReturnedBooks_ShouldExcludeReturnedBooks()
        {
            // Arrange
            var docGia = await CreateTestDocGia();
            var nhanVien = await CreateTestNhanVien();
            var sach = await CreateTestSach();
            var banSao = await CreateTestBanSao(sach.MaSach);

            // Create a borrow receipt
            var phieuMuon = new PhieuMuon
            {
                NgayMuon = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien
            };

            var selectedCopies = new List<BanSaoSach> { banSao };

            await _phieuMuonRepository.AddAsync(phieuMuon, selectedCopies);

            // Create a return receipt for this borrow
            var phieuTra = new PhieuTra
            {
                NgayTra = DateOnly.FromDateTime(DateTime.Now),
                MaDocGia = docGia.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                TienPhatKyNay = 0
            };

            var chiTietPhieuTra = new List<ChiTietPhieuTraInfo>
            {
                new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaBanSao = banSao.MaBanSao,
                    TienPhat = 0
                }
            };

            await _phieuTraRepository.AddAsync(phieuTra, chiTietPhieuTra);

            // Act
            var result = await _phieuTraRepository.GetDocGiaDangCoSachMuonAsync();

            // Assert
            result.Should().NotContain(dg => dg.MaDocGia == docGia.MaDocGia);
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
                TrangThai = "Có sẵn",
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
    }
} 