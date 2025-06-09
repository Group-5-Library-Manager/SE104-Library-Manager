using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Views;
using System.Windows;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels;

public partial class LoginViewModel(IStaffSessionManager staffSessionManager, IAuthService authService) : ObservableObject
{
    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private Visibility showErrorMessage = Visibility.Hidden;

    private CancellationTokenSource? errorMessageCts;

    [RelayCommand]
    public async Task Login(PasswordBox passwordBox)
    {
        string password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Tên đăng nhập và mật khẩu không được để trống.";
            ShowErrorMessageAsync(3);
            return;
        }

        try
        {
            int staffId = await authService.AuthenticateAsync(Username, password);
            staffSessionManager.SetCurrentStaffId(staffId);

            var mainWindow = App.ServiceProvider?.GetRequiredService<MainWindow>();
            if (mainWindow != null)
            {
                Window currentWindow = Application.Current.MainWindow;

                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                currentWindow?.Close();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            ShowErrorMessageAsync(3);
        }
    }

    private void ShowErrorMessageAsync(int second)
    {
        errorMessageCts?.Cancel();
        errorMessageCts = new CancellationTokenSource();
        var token = errorMessageCts.Token;

        ShowErrorMessage = Visibility.Visible;

        Task.Run(async () =>
        { 
            try
            {
                await Task.Delay(second * 1000);
                if (!token.IsCancellationRequested)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowErrorMessage = Visibility.Hidden;
                        ErrorMessage = string.Empty;
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, do nothing
            }
        });
    }
}
