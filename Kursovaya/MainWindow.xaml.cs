using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kursovaya
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Viewer(object sender, RoutedEventArgs e)
        {
            Viewer WindowView = new Viewer();
            WindowView.Show();
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            Create WindowView = new Create();
            WindowView.Show();
        }
    }
}