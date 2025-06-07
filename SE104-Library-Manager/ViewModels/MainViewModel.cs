using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Views;
using System.Windows;

namespace SE104_Library_Manager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUserSessionManager _userSessionManager;

        [ObservableProperty]
        private object? _currentView;

        public MainViewModel(IUserSessionManager userSessionManager)
        {
            _userSessionManager = userSessionManager;

            // Mặc định hiển thị trang Sách khi khởi động
            NavigateCommand.Execute("Account");
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            // Chuyển đổi giữa các trang dựa trên tham số
            switch (destination)
            {
                case "Account":
                    // CurrentView = new AccountViewModel();
                    break;
                case "Book":
                    // CurrentView = new BooksViewModel();
                    break;
                case "Reader":
                    CurrentView = App.ServiceProvider?.GetService(typeof(ReaderView)) as ReaderView;
                    break;
                case "Staff":
                    CurrentView = App.ServiceProvider?.GetService(typeof(StaffView)) as StaffView;
                    break;
                case "Borrow":
                    // CurrentView = new BorrowViewModel();
                    break;
                case "Return":
                    // CurrentView = new ReturnViewModel();
                    break;
                case "Statistic":
                    // CurrentView = new StatisticsViewModel();
                    break;
                case "Policy":
                    // CurrentView = new RulesViewModel();
                    break;
                default:
                    // CurrentView = new AccountViewModel();
                    break;
            }

            // Hiện tại các ViewModel chưa được tạo, bạn cần tạo thêm các ViewModel tương ứng
            // Tạm thời để comment để tránh lỗi khi biên dịch
        }

        [RelayCommand]
        private void Logout()
        {
            // Đăng xuất người dùng
            _userSessionManager.ClearCurrentUserProfile();

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
}
