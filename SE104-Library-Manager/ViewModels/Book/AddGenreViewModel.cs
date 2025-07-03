using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using SE104_Library_Manager.Views.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Book
{
    public partial class AddGenreViewModel(ITheLoaiRepository theLoaiRepo) : ObservableObject 
    {
        [ObservableProperty]
        private string genreName = string.Empty;

        [RelayCommand]
        public async Task AddAsync(AddGenreWindow window)
        {
            TheLoai theLoai = new TheLoai { TenTheLoai = GenreName.Trim() };

            try
            {
                await theLoaiRepo.AddAsync(theLoai);

                MessageBox.Show("Thêm thể loại thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        [RelayCommand]
        public void Cancel(AddGenreWindow window)
        {
            window.Close();
        }
    }
}
