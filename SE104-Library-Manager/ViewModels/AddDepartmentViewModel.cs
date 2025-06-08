using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class AddDepartmentViewModel(IBoPhanRepository boPhanRepo) : ObservableObject
{
    [ObservableProperty]
    private string departmentName = string.Empty;

    [RelayCommand]
    public async Task AddAsync(AddDepartmentWindow w)
    {
        var newDepartment = new BoPhan { TenBoPhan = DepartmentName.Trim() };
        try
        {
            await boPhanRepo.AddAsync(newDepartment);

            MessageBox.Show("Thêm bộ phận thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            w.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void Cancel(AddDepartmentWindow w)
    {
        w.Close();
    }
}
