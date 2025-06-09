using SE104_Library_Manager.ViewModels;
using System.Windows.Controls;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for ReadersView.xaml
    /// </summary>
    public partial class ReaderView : UserControl
    {
        public ReaderView(ReaderViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
