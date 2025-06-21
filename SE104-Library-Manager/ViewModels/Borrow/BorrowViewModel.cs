using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Views.Borrow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public partial class BorrowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PhieuMuon> dsPhieuMuon = new ObservableCollection<PhieuMuon>();

        [ObservableProperty]
        private PhieuMuon? selectedBorrow;

        private string searchQuery = string.Empty;
        private DispatcherTimer searchTimer;

        // Property with debounced search
        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                if (SetProperty(ref searchQuery, value))
                {
                    // Reset timer on each keystroke
                    searchTimer?.Stop();
                    searchTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(300) // 300ms delay
                    };
                    searchTimer.Tick += (s, e) =>
                    {
                        searchTimer.Stop();
                        Search();
                    };
                    searchTimer.Start();
                }
            }
        }

        private List<PhieuMuon> originalDsPhieuMuon = new List<PhieuMuon>();
        private readonly IPhieuMuonRepository phieuMuonRepo;
        private readonly INhanVienRepository nhanVienRepo;
        private readonly IDocGiaRepository docGiaRepo;
        private readonly IStaffSessionReader staffSessionReader;

        public BorrowViewModel(IPhieuMuonRepository phieuMuonRepository, INhanVienRepository nhanVienRepository,
                              IDocGiaRepository docGiaRepository, IStaffSessionReader staffSessionReader)
        {
            phieuMuonRepo = phieuMuonRepository;
            nhanVienRepo = nhanVienRepository;
            docGiaRepo = docGiaRepository;
            this.staffSessionReader = staffSessionReader;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                originalDsPhieuMuon = await phieuMuonRepo.GetAllAsync();
                DsPhieuMuon = new ObservableCollection<PhieuMuon>(originalDsPhieuMuon);
                SearchQuery = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                DsPhieuMuon = new ObservableCollection<PhieuMuon>(originalDsPhieuMuon);
                return;
            }

            var lowerValue = SearchQuery.ToLower();
            var filteredList = originalDsPhieuMuon.Where(pm =>
                pm.MaPhieuMuon.ToString().Contains(lowerValue) ||
                (pm.DocGia?.TenDocGia?.ToLower().Contains(lowerValue) ?? false) ||
                (pm.NhanVien?.TenNhanVien?.ToLower().Contains(lowerValue) ?? false) ||
                (pm.DocGia?.MaDocGia.ToString().ToLower().Contains(lowerValue) ?? false) ||
                (pm.NhanVien?.MaNhanVien.ToString().ToLower().Contains(lowerValue) ?? false) ||
                pm.NgayMuon.ToString("dd/MM/yyyy").Contains(lowerValue) ||
                pm.NgayMuon.ToString("MM/yyyy").Contains(lowerValue) ||
                pm.NgayMuon.ToString("yyyy").Contains(lowerValue)
            ).ToList();

            DsPhieuMuon = new ObservableCollection<PhieuMuon>(filteredList);
        }

        [RelayCommand]
        private void AddBorrow()
        {
            try
            {
                var addBorrowVM = App.ServiceProvider?.GetService(typeof(AddBorrowViewModel)) as AddBorrowViewModel;
                if (addBorrowVM == null)
                {
                    MessageBox.Show("Không thể tạo AddBorrowViewModel", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var addBorrowWindow = new AddBorrowWindow(addBorrowVM);
                addBorrowWindow.Owner = Application.Current.MainWindow;
                addBorrowWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = addBorrowWindow.ShowDialog();
                if (result == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cửa sổ thêm phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateBorrow()
        {
            if (SelectedBorrow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn để cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var updateBorrowVM = App.ServiceProvider?.GetService(typeof(UpdateBorrowViewModel)) as UpdateBorrowViewModel;
                if (updateBorrowVM == null)
                {
                    MessageBox.Show("Không thể tạo UpdateBorrowViewModel", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await updateBorrowVM.LoadBorrowData(SelectedBorrow);

                var updateBorrowWindow = new UpdateBorrowWindow(updateBorrowVM);
                updateBorrowWindow.Owner = Application.Current.MainWindow;
                updateBorrowWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = updateBorrowWindow.ShowDialog();
                if (result == true)
                {
                    _ = LoadDataAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cửa sổ cập nhật phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteBorrow()
        {
            if (SelectedBorrow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn để xóa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu mượn {SelectedBorrow.MaPhieuMuon}?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await phieuMuonRepo.DeleteAsync(SelectedBorrow.MaPhieuMuon);
                    await LoadDataAsync();
                    MessageBox.Show("Xóa phiếu mượn thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
