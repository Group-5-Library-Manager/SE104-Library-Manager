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
    public partial class AddAuthorViewModel(ITacGiaRepository tacGiaRepo) : ObservableObject
    {
        [ObservableProperty]
        private string authorName = string.Empty;

        [RelayCommand]
        public async Task AddAsync(AddAuthorWindow window)
        {
            TacGia tacGia = new TacGia { TenTacGia = AuthorName.Trim() };

            try
            {
                await tacGiaRepo.AddAsync(tacGia);

                MessageBox.Show("Thêm tác giả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        [RelayCommand]
        public void Cancel(AddAuthorWindow window)
        {
            window.Close();
        }
    }
}
