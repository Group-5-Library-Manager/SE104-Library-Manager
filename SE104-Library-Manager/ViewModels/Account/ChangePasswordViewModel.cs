using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using SE104_Library_Manager.Views;
using SE104_Library_Manager.Views.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SE104_Library_Manager.ViewModels.Account
{
    public partial class ChangePasswordViewModel(ITaiKhoanRepository taiKhoanRepository, IStaffSessionManager staffSessionManager) : ObservableObject
    {
        [ObservableProperty]
        private string oldPassword = "";

        [ObservableProperty]
        private string newPassword = "";

        [ObservableProperty]
        private string confirmPassword = "";

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private Visibility showErrorMessage = Visibility.Hidden;

        private CancellationTokenSource? errorMessageCts;

        [RelayCommand]
        public async Task ChangePassword(ChangePasswordWindow win)
        {
            if(string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
                ShowErrorMessageAsync(3);
                return;
            }
            if (ConfirmPassword != NewPassword)
            {
                ErrorMessage = "Mật khẩu nhập lại không chính xác.";
                ShowErrorMessageAsync(3);
                return;
            }

            try
            {
                var account = await taiKhoanRepository.GetByStaffIdAsync(staffSessionManager.CurrentStaffId);
                if(account == null)
                {
                    ErrorMessage = "Không tìm thấy tài khoản.";
                    ShowErrorMessageAsync(3);
                    return;
                }
                if(BCrypt.Net.BCrypt.Verify(OldPassword, account.MatKhau))
                {
                    await taiKhoanRepository.UpdatePasswordAsync(staffSessionManager.CurrentStaffId, newPassword);
                    var res = MessageBox.Show("Cập nhật mật khẩu thành công. Vui lòng đăng nhập lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    win?.Close();
                    if(res == MessageBoxResult.OK)
                    {
                        // Đăng xuất người dùng
                        staffSessionManager.ClearCurrentStaffId();

                        // Hiển thị cửa sổ đăng nhập
                        if (Application.Current.MainWindow != null)
                        {
                            var loginWindow = App.ServiceProvider?.GetService(typeof(Views.LoginWindow)) as Window;
                            if (loginWindow != null)
                            {
                                loginWindow.Show();
                                Application.Current.MainWindow.Close();
                                Application.Current.MainWindow = loginWindow;
                            }
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Mật khẩu cũ không chính xác.";
                    ShowErrorMessageAsync(3);
                    return;
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
}
