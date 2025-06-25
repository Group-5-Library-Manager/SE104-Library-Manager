using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SE104_Library_Manager.Views.Return;

namespace SE104_Library_Manager.ViewModels;

public partial class ReturnViewModel(
    IPhieuTraRepository phieuTraRepo,
    IDocGiaRepository docGiaRepo,
    IChiTietPhieuTraRepository chiTietPhieuTraRepo,
    IPhieuPhatRepository phieuPhatRepo
) : ObservableObject
{
    [ObservableProperty]
    private TabItem selectedTab = null!;

    // Phiếu trả
    [ObservableProperty] private ObservableCollection<PhieuTra> dsPhieuTra = new();
    [ObservableProperty] private PhieuTra? selectedPhieuTra;

    [ObservableProperty] private string searchQuery = string.Empty;
    [ObservableProperty] private ObservableCollection<PhieuTra> dsPhieuTraFiltered = new();

    // Phiếu phạt
    [ObservableProperty] private ObservableCollection<PhieuPhat> dsPhieuPhat = new();
    [ObservableProperty] private ObservableCollection<PhieuPhat> dsPhieuPhatFiltered = new();
    [ObservableProperty] private PhieuPhat? selectedPhieuPhat;

    [ObservableProperty] private string searchQueryPhieuPhat = string.Empty;

    public async Task LoadDataAsync()
    {
        DsPhieuTra = new ObservableCollection<PhieuTra>(await phieuTraRepo.GetAllAsync());
        DsPhieuTraFiltered = new ObservableCollection<PhieuTra>(DsPhieuTra);
        SelectedPhieuTra = null;
        _ = SearchPhieuTra();
    }

    partial void OnSearchQueryChanged(string value)
    {
        SearchPhieuTra().ConfigureAwait(false); 
    }

    [RelayCommand]
    public async Task SearchPhieuTra()
    {
        var query = SearchQuery?.Trim().ToLower() ?? "";

        if (string.IsNullOrEmpty(query))
        {
            DsPhieuTraFiltered = new ObservableCollection<PhieuTra>(DsPhieuTra);
            return;
        }

        var filtered = DsPhieuTra.Where(p =>
            $"pt{p.MaPhieuTra}".ToLower().Contains(query) ||
            $"dg{p.DocGia.MaDocGia}".ToLower().Contains(query) ||
            p.MaPhieuTra.ToString().Contains(query) ||
            p.DocGia.MaDocGia.ToString().Contains(query) ||
            (p.DocGia.TenDocGia?.ToLower().Contains(query) ?? false) ||
            p.NgayTra.ToString().Contains(query)
        ).ToList();

        DsPhieuTraFiltered = new ObservableCollection<PhieuTra>(filtered);
    }


    [RelayCommand]
    public async Task AddReturnReceipt()
    {
        // Kiểm tra độc giả có sách đang mượn
        var readersWithBorrowedBooks = await phieuTraRepo.GetDocGiaDangCoSachMuonAsync();
        if (readersWithBorrowedBooks == null || !readersWithBorrowedBooks.Any())
        {
            MessageBox.Show("Không có độc giả nào đang mượn sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddReturnReceiptWindow)) as AddReturnReceiptWindow;
        if (w == null) return;
        w.Owner = Application.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task OpenUpdateReturnWindow(int? maPhieuTra)
    {
        if (maPhieuTra == null)
        {
            MessageBox.Show("Vui lòng chọn phiếu trả để cập nhật", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(UpdateReturnReceiptWindow)) as UpdateReturnReceiptWindow;
        if (w == null) return;
        w.Owner = Application.Current.MainWindow;

        if (w.DataContext is UpdateReturnReceiptViewModel vm)
        {
            await vm.LoadDataAsync(maPhieuTra.Value);
        }

        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task DeleteReturnReceipt()
    {
        if (SelectedPhieuTra == null)
        {
            MessageBox.Show("Vui lòng chọn một phiếu trả để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu trả này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var docGia = SelectedPhieuTra.DocGia;
                if (docGia != null)
                {
                    // Cập nhật lại tổng nợ của độc giả
                    docGia.TongNo = Math.Max(0, docGia.TongNo - SelectedPhieuTra.TienPhatKyNay);
                    await docGiaRepo.UpdateAsync(docGia);
                }

                await chiTietPhieuTraRepo.DeleteByPhieuTraAsync(SelectedPhieuTra.MaPhieuTra);
                await phieuTraRepo.DeleteAsync(SelectedPhieuTra.MaPhieuTra);
                await LoadDataAsync();
                MessageBox.Show("Xóa phiếu trả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phiếu trả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    //PHIẾU PHẠT
    public async Task LoadPhieuPhatDataAsync()
    {
        DsPhieuPhat = new ObservableCollection<PhieuPhat>(await phieuPhatRepo.GetAllAsync());
        DsPhieuPhatFiltered = new ObservableCollection<PhieuPhat>(DsPhieuPhat);
        SelectedPhieuPhat = null;
        _ = SearchPhieuPhat();
    }

    partial void OnSearchQueryPhieuPhatChanged(string value) => SearchPhieuPhat().ConfigureAwait(false);


    [RelayCommand]
    public async Task SearchPhieuPhat()
    {
        var searchText = SearchQueryPhieuPhat?.Trim().ToLower() ?? "";

        if (string.IsNullOrEmpty(searchText))
        {
            DsPhieuPhatFiltered = new ObservableCollection<PhieuPhat>(DsPhieuPhat);
            return;
        }

        var filtered = DsPhieuPhat.Where(pp =>
            $"pp{pp.MaPhieuPhat}".ToLower().Contains(searchText) ||
            $"dg{pp.DocGia.MaDocGia}".ToLower().Contains(searchText) ||
            pp.MaPhieuPhat.ToString().Contains(searchText) ||
            pp.DocGia.MaDocGia.ToString().Contains(searchText) ||
            (pp.DocGia.TenDocGia?.ToLower().Contains(searchText) ?? false) ||
            pp.NgayLap.ToString().ToLower().Contains(searchText)
        );

        DsPhieuPhatFiltered = new ObservableCollection<PhieuPhat>(filtered);
    }


    [RelayCommand]
    public async Task AddPhieuPhat()
    {
        var readersWithDebt = await phieuPhatRepo.GetReadersWithDebtAsync();
        if (readersWithDebt == null || !readersWithDebt.Any())
        {
            MessageBox.Show("Không có độc giả nào còn nợ để lập phiếu phạt.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddPenaltyReceiptWindow)) as AddPenaltyReceiptWindow;
        if (w == null) return;

        w.Owner = Application.Current.MainWindow;
        w.ShowDialog();

        await LoadPhieuPhatDataAsync();
    }


    [RelayCommand]
    public async Task ExportPhieuPhat()
    {
        if (SelectedPhieuPhat == null)
        {
            MessageBox.Show("Vui lòng chọn phiếu phạt để xuất.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            bool exported = await phieuPhatRepo.ExportAsync(SelectedPhieuPhat);
            if (exported)
            {
                MessageBox.Show($"Xuất phiếu phạt thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất phiếu phạt: {ex.Message}",
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    [RelayCommand]
    public async Task DeletePhieuPhat()
    {
        if (SelectedPhieuPhat == null)
        {
            MessageBox.Show("Vui lòng chọn phiếu phạt để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu phạt này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var docGia = SelectedPhieuPhat.DocGia;
                if (docGia != null)
                {
                    // Cập nhật lại tổng nợ
                    docGia.TongNo += SelectedPhieuPhat.TienThu;
                    await docGiaRepo.UpdateAsync(docGia);
                }

                await phieuPhatRepo.DeleteAsync(SelectedPhieuPhat.MaPhieuPhat);
                await LoadPhieuPhatDataAsync();
                MessageBox.Show("Xóa phiếu phạt thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phiếu phạt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    partial void OnSelectedTabChanged(TabItem value)
    {
        if (value.Header.ToString() == "Trả sách")
        {
            LoadDataAsync().ConfigureAwait(false);
        }
        else
        {
            LoadPhieuPhatDataAsync().ConfigureAwait(false);
        }
    }

}