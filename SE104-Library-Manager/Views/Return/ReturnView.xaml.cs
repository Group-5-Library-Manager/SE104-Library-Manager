using SE104_Library_Manager.ViewModels;

using System.Windows.Controls;

namespace SE104_Library_Manager.Views.Return
{
    /// <summary>
    /// Interaction logic for ReturnView.xaml
    /// </summary>
    public partial class ReturnView : UserControl
    {
        public ReturnView(ReturnViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}