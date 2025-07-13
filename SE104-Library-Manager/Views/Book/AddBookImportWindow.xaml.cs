using SE104_Library_Manager.ViewModels.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SE104_Library_Manager.Views.Book
{
    /// <summary>
    /// Interaction logic for AddBookImportWindow.xaml
    /// </summary>
    public partial class AddBookImportWindow : Window
    {
        public AddBookImportWindow(AddBookImportViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }
        private bool IsTextNumeric(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }
    }
}
