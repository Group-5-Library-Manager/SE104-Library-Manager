using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views.Return;
using System.Collections.ObjectModel;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Return;

// Helper class để tạo ChiTietPhieuTra mà không cần MaPhieuTra
public class ChiTietPhieuTraInfo
{
    public int MaPhieuMuon { get; set; }
    public int MaBanSao { get; set; }
    public int TienPhat { get; set; }
}

public partial class AddReturnReceiptViewModel : ObservableObject
{
    private readonly IPhieuTraRepository phieuTraRepo;
    private readonly IChiTietPhieuTraRepository chiTietPhieuTraRepo;
    private readonly IQuyDinhRepository quyDinhRepo;
    private readonly IDocGiaRepository docGiaRepo;
    private readonly IStaffSessionReader staffSessionReader;
    private readonly INhanVienRepository nhanVienRepo;

    [ObservableProperty] private ObservableCollection<DocGia> readers = new();
    [ObservableProperty] private DocGia? selectedReader;
    [ObservableProperty] private ObservableCollection<CopyReturnItem> selectedCopies = new();
    [ObservableProperty] private int tienPhatKyNay;
    [ObservableProperty] private DateOnly returnDate = DateOnly.FromDateTime(DateTime.Now);
    [ObservableProperty] private NhanVien? currentStaff;
    [ObservableProperty] private int tongNoHienTai;

    public bool HasSelectedReader => SelectedReader != null;

    private int tongNoBanDau;

    public AddReturnReceiptViewModel(
        IPhieuTraRepository phieuTraRepo,
        IChiTietPhieuTraRepository chiTietPhieuTraRepo,
        IQuyDinhRepository quyDinhRepo,
        IDocGiaRepository docGiaRepo,
        IStaffSessionReader staffSessionReader,
        INhanVienRepository nhanVienRepo)
    {
        this.phieuTraRepo = phieuTraRepo;
        this.chiTietPhieuTraRepo = chiTietPhieuTraRepo;
        this.quyDinhRepo = quyDinhRepo;
        this.docGiaRepo = docGiaRepo;
        this.staffSessionReader = staffSessionReader;
        this.nhanVienRepo = nhanVienRepo;

        LoadDataAsync().ConfigureAwait(false);
    }

    public async Task LoadDataAsync()
    {
        Readers = new ObservableCollection<DocGia>(await phieuTraRepo.GetDocGiaDangCoSachMuonAsync());
        SelectedCopies.Clear();
        SelectedReader = null;
        TienPhatKyNay = 0;

        ReturnDate = DateOnly.FromDateTime(DateTime.Now);

        int currentStaffId = staffSessionReader.CurrentStaffId;
        CurrentStaff = await nhanVienRepo.GetByIdAsync(currentStaffId);
    }

    partial void OnSelectedReaderChanged(DocGia? value)
    {
        SelectedCopies.Clear();
        TienPhatKyNay = 0;
        if (value != null)
        {
            tongNoBanDau = value.TongNo;
        }
        else
        {
            tongNoBanDau = 0;
        }

        UpdateTongNoHienTai();
        OnPropertyChanged(nameof(HasSelectedReader));
    }

    [RelayCommand]
    private void AddCopy()
    {
        if (SelectedReader == null) return;

        var newItem = new CopyReturnItem(phieuTraRepo, quyDinhRepo, SelectedReader.MaDocGia, UpdateTienPhatKyNay);
        SelectedCopies.Add(newItem);
    }

    [RelayCommand]
    private void RemoveCopy(CopyReturnItem? item)
    {
        if (item != null)
        {
            SelectedCopies.Remove(item);
            UpdateTienPhatKyNay();
        }
    }

    private async void UpdateTienPhatKyNay()
    {
        int total = 0;
        foreach (var item in SelectedCopies)
        {
            total += await item.CalculateFineAsync();
        }
        TienPhatKyNay = total;
        UpdateTongNoHienTai();
    }

    private void UpdateTongNoHienTai()
    {
        TongNoHienTai = tongNoBanDau + TienPhatKyNay;
    }

    [RelayCommand]
    private async Task ConfirmReturnAsync()
    {
        if (SelectedReader == null || SelectedCopies.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn độc giả và ít nhất một bản sao để trả.");
            return;
        }

        var validCopies = SelectedCopies
            .Where(c => c.SelectedCopy != null)
            .ToList();

        if (validCopies.Count == 0)
        {
            MessageBox.Show("Danh sách bản sao không hợp lệ.");
            return;
        }

        // Kiểm tra bản sao trùng nhau
        var distinctCopies = validCopies.Select(c => c.SelectedCopy?.MaBanSao).Distinct().Count();
        if (distinctCopies < validCopies.Count)
        {
            MessageBox.Show("Có bản sao bị trùng trong danh sách. Vui lòng kiểm tra lại.");
            return;
        }

        try
        {
            // Thêm phiếu trả mới
            var phieuTra = new PhieuTra
            {
                MaNhanVien = staffSessionReader.CurrentStaffId,
                MaDocGia = SelectedReader.MaDocGia,
                NgayTra = ReturnDate,
                TienPhatKyNay = TienPhatKyNay,
                TongNo = TongNoHienTai
            };

            var dsChiTiet = new List<ChiTietPhieuTraInfo>();

            foreach (var item in validCopies)
            {
                if (item.SelectedCopy == null) continue;

                var chiTietMuon = await phieuTraRepo.GetChiTietMuonMoiNhatChuaTraAsync(item.SelectedCopy.MaBanSao);
                if (chiTietMuon == null) continue;

                dsChiTiet.Add(new ChiTietPhieuTraInfo
                {
                    MaPhieuMuon = chiTietMuon.MaPhieuMuon,
                    MaBanSao = item.SelectedCopy.MaBanSao,
                    TienPhat = item.Fine
                });
            }

            if (dsChiTiet.Count > 0)
            {
                await phieuTraRepo.AddAsync(phieuTra, dsChiTiet);
                MessageBox.Show("Trả sách thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                SelectedReader.TongNo = TongNoHienTai;
                await docGiaRepo.UpdateAsync(SelectedReader);

                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?.Close();
            }
            else
            {
                throw new InvalidOperationException("Không có bản sao nào hợp lệ để trả.");
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi trả sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public partial class CopyReturnItem : ObservableObject
    {
        private readonly IPhieuTraRepository phieuTraRepo;
        private readonly IQuyDinhRepository quyDinhRepo;
        private readonly int maDocGia;
        private readonly Action updateParentTotal;

        public ObservableCollection<BanSaoSach> BorrowedCopies { get; } = new();

        [ObservableProperty] private BanSaoSach? selectedCopy;
        [ObservableProperty] private DateOnly? borrowDate;
        [ObservableProperty] private int fine;

        public CopyReturnItem(IPhieuTraRepository repo, IQuyDinhRepository quyDinh, int maDocGia, Action onChange)
        {
            phieuTraRepo = repo;
            quyDinhRepo = quyDinh;
            this.maDocGia = maDocGia;
            updateParentTotal = onChange;
            _ = LoadBorrowedCopiesAsync();
        }

        private async Task LoadBorrowedCopiesAsync()
        {
            var chiTietDangMuon = await phieuTraRepo.GetBanSaoDangMuonByDocGiaAsync(maDocGia);
            var copies = chiTietDangMuon.Select(ct => ct.BanSaoSach).ToList();

            BorrowedCopies.Clear();
            foreach (var copy in copies)
                BorrowedCopies.Add(copy);
        }

        partial void OnSelectedCopyChanged(BanSaoSach? value)
        {
            if (value != null)
            {
                _ = UpdateFineAsync();
            }
            else
            {
                Fine = 0;
            }
        }

        public async Task<int> CalculateFineAsync()
        {
            if (SelectedCopy == null) return 0;
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var chiTiet = await phieuTraRepo.GetChiTietMuonMoiNhatChuaTraAsync(SelectedCopy.MaBanSao);

            if (chiTiet?.PhieuMuon != null)
            {
                BorrowDate = chiTiet.PhieuMuon.NgayMuon;
                
                var today = DateOnly.FromDateTime(DateTime.Now);
                int soNgayTre = (today.DayNumber - BorrowDate.Value.DayNumber) - quyDinh.SoNgayMuonToiDa;
                int finePerCopy = soNgayTre > 0 ? soNgayTre * quyDinh.TienPhatQuaHanMoiNgay : 0;
                return finePerCopy;
            }
            return 0;
        }

        private async Task UpdateFineAsync()
        {
            Fine = await CalculateFineAsync();
            updateParentTotal();
        }
    }

    [RelayCommand]
    public void Cancel(AddReturnReceiptWindow w)
    {
        w.Close();
    }
}