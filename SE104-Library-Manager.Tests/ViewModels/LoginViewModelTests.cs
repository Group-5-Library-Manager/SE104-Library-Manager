using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SE104_Library_Manager.Tests.ViewModels;

[TestClass]
public class LoginViewModelTests
{
    private Mock<IAuthService> _authServiceMock;
    private Mock<IStaffSessionManager> _sessionManagerMock;
    private LoginViewModel _viewModel;

    [TestInitialize]
    public void Initialize()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sessionManagerMock = new Mock<IStaffSessionManager>();
        _viewModel = new LoginViewModel(_sessionManagerMock.Object, _authServiceMock.Object);
    }

    [TestMethod]
    public void LoginCommand_ValidCredentials_ShouldAuthenticateAndSetStaffId()
    { 
        var staThread = new Thread(() =>
        {
            // Arrange
            _viewModel.Username = "testuser";
            var passwordBox = new PasswordBox { Password = "password123" };
            int expectedStaffId = 1;

            _authServiceMock
                .Setup(s => s.AuthenticateAsync("testuser", "password123"))
                .ReturnsAsync(expectedStaffId);
            // Act
            _viewModel.LoginCommand.ExecuteAsync(passwordBox);

            // Assert
            _authServiceMock.Verify(s => s.AuthenticateAsync("testuser", "password123"), Times.Once);
            _sessionManagerMock.Verify(s => s.SetCurrentStaffId(expectedStaffId), Times.Once);
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }

    [TestMethod]
    public void LoginCommand_InvalidCredentials_ShouldShowErrorMessage()
    {
        var staThread = new Thread(() =>
        {
            // Arrange
            _viewModel.Username = "testuser";
            var passwordBox = new PasswordBox { Password = "wrongpass" };
            string errorMessage = "Invalid credentials";

            _authServiceMock
                .Setup(s => s.AuthenticateAsync("testuser", "wrongpass"))
                .ThrowsAsync(new UnauthorizedAccessException(errorMessage));
            // Act
            _viewModel.LoginCommand.ExecuteAsync(passwordBox);

            // Assert
            Assert.AreEqual(errorMessage, _viewModel.ErrorMessage);
            _sessionManagerMock.Verify(s => s.SetCurrentStaffId(It.IsAny<int>()), Times.Never);
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }

    [TestMethod]
    public void LoginCommand_EmptyUsername_ShouldShowError()
    {
        var staThread = new Thread(() =>
        {
            // Arrange
            _viewModel.Username = "";
            var passwordBox = new PasswordBox { Password = "password123" };
            // Act
            _viewModel.LoginCommand.ExecuteAsync(passwordBox);

            // Assert
            Assert.AreEqual("Tên đăng nhập và mật khẩu không được để trống.", _viewModel.ErrorMessage);
            _authServiceMock.Verify(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }

    [TestMethod]
    public void LoginCommand_EmptyPassword_ShouldShowError()
    {
        var staThread = new Thread(() =>
        {
            // Arrange
            _viewModel.Username = "testuser";
            var passwordBox = new PasswordBox { Password = "" };
            // Act
            _viewModel.LoginCommand.ExecuteAsync(passwordBox).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Tên đăng nhập và mật khẩu không được để trống.", _viewModel.ErrorMessage);
            _authServiceMock.Verify(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }
}
