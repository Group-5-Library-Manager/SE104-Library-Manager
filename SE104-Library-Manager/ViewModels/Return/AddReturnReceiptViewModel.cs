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

public class SelectableCopy
{
    public int MaBanSao { get; set; }
    public string? TenSach { get; set; }
    public int MaPhieuMuon { get; set; }
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

    private List<SelectableCopy> allSelectableCopies = new();

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

    private async Task LoadAllBorrowedCopiesAsync(int maDocGia)
    {
        var chiTietDangMuon = await phieuTraRepo.GetBanSaoDangMuonByDocGiaAsync(maDocGia);
        allSelectableCopies = chiTietDangMuon.Select(ct => new SelectableCopy
        {
            MaBanSao = ct.BanSaoSach.MaBanSao,
            TenSach = ct.BanSaoSach.Sach?.TenSach ?? $"Bản sao {ct.BanSaoSach.MaBanSao}",
            MaPhieuMuon = ct.MaPhieuMuon
        }).ToList();
        UpdateAllAvailableCopies();
    }

    private void UpdateAllAvailableCopies()
    {
        var selectedIds = SelectedCopies
            .Where(i => i.SelectedCopy != null)
            .Select(i => i.SelectedCopy!.MaBanSao)
            .ToHashSet();
        foreach (var item in SelectedCopies)
        {
            var current = item.SelectedCopy;
            var list = allSelectableCopies
                .Where(s => !selectedIds.Contains(s.MaBanSao) || (current != null && s.MaBanSao == current.MaBanSao))
                .OrderBy(s => s.TenSach)
                .ToList();
            item.SyncAvailableCopies(list);
        }
    }

    partial void OnSelectedReaderChanged(DocGia? value)
    {
        SelectedCopies.Clear();
        TienPhatKyNay = 0;
        if (value != null)
        {
            tongNoBanDau = value.TongNo;
            _ = LoadAllBorrowedCopiesAsync(value.MaDocGia);
            // Sau khi load xong danh sách bản sao, nếu còn bản sao thì tự động thêm 1 dòng combobox trống
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                if (allSelectableCopies.Count > 0)
                {
                    var newItem = new CopyReturnItem(this);
                    SelectedCopies.Add(newItem);
                    UpdateAllAvailableCopies();
                }
            });
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
        // Lấy danh sách bản sao chưa được chọn
        var selectedIds = SelectedCopies
            .Where(i => i.SelectedCopy != null)
            .Select(i => i.SelectedCopy!.MaBanSao)
            .ToHashSet();
        var available = allSelectableCopies
            .Where(s => !selectedIds.Contains(s.MaBanSao))
            .ToList();
        if (available.Count == 0)
        {
            MessageBox.Show("Không còn sách nào để trả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var newItem = new CopyReturnItem(this);
        SelectedCopies.Add(newItem);
        UpdateAllAvailableCopies();
    }

    [RelayCommand]
    private void RemoveCopy(CopyReturnItem? item)
    {
        if (item != null)
        {
            SelectedCopies.Remove(item);
            UpdateTienPhatKyNay();
            UpdateAllAvailableCopies();
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
        private readonly AddReturnReceiptViewModel vm;
        public CopyReturnItem(AddReturnReceiptViewModel vm)
        {
            this.vm = vm;
            AvailableCopies = new ObservableCollection<SelectableCopy>();
        }
        public ObservableCollection<SelectableCopy> AvailableCopies { get; }
        [ObservableProperty] private SelectableCopy? selectedCopy;
        partial void OnSelectedCopyChanged(SelectableCopy? value)
        {
            _ = UpdateFineAsync();
            vm.UpdateAllAvailableCopies();
        }
        public void SyncAvailableCopies(List<SelectableCopy> newList)
        {
            var selectedId = SelectedCopy?.MaBanSao;
            for (int i = AvailableCopies.Count - 1; i >= 0; i--)
            {
                if (!newList.Any(s => s.MaBanSao == AvailableCopies[i].MaBanSao))
                    AvailableCopies.RemoveAt(i);
            }
            foreach (var copy in newList)
            {
                if (!AvailableCopies.Any(s => s.MaBanSao == copy.MaBanSao))
                    AvailableCopies.Add(copy);
            }
            if (selectedId != null && !AvailableCopies.Any(s => s.MaBanSao == selectedId))
                SelectedCopy = null;
        }
        [ObservableProperty] private DateOnly? borrowDate;
        [ObservableProperty] private int fine;

        public async Task<int> CalculateFineAsync()
        {
            if (SelectedCopy == null) return 0;
            var quyDinh = await vm.quyDinhRepo.GetQuyDinhAsync();
            var chiTiet = await vm.phieuTraRepo.GetChiTietMuonMoiNhatChuaTraAsync(SelectedCopy.MaBanSao);

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
            vm.UpdateTienPhatKyNay();
        }
    }

    [RelayCommand]
    public void Cancel(AddReturnReceiptWindow w)
    {
        w.Close();
    }
}