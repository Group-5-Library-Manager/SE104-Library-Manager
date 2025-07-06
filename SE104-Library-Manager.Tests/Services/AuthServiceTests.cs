using Moq;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<ITaiKhoanRepository> _taiKhoanRepositoryMock;
        private IAuthService _authService;

        [TestInitialize]
        public void Initialize()
        {
            _taiKhoanRepositoryMock = new Mock<ITaiKhoanRepository>();
            _authService = new AuthService(_taiKhoanRepositoryMock.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_ValidCredentials_ShouldReturnMaNhanVien()
        {
            // Arrange
            string username = "testuser";
            string password = "password123";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            int expectedMaNhanVien = 1;

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = username,
                MatKhau = hashedPassword,
                MaNhanVien = expectedMaNhanVien,
                MaVaiTro = 1
            };

            _taiKhoanRepositoryMock.Setup(repo => repo.GetByCredentialsAsync(username))
                .ReturnsAsync(taiKhoan);

            // Act
            int result = await _authService.AuthenticateAsync(username, password);

            // Assert
            Assert.AreEqual(expectedMaNhanVien, result);
            _taiKhoanRepositoryMock.Verify(repo => repo.GetByCredentialsAsync(username), Times.Once);
        }

        [TestMethod]
        public async Task AuthenticateAsync_InvalidUsername_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            string username = "invaliduser";
            string password = "password123";

            _taiKhoanRepositoryMock.Setup(repo => repo.GetByCredentialsAsync(username))
                .ReturnsAsync((TaiKhoan)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                async () => await _authService.AuthenticateAsync(username, password));

            _taiKhoanRepositoryMock.Verify(repo => repo.GetByCredentialsAsync(username), Times.Once);
        }

        [TestMethod]
        public async Task AuthenticateAsync_InvalidPassword_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            string username = "testuser";
            string correctPassword = "password123";
            string wrongPassword = "wrongpassword";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = username,
                MatKhau = hashedPassword,
                MaVaiTro = 1,
                MaNhanVien = 1
            };

            _taiKhoanRepositoryMock.Setup(repo => repo.GetByCredentialsAsync(username))
                .ReturnsAsync(taiKhoan);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                async () => await _authService.AuthenticateAsync(username, wrongPassword));

            _taiKhoanRepositoryMock.Verify(repo => repo.GetByCredentialsAsync(username), Times.Once);
        }
    }
}
