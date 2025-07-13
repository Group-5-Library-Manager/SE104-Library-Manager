using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Views;
using SE104_Library_Manager.Views.Borrow;
using SE104_Library_Manager.Views.Policy;
using SE104_Library_Manager.Views.Return;
using SE104_Library_Manager.Views.Statistic;
using SE104_Library_Manager.Views.Account;
using SE104_Library_Manager.Views.Book;
using System.Windows;

namespace SE104_Library_Manager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IStaffSessionManager _staffSessionManager;

        [ObservableProperty]
        private Visibility _adminItemsVisibility = Visibility.Collapsed; // Ẩn/Hiển các mục cần quyền quản trị viên (Nhân viên, Quy định, ...)

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
                AdminItemsVisibility = Visibility.Visible; // Hiển thị các mục quản trị viên
            }
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            // Chuyển đổi giữa các trang dựa trên tham số
            switch (destination)
            {
                case "Account":
                    CurrentView = App.ServiceProvider?.GetService(typeof(AccountView)) as AccountView;
                    break;
                case "Book":
                    CurrentView = App.ServiceProvider?.GetService(typeof(BookView)) as BookView;
                    break;
                case "Reader":
                    CurrentView = App.ServiceProvider?.GetService(typeof(ReaderView)) as ReaderView;
                    break;
                case "Staff":
                    CurrentView = App.ServiceProvider?.GetService(typeof(StaffView)) as StaffView;
                    break;
                case "Borrow":
                    CurrentView = App.ServiceProvider?.GetService(typeof(BorrowView)) as BorrowView;
                    break;
                case "Return":
                    CurrentView = App.ServiceProvider?.GetService(typeof(ReturnView)) as ReturnView;
                    break;
                case "Statistic":
                    CurrentView = App.ServiceProvider?.GetService(typeof(StatisticView)) as StatisticView;
                    break;
                case "Policy":
                    CurrentView = App.ServiceProvider?.GetService(typeof(PolicyView)) as PolicyView;
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
