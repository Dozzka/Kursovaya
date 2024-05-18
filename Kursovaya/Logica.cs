using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Data.Sqlite;
using static Kursovaya.Create;
namespace Kursovaya
{
    static class Logica
    {
        ///////////////////////////////////////////////
        //                Общая логика               //
        ///////////////////////////////////////////////

        //          Подргузка в Combobox            //

        static public List<string> CBLoader(string What, string From, string connectionString)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> items = new List<string>();
                connection.Open();
                try
                {
                    SqliteCommand command = new SqliteCommand(@$"SELECT {What} FROM {From}", connection);
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        items.Add(reader.GetString(0));
                    }
                    return items;
                }
                catch { return items; }
                finally { connection.Close(); }
            }
        }

        static public List<string> CBLoader(string What1, string What2, string From, string connectionString, string Sep = " ")
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> items = new List<string>();
                connection.Open();
                try
                {
                    SqliteCommand command = new SqliteCommand(@$"SELECT {What1},{What2} FROM {From}", connection);
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        items.Add(reader.GetString(0) + Sep.ToString() + reader.GetString(1));
                    }
                    return items;
                }
                catch (Exception ex) { return items; }
                finally { connection.Close(); }
            }
        }
        ///////////////////////////////////////////////
        //          Логика для VIEWER                //
        ///////////////////////////////////////////////
        //          Подргузка в DataGreed            //
        static public DataTable LoadDataGreed(string connectionString)
        {
            DataTable dataTable = new DataTable();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                try
                {
                    string sqlQuery = "SELECT Дата, [День Недели].Название День, Пара.Начало || '-' || Пара.Конец AS Время, " +
                        "Аудитория_Корпус || '-' || Аудитория_Номер AS Аудитория, " +
                        "GROUP_CONCAT(Группа.Номер, ', ') AS Группа, Дисциплина.Название AS Дисциплина, " +
                        "Преподаватель.Фамилия || ' ' || Преподаватель.Имя || ' ' || IFNULL(Преподаватель.Отчество,'') AS Преподаватель " +
                        "FROM Расписание " +
                        "INNER JOIN [День Недели] ON [День Недели_ID] = [День Недели].ID " +
                        "INNER JOIN Группа ON Группа_ID = Группа.ID " +
                        "INNER JOIN Пара ON [Пара_Номер пары] = Пара.[Номер пары] " +
                        "INNER JOIN [Учебный План] ON [Учебный План].ID = [Учебный План_ID] " +
                        "INNER JOIN Дисциплина ON [Учебный План].ID = Дисциплина.ID " +
                        "INNER JOIN Преподаватель ON Преподаватель_ID = Преподаватель.ID";



                    SqliteCommand command = new SqliteCommand(sqlQuery, connection);
                    SqliteDataReader reader = command.ExecuteReader();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dataTable.Columns.Add(reader.GetName(i));
                    }

                    while (reader.Read())
                    {
                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader[i];
                        }
                        dataTable.Rows.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка");
                }
                finally
                {
                    connection.Close();
                }
            }


            return dataTable;
        }
        ///////////////////////////////////////////////
        //          Логика для CREATE                //
        ///////////////////////////////////////////////
        static public List<string> GetTime(string connectionString)
        {
            string sqlQuery = "SELECT Начало || ' ' || Конец FROM Пара";
            List<string> time = new List<string>();
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                try
                {
                    SqliteCommand command = new SqliteCommand(sqlQuery, connection);
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        time.Add(reader.GetString(0));
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка");
                }
                finally
                {
                    connection.Close();
                }
            }
            return time;
        }
        // Даты недели из дня
        public static List<DateTime> GetDaysOfWeek(DateTime DateDay)
        {
            List<DateTime> weekDates = new List<DateTime>();
            // Найти дату понедельника
            
            DateTime monday = DateDay.AddDays(-(int)DateDay.DayOfWeek + (int)DayOfWeek.Monday);
            for (int i = 0; i < 6; i++)
            {
                weekDates.Add(monday.AddDays(i));
            }
            return weekDates;
        } 
        // Подгрузка Дисциплин в LISTBOX

        /*            Возвращает Лист(Дисциплина_ID, Дисциплина_Название,   ФИО_Преподователя(3), Кол-во часов)*/
        public static List<(int, string, int, string, float)> GetDiscip(string connectionString, string GroupName)
        {
            List<(int, string, int, string, float)> result = new List<(int, string, int, string, float)>();
            string query = "SELECT Дисциплина.ID, Дисциплина.Название, Преподаватель.ID, Преподаватель.Фамилия, " +
                           "Преподаватель.Имя, COALESCE(Преподаватель.Отчество, '') , [Учебный План].[Кол-во часов] " +
                           "FROM Дисциплина " +
                           "INNER JOIN ПреподИДисциплина ON Дисциплина.ID = ПреподИДисциплина.Дисциплина_ID " +
                           "INNER JOIN Преподаватель ON ПреподИДисциплина.Преподаватель_ID = Преподаватель.ID " +
                           "INNER JOIN [Учебный План] ON Дисциплина.ID = [Учебный План].Дисциплина_ID " +
                           "INNER JOIN Курс ON [Учебный План].Курс_ID = Курс.ID " +
                           "INNER JOIN Группа ON Курс.ID = Группа.Курс_ID " +
                           "WHERE Группа.Номер = @GroupName";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                try
                {
                    SqliteCommand command = new SqliteCommand(query, connection);
                    command.Parameters.AddWithValue("@GroupName", GroupName);
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Считываем данные из результата запроса
                        int discipID = reader.GetInt32(0);
                        string disciplineName = reader.GetString(1);
                        int teacherID = reader.GetInt32(2);
                        string teacherName = reader.GetString(3) + " " + reader.GetString(4) + " " + reader.GetString(5);
                        float hours = reader.GetFloat(6);
                        result.Add((discipID, disciplineName, teacherID, teacherName, hours));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка");
                }
                finally
                {
                    connection.Close();
                }
            }
            return result;
        }

    }




    // Конвертер для резиновой верстки
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double percentage = 100;
            if (value is double && double.TryParse(parameter.ToString(), out percentage))
            {
                double actualValue = (double)value;
                return actualValue * percentage;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
