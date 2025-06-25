using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SE104_Library_Manager.ViewModels.Policy;
public partial class PolicyViewModel : ObservableObject
{
    private readonly IQuyDinhRepository quyDinhRepo;

    public PolicyViewModel(IQuyDinhRepository quyDinhRepo)
    {
        this.quyDinhRepo = quyDinhRepo;
        LoadDataAsync();
    }

    [ObservableProperty] private QuyDinh currentQuyDinh = new();

    public async void LoadDataAsync()
    {
        CurrentQuyDinh = await quyDinhRepo.GetQuyDinhAsync();
    }

    [RelayCommand]
    public async Task Save(UserControl uc)
    {
        if (HasValidationError(uc))
        {
            MessageBox.Show("Vui lòng sửa tất cả ô nhập còn lỗi (viền đỏ) thành dạng số và không được để trống.",
                "Lỗi nhập liệu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        try
        {
            await quyDinhRepo.UpdateAsync(CurrentQuyDinh);
            MessageBox.Show("Cập nhật quy định thành công!",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi cập nhật quy định: {ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
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
