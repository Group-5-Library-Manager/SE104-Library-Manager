using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Views.Borrow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public partial class UpdateBorrowViewModel : ObservableObject
    {
        [ObservableProperty]
        private int borrowId;

        [ObservableProperty]
        private DateOnly borrowDate;

        [ObservableProperty]
        private string formattedBorrowDate = string.Empty;

        [ObservableProperty]
        private ObservableCollection<DocGia> readers = new ObservableCollection<DocGia>();

        [ObservableProperty]
        private DocGia? selectedReader;

        [ObservableProperty]
        private ObservableCollection<Sach> allBooks = new ObservableCollection<Sach>();

        [ObservableProperty]
        private ObservableCollection<BanSaoSach> selectedCopies = new();

        [ObservableProperty]
        private NhanVien? currentStaff;

        private int maxBorrowCount = 5;

        private readonly IPhieuMuonRepository phieuMuonRepo;
        private readonly IDocGiaRepository docGiaRepo;
        private readonly INhanVienRepository nhanVienRepo;
        private readonly IStaffSessionReader staffSessionReader;
        private readonly IQuyDinhRepository quyDinhRepo;

        public IRelayCommand SelectCopiesCommand { get; set; }

        private bool _isUpdatingAvailableBooks = false;

        public UpdateBorrowViewModel(IPhieuMuonRepository phieuMuonRepository, IDocGiaRepository docGiaRepository,
                                    INhanVienRepository nhanVienRepository, IStaffSessionReader staffSessionReader, IQuyDinhRepository quyDinhRepository)
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
                Readers = new ObservableCollection<DocGia>(dsDocGia);

                var dsSach = await phieuMuonRepo.GetAvailableBooksAsync();
                AllBooks = new ObservableCollection<Sach>(dsSach);

                if (staffSessionReader.CurrentStaffId > 0)
                {
                    CurrentStaff = await nhanVienRepo.GetByIdAsync(staffSessionReader.CurrentStaffId);
                }
                maxBorrowCount = quyDinhRepo.GetQuyDinhAsync().Result.SoSachMuonToiDa;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadBorrowData(PhieuMuon phieuMuon)
        {
            // Load all borrowed copies with Sach data
            var borrowedCopies = phieuMuon.DsChiTietPhieuMuon.Select(ct => ct.BanSaoSach).ToList();
            SelectedCopies = new ObservableCollection<BanSaoSach>(borrowedCopies);
            BorrowId = phieuMuon.MaPhieuMuon;
            BorrowDate = phieuMuon.NgayMuon;
            FormattedBorrowDate = phieuMuon.NgayMuon.ToString("dd/MM/yyyy");
            SelectedReader = Readers.FirstOrDefault(r => r.MaDocGia == phieuMuon.MaDocGia);
            
            // Ensure all copies have Sach data loaded
            foreach (var copy in SelectedCopies)
            {
                if (copy.Sach == null)
                {
                    // This shouldn't happen if we properly loaded the data, but just in case
                    var copyWithSach = phieuMuonRepo.GetAvailableBanSaoSach()
                        .FirstOrDefault(c => c.MaBanSao == copy.MaBanSao);
                    if (copyWithSach != null)
                    {
                        copy.Sach = copyWithSach.Sach;
                    }
                }
            }
        }

        // Note: This view model needs to be refactored to use copy-based system
        // For now, we'll keep the basic functionality without the book selection methods

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
            var returnedBanSao = await phieuMuonRepo.HasReturnedBooksAsync(BorrowId);
            if (returnedBanSao)
            {
                MessageBox.Show("Không thể cập nhật phiếu mượn đã có bản sao được trả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var phieuMuon = new PhieuMuon
                {
                    MaPhieuMuon = BorrowId,
                    NgayMuon = BorrowDate,
                    MaDocGia = SelectedReader.MaDocGia,
                    MaNhanVien = staffSessionReader.CurrentStaffId
                };
                
                await phieuMuonRepo.UpdateAsync(phieuMuon, SelectedCopies.ToList());
                MessageBox.Show("Cập nhật phiếu mượn thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSelectCopiesWindow()
        {
            try
            {
                // Lấy toàn bộ bản sao sách
                var allCopies = phieuMuonRepo.GetAllBanSaoSach().ToList();

                // Các bản sao đang mượn ở phiếu mượn hiện tại
                var currentlyBorrowedCopies = SelectedCopies.ToList();
                var currentlyBorrowedIds = currentlyBorrowedCopies.Select(c => c.MaBanSao).ToHashSet();

                // Lấy danh sách các bản sao đang được mượn bởi các phiếu mượn khác (chưa trả)
                var lockedCopyIds = phieuMuonRepo.GetLockedBanSaoSachIds(BorrowId).ToHashSet();

                // Lọc lại allCopies:
                // - Bao gồm những bản sao đang có sẵn
                // - Hoặc nằm trong SelectedCopies (đang mượn ở phiếu mượn hiện tại)
                var filteredCopies = allCopies
                    .Where(copy =>
                        copy.TinhTrang == "Có sẵn" ||
                        currentlyBorrowedIds.Contains(copy.MaBanSao))
                    .ToList();

                // Đảm bảo các bản sao trong danh sách hiện tại có đầy đủ dữ liệu
                for (int i = 0; i < filteredCopies.Count; i++)
                {
                    if (currentlyBorrowedIds.Contains(filteredCopies[i].MaBanSao))
                    {
                        // Thay thế bản sao bằng bản đang chọn (đã có đầy đủ thông tin)
                        filteredCopies[i] = currentlyBorrowedCopies.First(c => c.MaBanSao == filteredCopies[i].MaBanSao);
                    }
                }

                var vm = new SelectCopiesViewModel(filteredCopies, SelectedCopies, maxBorrowCount);
                var window = new SelectCopiesWindow { DataContext = vm, Owner = Application.Current.MainWindow };
                if (window.ShowDialog() == true)
                {
                    SelectedCopies = new ObservableCollection<BanSaoSach>(vm.SelectedCopies);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở window chọn bản sao: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
