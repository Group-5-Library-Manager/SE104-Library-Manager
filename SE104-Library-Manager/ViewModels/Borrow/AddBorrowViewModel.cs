using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SE104_Library_Manager.Views.Borrow;
using System.Windows.Interop;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public partial class AddBorrowViewModel: ObservableObject
    {
        [ObservableProperty]
        private DateOnly borrowDate = DateOnly.FromDateTime(DateTime.Now);

        [ObservableProperty]
        private ObservableCollection<DocGia> readers = new ObservableCollection<DocGia>();

        [ObservableProperty]
        private DocGia? selectedReader;

        [ObservableProperty]
        private ObservableCollection<BanSaoSach> selectedCopies = new();

        public IRelayCommand SelectCopiesCommand { get; set; }

        [ObservableProperty]
        private NhanVien? currentStaff;

        private int maxBorrowCount = 5;

        private readonly IPhieuMuonRepository phieuMuonRepo;
        private readonly IDocGiaRepository docGiaRepo;
        private readonly INhanVienRepository nhanVienRepo;
        private readonly IStaffSessionReader staffSessionReader;
        private readonly IQuyDinhRepository quyDinhRepo;

        public AddBorrowViewModel(IPhieuMuonRepository phieuMuonRepository, IDocGiaRepository docGiaRepository,
                                 INhanVienRepository nhanVienRepository ,IStaffSessionReader staffSessionReader, IQuyDinhRepository quyDinhRepository)
        {
            phieuMuonRepo = phieuMuonRepository;
            docGiaRepo = docGiaRepository;
            nhanVienRepo = nhanVienRepository;
            this.staffSessionReader = staffSessionReader;
            quyDinhRepo = quyDinhRepository;

            SelectCopiesCommand = new RelayCommand(OpenSelectCopiesWindow);
            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var dsDocGia = await docGiaRepo.GetAllAsync();
                var validReaders = new List<DocGia>();
                foreach (var reader in dsDocGia)
                {
                    if (!await phieuMuonRepo.HasOverdueBooksAsync(reader.MaDocGia))
                    {
                        validReaders.Add(reader);
                    }
                }
                Readers = new ObservableCollection<DocGia>(validReaders);

                if (staffSessionReader.CurrentStaffId > 0)
                {
                    CurrentStaff = await nhanVienRepo.GetByIdAsync(staffSessionReader.CurrentStaffId);
                }
                BorrowDate = DateOnly.FromDateTime(DateTime.Now);
                maxBorrowCount = quyDinhRepo.GetQuyDinhAsync().Result.SoSachMuonToiDa;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSelectCopiesWindow()
        {
            var availableCopies = phieuMuonRepo.GetAvailableBanSaoSach();
            var vm = new SelectCopiesViewModel(availableCopies, SelectedCopies, maxBorrowCount);
            var window = new SelectCopiesWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                SelectedCopies = new ObservableCollection<BanSaoSach>(vm.SelectedCopies);
            }
        }

        [RelayCommand]
        private async Task SaveBorrow(Window window)
        {
            if (SelectedReader == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedCopies.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một bản sao để mượn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var phieuMuon = new PhieuMuon
                {
                    NgayMuon = BorrowDate,
                    MaDocGia = SelectedReader.MaDocGia,
                    MaNhanVien = staffSessionReader.CurrentStaffId
                };

                await phieuMuonRepo.AddAsync(phieuMuon, SelectedCopies.ToList());

                MessageBox.Show("Tạo phiếu mượn thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
