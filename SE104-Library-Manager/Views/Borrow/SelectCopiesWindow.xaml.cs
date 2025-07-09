using System.Windows;

namespace SE104_Library_Manager.Views.Borrow
{
    /// <summary>
    /// Interaction logic for SelectCopiesWindow.xaml
    /// </summary>
    public partial class SelectCopiesWindow : Window
    {
        public SelectCopiesWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
} 