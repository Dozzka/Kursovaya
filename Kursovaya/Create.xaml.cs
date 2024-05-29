using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Kursovaya
{
    public partial class Create : Window
    {
        string connectionString
        {
            get
            {
                return @"Data Source=" + ConfigurationManager.AppSettings["PathToDb"];
            }
        }

        public Create()
        {
            InitializeComponent();
            GroupCB.ItemsSource = Logica.CBLoader("Номер", "Группа", connectionString);
        }

        // ЛистБокс с наименованиями предметов //

        private void LoadListDiscepline(string? Group)
        {
            ListDiscepline.Items.Clear();

            foreach (var i in Logica.GetDiscip(connectionString, Group))
            {
                DataOfRaspis dataOfRaspis = new DataOfRaspis(i.Item1, i.Item2, i.Item3, i.Item4, i.Item5, i.Item6);
                ListDiscepline.Items.Add(dataOfRaspis);
            }
        }



        ///////////////////////////////////////////////////////////
        //                   Р А С П И С А Н И Е                 //
        ///     ListBox <= Canvas <- Rectangle <= TextBlock      //
        /////////////////////////////////////////////////////////// 
        private void CreateRaspisSkeleton()
        {

            // Проверка валидности CB 
            string? Group = null;
            if (GroupCB.SelectedItem != null)
            {
                Group = GroupCB.SelectedItem.ToString();
            }
            else { MessageBox.Show("Нужно выбрать группу", "Ошибка", MessageBoxButton.OK); return; }
            DateTime dateTime;
            if (WeekChooser.SelectedDate != null)
            {
                dateTime = DateTime.Parse(WeekChooser.Text);
            }
            else { MessageBox.Show("Нужно выбрать неделю", "Ошибка", MessageBoxButton.OK); return; }

            List<(DateTime Date, string DayName)> weekDays = Logica.GetDaysOfWeek(dateTime);
            RaspisanieGrid.DataContext = Group;
            LoadListDiscepline(Group);

            RaspisanieGrid.ItemsSource = null; // Очистка старых данных
            RaspisanieGrid.Columns.Clear();

            DataGridTextColumn timeColumn = new DataGridTextColumn
            {
                Header = "Время",
                Binding = new Binding("Time"),
                Width = DataGridLength.Auto
            };
            RaspisanieGrid.Columns.Add(timeColumn);

            foreach (var day in weekDays)
            {
                DataGridTextColumn dayColumn = new DataGridTextColumn
                {
                    Header = $"{day.DayName} {day.Date:dd.MM.yy}",
                    //CellStyle = (Style)Resources["DateCell"],
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)

                };
                RaspisanieGrid.Columns.Add(dayColumn);
            }
            FillTimeRaspis();
            FillDataRaspis(Group, weekDays);
            Save.IsEnabled = true;
        }
        private void FillTimeRaspis()
        {
            List<string> times = Logica.GetTime(connectionString);
            List<ScheduleItemTime> scheduleItemTimes = new List<ScheduleItemTime>();
            foreach (string ItemTime in times)
            {
                scheduleItemTimes.Add(new ScheduleItemTime { Time = ItemTime.Replace("-", "\n") });
            }
            RaspisanieGrid.ItemsSource = scheduleItemTimes;
        }

        // Подгрузка инфы с бд
        private void FillDataRaspis(string Group, List<(DateTime Date, string DayName)> Dates)
        {
            List<(DataOfRaspis Disciplins, int x, int y)> Data = Logica.LoadDataForRaspis(connectionString, Group, Dates);

            RaspisanieGrid.UpdateLayout();

            foreach (var item in Data)
            {
                int column = item.x + 1;
                int row = item.y - 1;

                var cell = GetCell(row, column);
                if (cell != null)
                {
                    cell.Content = item.Disciplins;
                    item.Disciplins.MouseRightButtonDown += DataOfRaspis_MouseRightButtonDown;
                }
            }

            Update_Hours();
        }

        private void dataGrid_Drop(object sender, DragEventArgs e)
        {
            DataGrid grid = sender as DataGrid;

            if (grid != null)
            {
                Point dropPoint = e.GetPosition(grid);
                UIElement element = grid.InputHitTest(dropPoint) as UIElement;
                DataGridCell cell = GetParent<DataGridCell>(element);
                if (cell != null)
                {
                    if (e.Data.GetDataPresent(typeof(DataOfRaspis)))
                    {
                        DataOfRaspis CurrentCanvas = e.Data.GetData(typeof(DataOfRaspis)) as DataOfRaspis;
                        if (CurrentCanvas != null)
                        {
                            DataOfRaspis cell_content = new DataOfRaspis(CurrentCanvas, connectionString);
                            cell.Content = cell_content;
                            cell_content.MouseRightButtonDown += DataOfRaspis_MouseRightButtonDown;
                            DeleteCollision(cell_content, cell);
                            Update_Hours();
                        }
                    }
                }
            }
        }


        // Метод для получения ячейки по координатам
        private DataGridCell GetCell(int row, int column)
        {
            DataGridRow rowContainer = RaspisanieGrid.ItemContainerGenerator.ContainerFromIndex(row) as DataGridRow;
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                return cell;
            }
            return null;
        }

        private void DeleteCollision(DataOfRaspis data, DataGridCell cell)
        {
            var Time = GetRow(RaspisanieGrid, cell).Item;

            if (Time != null && Time.GetType() == typeof(ScheduleItemTime) && cell.Column != null)
            {
                ScheduleItemTime time = Time as ScheduleItemTime;
                var origSource = data.Auditorum.ItemsSource as IList<string>;
                List<string> ListAudotorum = Logica.GetCollisions(connectionString, data.Disciplin_ID, Convert.ToDateTime(cell.Column.Header).ToString("dd.MM.yyyy"), time.Time);
                var updatedSource = origSource.ToList();
                foreach (string item in ListAudotorum)
                {
                    updatedSource.Remove(item);
                }
                data.Auditorum.ItemsSource = new ObservableCollection<string>(updatedSource);
            }


        }
        public void Update_Hours()
        {
            ComboBoxItem comboBoxItem = new ComboBoxItem();
            Logica.GetHours(connectionString, ListDiscepline, RaspisanieGrid, GroupCB.Text, DateTime.Parse(WeekChooser.Text));
        }

        //  Родителя ищем x2
        public static T GetParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null && !(obj is T))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as T;
        }
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        private static DataGridRow GetRow(DataGrid dataGrid, DataGridCell cell)
        {
            DependencyObject parent = cell;
            while (parent != null && !(parent is DataGridRow))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as DataGridRow;
        }
        private void SaveBT_Click(object sender, RoutedEventArgs e)
        {
            Logica.LoadToDB(connectionString, RaspisanieGrid, GroupCB.SelectedItem.ToString());
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            CreateRaspisSkeleton();
        }

        private void GroupCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Save.IsEnabled = false;
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Adder adder = new Adder();
            adder.ShowDialog();
        }

        // При изменении тригер на изменение часов
        private void DataOfRaspis_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Update_Hours();
        }
    }
}
