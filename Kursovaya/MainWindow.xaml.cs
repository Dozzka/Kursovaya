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
            Test();
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

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }
        private bool Test() 
        {
            bool Contact = Logica.CheckDB();
            RadioState.IsChecked = Contact;
            if (Contact) 
            {
                ViewBT.IsEnabled = true;
                CreateBT.IsEnabled = true;
            }
            else 
            {
                ViewBT.IsEnabled = false;
                CreateBT.IsEnabled = false;
            }
            return Contact;
        }
        private void Change(object sender, RoutedEventArgs e)
        {
            RadioState.IsChecked = null;
            ViewBT.IsEnabled = false;
            CreateBT.IsEnabled = false;
            PathToDB window = new PathToDB();
            window.Show();
        }
    }
}