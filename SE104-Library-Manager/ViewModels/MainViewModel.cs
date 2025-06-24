using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Views;
using SE104_Library_Manager.Views.Return;
using System.Windows;

namespace SE104_Library_Manager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IStaffSessionManager _staffSessionManager;

        [ObservableProperty]
        private Visibility _showAdminItems = Visibility.Collapsed; // Ẩn/Hiển các mục cần quyền quản trị viên (Nhân viên, Quy định, ...)

        [ObservableProperty]
        private object? _currentView;

        public MainViewModel(IStaffSessionManager staffSessionManager)
        {
            _staffSessionManager = staffSessionManager;

            // Mặc định hiển thị trang Sách khi khởi động
            NavigateCommand.Execute("Account");

            // Kiểm tra quyền của nhân viên hiện tại
            if (_staffSessionManager.GetCurrentStaffRole() == "Quản trị viên")
            {
                ShowAdminItems = Visibility.Visible; // Hiển thị các mục quản trị viên
            }
            else
            {
                ShowAdminItems = Visibility.Collapsed; // Ẩn các mục quản trị viên
            }
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
                    CurrentView = App.ServiceProvider?.GetService(typeof(ReturnView)) as ReturnView;
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
            _staffSessionManager.ClearCurrentStaffId();

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
