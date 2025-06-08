using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class AddReaderTypeViewModel(ILoaiDocGiaRepository loaiDocGiaRepo) : ObservableObject
{
    [ObservableProperty]
    private string readerTypeName = string.Empty;

    [RelayCommand]
    public async Task AddAsync(AddReaderTypeWindow window)
    {
        LoaiDocGia loaiDocGia = new LoaiDocGia { TenLoaiDocGia = ReaderTypeName.Trim() };

        try
        {
            await loaiDocGiaRepo.AddAsync(loaiDocGia);

            MessageBox.Show("Thêm loại độc giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

    [RelayCommand]
    public void Cancel(AddReaderTypeWindow window)
    { 
        window.Close();
    }
}
