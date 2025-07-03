using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Repositories;
using SE104_Library_Manager.Services;
using SE104_Library_Manager.Views.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Account
{
    public partial class AccountViewModel(IStaffSessionManager staffSessionManager, INhanVienRepository nhanVienRepository, ITaiKhoanRepository taiKhoanRepository) : ObservableObject
    {
        [ObservableProperty]
        private NhanVien currentNhanVien = null;

        [ObservableProperty]
        private TaiKhoan currentTaiKhoan = null;
        public async Task LoadCurrentStaffAccount()
        {
            CurrentNhanVien = await nhanVienRepository.GetByIdAsync(staffSessionManager.CurrentStaffId);
            CurrentTaiKhoan = await taiKhoanRepository.GetByStaffIdAsync(staffSessionManager.CurrentStaffId);
        }

        [RelayCommand]
        public void ChangePassword()
        {
            var cpw = App.ServiceProvider?.GetService(typeof(ChangePasswordWindow)) as Window;
            cpw.ShowDialog();
        }
    }
}
