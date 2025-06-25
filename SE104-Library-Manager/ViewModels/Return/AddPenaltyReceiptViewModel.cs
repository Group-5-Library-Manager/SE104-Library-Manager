using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views.Return;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SE104_Library_Manager.ViewModels.Return;

public partial class AddPenaltyReceiptViewModel : ObservableObject
{
    private readonly IDocGiaRepository docGiaRepo;
    private readonly IPhieuPhatRepository phieuPhatRepo;

    public AddPenaltyReceiptViewModel(IDocGiaRepository docGiaRepo, IPhieuPhatRepository phieuPhatRepo)
    {
        this.docGiaRepo = docGiaRepo;
        this.phieuPhatRepo = phieuPhatRepo;

        IssueDate = DateOnly.FromDateTime(DateTime.Now);
        LoadReadersWithDebt();
    }

    [ObservableProperty] private ObservableCollection<DocGia> readersWithDebt = new();
    [ObservableProperty] private DocGia? selectedReader;

    [ObservableProperty] private int totalDebt;
    [ObservableProperty] private int receivedAmount;
    [ObservableProperty] private int remainingAmount;

    [ObservableProperty] private DateOnly issueDate;

    [RelayCommand]
    private async Task LoadReadersWithDebt()
    {
        var readers = await phieuPhatRepo.GetReadersWithDebtAsync();
        ReadersWithDebt = new ObservableCollection<DocGia>(readers);
    }

    partial void OnSelectedReaderChanged(DocGia? value)
    {
        if (value != null)
        {
            TotalDebt = value.TongNo;
            ReceivedAmount = value.TongNo;
            RemainingAmount = 0;
        }
    }

    partial void OnReceivedAmountChanged(int value)
    {
        if (SelectedReader != null)
        {
            RemainingAmount = Math.Max(0, TotalDebt - value);
        }
    }

    [RelayCommand]
    public async Task Save(AddPenaltyReceiptWindow w)
    {
        if (SelectedReader == null) return;

        // Kiểm tra số tiền thu
        if (HasValidationError(w))
        {
            MessageBox.Show("Tiền thu phải đúng định dạng số.",
                "Lỗi nhập liệu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (ReceivedAmount > TotalDebt)
        {
            MessageBox.Show($"Tiền thu không được lớn hơn tổng nợ.",
                "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var penalty = new PhieuPhat
        {
            MaDocGia = SelectedReader.MaDocGia,
            NgayLap = IssueDate,
            TongNo = TotalDebt,
            TienThu = ReceivedAmount,
            ConLai = RemainingAmount
        };
        await phieuPhatRepo.AddAsync(penalty);

        // Cập nhật lại tổng nợ
        SelectedReader.TongNo = RemainingAmount;
        await docGiaRepo.UpdateAsync(SelectedReader);

        MessageBox.Show($"Lập phiếu thu phạt thành công cho {SelectedReader.TenDocGia}.",
            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

        w?.Close();
    }

    [RelayCommand]
    public void Cancel(AddPenaltyReceiptWindow w)
    {
        w.Close();
    }
    public static bool HasValidationError(DependencyObject parent)
    {
        if (Validation.GetHasError(parent))
            return true;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (HasValidationError(child))
                return true;
        }
        return false;
    }
}
