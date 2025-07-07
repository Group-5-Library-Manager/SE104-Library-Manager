using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Tests.Repositories
{
    [TestClass]
    public class DocGiaRepositoryTests : TestBase
    {
        private IDocGiaRepository _docGiaRepository;
        private IQuyDinhRepository _quyDinhRepository;
        private ILoaiDocGiaRepository _loaiDocGiaRepository;

        [TestInitialize]
        public void Initialize()
        {
            _docGiaRepository = ServiceProvider.GetRequiredService<IDocGiaRepository>();
            _quyDinhRepository = ServiceProvider.GetRequiredService<IQuyDinhRepository>();
            _loaiDocGiaRepository = ServiceProvider.GetRequiredService<ILoaiDocGiaRepository>();

            // Seed basic data
            SeedBasicData();
        }

        protected override void SeedData()
        {
            // Additional test-specific data can be added here
        }

        [TestMethod]
        public async Task AddAsync_ValidDocGia_ShouldAddSuccessfully()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "Nguyễn Văn A",
                DiaChi = "123 Đường ABC",
                Email = "nguyenvana@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(2000, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            // Act
            await _docGiaRepository.AddAsync(docGia);

            // Assert
            var result = await _docGiaRepository.GetByIdAsync(docGia.MaDocGia);
            result.Should().NotBeNull();
            result.TenDocGia.Should().Be("Nguyễn Văn A");
            result.Email.Should().Be("nguyenvana@example.com");
        }

        [TestMethod]
        public async Task AddAsync_InvalidAge_ShouldThrowException()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "Nguyễn Văn B",
                DiaChi = "456 Đường XYZ",
                Email = "nguyenvanb@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(2010, 1, 1), // Tuổi < TuoiDocGiaToiThieu
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _docGiaRepository.AddAsync(docGia));
        }

        [TestMethod]
        public async Task UpdateAsync_ValidDocGia_ShouldUpdateSuccessfully()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "Nguyễn Văn C",
                DiaChi = "789 Đường DEF",
                Email = "nguyenvanc@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(2000, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            await _docGiaRepository.AddAsync(docGia);

            // Act
            docGia.TenDocGia = "Nguyễn Văn C (Updated)";
            docGia.DiaChi = "789 Đường DEF (Updated)";
            await _docGiaRepository.UpdateAsync(docGia);

            // Assert
            var result = await _docGiaRepository.GetByIdAsync(docGia.MaDocGia);
            result.Should().NotBeNull();
            result.TenDocGia.Should().Be("Nguyễn Văn C (Updated)");
            result.DiaChi.Should().Be("789 Đường DEF (Updated)");
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingDocGia_ShouldMarkAsDeleted()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "Nguyễn Văn D",
                DiaChi = "101 Đường GHI",
                Email = "nguyenvand@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(2000, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            await _docGiaRepository.AddAsync(docGia);

            // Act
            await _docGiaRepository.DeleteAsync(docGia.MaDocGia);

            // Assert
            var result = await _docGiaRepository.GetByIdAsync(docGia.MaDocGia);
            result.Should().BeNull(); // Vì GetByIdAsync chỉ trả về các độc giả chưa bị xóa
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllNonDeletedDocGia()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia1 = new DocGia
            {
                TenDocGia = "Nguyễn Văn E",
                DiaChi = "202 Đường JKL",
                Email = "nguyenvane@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(1995, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            var docGia2 = new DocGia
            {
                TenDocGia = "Trần Thị F",
                DiaChi = "303 Đường MNO",
                Email = "tranthif@example.com",
                MaLoaiDocGia = loaiDocGia[1].MaLoaiDocGia,
                NgaySinh = new DateOnly(1990, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            await _docGiaRepository.AddAsync(docGia1);
            await _docGiaRepository.AddAsync(docGia2);

            // Act
            var result = await _docGiaRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(d => d.TenDocGia == "Nguyễn Văn E");
            result.Should().Contain(d => d.TenDocGia == "Trần Thị F");
        }

        [TestMethod]
        public async Task AddAsync_DuplicateEmail_ShouldThrowException()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia1 = new DocGia
            {
                TenDocGia = "Nguyễn Văn G",
                DiaChi = "404 Đường PQR",
                Email = "duplicate@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(1995, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            var docGia2 = new DocGia
            {
                TenDocGia = "Trần Thị H",
                DiaChi = "505 Đường STU",
                Email = "duplicate@example.com", // Same email
                MaLoaiDocGia = loaiDocGia[1].MaLoaiDocGia,
                NgaySinh = new DateOnly(1990, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            await _docGiaRepository.AddAsync(docGia1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _docGiaRepository.AddAsync(docGia2));
        }

        [TestMethod]
        public async Task ValidateDocGia_InvalidEmail_ShouldThrowException()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "Nguyễn Văn I",
                DiaChi = "606 Đường VWX",
                Email = "invalid-email", // Invalid email format
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(1995, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _docGiaRepository.AddAsync(docGia));
        }

        [TestMethod]
        public async Task ValidateDocGia_EmptyName_ShouldThrowException()
        {
            // Arrange
            var loaiDocGia = await _loaiDocGiaRepository.GetAllAsync();
            var docGia = new DocGia
            {
                TenDocGia = "", // Empty name
                DiaChi = "707 Đường YZ",
                Email = "test@example.com",
                MaLoaiDocGia = loaiDocGia[0].MaLoaiDocGia,
                NgaySinh = new DateOnly(1995, 1, 1),
                NgayLapThe = DateOnly.FromDateTime(DateTime.Now),
                TongNo = 0
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _docGiaRepository.AddAsync(docGia));
        }
    }
}
