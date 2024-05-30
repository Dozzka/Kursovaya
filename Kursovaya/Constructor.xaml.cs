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
    public partial class Constructor : Window
    {
        public Constructor(string name)
        {
            InitializeComponent();
            switch (name) 
            {
                case "Course":
                    SelectTabByName("CourseTab");
                    break;
                case "Lect":
                    SelectTabByName("LectTab");
                    break;
                case "Plan":
                    SelectTabByName("PlanTab");
                    break;
                case "Discip":
                    SelectTabByName("DiscipTab");
                    break;
                case "Group":
                    SelectTabByName("GroupTab");
                    break;
                case "Auditor":
                    SelectTabByName("AuditorTab");
                    break;
                default:
                    MessageBox.Show("Нет такой вкладки","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                    break;
            }
        }
        private void SelectTabByName(string tabName)
        {
            foreach (TabItem tab in Tabs.Items)
            {
                if (tab.Name == tabName)
                {
                    Tabs.SelectedItem = tab;
                    Show();
                    break;
                }
            }
        }
    }
}
