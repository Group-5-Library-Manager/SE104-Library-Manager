using SE104_Library_Manager.ViewModels;
using System.Windows;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for AddPositionWindow.xaml
    /// </summary>
    public partial class AddPositionWindow : Window
    {
        public AddPositionWindow(AddPositionViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
