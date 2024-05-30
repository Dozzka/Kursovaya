using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для Adder.xaml
    /// </summary>
    public partial class Adder : Window
    {
        public Adder()
        {
            InitializeComponent();
        }

        private void BT_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Constructor constructor = new Constructor(button.Name);
        }
    }
}
