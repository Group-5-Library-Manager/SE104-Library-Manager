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
    public void Add(AddReaderTypeWindow window)
    {
        ReaderTypeName = ReaderTypeName.Trim();
        LoaiDocGia loaiDocGia = new LoaiDocGia { TenLoaiDocGia = ReaderTypeName };

        try
        {
            loaiDocGiaRepo.AddAsync(loaiDocGia).GetAwaiter().GetResult();
            MessageBox.Show("Thêm loại độc giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi thêm loại độc giả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        window.Close();
    }

    [RelayCommand]
    public void Cancel(AddReaderTypeWindow window)
    { 
        window.Close();
    }
}
