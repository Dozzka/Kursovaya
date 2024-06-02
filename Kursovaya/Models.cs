using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kursovaya
{
    //Модели для Добавления
    public class PlanItem
    {
        public int? id = null;
        public string DisciplineId { get; set; }
        public string Discipline { get; set; }
        public string LectId { get; set; }
        public string Lect { get; set; }
        public float Hours { get; set; }
        public PlanItem(string IdDiscip, string discip, string IdLect, string lect, float hours)
        {
            DisciplineId = IdDiscip;
            Discipline = discip;
            LectId = IdLect;
            Lect = lect;
            Hours = hours;
        }
        public PlanItem(int ID, string IdDiscip, string discip, string IdLect, string lect, float hours)
        {
            id = ID;
            DisciplineId = IdDiscip;
            Discipline = discip;
            LectId = IdLect;
            Lect = lect;
            Hours = hours;
        }
        public PlanItem(PlanItem original)
        {
            id = original.id;
            DisciplineId = original.DisciplineId;
            Discipline = original.Discipline;
            LectId = original.LectId;
            Lect = original.Lect;
            Hours = original.Hours;
        }
    }

    public class ComboBoxItemForAdd
    {
        public string Text { get; set; }
        public string Id { get; set; }
        public string IdSec { get; set; }


        public ComboBoxItemForAdd(string id, string text)
        {
            Text = text;
            Id = id;
            IdSec = "-1";
        }
        public ComboBoxItemForAdd(string id, string idSec, string text)
        {
            Text = text;
            Id = id;
            IdSec = idSec;
        }

        // Когда будет CB вызывать метод будет текст
        public override string ToString()
        {
            return Text;
        }
    }

    public class ListBoxItemForAdd : INotifyPropertyChanged
    {
        private int id;
        private string text;
        private bool checkBoxState;

        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        public bool CheckBoxState
        {
            get { return checkBoxState; }
            set { SetProperty(ref checkBoxState, value); }
        }

        public ListBoxItemForAdd(int id, string text, bool isChecked)
        {
            Id = id;
            Text = text;
            CheckBoxState = isChecked;
        }

        public ListBoxItemForAdd(int id, string text)
        {
            Id = id;
            Text = text;
            CheckBoxState = false;
        }

        public override string ToString()
        {
            return Text;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // Модель для времени
    public class ScheduleItemTime
    {
        public string Time { get; set; }
    }
    // логическая модель для блока в расписании
    public class DataOfRaspis : Canvas
    {
        public TextBlock Disciplin { get; set; }
        public TextBlock Lecturer { get; set; }
        public TextBlock HoursTB { get; set; }
        public ComboBox Auditorum { get; set; }
        public int Disciplin_ID { get; set; }
        public int Plan_ID { get; set; }
        public int Lecturer_ID { get; set; }
        public float Hours { get; set; }
        public bool fromDb { get; set; }

        // Визуальная модель для блока в листе дисциплин
        public DataOfRaspis(int Discip_ID, string discip, int plan_id, int lect_ID, string lect, float Hours)
        {
            Disciplin_ID = Discip_ID;
            Lecturer_ID = lect_ID;
            this.Hours = Hours;
            Plan_ID = plan_id;
            Rectangle rectangle = new Rectangle();
            Disciplin = new TextBlock();
            Lecturer = new TextBlock();
            HoursTB = new TextBlock();
            Children.Add(rectangle);
            Children.Add(Lecturer);
            Children.Add(Disciplin);
            Children.Add(HoursTB);

            Width = 150;
            Height = 150;
            rectangle.Width = Width;
            rectangle.Height = Height;
            rectangle.VerticalAlignment = VerticalAlignment.Top;
            rectangle.RadiusX = 10;
            rectangle.RadiusY = 10;
            rectangle.Fill = Brushes.LightGray;

            Disciplin.Text = discip;
            Disciplin.MaxWidth = rectangle.Width;
            Disciplin.Style = (Style)Application.Current.Resources["TextBlockStyle"];


            Lecturer.Text = lect;
            Lecturer.MaxWidth = rectangle.Width;
            Lecturer.Style = (Style)Application.Current.Resources["TextBlockStyle"];

            HoursTB.Text = Hours.ToString();
            HoursTB.MaxWidth = rectangle.Width;
            HoursTB.Style = (Style)Application.Current.Resources["TextBlockStyle"];

            Canvas.SetTop(Disciplin, 0);
            Canvas.SetTop(Lecturer, (Height - Lecturer.ActualHeight) / 2);
            Canvas.SetBottom(HoursTB, 0);

            Margin = new Thickness(0, 20, 0, 0);
            MouseMove += Canvas_MouseMove;
        }


        // Модель После переноса в DataGreed
        public DataOfRaspis(DataOfRaspis origData, string connectionString)
        {
            fromDb = false;
            Disciplin_ID = origData.Disciplin_ID;
            Lecturer_ID = origData.Lecturer_ID;
            Plan_ID = origData.Plan_ID;
            Rectangle rectangle = new Rectangle();
            Rectangle head = new Rectangle();
            Disciplin = new TextBlock();
            Lecturer = new TextBlock();
            Auditorum = new ComboBox();
            Auditorum.ItemsSource = Logica.CBLoader("Корпус", "Номер", "Аудитория", connectionString, "-");

            Children.Add(rectangle);
            Children.Add(head);
            Children.Add(Lecturer);
            Children.Add(Auditorum);
            Children.Add(Disciplin);
            Width = 150;
            Height = 150;

            head.Width = Width;
            head.Height = 30;
            head.VerticalAlignment = VerticalAlignment.Top;
            head.RadiusX = 10;
            head.RadiusY = 10;
            head.Fill = Brushes.LightBlue;

            rectangle.Width = Width;
            rectangle.Height = Height;
            rectangle.VerticalAlignment = VerticalAlignment.Top;
            rectangle.RadiusX = 10;
            rectangle.RadiusY = 10;
            rectangle.Fill = Brushes.LightGray;

            Disciplin.Text = origData.Disciplin.Text;

            Disciplin.TextWrapping = TextWrapping.Wrap;


            Lecturer.Text = origData.Lecturer.Text;
            Lecturer.TextWrapping = TextWrapping.Wrap;

            Canvas.SetTop(Disciplin, 0);
            Canvas.SetTop(Lecturer, (Height - Lecturer.ActualHeight) / 2);
            Canvas.SetBottom(Auditorum, 0);

            Margin = new Thickness(0, 20, 0, 0);

            head.MouseMove += Head_MouseMove;
            head.MouseRightButtonDown += Head_RightClick;

        }
        // Для погрузки из БД
        public DataOfRaspis(int Discip_ID, string discip, int plan_id, int lect_ID, string Corpus, string Audit, string lect, string connectionString)
        {
            fromDb = true;
            Disciplin_ID = Discip_ID;
            Lecturer_ID = lect_ID;
            Plan_ID = plan_id;
            Rectangle rectangle = new Rectangle();
            Rectangle head = new Rectangle();
            Disciplin = new TextBlock();
            Lecturer = new TextBlock();
            Auditorum = new ComboBox();
            Auditorum.ItemsSource = Logica.CBLoader("Корпус", "Номер", "Аудитория", connectionString, "-"); ;
            Auditorum.SelectedItem = Corpus + "-" + Audit;

            Children.Add(rectangle);
            Children.Add(head);
            Children.Add(Lecturer);
            Children.Add(Disciplin);
            Children.Add(Auditorum);

            Width = 150;
            Height = 150;

            head.Width = Width;
            head.Height = 30;
            head.VerticalAlignment = VerticalAlignment.Top;
            head.RadiusX = 10;
            head.RadiusY = 10;
            head.Fill = Brushes.LightBlue;

            rectangle.Width = Width;
            rectangle.Height = Height;
            rectangle.VerticalAlignment = VerticalAlignment.Top;
            rectangle.RadiusX = 10;
            rectangle.RadiusY = 10;
            rectangle.Fill = Brushes.LightGray;

            Disciplin.Text = discip;

            Disciplin.TextWrapping = TextWrapping.Wrap;


            Lecturer.Text = lect;
            Lecturer.TextWrapping = TextWrapping.Wrap;

            Canvas.SetTop(Disciplin, 0);
            Canvas.SetTop(Lecturer, (Height - Lecturer.ActualHeight) / 2);
            Canvas.SetBottom(Auditorum, 0);

            Margin = new Thickness(0, 20, 0, 0);

            Auditorum.SelectionChanged += Changed_CB;
            head.MouseMove += Head_MouseMove;
            head.MouseRightButtonDown += Head_RightClick;
        }

        private void Changed_CB(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            DataOfRaspis parent = FindParent<DataOfRaspis>(comboBox);
            parent.fromDb = false;
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvas = sender as Canvas;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(canvas, canvas, DragDropEffects.Copy);
            }
        }
        private void Head_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle head = sender as Rectangle;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Canvas canvas = VisualTreeHelper.GetParent(head) as Canvas;
                if (canvas != null)
                {
                    DragDrop.DoDragDrop(canvas, canvas, DragDropEffects.Copy);
                }
            }
        }
        private void Head_RightClick(object sender, MouseEventArgs e)
        {
            Rectangle head = sender as Rectangle;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Canvas parentCanvas = VisualTreeHelper.GetParent(head) as Canvas;
                if (parentCanvas != null)
                {
                    DataGridCell cell = FindParent<DataGridCell>(parentCanvas);
                    if (cell != null)
                    {
                        cell.Content = null;

                    }
                }
            }
        }
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }

}
