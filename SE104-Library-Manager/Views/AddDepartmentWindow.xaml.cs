using SE104_Library_Manager.ViewModels;
using System.Windows;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for AddDepartmentWindow.xaml
    /// </summary>
    public partial class AddDepartmentWindow : Window
    {
        public AddDepartmentWindow(AddDepartmentViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
