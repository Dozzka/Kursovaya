﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Xml.Linq;
using System.Reflection.Metadata;
namespace Kursovaya
{
    static class Logica
    {
        ///////////////////////////////////////////////
        //                Общая логика               //
        ///////////////////////////////////////////////

        // Проверка что база данных существует и работает
        public static bool CheckDB()
        {
            string PathToDb = ConfigurationManager.AppSettings["PathToDb"];
            if (File.Exists(PathToDb))
            {
                List<string> reference = new List<string> { "Аудитория", "Группа",
                                                            "Дисциплина", "Курс",
                                                            "Пара", "ПреподИДисциплина",
                                                            "Преподаватель", "Расписание",
                                                            "Тип Курса", "Учебный План"};
                string connecionstr = @"Data Source=" + PathToDb;
                using (SqliteConnection connection = new SqliteConnection(connecionstr))
                {
                    connection.Open();
                    List<string> FromDB = new List<string>();
                    string query = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tableName = reader.GetString(0);
                                FromDB.Add(tableName);
                            }
                        }
                    }
                    if (reference.All(r => FromDB.Contains(r))) { return true; }
                }
            }

            return false;
        }

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
                    string sqlQuery = "SELECT Дата, Пара.Начало || '-' || Пара.Конец AS Время, " +
                        "Аудитория_Корпус || '-' || Аудитория_Номер AS Аудитория, " +
                        "GROUP_CONCAT(Группа.Номер, ', ') AS Группа, Дисциплина.Название AS Дисциплина, " +
                        "Преподаватель.Фамилия || ' ' || Преподаватель.Имя || ' ' || IFNULL(Преподаватель.Отчество,'') AS Преподаватель " +
                        "FROM Расписание " +
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
        public static List<(DateTime, string)> GetDaysOfWeek(DateTime DateDay)
        {
            List<(DateTime, string)> weekDates = new List<(DateTime, string)>();
            // Найти дату понедельника
            DateTime startOfWeek = DateDay;
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }
            for (int i = 0; i < 6; i++)
            {
                DateTime currentDate = startOfWeek.AddDays(i);
                string dayName = currentDate.ToString("dddd", new System.Globalization.CultureInfo("ru-RU"));
                dayName = char.ToUpper(dayName[0]) + dayName.Substring(1);
                weekDates.Add((currentDate, dayName));
            }
            return weekDates;
        }

        // Подгрузка Дисциплин в LISTBOX

        /*            Возвращает Лист(Дисциплина_ID, Дисциплина_Название, План_ID,  ФИО_Преподователя(3), Кол-во часов)*/
        public static List<(int, string, int, int, string, float)> GetDiscip(string connectionString, string GroupName)
        {
            List<(int, string, int, int, string, float)> result = new List<(int, string, int, int, string, float)>();
            string query = @"SELECT Дисциплина.ID, Дисциплина.Название,[Учебный План].ID, Преподаватель.ID, Преподаватель.Фамилия, 
                           Преподаватель.Имя, COALESCE(Преподаватель.Отчество, '') , [Учебный План].[Кол-во часов] 
                           FROM Группа
                           INNER JOIN Курс ON Группа.Курс_ID = Курс.ID
                           INNER JOIN [Учебный План] ON Курс.ID = [Учебный План].Курс_ID
                           INNER JOIN Дисциплина ON [Учебный План].Дисциплина_ID = Дисциплина.ID
                           INNER JOIN Преподаватель ON [Учебный План].Препод_ID = Преподаватель.ID
                           WHERE Группа.Номер = @GroupName";

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
                        int planId = reader.GetInt32(2);
                        int teacherID = reader.GetInt32(3);
                        string teacherName = reader.GetString(4) + " " + reader.GetString(5) + " " + reader.GetString(6);
                        float hours = reader.GetFloat(7);
                        result.Add((discipID, disciplineName, planId, teacherID, teacherName, hours));
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
        // Подгрузка данных для расписания
        public static List<(DataOfRaspis Disciplins, int x, int y)> LoadDataForRaspis(string connectionString, string Group, List<(DateTime Date, string)> Data)
        {
            List<(DataOfRaspis Disciplins, int x, int y)> DisciplinsList = new List<(DataOfRaspis, int x, int y)>();
            string query = @"SELECT Дата, Аудитория_Корпус, Аудитория_Номер, [Пара_Номер пары],
                            [Учебный План_ID], [Учебный План].Дисциплина_ID as ДисциплинаID, Дисциплина.Название as НазвДисцип,
                            Преподаватель_ID,[Пара_Номер пары], Преподаватель.Фамилия as LN, Преподаватель.Имя AS FN, Преподаватель.Отчество AS FatN
                            FROM Расписание INNER JOIN [Учебный План] ON [Учебный План].ID = Расписание.[Учебный План_ID]
                            INNER JOIN Дисциплина ON Дисциплина.ID = [Учебный План].Дисциплина_ID
                            INNER JOIN Преподаватель ON Расписание.Преподаватель_ID = Преподаватель.ID 
                            INNER JOIN Группа ON Расписание.Группа_ID = Группа.ID
                            WHERE Группа.Номер = @Group AND substr(Дата,7)||substr(Дата,4,2)||substr(Дата,1,2) BETWEEN @StartDate AND @EndDate";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@StartDate", Data[0].Date.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@EndDate", Data[Data.Count - 1].Date.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@Group", Group);
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    //Заполнение дисциплин
                    string date = reader["Дата"].ToString();
                    int x = Data.FindIndex(item => item.Date.ToString("dd.MM.yyyy") == date);
                    int y = Convert.ToInt32(reader["Пара_Номер пары"]);
                    if (x < 0) { continue; }
                    int plan = Convert.ToInt32(reader["Учебный План_ID"]);
                    int DiscipId = Convert.ToInt32(reader["ДисциплинаID"]);
                    string DiscipName = reader["НазвДисцип"].ToString();
                    int LectId = Convert.ToInt32(reader["Преподаватель_ID"]);
                    string lectLas = reader["LN"].ToString();
                    string lectFir = reader["FN"].ToString();
                    string? lectFat = reader["FatN"].ToString();
                    string lectFullName = lectLas + " " + lectFir + " " + lectFat;
                    string Corpus = reader["Аудитория_Корпус"].ToString();
                    string Audit = reader["Аудитория_Номер"].ToString();

                    DataOfRaspis disciplin = new DataOfRaspis(DiscipId, DiscipName, plan, LectId, Corpus, Audit, lectFullName, connectionString);
                    DisciplinsList.Add((disciplin, x, y));
                }
                return DisciplinsList;
            }
        }

        // Узнать аудиторию для дисциплины на DG
        public static List<string> GetCollisions(string connectionString, int discipId, string Date, string time)
        {
            List<string> result = new List<string>();
            int pairNumber = -1;
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand(@"SELECT [Номер пары] 
                                                            FROM Пара
                                                            WHERE Начало = @Start AND
                                                            Конец = @End", connection);
                command.Parameters.AddWithValue("@Start", time.Split(' ')[0]);
                command.Parameters.AddWithValue("@End", time.Split(' ')[1]);
                SqliteDataReader reader = command.ExecuteReader();
                {
                    if (reader.Read())
                    {
                        pairNumber = reader.GetInt32(0);
                    }
                }
                SqliteCommand Collcommand = new SqliteCommand(@"SELECT Аудитория_Корпус, Аудитория_Номер FROM Расписание
                                                            WHERE [Пара_Номер пары] = @Para AND Дата = @Date AND
                                                            [Учебный План_ID] != @Plan", connection);
                Collcommand.Parameters.AddWithValue("@Para", pairNumber);
                Collcommand.Parameters.AddWithValue("@Date", Date);
                Collcommand.Parameters.AddWithValue("@Plan", discipId);
                SqliteDataReader readerColl = Collcommand.ExecuteReader();
                while (readerColl.Read())
                {
                    result.Add(readerColl.GetString(0) + "-" + readerColl.GetString(1));
                }
            }
            return result;
        }

        public static void GetHours(string connectionString, ListBox listBox, DataGrid current_DGV, string group, DateTime Day)
        {
            List<(DateTime, string)> Week = GetDaysOfWeek(Day);
            foreach (var Block in listBox.Items)
            {
                if (Block.GetType() == typeof(DataOfRaspis))
                {
                    DataOfRaspis ListItem = (DataOfRaspis)Block;
                    float result;
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        int IdGroup = -1;
                        SqliteCommand GetGroup = new SqliteCommand(@"SELECT ID
                                                                 FROM Группа
                                                                 WHERE Номер = @Group", connection);
                        GetGroup.Parameters.AddWithValue("@Group", group);
                        SqliteDataReader reader = GetGroup.ExecuteReader();
                        while (reader.Read())
                        {
                            IdGroup = Convert.ToInt32(reader["ID"]);
                        }
                        string query = @"SELECT COUNT(*) FROM Расписание
                                    WHERE Группа_ID = @GroupId AND Дата NOT BETWEEN @Start
                                    AND @End AND [Учебный План_ID] = @Plan";
                        SqliteCommand GetHours = new SqliteCommand(query, connection);
                        GetHours.Parameters.AddWithValue("@GroupId", IdGroup);
                        GetHours.Parameters.AddWithValue("@Start", Week[0].Item1.ToShortDateString());
                        GetHours.Parameters.AddWithValue("@End", Week[Week.Count - 1].Item1.ToShortDateString());
                        GetHours.Parameters.AddWithValue("@Plan", ListItem.Plan_ID);
                        result = Convert.ToSingle(GetHours.ExecuteScalar());

                        foreach (var rowItem in current_DGV.Items)
                        {
                            foreach (var column in current_DGV.Columns)
                            {
                                var cellContent = column.GetCellContent(rowItem);

                                if (cellContent != null)
                                {
                                    if (cellContent.GetType() == typeof(DataOfRaspis))
                                    {
                                        DataOfRaspis item = cellContent as DataOfRaspis;
                                        if (item.Plan_ID == ListItem.Plan_ID)
                                        {
                                            result++;
                                        }
                                    }
                                }
                            }
                        }
                        float CurrentHours = (ListItem.Hours - result * 1.5f);
                        ListItem.HoursTB.Text = CurrentHours.ToString();
                        if ((int)CurrentHours == 0)
                        {
                            ListItem.IsEnabled = false;
                        }

                    }
                }
            }

        }
        public static void LoadToDB(string connectionString, DataGrid dataGrid, string group)
        {
            List<(string date, string time)> listNull = new List<(string, string)>();
            List<(string date, string time, DataOfRaspis data)> listData = new List<(string, string, DataOfRaspis)>();


            foreach (var rowItem in dataGrid.Items)
            {
                foreach (var column in dataGrid.Columns)
                {
                    var cellContent = column.GetCellContent(rowItem);


                    ScheduleItemTime time = rowItem as ScheduleItemTime;
                    string currentTime = time.Time;

                    DataGridTextColumn date = column as DataGridTextColumn;
                    string currentDate = date.Header.ToString();


                    if (cellContent != null)
                    {
                        if (cellContent.GetType() == typeof(DataOfRaspis))
                        {
                            DataOfRaspis item = cellContent as DataOfRaspis;
                            if (item.Auditorum.SelectedItem == null)
                            {
                                MessageBox.Show("Не везде выставленна аудитория", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            listData.Add((Convert.ToDateTime(currentDate).ToString("dd.MM.yyyy"), currentTime, item));
                        }
                    }
                    else
                    {
                        listNull.Add((Convert.ToDateTime(currentDate).ToString("dd.MM.yyyy"), currentTime));
                    }
                }
            }


            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                SqliteTransaction transaction = connection.BeginTransaction();
                try
                {
                    int IdGroup = -1;
                    SqliteCommand GetGroup = new SqliteCommand(@"SELECT ID
                                                                FROM Группа
                                                                WHERE Номер = @Group", connection);
                    GetGroup.Transaction = transaction;
                    GetGroup.Parameters.AddWithValue("@Group", group);
                    SqliteDataReader reader = GetGroup.ExecuteReader();
                    while (reader.Read())
                    {
                        IdGroup = Convert.ToInt32(reader["ID"]);
                    }

                    foreach (var nulls in listNull)
                    {
                        // Добыча id_пара 
                        int idPara = -1;
                        SqliteCommand command = new SqliteCommand(@"SELECT [Номер пары] 
                                                                    FROM Пара
                                                                    WHERE Начало = @Start AND Конец = @End", connection);
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@Start", nulls.time.Split(' ')[0]);
                        command.Parameters.AddWithValue("@End", nulls.time.Split(' ')[1]);
                        SqliteDataReader reader1 = command.ExecuteReader();
                        while (reader1.Read())
                        {
                            idPara = Convert.ToInt32(reader1["Номер Пары"]);
                        }

                        SqliteCommand deleteCom = new SqliteCommand(@"DELETE FROM Расписание
                                                                    WHERE [Пара_Номер пары] = @Para AND
                                                                    Дата = @Date AND
                                                                    Группа_ID = @Group", connection);
                        deleteCom.Transaction = transaction;
                        deleteCom.Parameters.AddWithValue("@Para", idPara);
                        deleteCom.Parameters.AddWithValue("@Date", nulls.date);
                        deleteCom.Parameters.AddWithValue("@Group", IdGroup);
                        deleteCom.ExecuteNonQuery();
                    }
                    foreach (var item in listData)
                    {

                        if (!item.data.fromDb)
                        {
                            int idPara = -1;
                            SqliteCommand command = new SqliteCommand(@"SELECT [Номер пары] 
                                                                        FROM Пара
                                                                        WHERE Начало = @Start AND
                                                                        Конец = @End", connection);
                            command.Transaction = transaction;
                            command.Parameters.AddWithValue("@Start", item.time.Split(' ')[0]);
                            command.Parameters.AddWithValue("@End", item.time.Split(' ')[1]);
                            SqliteDataReader reader1 = command.ExecuteReader();
                            while (reader1.Read())
                            {
                                idPara = Convert.ToInt32(reader1["Номер Пары"]);
                            }
                            // Посадочное место пусто.
                            SqliteCommand DelForNewData = new SqliteCommand(@"DELETE FROM Расписание
                                                                              WHERE Дата = @Date AND
                                                                              [Пара_Номер пары] = @Para AND
                                                                              Группа_ID = @Group", connection);
                            DelForNewData.Transaction = transaction;
                            DelForNewData.Parameters.AddWithValue("@Date", item.date);
                            DelForNewData.Parameters.AddWithValue("@Para", idPara);
                            DelForNewData.Parameters.AddWithValue("@Group", IdGroup);
                            DelForNewData.ExecuteNonQuery();

                            // Сохранение изменений
                            SqliteCommand AddCommand = new SqliteCommand(@"
                            INSERT INTO Расписание 
                            VALUES(@Date,@AudCorp,@AudNum,@Plan,@Para,@LectId,@GroupId)", connection);
                            AddCommand.Transaction = transaction;
                            AddCommand.Parameters.AddWithValue("@Date", item.date);
                            var SplitInf = item.data.Auditorum.SelectedItem.ToString().Split('-');
                            AddCommand.Parameters.AddWithValue("@AudCorp", SplitInf[0]);
                            AddCommand.Parameters.AddWithValue("@AudNum", SplitInf[1]);
                            AddCommand.Parameters.AddWithValue("@Plan", item.data.Plan_ID);
                            AddCommand.Parameters.AddWithValue("@Para", idPara);
                            AddCommand.Parameters.AddWithValue("@LectId", item.data.Lecturer_ID);
                            AddCommand.Parameters.AddWithValue("@GroupId", IdGroup);
                            AddCommand.ExecuteNonQuery();

                        }
                    }
                    transaction.Commit();
                    MessageBox.Show("Сохранение прошло успешно", "Успех");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при выполнении сохранения: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        // Для подгрузки инфы для Комбобоксов в Constructor
        public static List<Dictionary<string, string>> GetCBForAdd(string connectionString, string tabName)
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
            string query;
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                switch (tabName)
                {
                    case "TypeCourse":
                        query = @"SELECT ID,Название FROM [Тип Курса]";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var content = new Dictionary<string, string>
                                {
                                    {"id", reader.GetInt32(0).ToString()},
                                    { "Тип", reader.GetString(1) }
                                };
                                data.Add(content);
                            }
                        }
                        break;
                    case "Course":
                        query = @"SELECT ID,Название_Курса FROM Курс";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var content = new Dictionary<string, string>
                                {
                                    {"id", reader.GetInt32(0).ToString()},
                                    { "Название_Курса", reader.GetString(1) }
                                };
                                data.Add(content);
                            }
                        }
                        break;

                    case "Lect":
                        query = @"SELECT ID,Фамилия,Имя, Отчество FROM Преподаватель ORDER BY Фамилия";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var content = new Dictionary<string, string>
                                {
                                    {"id", reader.GetInt32(0).ToString()},
                                    { "ФИО", reader.GetString(1) + " " + reader.GetString(2) + (!reader.IsDBNull(3) ? " " + reader.GetString(3) : null)}
                                  
                                };
                                data.Add(content);
                            }
                        }
                        break;

                    case "Plan":
                        query = @"SELECT DISTINCT Курс_ID, Название FROM [Учебный План]";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var content = new Dictionary<string, string>
                                {
                                    {"id", reader.GetInt32(0).ToString()},
                                    {"Название", reader.GetString(1)}
                                };
                                data.Add(content);
                            }
                        }
                        break;
                    case "Discip":
                        query = @"SELECT ID,Название FROM Дисциплина";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                var content = new Dictionary<string, string>
                                {
                                    {"id", reader.GetInt32(0).ToString()},
                                    { "Название", reader.GetString(1) },

                                };
                                data.Add(content);
                            }
                        }
                        break;
                    case "Group":
                        query = @"SELECT ID,Номер FROM Группа";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var content = new Dictionary<string, string>
                                {
                                    {"id", reader.GetInt32(0).ToString()},
                                    { "Номер", reader.GetString(1) },
                                };
                                data.Add(content);
                            }
                        }
                        break;
                    case "Auditor":
                        query = @"SELECT Корпус,Номер FROM Аудитория";
                        using (SqliteCommand command = new SqliteCommand(query, connection))
                        {
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var content = new Dictionary<string, string>
                                {
                                    { "Корпус", reader.GetString(0)},
                                    { "Номер", reader.GetString(1) }
                                };
                                data.Add(content);
                            }
                        }
                        break;
                    default:
                        return data;
                }
            }
            return data;
        }
        // Узнать какие щас есть предметы у Преподавателя
        public static List<int> GetLectDiscip(string connectionString, int LectId)
        {
            List<int> result = new List<int>();
            using (SqliteConnection connection = new SqliteConnection(connectionString)) 
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand(@"SELECT Дисциплина_ID FROM ПреподИДисциплина
                                                            WHERE Преподаватель_ID = @LectID", connection);
                command.Parameters.AddWithValue("@LectID", LectId);
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read()) 
                {
                    result.Add(reader.GetInt32(0));
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
                double result = actualValue * percentage;
                return result > 1 ? result : 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}