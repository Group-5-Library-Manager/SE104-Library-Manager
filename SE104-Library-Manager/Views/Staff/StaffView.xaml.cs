using SE104_Library_Manager.ViewModels;
using System.Windows.Controls;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for StaffView.xaml
    /// </summary>
    public partial class StaffView : UserControl
    {
        public StaffView(StaffViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
