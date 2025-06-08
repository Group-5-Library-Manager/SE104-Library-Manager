using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class AddPositionViewModel(IChucVuRepository chucVuRepo) : ObservableObject
{
    [ObservableProperty]
    private string positionName = string.Empty;

    [RelayCommand]
    public async Task AddAsync(AddPositionWindow window)
    {
        var newPosition = new Entities.ChucVu { TenChucVu = PositionName.Trim() };
        try
        {
            await chucVuRepo.AddAsync(newPosition);

            MessageBox.Show("Thêm chức vụ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void Cancel(AddPositionWindow window)
    {
        window.Close();
    }
}
