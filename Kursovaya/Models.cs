using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Kursovaya
{
    public class ScheduleItemTime // Модель для времени
    {
        public string Time { get; set; }
    }
    public class DataOfRaspis : Canvas // Модель для блока в расписании
    {
        public TextBlock Disciplin { get; set; }
        public TextBlock Lecturer { get; set; }
        public TextBlock HoursTB { get; set; }
        public ComboBox Auditorum { get; set; }
        public int Disciplin_ID { get; set; }
        public int Lecturer_ID { get; set; }
        public float Hours { get; set; }



        public DataOfRaspis(int Discip_ID, string discip,int lect_ID, string lect, float Hours) 
            {
            Disciplin_ID = Discip_ID;
            Lecturer_ID = lect_ID;
            this.Hours = Hours;

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
        public DataOfRaspis(DataOfRaspis origData, string connectionString) //Модель После переноса
        {


            Disciplin_ID = origData.Disciplin_ID;
            Lecturer_ID = origData.Lecturer_ID;
            Hours = origData.Hours;
            Rectangle rectangle = new Rectangle();
            Rectangle head = new Rectangle();
            Disciplin = new TextBlock();
            Lecturer = new TextBlock();
            Auditorum = new ComboBox();
            Auditorum.ItemsSource = Logica.CBLoader("Корпус", "Номер", "Аудитория", connectionString, "-"); ;

            
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

            Disciplin.Text = origData.Disciplin.Text;

            Disciplin.TextWrapping = TextWrapping.Wrap;


            Lecturer.Text = origData.Lecturer.Text;
            Lecturer.TextWrapping = TextWrapping.Wrap;

            Canvas.SetTop(Disciplin, 0);
            Canvas.SetTop(Lecturer, (Height - Lecturer.ActualHeight) / 2);
            Canvas.SetBottom(Auditorum,0);

            Margin = new Thickness(0, 20, 0, 0);
            head.MouseMove += Head_MouseMove;
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
    }
}
