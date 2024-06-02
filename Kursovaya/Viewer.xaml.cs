using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
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
    /// Логика взаимодействия для Viewer.xaml
    /// </summary>
    public partial class Viewer : Window
    {
        string connectionString
        {
            get
            {
                return @"Data Source=" + ConfigurationManager.AppSettings["PathToDb"];
            }
        }
        public Viewer()
        {
            InitializeComponent();
            Loading();
        }


        private void Loading()
        {
            GroupCB.ItemsSource = Logica.CBLoader("Номер", "Группа", connectionString);
            AudCB.ItemsSource = Logica.CBLoader("Корпус","Номер","Аудитория",connectionString,"-");
            Data.ItemsSource = Logica.LoadDataGreed(connectionString).DefaultView;
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null) 
            {
                comboBox.IsDropDownOpen = true;
            }
            
        }


        private void GroupCB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (Data.ItemsSource != null)
            {
                string filterText = comboBox.Text;
                (Data.ItemsSource as DataView).RowFilter = $"Группа LIKE '%{comboBox.Text}%'";
            }
        }
     }
}

