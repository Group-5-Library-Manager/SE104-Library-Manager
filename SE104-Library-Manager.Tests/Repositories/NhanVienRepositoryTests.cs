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
    public class NhanVienRepositoryTests : TestBase
    {
        private INhanVienRepository _nhanVienRepository;
        private IBangCapRepository _bangCapRepository;
        private IBoPhanRepository _boPhanRepository;
        private IChucVuRepository _chucVuRepository;
        private ITaiKhoanRepository _taiKhoanRepository;

        [TestInitialize]
        public void Initialize()
        {
            _nhanVienRepository = ServiceProvider.GetRequiredService<INhanVienRepository>();
            _bangCapRepository = ServiceProvider.GetRequiredService<IBangCapRepository>();
            _boPhanRepository = ServiceProvider.GetRequiredService<IBoPhanRepository>();
            _chucVuRepository = ServiceProvider.GetRequiredService<IChucVuRepository>();
            _taiKhoanRepository = ServiceProvider.GetRequiredService<ITaiKhoanRepository>();

            // Seed basic data
            SeedBasicData();
        }

        [TestMethod]
        public async Task AddAsync_ValidNhanVien_ShouldAddSuccessfully()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Nguyễn Văn A",
                DiaChi = "123 Đường ABC",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
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
            // Act
            await _nhanVienRepository.AddAsync(nhanVien, taiKhoan);

            // Assert
            var result = await _nhanVienRepository.GetByIdAsync(nhanVien.MaNhanVien);
            result.Should().NotBeNull();
            result.TenNhanVien.Should().Be("Nguyễn Văn A");
            result.DienThoai.Should().Be("0123456789");
        }

        [TestMethod]
        public async Task AddAsync_EmptyName_ShouldThrowException()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "", // Empty name
                DiaChi = "123 Đường ABC",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
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
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _nhanVienRepository.AddAsync(nhanVien, taiKhoan));
        }

        [TestMethod]
        public async Task AddAsync_InvalidPhoneNumber_ShouldThrowException()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Nguyễn Văn B",
                DiaChi = "456 Đường XYZ",
                DienThoai = "invalid-phone", // Invalid phone number
                NgaySinh = new DateOnly(1990, 1, 1),
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
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _nhanVienRepository.AddAsync(nhanVien, taiKhoan));
        }

        [TestMethod]
        public async Task AddAsync_DuplicatePhoneNumber_ShouldThrowException()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien1 = new NhanVien
            {
                TenNhanVien = "Nguyễn Văn C",
                DiaChi = "789 Đường DEF",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
                MaChucVu = chucVu[0].MaChucVu,
                MaBangCap = bangCap[0].MaBangCap,
                MaBoPhan = boPhan[0].MaBoPhan
            };

            var nhanVien2 = new NhanVien
            {
                TenNhanVien = "Trần Thị D",
                DiaChi = "101 Đường GHI",
                DienThoai = "0123456789", // Same phone number
                NgaySinh = new DateOnly(1985, 1, 1),
                MaChucVu = chucVu[1].MaChucVu,
                MaBangCap = bangCap[1].MaBangCap,
                MaBoPhan = boPhan[1].MaBoPhan
            };
            //sample account
            var taiKhoan1 = new TaiKhoan
            {
                TenDangNhap = "testuser",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = nhanVien1.MaNhanVien,
                MaVaiTro = 1 // Assuming 1 is the role ID for staff
            };
            var taiKhoan2 = new TaiKhoan
            {
                TenDangNhap = "testuser2",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = nhanVien2.MaNhanVien,
                MaVaiTro = 1 // Assuming 1 is the role ID for staff
            };
            await _nhanVienRepository.AddAsync(nhanVien1, taiKhoan1);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _nhanVienRepository.AddAsync(nhanVien2, taiKhoan2));
        }

        [TestMethod]
        public async Task UpdateAsync_ValidNhanVien_ShouldUpdateSuccessfully()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Original Name",
                DiaChi = "Original Address",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
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

            // Act
            nhanVien.TenNhanVien = "Updated Name";
            nhanVien.DiaChi = "Updated Address";
            await _nhanVienRepository.UpdateAsync(nhanVien);

            // Assert
            var result = await _nhanVienRepository.GetByIdAsync(nhanVien.MaNhanVien);
            result.Should().NotBeNull();
            result.TenNhanVien.Should().Be("Updated Name");
            result.DiaChi.Should().Be("Updated Address");
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingNhanVien_ShouldMarkAsDeleted()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Staff to Delete",
                DiaChi = "Delete Address",
                DienThoai = "0987654321",
                NgaySinh = new DateOnly(1990, 1, 1),
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
            await _nhanVienRepository.AddAsync(nhanVien,taiKhoan);

            // Act
            await _nhanVienRepository.DeleteAsync(nhanVien.MaNhanVien);

            // Assert
            var result = await _nhanVienRepository.GetByIdAsync(nhanVien.MaNhanVien);
            result.Should().BeNull(); // Should be null because it's marked as deleted
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllNonDeletedNhanVien()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien1 = new NhanVien
            {
                TenNhanVien = "Staff 1",
                DiaChi = "Address 1",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
                MaChucVu = chucVu[0].MaChucVu,
                MaBangCap = bangCap[0].MaBangCap,
                MaBoPhan = boPhan[0].MaBoPhan
            };

            var nhanVien2 = new NhanVien
            {
                TenNhanVien = "Staff 2",
                DiaChi = "Address 2",
                DienThoai = "0987654321",
                NgaySinh = new DateOnly(1985, 1, 1),
                MaChucVu = chucVu[1].MaChucVu,
                MaBangCap = bangCap[1].MaBangCap,
                MaBoPhan = boPhan[1].MaBoPhan
            };
            //sample account
            var taiKhoan1 = new TaiKhoan
            {
                TenDangNhap = "testuser",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = nhanVien1.MaNhanVien,
                MaVaiTro = 1 // Assuming 1 is the role ID for staff
            };
            var taiKhoan2 = new TaiKhoan
            {
                TenDangNhap = "testuser2",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = nhanVien2.MaNhanVien,
                MaVaiTro = 1 // Assuming 1 is the role ID for staff
            };
            await _nhanVienRepository.AddAsync(nhanVien1, taiKhoan1);
            await _nhanVienRepository.AddAsync(nhanVien2, taiKhoan2);

            // Act
            var result = await _nhanVienRepository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(n => n.TenNhanVien == "Staff 1");
            result.Should().Contain(n => n.TenNhanVien == "Staff 2");
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingNhanVien_ShouldReturnNhanVien()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Specific Staff",
                DiaChi = "Specific Address",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
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

            // Act
            var result = await _nhanVienRepository.GetByIdAsync(nhanVien.MaNhanVien);

            // Assert
            result.Should().NotBeNull();
            result.TenNhanVien.Should().Be("Specific Staff");
            result.BangCap.Should().NotBeNull();
            result.BoPhan.Should().NotBeNull();
            result.ChucVu.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistentNhanVien_ShouldReturnNull()
        {
            // Act
            var result = await _nhanVienRepository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task ValidateNhanVien_NullNhanVien_ShouldThrowException()
        {
            //sample account
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = "testuser",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("password123"),
                MaNhanVien = 1,
                MaVaiTro = 1 // Assuming 1 is the role ID for staff
            };
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _nhanVienRepository.AddAsync(null, taiKhoan));
        }

        [TestMethod]
        public async Task ValidateNhanVien_InvalidChucVu_ShouldThrowException()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Test Staff",
                DiaChi = "Test Address",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
                MaChucVu = -1, // Invalid ChucVu ID
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
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _nhanVienRepository.AddAsync(nhanVien, taiKhoan));
        }

        [TestMethod]
        public async Task ValidateNhanVien_EmptyAddress_ShouldThrowException()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Test Staff",
                DiaChi = "", // Empty address
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
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
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _nhanVienRepository.AddAsync(nhanVien, taiKhoan));
        }

        [TestMethod]
        public async Task ValidateNhanVien_InvalidBirthDate_ShouldThrowException()
        {
            // Arrange
            var bangCap = await _bangCapRepository.GetAllAsync();
            var boPhan = await _boPhanRepository.GetAllAsync();
            var chucVu = await _chucVuRepository.GetAllAsync();

            var nhanVien = new NhanVien
            {
                TenNhanVien = "Test Staff",
                DiaChi = "Test Address",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(2010, 1, 1), // Too young
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

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _nhanVienRepository.AddAsync(nhanVien, taiKhoan));
        }
    }
}