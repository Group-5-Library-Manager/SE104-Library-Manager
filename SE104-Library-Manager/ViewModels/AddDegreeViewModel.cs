using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class AddDegreeViewModel(IBangCapRepository bangCapRepo) : ObservableObject
{
    [ObservableProperty]
    private string degreeName = string.Empty;

    [RelayCommand]
    public async Task AddAsync(AddDegreeWindow w)
    {
        var newDegree = new BangCap { TenBangCap = DegreeName.Trim() };

        try
        {
            await bangCapRepo.AddAsync(newDegree);

            MessageBox.Show($"Thêm bằng cấp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            w.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void Cancel(AddDegreeWindow w)
    {
       w.Close();
    }
}
