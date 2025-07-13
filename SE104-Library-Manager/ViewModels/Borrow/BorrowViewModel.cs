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

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private Visibility showAddButton = Visibility.Collapsed;

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

            if (staffSessionReader.GetCurrentStaffRole() == "Thủ thư")
            {
                ShowAddButton = Visibility.Visible;
            }

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                originalDsPhieuMuon = await phieuMuonRepo.GetAllAsync();
                DsPhieuMuon = new ObservableCollection<PhieuMuon>(originalDsPhieuMuon);
                SelectedBorrow = null;
                SearchQuery = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            Search();
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
                $"pm{pm.MaPhieuMuon}".ToLower().Contains(lowerValue) ||
                $"dg{pm.DocGia?.MaDocGia}".ToLower().Contains(lowerValue) ||
                $"nv{pm.NhanVien?.MaNhanVien}".ToLower().Contains(lowerValue) ||
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
                // Kiểm tra xem phiếu mượn có sách đã trả hay không
                var hasSachDaTra = await phieuMuonRepo.HasReturnedBooksAsync(SelectedBorrow.MaPhieuMuon);
                if (hasSachDaTra)
                {
                    MessageBox.Show($"Không thể cập nhật phiếu mượn PM{SelectedBorrow.MaPhieuMuon} vì có sách đã được trả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Check if the selected reader is violating
                if (await phieuMuonRepo.HasOverdueBooksAsync(SelectedBorrow.MaDocGia))
                {
                    MessageBox.Show("Độc giả đang vi phạm (có sách quá hạn chưa trả). Không thể cập nhật phiếu mượn cho độc giả này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var updateBorrowVM = App.ServiceProvider?.GetService(typeof(UpdateBorrowViewModel)) as UpdateBorrowViewModel;
                if (updateBorrowVM == null)
                {
                    MessageBox.Show("Không thể tạo UpdateBorrowViewModel", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Reload the borrow data from database to ensure we have all navigation properties
                var freshBorrowData = await phieuMuonRepo.GetByIdAsync(SelectedBorrow.MaPhieuMuon);
                if (freshBorrowData != null)
                {
                    await updateBorrowVM.LoadBorrowData(freshBorrowData);
                }
                else
                {
                    await updateBorrowVM.LoadBorrowData(SelectedBorrow);
                }

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

            try
            {
                // Kiểm tra xem phiếu mượn có sách đã trả hay không
                var hasSachDaTra = await phieuMuonRepo.HasReturnedBooksAsync(SelectedBorrow.MaPhieuMuon);
                if (hasSachDaTra)
                {
                    MessageBox.Show($"Không thể xóa phiếu mượn PM{SelectedBorrow.MaPhieuMuon} vì có sách đã được trả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi kiểm tra phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
