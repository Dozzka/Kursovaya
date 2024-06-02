using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public partial class Edit : Window
    {
        string connectionString
        {
            get
            {
                return @"Data Source=" + ConfigurationManager.AppSettings["PathToDb"];
            }
        }
        public int id;
        public string PlanName;
        public int Course;
        List<(int discipid, int lectid)> wrongData = new List<(int discipid, int lectid)>();
        public Edit(List<PlanItem> planItems, PlanItem planItem, string CourseId, string PlanName)
        {
            InitializeComponent();
            id = (int)planItem.id;
            PlanName = PlanName;
            Course = Convert.ToInt32(CourseId);
            Hours.Text = planItem.Hours.ToString();
            ID.Text = planItem.id.ToString();
            foreach (PlanItem item in planItems)
            {
                if (item != planItem)
                {
                    wrongData.Add((Convert.ToInt32(item.DisciplineId), Convert.ToInt32(item.LectId)));
                }
            }
            var newItem = new ComboBoxItemForAdd("-1", "-");
            Prepod.Items.Add(newItem);
            Prepod.SelectedItem = newItem;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                SqliteCommand GetDiscip = new SqliteCommand(@"SELECT ID, Название FROM Дисциплина", connection);
                using (SqliteDataReader reader = GetDiscip.ExecuteReader())
                { while (reader.Read())
                    {
                        ComboBoxItemForAdd newitem = new ComboBoxItemForAdd(reader.GetInt32(0).ToString(), reader.GetString(1));
                        Discip.Items.Add(newitem);
                        if (newitem.Id == planItem.DisciplineId)
                        {
                            Discip.SelectedItem = newitem;
                        }

                    }
                }

                SqliteCommand GetLect = new SqliteCommand(@"SELECT Преподаватель_ID, Фамилия, Имя, Отчество 
                                                        FROM ПреподИДисциплина INNER JOIN Преподаватель ON Преподаватель_ID = Преподаватель.ID
                                                        WHERE Дисциплина_ID = @DiscipId", connection);
                GetLect.Parameters.AddWithValue("@DiscipId", Convert.ToInt32(((ComboBoxItemForAdd)Discip.SelectedItem).Id));
                using (SqliteDataReader reader = GetLect.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string LectId = reader.GetInt32(0).ToString();
                        string LName = reader.GetString(1);
                        string FName = reader.GetString(2);
                        string? FatName = reader.IsDBNull(3)? null: " " +reader.GetString(3);
                        string fio = "(" + LectId + ")" + LName + " " + FName + FatName;
                        ComboBoxItemForAdd newItemLect = new ComboBoxItemForAdd(LectId, fio);
                        Prepod.Items.Add(newItemLect);
                        if(newItemLect.Id == planItem.LectId) 
                        {
                            Prepod.SelectedItem = newItemLect;
                        }
                    }
                }
            }
        }

        private void Commit(object sender, RoutedEventArgs e)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                try
                {
                    float hours = 0f;
                    if (Hours.Text.Replace(" ", "") == "")
                    {
                        throw new InvalidOperationException("Не число");
                    }
                    if (!float.TryParse(Hours.Text, out hours) || hours <= 0)
                    {
                        throw new InvalidOperationException("Не верно введено кол-во часов");
                    }
                    if (hours % 1.5 != 0)
                    {
                        throw new InvalidOperationException("Должно быть кратно 1.5");
                    }
                    SqliteCommand command = new SqliteCommand(@"UPDATE [Учебный План] 
                                                SET Курс_ID = @Course, Дисциплина_ID = @DiscipId, Препод_ID = @LectId, [Кол-во часов] = @Hours 
                                                WHERE ID = @id",connection);
                    command.Parameters.AddWithValue("@NamePlan", PlanName);
                    command.Parameters.AddWithValue("@Course", Course);
                    command.Parameters.AddWithValue("@DiscipId", ((ComboBoxItemForAdd)Discip.SelectedItem).Id);
                    command.Parameters.AddWithValue("@LectId", ((ComboBoxItemForAdd)Prepod.SelectedItem).Id);
                    command.Parameters.AddWithValue("@Hours", hours);
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
