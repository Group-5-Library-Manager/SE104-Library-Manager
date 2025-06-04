using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Models;
using SE104_Library_Manager.Views;
using System.Windows;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels;

public partial class LoginViewModel(IUserSessionManager userSessionManager, IAuthService authService) : ObservableObject
{
    [ObservableProperty]
    private string username = String.Empty;

    [ObservableProperty]
    private string errorMessage = String.Empty;

    [ObservableProperty]
    private Visibility showErrorMessage = Visibility.Hidden;

    [RelayCommand]
    public async Task LoginAsync(PasswordBox passwordBox)
    {
        string password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Tên đăng nhập và mật khẩu không được để trống.";
            await ShowErrorMessageAsync(3);
            return;
        }

        try
        {
            UserProfile userProfile = await authService.AuthenticateAsync(Username, password);
            userSessionManager.SetCurrentUserProfile(userProfile);

            ErrorMessage = "Đăng nhập thành công!";
            await ShowErrorMessageAsync(3);
            if (userProfile != null)
            {
                var mainWindow = App.ServiceProvider?.GetRequiredService<MainWindow>();
                if (mainWindow != null)
                {
                    Window currentWindow = Application.Current.MainWindow;

                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();

                    currentWindow?.Close();
                }
            }   
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await ShowErrorMessageAsync(3);
        }
    }

    private async Task ShowErrorMessageAsync(int second)
    {
        ShowErrorMessage = Visibility.Visible;

        await Task.Delay(second * 1000);

        ShowErrorMessage = Visibility.Hidden;
    }
}
