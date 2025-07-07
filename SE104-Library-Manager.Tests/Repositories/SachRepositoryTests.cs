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
    public class SachRepositoryTests : TestBase
    {
        private ISachRepository _sachRepository;
        private IQuyDinhRepository _quyDinhRepository;
        private ITheLoaiRepository _theLoaiRepository;
        private ITacGiaRepository _tacGiaRepository;
        private INhaXuatBanRepository _nhaXuatBanRepository;

        [TestInitialize]
        public void Initialize()
        {
            _sachRepository = ServiceProvider.GetRequiredService<ISachRepository>();
            _quyDinhRepository = ServiceProvider.GetRequiredService<IQuyDinhRepository>();
            _theLoaiRepository = ServiceProvider.GetRequiredService<ITheLoaiRepository>();
            _tacGiaRepository = ServiceProvider.GetRequiredService<ITacGiaRepository>();
            _nhaXuatBanRepository = ServiceProvider.GetRequiredService<INhaXuatBanRepository>();

            // Seed basic data
            SeedBasicData();
        }

        [TestMethod]
        public async Task AddAsync_ValidSach_ShouldAddSuccessfully()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Truyện Kiều",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            // Act
            await _sachRepository.AddAsync(sach);

            // Assert
            var result = await _sachRepository.GetByIdAsync(sach.MaSach);
            result.Should().NotBeNull();
            result.TenSach.Should().Be("Truyện Kiều");
            result.TrangThai.Should().Be("Có sẵn");
        }

        [TestMethod]
        public async Task AddAsync_EmptyBookName_ShouldThrowException()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "", // Empty book name
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _sachRepository.AddAsync(sach));
        }

        [TestMethod]
        public async Task AddAsync_InvalidPublicationYear_ShouldThrowException()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Old Book",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2010, // Too old (more than 8 years)
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _sachRepository.AddAsync(sach));
        }

        [TestMethod]
        public async Task AddAsync_NegativeValue_ShouldThrowException()
        {
            // Arrange
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
                TriGia = -1000, // Negative value
                TrangThai = "Có sẵn"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _sachRepository.AddAsync(sach));
        }

        [TestMethod]
        public async Task UpdateAsync_ValidSach_ShouldUpdateSuccessfully()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Original Book",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            await _sachRepository.AddAsync(sach);

            // Act
            sach.TenSach = "Updated Book";
            sach.TriGia = 75000;
            await _sachRepository.UpdateAsync(sach);

            // Assert
            var result = await _sachRepository.GetByIdAsync(sach.MaSach);
            result.Should().NotBeNull();
            result.TenSach.Should().Be("Updated Book");
            result.TriGia.Should().Be(75000);
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingSach_ShouldMarkAsDeleted()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Book to Delete",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            await _sachRepository.AddAsync(sach);

            // Act
            await _sachRepository.DeleteAsync(sach.MaSach);

            // Assert
            var result = await _sachRepository.GetByIdAsync(sach.MaSach);
            result.Should().BeNull(); // Should be null because it's marked as deleted
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllNonDeletedSach()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach1 = new Sach
            {
                TenSach = "Book 1",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            var sach2 = new Sach
            {
                TenSach = "Book 2",
                MaTheLoai = theLoai[1].MaTheLoai,
                MaTacGia = tacGia[1].MaTacGia,
                NamXuatBan = 2021,
                MaNhaXuatBan = nhaXuatBan[1].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 75000,
                TrangThai = "Đã mượn"
            };

            await _sachRepository.AddAsync(sach1);
            await _sachRepository.AddAsync(sach2);

            // Act
            var result = await _sachRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.TenSach == "Book 1");
            result.Should().Contain(s => s.TenSach == "Book 2");
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingSach_ShouldReturnSach()
        {
            // Arrange
            var theLoai = await _theLoaiRepository.GetAllAsync();
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Specific Book",
                MaTheLoai = theLoai[0].MaTheLoai,
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            await _sachRepository.AddAsync(sach);

            // Act
            var result = await _sachRepository.GetByIdAsync(sach.MaSach);

            // Assert
            result.Should().NotBeNull();
            result.TenSach.Should().Be("Specific Book");
            result.TheLoai.Should().NotBeNull();
            result.TacGia.Should().NotBeNull();
            result.NhaXuatBan.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistentSach_ShouldReturnNull()
        {
            // Act
            var result = await _sachRepository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task ValidateSach_NullSach_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _sachRepository.AddAsync(null));
        }

        [TestMethod]
        public async Task ValidateSach_InvalidTheLoai_ShouldThrowException()
        {
            // Arrange
            var tacGia = await _tacGiaRepository.GetAllAsync();
            var nhaXuatBan = await _nhaXuatBanRepository.GetAllAsync();

            var sach = new Sach
            {
                TenSach = "Test Book",
                MaTheLoai = -1, // Invalid TheLoai ID
                MaTacGia = tacGia[0].MaTacGia,
                NamXuatBan = 2020,
                MaNhaXuatBan = nhaXuatBan[0].MaNhaXuatBan,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = 50000,
                TrangThai = "Có sẵn"
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _sachRepository.AddAsync(sach));
        }

        [TestMethod]
        public async Task ValidateSach_EmptyTrangThai_ShouldThrowException()
        {
            // Arrange
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
                TrangThai = "" // Empty status
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _sachRepository.AddAsync(sach));
        }
    }
} 