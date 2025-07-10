using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public class SelectableBanSaoSach : ObservableObject
    {
        public BanSaoSach BanSaoSach { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        public SelectableBanSaoSach(BanSaoSach bss, bool selected = false)
        {
            BanSaoSach = bss;
            IsSelected = selected;
        }
    }

    public partial class SelectCopiesViewModel : ObservableObject
    {
        [ObservableProperty]
        private Sach? selectedBook;

        [ObservableProperty]
        private ObservableCollection<BanSaoSach> allCopies = new();

        [ObservableProperty]
        private ObservableCollection<BanSaoSach> filteredCopies = new();

        [ObservableProperty]
        private ObservableCollection<BanSaoSach> selectedCopies = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private int maxSelection = 1;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool canAdd = true;

        [ObservableProperty]
        private string addButtonTooltip = string.Empty;

        [ObservableProperty]
        private ObservableCollection<SelectableBanSaoSach> selectableCopies = new();
        private List<SelectableBanSaoSach> allSelectableCopies = new();

        [ObservableProperty]
        private string selectedCountText = "Đã chọn: 0/0";


        partial void OnSelectedCopiesChanged(ObservableCollection<BanSaoSach> value)
        {
            if (value.Count > MaxSelection)
            {
                CanAdd = false;
                AddButtonTooltip = $"Không thể mượn nhiều hơn {MaxSelection} sách quy định";
            }
            else
            {
                CanAdd = true;
                AddButtonTooltip = string.Empty;
            }
            
            // Cập nhật text hiển thị số lượng đã chọn
            SelectedCountText = $"Đã chọn: {value.Count}/{MaxSelection}";
        }

        public SelectCopiesViewModel(IEnumerable<BanSaoSach> availableCopies, IEnumerable<BanSaoSach>? preselected = null, int max = 1)
        {
            AllCopies = new ObservableCollection<BanSaoSach>(availableCopies);
            FilteredCopies = new ObservableCollection<BanSaoSach>(availableCopies);
            SelectedCopies = new ObservableCollection<BanSaoSach>(preselected ?? new List<BanSaoSach>());
            MaxSelection = max;
            SearchText = string.Empty;
            
            // Khởi tạo text hiển thị ban đầu
            SelectedCountText = $"Đã chọn: {SelectedCopies.Count}/{MaxSelection}";
            
            // Wrap for selection - ensure preselected copies are marked as selected
            var preselectedIds = SelectedCopies.Select(s => s.MaBanSao).ToHashSet();

            // Khởi tạo tất cả selectable và gắn handler
            allSelectableCopies = AllCopies.Select(bss =>
            {
                var isSelected = preselectedIds.Contains(bss.MaBanSao);
                var item = new SelectableBanSaoSach(bss, isSelected);
                item.PropertyChanged += SelectableCopy_PropertyChanged;
                return item;
            }).ToList();

            SelectableCopies = new ObservableCollection<SelectableBanSaoSach>(allSelectableCopies);
        }
        private void SelectableCopy_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SelectableBanSaoSach sc && e.PropertyName == nameof(SelectableBanSaoSach.IsSelected))
            {
                if (sc.IsSelected && !SelectedCopies.Any(x => x.MaBanSao == sc.BanSaoSach.MaBanSao))
                {
                    SelectedCopies.Add(sc.BanSaoSach);
                }
                else if (!sc.IsSelected && SelectedCopies.Any(x => x.MaBanSao == sc.BanSaoSach.MaBanSao))
                {
                    SelectedCopies.Remove(SelectedCopies.First(x => x.MaBanSao == sc.BanSaoSach.MaBanSao));
                }
                OnSelectedCopiesChanged(SelectedCopies);
            }
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SelectableCopies = new ObservableCollection<SelectableBanSaoSach>(allSelectableCopies);
            }
            else
            {
                var lower = SearchText.ToLower();
                var filtered = allSelectableCopies
                    .Where(sc =>
                        sc.BanSaoSach.MaBanSao.ToString().Contains(lower) ||
                        sc.BanSaoSach.MaSach.ToString().Contains(lower) ||
                        (sc.BanSaoSach.Sach != null && sc.BanSaoSach.Sach.TenSach.ToLower().Contains(lower)))
                    .ToList();

                SelectableCopies = new ObservableCollection<SelectableBanSaoSach>(filtered);
            }
        }
        partial void OnSearchTextChanged(string value)
        {
            Search();
        }
    }
} 