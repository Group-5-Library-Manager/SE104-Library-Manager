using CommunityToolkit.Mvvm.ComponentModel;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels.Statistic
{
    public partial class StatisticViewModel : ObservableObject
    {
        [ObservableProperty]
        private TabItem? _selectedTab;

        // Child ViewModels
        public BorrowingStatisticViewModel BorrowingStatisticViewModel { get; }
        public LateReturnStatisticViewModel LateReturnStatisticViewModel { get; }
        public RevenueStatisticViewModel RevenueStatisticViewModel { get; }

        public StatisticViewModel(
            DatabaseService dbService,
            IQuyDinhRepository quyDinhRepo)
        {
            // Initialize child ViewModels
            BorrowingStatisticViewModel = new BorrowingStatisticViewModel(dbService);
            LateReturnStatisticViewModel = new LateReturnStatisticViewModel(dbService, quyDinhRepo);
            RevenueStatisticViewModel = new RevenueStatisticViewModel(dbService);
        }

        partial void OnSelectedTabChanged(TabItem? value)
        {
            // Handle tab selection changes if needed
            // This can be used for lazy loading or cleanup operations
            if (value != null)
            {
                // Optional: Trigger data loading when tab is selected
                var header = value.Header?.ToString();
                switch (header)
                {
                    case "Tình hình mượn":
                        // BorrowingStatisticViewModel operations if needed
                        break;
                    case "Sách trả trễ":
                        // LateReturnStatisticViewModel operations if needed
                        break;
                    case "Tiền thu phạt":
                        // RevenueStatisticViewModel operations if needed
                        break;
                }
            }
        }
    }
}