using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Kursovaya
{
    public partial class Create : Window
    {
        string connectionString = $@"Data Source = E:\Учеба\Управление Данными\Raspisanie.db";


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
                DataOfRaspis dataOfRaspis = new DataOfRaspis(i.Item1,i.Item2,i.Item3,i.Item4,i.Item5);
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

            foreach(var day in weekDays) 
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
            FillDataRaspis(Group,weekDays);
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
        private void FillDataRaspis(string Group, List<(DateTime Date, string DayName)> Dates) 
        {

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvas = sender as Canvas;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(canvas, canvas, DragDropEffects.Copy);
            }
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
                            cell.Content = new DataOfRaspis(CurrentCanvas,connectionString);
                            
                        }
                    }
                }
            }
        }


    //  Родителя ищем
    public static T GetParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null && !(obj is T))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as T;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateRaspisSkeleton();
        }

        private void SaveBT_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
