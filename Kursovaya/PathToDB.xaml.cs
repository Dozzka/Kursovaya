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
using System.IO;
using Microsoft.Win32;
using System.Configuration;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography.X509Certificates;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для PathToDB.xaml
    /// </summary>
    public partial class PathToDB : Window
    {
        public string ConnectionString;
        public PathToDB()
        {
            InitializeComponent();
        }

        private void CreateNew(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Выберите директорию и введите имя файла",
                Filter = "Database files (*.db)|*.db",
                DefaultExt = "db",
                AddExtension = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                    if (CreateStructOfDB(filePath))
                    {
                        SavePathToConfig(filePath);
                        MessageBox.Show("Файл успешно создан: " + filePath, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                }

                else
                {
                    MessageBox.Show("Файл уже существует: " + filePath, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл базы данных",
                Filter = "Database files (*.db)|*.db",
                DefaultExt = "db"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                SavePathToConfig(filePath);

                MessageBox.Show("Файл выбран: " + filePath, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private void SavePathToConfig(string path)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings["PathToDb"] == null)
            {
                config.AppSettings.Settings.Add("PathToDb", path);
            }
            else
            {
                config.AppSettings.Settings["PathToDb"].Value = path;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        private bool CreateStructOfDB(string path)
        {
            string connectionString = $"Data Source={path}";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                SqliteTransaction transaction = connection.BeginTransaction();
                try
                {
                    foreach (string cmd in sqlCommands)
                    {
                        SqliteCommand command = new SqliteCommand(cmd,connection);
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex) 
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при создании структуры БД: " + ex.Message);
                    return false;
                }
                finally { connection.Close(); }
            }
        }
        readonly string[] sqlCommands =
                {
                    @"CREATE TABLE ""Учебный План"" (
                    ""ID""	INTEGER NOT NULL,
                    ""Название""	varchar(40) NOT NULL,
                    ""Курс_ID""	int NOT NULL,
                    ""Дисциплина_ID""	int NOT NULL,
                    ""Препод_ID""	INTEGER,
                    ""Кол-во часов""	float(5, 2) NOT NULL,
                    FOREIGN KEY(""Препод_ID"") REFERENCES ""Преподаватель""(""ID"") ON DELETE SET NULL ON UPDATE CASCADE,
                    PRIMARY KEY(""ID"" AUTOINCREMENT),
                    FOREIGN KEY(""Дисциплина_ID"") REFERENCES ""Дисциплина""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY(""Курс_ID"") REFERENCES ""Курс""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE
                    )",

                    @"CREATE TABLE ""Тип Курса"" (
                    ""ID"" INTEGER NOT NULL,
                    ""Название"" varchar(30) NOT NULL,
                    CONSTRAINT ""Тип Курса_pk"" PRIMARY KEY(""ID"" AUTOINCREMENT)
                    )",
                    @"CREATE TABLE ""Расписание"" (
                    ""Дата""	TEXT,
                    ""Аудитория_Корпус""	TEXT,
                    ""Аудитория_Номер""	TEXT,
                    ""Учебный План_ID""	int,
                    ""Пара_Номер пары""	int,
                    ""Преподаватель_ID""	int,
                    ""Группа_ID""	int,
                    FOREIGN KEY(""Преподаватель_ID"") REFERENCES ""Преподаватель""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY(""Пара_Номер пары"") REFERENCES ""Пара""(""Номер пары"") ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY(""Учебный План_ID"") REFERENCES ""Учебный План""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY(""Группа_ID"") REFERENCES ""Группа""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE
                    )",
                    @"CREATE TABLE ""Преподаватель"" (
                    ""ID""	INTEGER NOT NULL,
                    ""Фамилия""	varchar(40) NOT NULL,
                    ""Имя""	varchar(40) NOT NULL,
                    ""Отчество""	varchar(40),
                    CONSTRAINT ""Преподаватель_pk"" PRIMARY KEY(""ID"" AUTOINCREMENT)
                    )",
                    @"CREATE TABLE ""ПреподИДисциплина"" (
                    ""Дисциплина_ID""	int NOT NULL,
                    ""Преподаватель_ID""	int NOT NULL,
                    FOREIGN KEY(""Преподаватель_ID"") REFERENCES ""Преподаватель""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY(""Дисциплина_ID"") REFERENCES ""Дисциплина""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE
                    )",
                    @"CREATE TABLE ""Пара"" (
                    ""Номер пары"" int NOT NULL CONSTRAINT ""Пара_pk"" PRIMARY KEY,
                    ""Начало"" time NOT NULL,
                    ""Конец"" time NOT NULL
                    )",
                    @"CREATE TABLE ""Курс"" (
	                ""ID""	INTEGER NOT NULL,
	                ""Название_Курса""	varchar(40) NOT NULL,
	                ""Тип Курса_ID""	int NOT NULL,
	                FOREIGN KEY(""Тип Курса_ID"") REFERENCES ""Тип Курса""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE,
	                CONSTRAINT ""Курс_pk"" PRIMARY KEY(""ID"" AUTOINCREMENT)
                    )",
                    @"CREATE TABLE ""Дисциплина"" (
	                ""ID""	INTEGER NOT NULL,
	                ""Название""	varchar(40) NOT NULL,
	                CONSTRAINT ""Дисциплина_pk"" PRIMARY KEY(""ID"" AUTOINCREMENT)
                    )",
                    @"CREATE TABLE ""Группа"" (
	                ""ID""	INTEGER NOT NULL,
	                ""Номер""	varchar(40) NOT NULL,
	                ""Курс_ID""	int NOT NULL,
	                FOREIGN KEY(""Курс_ID"") REFERENCES ""Курс""(""ID"") ON DELETE CASCADE ON UPDATE CASCADE,
	                CONSTRAINT ""Группа_pk"" PRIMARY KEY(""ID"" AUTOINCREMENT)
                     )",
                    @"CREATE TABLE ""Аудитория"" (
	                    ""Корпус""	TEXT,
	                    ""Номер""	TEXT,
	                    CONSTRAINT ""Аудитория_pk"" PRIMARY KEY(""Корпус"",""Номер"")
                    )",
                    @"INSERT INTO ""main"".""Пара"" (""Номер пары"", ""Начало"", ""Конец"") VALUES ('1', '8:00', '9:30');",
                    @"INSERT INTO ""main"".""Пара"" (""Номер пары"", ""Начало"", ""Конец"") VALUES ('2', '9:45', '11:15');",
                    @"INSERT INTO ""main"".""Пара"" (""Номер пары"", ""Начало"", ""Конец"") VALUES ('3', '11:30', '13:00');",
                    @"INSERT INTO ""main"".""Пара"" (""Номер пары"", ""Начало"", ""Конец"") VALUES ('4', '13:20', '14:50');",
                    @"INSERT INTO ""main"".""Пара"" (""Номер пары"", ""Начало"", ""Конец"") VALUES ('5', '15:10', '16:40');",
                    @"INSERT INTO ""main"".""Пара"" (""Номер пары"", ""Начало"", ""Конец"") VALUES ('6', '16:55', '18:25');"
            };
         }
    }

