using SE104_Library_Manager.ViewModels.Policy;
using System.Windows.Controls;

namespace SE104_Library_Manager.Views.Policy
{
    /// <summary>
    /// Interaction logic for PolicyView.xaml
    /// </summary>
    public partial class PolicyView : UserControl
    {
        public PolicyView(PolicyViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
