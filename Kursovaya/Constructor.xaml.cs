using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Kursovaya
{
    public partial class Constructor : Window
    {
        string connectionString
        {
            get
            {
                return @"Data Source=" + ConfigurationManager.AppSettings["PathToDb"];
            }
        }
        string currentTabName = null;
        TextBlock currentId = null;
        ComboBox currentCB = null;
        List<int> BufferForPlan = new List<int>();

        public ObservableCollection<PlanItem> PlanItems { get; set; }
        public ICommand RemoveCommand { get; set; }

        // Изначальный вариант при загрузке
        public Constructor(string name)
        {
            InitializeComponent();
            PlanItems = new ObservableCollection<PlanItem>();
            RemoveCommand = new RemoveItemCommand(PlanItems, BufferForPlan);
            DataContext = this;
            switch (name) 
            {
                case "Course":
                    SelectTabByName("CourseTab");
                    currentCB = CBCourse;
                    currentCB.Items.Add(new ComboBoxItemForAdd("-1", "Добавление"));
                    currentCB.SelectedIndex = 0;
                    currentId = Course_ID;
                    List <Dictionary<string,string>> CBdataForCourse = Logica.GetCBForAdd(connectionString,name);
                    foreach(Dictionary<string, string> DictorItem in CBdataForCourse) 
                    {
                        DictorItem.TryGetValue("id", out string id);
                        DictorItem.TryGetValue("Название_Курса", out string nameCourse);
                        ComboBoxItemForAdd item = new ComboBoxItemForAdd(id, nameCourse);
                        currentCB.Items.Add(item);
                    }

                    List<Dictionary<string, string>> CbType = Logica.GetCBForAdd(connectionString, "TypeCourse");
                    foreach (Dictionary<string, string> DictorItem in CbType)
                    {
                        DictorItem.TryGetValue("id", out string id_type_course);
                        DictorItem.TryGetValue("Тип", out string nameCourse);
                        ComboBoxItemForAdd item = new ComboBoxItemForAdd(id_type_course, nameCourse);
                        TypeCourse.Items.Add(item);
                        TypeCourse.SelectedIndex = 0;
                    }
                break;

                case "Lect":
                    SelectTabByName("LectTab");
                    currentCB = CBlect;
                    currentCB.Items.Add(new ComboBoxItemForAdd("-1", "Добавление"));
                    currentCB.SelectedIndex = 0;
                    currentId = Lect_ID;
                    List <Dictionary<string,string>> CBdataForLect = Logica.GetCBForAdd(connectionString,name);
                    foreach(Dictionary<string, string> DictorItem in CBdataForLect) 
                    {
                        DictorItem.TryGetValue("id", out string id);
                        DictorItem.TryGetValue("ФИО", out string FIO);
                        ComboBoxItemForAdd item = new ComboBoxItemForAdd(id, "("+id+")" + FIO);
                        currentCB.Items.Add(item);
                    }

                    // Добавление всех дисциплин
                    List<Dictionary<string, string>> DiscipForLect = Logica.GetCBForAdd(connectionString, "Discip");
                    foreach (Dictionary<string, string> DictorItem in DiscipForLect)
                    {
                        DictorItem.TryGetValue("id", out string idDiscip);
                        DictorItem.TryGetValue("Название", out string nameDiscip);
                        ListDiscips.Items.Add(new ListBoxItemForAdd(Convert.ToInt32(idDiscip), nameDiscip));
                    }
                    break;

                case "Plan":
                    SelectTabByName("PlanTab");
                    currentCB = CBPlan;
                    currentCB.Items.Add(new ComboBoxItemForAdd("-1", "Добавление"));
                    currentCB.SelectedIndex = 0;

                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand(@"SELECT ID, Название_Курса FROM Курс WHERE ID NOT IN (SELECT DISTINCT Курс_ID FROM [Учебный План])", connection);
                        SqliteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            CourseIDCB.Items.Add(new ComboBoxItemForAdd(reader.GetInt32(0).ToString(), reader.GetString(1)));
                        }
                    }

                        List<Dictionary<string, string>> CBDataForPlan = Logica.GetCBForAdd(connectionString, name);
                    foreach(Dictionary<string, string> DictorItem in CBDataForPlan) 
                    {
                        DictorItem.TryGetValue("id", out string id);
                        DictorItem.TryGetValue("Название", out string NamePlan);
                        ComboBoxItemForAdd item = new ComboBoxItemForAdd(id, NamePlan);
                        currentCB.Items.Add(item);
                    }

                    // Добавление всех дисциплин
                    List<Dictionary<string, string>> DiscipForPlan = Logica.GetCBForAdd(connectionString, "Discip");
                    foreach (Dictionary<string, string> DictorItem in DiscipForPlan)
                    {
                        DictorItem.TryGetValue("id", out string idDiscip);
                        DictorItem.TryGetValue("Название", out string nameDiscip);
                        CBPlanDiscip.Items.Add(new ComboBoxItemForAdd(idDiscip, nameDiscip));
                    }


                    break;

                case "Discip":
                    SelectTabByName("DiscipTab");
                    currentCB = CBDiscip;
                    currentCB.Items.Add(new ComboBoxItemForAdd("-1", "Добавление"));
                    currentCB.SelectedIndex = 0;
                    List<Dictionary<string, string>> CBdataForDiscip = Logica.GetCBForAdd(connectionString, name);
                    foreach (Dictionary<string, string> DictorItem in CBdataForDiscip)
                    {
                        DictorItem.TryGetValue("id", out string id);
                        DictorItem.TryGetValue("Название", out string namePlan);
                        currentCB.Items.Add(new ComboBoxItemForAdd(id, namePlan));
                    }
                    break;


                case "Group":
                    SelectTabByName("GroupTab");
                    currentCB = CBGroup;
                    currentCB.Items.Add(new ComboBoxItemForAdd("-1", "Добавление"));
                    currentCB.SelectedIndex = 0;
                    currentId = GroupId;
                    List<Dictionary<string, string>> CBdataForGroup = Logica.GetCBForAdd(connectionString, name);
                    foreach (Dictionary<string, string> DictorItem in CBdataForGroup)
                    {
                        DictorItem.TryGetValue("id", out string id);
                        DictorItem.TryGetValue("Номер", out string nameCourse);
                        ComboBoxItemForAdd item = new ComboBoxItemForAdd(id, nameCourse);
                        currentCB.Items.Add(item);
                    }

                    List<Dictionary<string, string>> DataCBCourseForGroup = Logica.GetCBForAdd(connectionString, "Course");
                    foreach (Dictionary<string, string> DictorItem in DataCBCourseForGroup)
                    {
                        DictorItem.TryGetValue("id", out string id_Course);
                        DictorItem.TryGetValue("Название_Курса", out string nameCourse);
                        ComboBoxItemForAdd item = new ComboBoxItemForAdd(id_Course, nameCourse);
                        CBCourseForGroup.Items.Add(item);
                    }
                    break;

                case "Auditor":
                    SelectTabByName("AuditorTab");
                    currentCB = CBAuditor;
                    currentCB.Items.Add(new ComboBoxItemForAdd("-1", "Добавление"));
                    currentCB.SelectedIndex = 0;
                    currentId = Lect_ID;
                    List<Dictionary<string, string>> CBdataForAuditor = Logica.GetCBForAdd(connectionString, name);
                    foreach (Dictionary<string, string> DictorItem in CBdataForAuditor)
                    {
                        DictorItem.TryGetValue("Корпус", out string Corp);
                        DictorItem.TryGetValue("Номер", out string Audit);
                        currentCB.Items.Add(new ComboBoxItemForAdd(Corp, Audit, Corp + "-" + Audit));
                    }
                    break;
                default:
                    MessageBox.Show("Нет такой вкладки","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                    break;
            }
            currentTabName = name;
            if (currentCB != null)
            {
                currentCB.SelectionChanged += SelectionChanged;
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
        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItemForAdd cbItem = (ComboBoxItemForAdd)comboBox.SelectedItem;
            Select(cbItem);
            
        }
        // Если что-то выбрали
        private void Select(ComboBoxItemForAdd cbItem) 
        {
            switch (currentTabName)
            {
                case "Course":
                    if (cbItem.Id == "-1")
                    {
                        Course_ID.Text = "";
                        InputNameCourse.Text = "";
                        TypeCourse.SelectedIndex = 0;
                        ChangeBT.IsEnabled = false;
                        DeleteBT.IsEnabled = false;
                        AddBt.IsEnabled = true;
                        break;
                    }
                    else 
                    {
                        ChangeBT.IsEnabled = true;
                        DeleteBT.IsEnabled = true;
                        AddBt.IsEnabled = false;
                        Course_ID.Text = cbItem.Id;
                    }
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        // Подгрузка для формы
                        SqliteCommand command = new SqliteCommand("SELECT Название_Курса, [Тип Курса_ID] FROM Курс WHERE ID = @id", connection);
                        command.Parameters.AddWithValue("@id", cbItem.Id);
                        var reader = command.ExecuteReader();
                        if (reader.Read()) 
                        {
                            InputNameCourse.Text = reader["Название_Курса"].ToString();
                            string currentCourseTypeId = reader["Тип Курса_ID"].ToString();
                            var selectedItem = TypeCourse.Items.Cast<dynamic>().FirstOrDefault(item => item.Id == currentCourseTypeId);
                            if (selectedItem != null)
                            {
                                TypeCourse.SelectedItem = selectedItem;
                            }
                        }
                    }
                    break;
                case "Lect":
                    List<int> CurrentLectDataOfDiscipIDs = new List<int>();

                    if (cbItem.Id == "-1")
                    {
                        CurrentLectDataOfDiscipIDs.Clear();
                        Lect_ID.Text = "";
                        InputFNameLect.Text = "";
                        InputLNameLect.Text = "";
                        InputFatNameLect.Text = "";
                        foreach (ListBoxItemForAdd item in ListDiscips.Items) 
                        {
                            item.CheckBoxState = false;
                        }
                        ChangeBT.IsEnabled = false;
                        DeleteBT.IsEnabled = false;
                        AddBt.IsEnabled = true;
                        break;
                    }
                    else
                    {
                        CurrentLectDataOfDiscipIDs.AddRange(Logica.GetLectDiscip(connectionString, Convert.ToInt32(cbItem.Id)));
                        ChangeBT.IsEnabled = true;
                        DeleteBT.IsEnabled = true;
                        AddBt.IsEnabled = false;
                        Lect_ID.Text = cbItem.Id;
                    }
                    foreach (ListBoxItemForAdd item in ListDiscips.Items)
                    {
                        item.CheckBoxState = CurrentLectDataOfDiscipIDs.Contains(item.Id);
                    }

                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        // Подгрузка для формы
                        SqliteCommand command = new SqliteCommand("SELECT Фамилия, Имя, Отчество FROM Преподаватель WHERE ID = @id", connection);
                        command.Parameters.AddWithValue("@id", cbItem.Id);
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            InputLNameLect.Text = reader["Фамилия"].ToString();
                            InputFNameLect.Text = reader["Имя"].ToString();
                            InputFatNameLect.Text = reader["Отчество"].ToString();
                        }
                    }
                    break;
                case "Plan":
                    if (cbItem.Id == "-1")
                    {
                        CourseIDCB.Items.Clear();
                        // Показать перечень Курсов которые не заняты.
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand(@"SELECT ID, Название_Курса FROM Курс WHERE ID NOT IN (SELECT DISTINCT Курс_ID FROM [Учебный План])", connection);
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                CourseIDCB.Items.Add(new ComboBoxItemForAdd(reader.GetInt32(0).ToString(), reader.GetString(1)));
                            }
                        }
                        InputPlanName.Text = "";
                        ChangeBT.IsEnabled = false;
                        DeleteBT.IsEnabled = false;
                        AddBt.IsEnabled = true;
                        ClearItemsFromPlan();
                        BufferForPlan.Clear();
                        break;
                    }
                    else
                    {
                        currentId = null;
                        CourseIDCB.Items.Clear();
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();

                            SqliteCommand command = new SqliteCommand(@"SELECT ID, Название_Курса FROM Курс WHERE ID NOT IN (SELECT DISTINCT Курс_ID FROM [Учебный План])", connection);
                            SqliteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                CourseIDCB.Items.Add(new ComboBoxItemForAdd(reader.GetInt32(0).ToString(), reader.GetString(1)));
                            }
                            reader.Close();

                            SqliteCommand GetCurrentCourse = new SqliteCommand(@"SELECT Курс_ID,Название_Курса FROM [Учебный План]
                                                                                INNER JOIN Курс ON Курс_ID = Курс.ID
                                                                                WHERE Название = @NamePlan", connection);
                            GetCurrentCourse.Parameters.AddWithValue("@NamePlan", ((ComboBoxItemForAdd)CBPlan.SelectedItem).Text);
                            SqliteDataReader reader1 = GetCurrentCourse.ExecuteReader();
                            if (reader1.Read()) 
                            {
                                ComboBoxItemForAdd NewItem = new ComboBoxItemForAdd(reader1.GetInt32(0).ToString(), reader1.GetString(1));
                                CourseIDCB.Items.Add(NewItem);
                                CourseIDCB.SelectedItem = NewItem;
                            }
                        }
                        BufferForPlan.Clear();
                        ChangeBT.IsEnabled = true;
                        DeleteBT.IsEnabled = true;
                        AddBt.IsEnabled = false;
                        InputPlanName.Text = cbItem.Text;
                    }
                    List<PlanItem> result = new List<PlanItem>();
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        try
                        {
                            SqliteCommand GetPlan = new SqliteCommand(@"SELECT [Учебный План].ID, Дисциплина_ID, Дисциплина.Название, Препод_ID,
                                                        Преподаватель.Фамилия, Преподаватель.Имя, Преподаватель.Отчество, [Кол-во часов]
                                                        FROM [Учебный План] 
                                                        INNER JOIN Дисциплина ON Дисциплина_ID = Дисциплина.ID
                                                        LEFT JOIN Преподаватель ON [Учебный План].Препод_ID = Преподаватель.ID
                                                        WHERE Курс_ID = @Course", connection);
                            GetPlan.Parameters.AddWithValue("@Course", Convert.ToInt32(cbItem.Id));
                            SqliteDataReader reader = GetPlan.ExecuteReader();
                            while (reader.Read())
                            {
                                int ID = reader.GetInt32(0);
                                string discipID = reader.GetInt32(1).ToString();
                                string discipName = reader.GetString(2);
                                string fio = "Преподаватель не установлен";
                                string LectId = "-1";
                                if (!reader.IsDBNull(3)) 
                                {
                                    LectId = reader.GetInt32(3).ToString();
                                    string lastName = reader.GetString(4);
                                    string firstName =  reader.GetString(5);
                                    string fatName = reader.IsDBNull(6) ? null : " " + reader.GetString(6);
                                    fio = "("+LectId+")" + lastName + " " + firstName + fatName;
                                }

                                float hours = reader.GetFloat(7);

                                AddItem(ID, discipID, discipName, LectId, fio, hours);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Discip":
                    if (cbItem.Id == "-1")
                    {
                        Discip_ID.Text = "";
                        InputNameDiscip.Text = "";
                        ChangeBT.IsEnabled = false;
                        DeleteBT.IsEnabled = false;
                        AddBt.IsEnabled = true;
                        break;
                    }
                    else
                    {
                        ChangeBT.IsEnabled = true;
                        DeleteBT.IsEnabled = true;
                        AddBt.IsEnabled = false;
                        Discip_ID.Text = cbItem.Id;
                    }
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        SqliteCommand command = new SqliteCommand("SELECT Название FROM Дисциплина WHERE ID = @id", connection);
                        command.Parameters.AddWithValue("@id", cbItem.Id);
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            InputNameDiscip.Text = reader["Название"].ToString();
                        }
                    }

                    break;
                case "Group":
                    if (cbItem.Id == "-1")
                    {
                        GroupId.Text = "";
                        InputNameGroup.Text = "";
                        CBCourseForGroup.SelectedIndex = -1;
                        ChangeBT.IsEnabled = false;
                        DeleteBT.IsEnabled = false;
                        AddBt.IsEnabled = true;
                        break;
                    }
                    else
                    {
                        ChangeBT.IsEnabled = true;
                        DeleteBT.IsEnabled = true;
                        AddBt.IsEnabled = false;
                        GroupId.Text = cbItem.Id;
                    }
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand("SELECT Номер,Курс_ID FROM Группа WHERE ID = @id", connection);
                        command.Parameters.AddWithValue("@id", cbItem.Id);
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            InputNameGroup.Text = reader["Номер"].ToString();
                            string currentCourse = reader["Курс_ID"].ToString();
                            var selectedItem = CBCourseForGroup.Items.Cast<dynamic>().FirstOrDefault(item => item.Id == currentCourse);
                            if (selectedItem != null)
                            {
                                CBCourseForGroup.SelectedItem = selectedItem;
                            }
                        }
                    }
                    break;

                case "Auditor":
                    if (cbItem.Id == "-1" && cbItem.IdSec == "-1")
                    {
                        InputCorpus.Text = "";
                        InputAuditor.Text = "";
                        ChangeBT.IsEnabled = false;
                        DeleteBT.IsEnabled = false;
                        AddBt.IsEnabled = true;
                        break;
                    }
                    else
                    {
                        ChangeBT.IsEnabled = true;
                        DeleteBT.IsEnabled = true;
                        AddBt.IsEnabled = false;
                    }
                    InputCorpus.Text = cbItem.Id;
                    InputAuditor.Text = cbItem.IdSec;
                    break;
            }



        }
        // Обработка кнопок
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Точно удалить? \nВозможна потеря данных", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                switch (currentTabName)
                {
                    case "Course":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand(@"DELETE FROM Курс WHERE ID = @Id", connection);
                            command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            try
                            {
                                command.ExecuteNonQuery();
                                MessageBox.Show("Запись удалена");
                                RestartForm();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        break;
                    case "Lect":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand(@"DELETE FROM Преподаватель WHERE ID = @Id", connection);
                            command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            try
                            {
                                command.ExecuteNonQuery();
                                MessageBox.Show("Запись удалена");
                                RestartForm();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        break;
                    case "Plan":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand(@"DELETE FROM [Учебный План] WHERE Название = @Name", connection);
                            command.Parameters.AddWithValue("@Name", ((ComboBoxItemForAdd)CBPlan.SelectedItem).Text);
                            try
                            {
                                command.ExecuteNonQuery();
                                MessageBox.Show("Запись удалена");
                                RestartForm();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        break;

                    case "Discip":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand(@"DELETE FROM Дисциплина WHERE ID = @Id", connection);
                            command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            try
                            {
                                command.ExecuteNonQuery();
                                MessageBox.Show("Запись удалена");
                                RestartForm();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        break;
                    case "Group":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            try
                            {
                                SqliteCommand command = new SqliteCommand(@"DELETE FROM Группа WHERE ID = @Id", connection);
                                command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                                command.ExecuteNonQuery();
                                MessageBox.Show("Запись удалена");
                                RestartForm();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        break;
                    case "Auditor":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();
                            SqliteTransaction transaction = connection.BeginTransaction();
                            try {
                                string Corpus = ((ComboBoxItemForAdd)currentCB.SelectedItem).Id;
                                string Auditor = ((ComboBoxItemForAdd)currentCB.SelectedItem).IdSec;
                                SqliteCommand DeleteFromAud = new SqliteCommand(@"DELETE FROM Аудитория WHERE Корпус = @Corpus AND Номер = @Auditor", connection,transaction);
                                DeleteFromAud.Parameters.AddWithValue("@Corpus", Corpus);
                                DeleteFromAud.Parameters.AddWithValue("@Auditor", Auditor);
                                DeleteFromAud.ExecuteNonQuery();

                                SqliteCommand DeleteFromRaspis = new SqliteCommand(@"DELETE FROM Расписание WHERE Аудитория_Корпус = @Corpus AND Аудитория_Номер = @Auditor", connection,transaction);
                                DeleteFromRaspis.Parameters.AddWithValue("@Corpus", Corpus);
                                DeleteFromRaspis.Parameters.AddWithValue("@Auditor", Auditor);
                                DeleteFromRaspis.ExecuteNonQuery();

                                transaction.Commit();
                                MessageBox.Show("Запись удалена");
                                RestartForm();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        break;
                }
            }
        }


        private void Change(object sender, RoutedEventArgs e)
        {
            switch (currentTabName)
            {
                case "Course":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand(@"UPDATE Курс SET Название_Курса = @NewName, [Тип Курса_ID] = @NewType WHERE ID = @Id", connection);
                        command.Parameters.AddWithValue("@NewName", InputNameCourse.Text);
                        command.Parameters.AddWithValue("@NewType", Convert.ToInt32(((ComboBoxItemForAdd)TypeCourse.SelectedItem).Id));
                        command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                        
                        try
                        {
                            if (InputNameCourse.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputNameCourse.Text && item.Id != currentId.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }
                            command.ExecuteNonQuery();
                            MessageBox.Show("Изменено");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Lect":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteTransaction transaction = connection.BeginTransaction();
                        try
                        {
                            if (InputFNameLect.Text.Replace(" ", "") == "" || InputLNameLect.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            SqliteCommand command = new SqliteCommand();
                            command.Connection = connection;
                            command.Transaction = transaction;
                            if (InputFatNameLect.Text == "")
                            {
                                command.CommandText = @"UPDATE Преподаватель SET Фамилия = @NewLName, Имя = @NewFName WHERE ID = @Id";
                                command.Parameters.AddWithValue("@NewFName", InputFNameLect.Text);
                                command.Parameters.AddWithValue("@NewLName", InputLNameLect.Text);
                                command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            }
                            else 
                            {
                                command.CommandText = @"UPDATE Преподаватель SET Фамилия = @NewLName, Имя = @NewFName, Отчество = @FatName WHERE ID = @Id";
                                command.Parameters.AddWithValue("@NewFName", InputFNameLect.Text);
                                command.Parameters.AddWithValue("@NewLName", InputLNameLect.Text);
                                command.Parameters.AddWithValue("@FatName", InputFatNameLect.Text);
                                command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            }
                            command.ExecuteNonQuery();
                            // Удаляем все записи связанные с Преподом в ПреподИдисцип и вставляем новые
                            SqliteCommand DeleteCom = new SqliteCommand(@"DELETE FROM ПреподИДисциплина WHERE Преподаватель_ID = @Id");
                            DeleteCom.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            foreach (ListBoxItemForAdd itemForAdd in ListDiscips.Items)
                            {
                                if (itemForAdd.CheckBoxState == true)
                                {
                                    SqliteCommand commandForAddDiscipAndLect = new SqliteCommand("INSERT INTO ПреподИДисциплина (Дисциплина_ID, Преподаватель_ID) VALUES (@DiscipId, @LectId)", connection, transaction);
                                    commandForAddDiscipAndLect.Parameters.AddWithValue("@DiscipId", itemForAdd.Id);
                                    commandForAddDiscipAndLect.Parameters.AddWithValue("@LectId", Convert.ToInt32(currentId.Text));
                                    commandForAddDiscipAndLect.ExecuteNonQuery();
                                }
                                else 
                                {
                                    // Очистка Из Учебных планов если галка была снята
                                    SqliteCommand Update = new SqliteCommand(@"UPDATE [Учебный План] SET Препод_ID = NULL WHERE Препод_ID = @LectID AND Дисциплина_ID = @DiscipID", connection, transaction);
                                    Update.Parameters.AddWithValue("@LectID", Convert.ToInt32(currentId.Text));
                                    Update.Parameters.AddWithValue("@DiscipID", Convert.ToInt32(itemForAdd.Id));
                                    Update.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                            MessageBox.Show("Изменено");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Plan":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        using (SqliteTransaction transaction = connection.BeginTransaction())
                        {
                            using (SqliteCommand command = new SqliteCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;

                                try
                                {
                                    if (string.IsNullOrWhiteSpace(InputPlanName.Text))
                                    {
                                        throw new InvalidOperationException("Не заполнены обязательные поля");
                                    }


                                    foreach (PlanItem item in PlanItems)
                                    {
                                        command.Parameters.Clear(); // Очистить параметры перед каждым выполнением запроса

                                        if (item.id != null && item.LectId != "-1")
                                        {
                                            command.CommandText = @"UPDATE [Учебный План] 
                                                SET Название = @NamePlan,Курс_ID = @Course, Дисциплина_ID = @DiscipId, Препод_ID = @LectId, [Кол-во часов] = @Hours 
                                                WHERE ID = @id";
                                            command.Parameters.AddWithValue("@NamePlan", InputPlanName.Text);
                                            command.Parameters.AddWithValue("@Course", Convert.ToInt32(((ComboBoxItemForAdd)CourseIDCB.SelectedItem).Id));
                                            command.Parameters.AddWithValue("@DiscipId", Convert.ToInt32(item.DisciplineId));
                                            command.Parameters.AddWithValue("@LectId", Convert.ToInt32(item.LectId));
                                            command.Parameters.AddWithValue("@Hours", item.Hours);
                                            command.Parameters.AddWithValue("@id", Convert.ToInt32(item.id));
                                            command.ExecuteNonQuery();
                                        }
                                        else if (item.id != null)
                                        {
                                            command.CommandText = @"UPDATE [Учебный План] 
                                                SET Название = @NamePlan,Курс_ID = @Course, Дисциплина_ID = @DiscipId, [Кол-во часов] = @Hours 
                                                WHERE ID = @id";
                                            command.Parameters.AddWithValue("@NamePlan", InputPlanName.Text);
                                            command.Parameters.AddWithValue("@Course", Convert.ToInt32(((ComboBoxItemForAdd)CourseIDCB.SelectedItem).Id));
                                            command.Parameters.AddWithValue("@DiscipId", Convert.ToInt32(item.DisciplineId));
                                            command.Parameters.AddWithValue("@Hours", item.Hours);
                                            command.Parameters.AddWithValue("@id", Convert.ToInt32(item.id));
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            command.CommandText = @"INSERT INTO [Учебный План](Название, Курс_ID, Дисциплина_ID, Препод_ID, [Кол-во часов]) 
                                                VALUES(@Name, @Course, @Discip, @Lect, @Hours)";
                                            command.Parameters.AddWithValue("@Name", InputPlanName.Text);
                                            command.Parameters.AddWithValue("@Course", Convert.ToInt32(((ComboBoxItemForAdd)CourseIDCB.SelectedItem).Id));
                                            command.Parameters.AddWithValue("@Discip", Convert.ToInt32(item.DisciplineId));
                                            command.Parameters.AddWithValue("@Lect", Convert.ToInt32(item.LectId));
                                            command.Parameters.AddWithValue("@Hours", item.Hours);
                                            command.ExecuteNonQuery();
                                        }
                                    }

                                    foreach (var item in BufferForPlan)
                                    {
                                        command.Parameters.Clear(); // Очистить параметры перед каждым выполнением запроса
                                        command.CommandText = @"DELETE FROM [Учебный План] WHERE ID = @id";
                                        command.Parameters.AddWithValue("@id", item);
                                        command.ExecuteNonQuery();
                                    }

                                    transaction.Commit();
                                    MessageBox.Show("Изменено");
                                    RestartForm();
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                finally { connection.Close(); }
                            }
                        }
                    }
                    break;
                case "Discip":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        try
                        {
                            if (InputNameDiscip.Text.Replace(" ", "") == "")
                            {
                                 throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputNameDiscip.Text && item.Id != currentId.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }
                            SqliteCommand command = new SqliteCommand(@"UPDATE Дисциплина SET Название = @NewName WHERE ID = @Id", connection);
                            command.Parameters.AddWithValue("@NewName", InputNameDiscip.Text);
                            command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            command.ExecuteNonQuery();
                            MessageBox.Show("Изменено");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    break;
                case "Group":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        try
                        {
                            if (InputNameGroup.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputNameGroup.Text && item.Id != currentId.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }
                            if (CBCourseForGroup.SelectedIndex == -1)
                            {
                                throw new InvalidOperationException("Курс не выбран");
                            }

                            SqliteCommand command = new SqliteCommand(@"UPDATE Группа SET Номер = @NewName WHERE ID = @Id", connection);
                            command.Parameters.AddWithValue("@NewName", InputNameGroup.Text);
                            command.Parameters.AddWithValue("@Id", Convert.ToInt32(currentId.Text));
                            command.ExecuteNonQuery();
                            MessageBox.Show("Изменено");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Auditor":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteTransaction transaction = connection.BeginTransaction();
                        try 
                        {
                            if (InputCorpus.Text.Replace(" ", "") == "" || InputAuditor.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            if (InputCorpus.Text.Replace(" ", "") == "-1" || InputAuditor.Text.Replace(" ", "") == "-1")
                            {
                                throw new InvalidOperationException("Резерв");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if ((item.Text == InputCorpus.Text + "-" + InputAuditor.Text) && item.Text != currentCB.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }
                            string oldCorp = ((ComboBoxItemForAdd)currentCB.SelectedItem).Id;
                            string OldAud = ((ComboBoxItemForAdd)currentCB.SelectedItem).IdSec;

                            SqliteCommand UpdateAudit = new SqliteCommand(@"UPDATE Аудитория SET Корпус = @NewCorp, Номер = @NewAud WHERE Корпус = @OldCorp AND Номер = @OldAud", connection,transaction);
                            UpdateAudit.Parameters.AddWithValue("@NewCorp", InputCorpus.Text);
                            UpdateAudit.Parameters.AddWithValue("@NewAud", InputAuditor.Text);
                            UpdateAudit.Parameters.AddWithValue("@OldCorp", oldCorp);
                            UpdateAudit.Parameters.AddWithValue("@OldAud", OldAud);
                            UpdateAudit.ExecuteNonQuery();

                            SqliteCommand UpdateRaspis = new SqliteCommand(@"UPDATE Расписание SET Аудитория_Корпус = @NewCorp, Аудитория_Номер = @NewAud WHERE Аудитория_Корпус = @OldCorp AND Аудитория_Номер = @OldAud", connection, transaction);
                            UpdateRaspis.Parameters.AddWithValue("@NewCorp", InputCorpus.Text);
                            UpdateRaspis.Parameters.AddWithValue("@NewAud", InputAuditor.Text);
                            UpdateRaspis.Parameters.AddWithValue("@OldCorp", oldCorp);
                            UpdateRaspis.Parameters.AddWithValue("@OldAud", OldAud);
                            UpdateRaspis.ExecuteNonQuery();

                            transaction.Commit();
                            MessageBox.Show("Изменено");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
            }
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            switch (currentTabName)
            {
                case "Course":
                    int IdTypeCouse = Convert.ToInt32(((ComboBoxItemForAdd)TypeCourse.SelectedItem).Id);
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand(@"INSERT INTO Курс(Название_Курса,[Тип Курса_ID]) VALUES(@Name,@IdType)",connection);
                        command.Parameters.AddWithValue("@Name", InputNameCourse.Text);
                        command.Parameters.AddWithValue("@IdType", IdTypeCouse);
                        try
                        {
                            if (InputNameCourse.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputNameCourse.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }
                            command.ExecuteNonQuery();
                            MessageBox.Show("Запись добавлена");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Lect":
                        using (SqliteConnection connection = new SqliteConnection(connectionString))
                        {
                        connection.Open();
                        SqliteTransaction transaction = connection.BeginTransaction();
                        try
                        {
                            if (InputFNameLect.Text == "" || InputLNameLect.Text == "")
                            { throw new InvalidOperationException("Обязательные поля не заполнены"); }

                            SqliteCommand commandForAddLect = new SqliteCommand();
                            commandForAddLect.Connection = connection;
                            commandForAddLect.Transaction = transaction;

                            // Добавление преподавателя
                            if (InputFatNameLect.Text == "")
                            {
                                commandForAddLect.CommandText = @"INSERT INTO Преподаватель(Фамилия,Имя) VALUES(@LName,@FName)";
                                commandForAddLect.Parameters.AddWithValue("@LName", InputLNameLect.Text);
                                commandForAddLect.Parameters.AddWithValue("@FName", InputFNameLect.Text);
                            }
                            else
                            {
                                commandForAddLect.CommandText = @"INSERT INTO Преподаватель(Фамилия,Имя,Отчество) VALUES(@LName,@FName,@FatName)";
                                commandForAddLect.Parameters.AddWithValue("@LName", InputLNameLect.Text);
                                commandForAddLect.Parameters.AddWithValue("@FName", InputFNameLect.Text);
                                commandForAddLect.Parameters.AddWithValue("@FatName", InputFatNameLect.Text);
                            }
                            commandForAddLect.ExecuteNonQuery();

                            // Перехват ID преподавателя
                            SqliteCommand idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection, transaction);
                            int lastInsertedIdLect = Convert.ToInt32(idCommand.ExecuteScalar());

                            if (lastInsertedIdLect == -1)
                            {
                                throw new InvalidOperationException("Не получилось создать Преподавателя.");
                            }

                            foreach (ListBoxItemForAdd itemForAdd in ListDiscips.Items)
                            {
                                if (itemForAdd.CheckBoxState == true)
                                {
                                    SqliteCommand commandForAddDiscipAndLect = new SqliteCommand("INSERT INTO ПреподИДисциплина (Дисциплина_ID, Преподаватель_ID) VALUES (@DiscipId, @LectId)", connection, transaction);
                                    commandForAddDiscipAndLect.Parameters.AddWithValue("@DiscipId", itemForAdd.Id);
                                    commandForAddDiscipAndLect.Parameters.AddWithValue("@LectId", lastInsertedIdLect);
                                    commandForAddDiscipAndLect.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Запись добавлена");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;

                case "Plan":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteTransaction transaction = connection.BeginTransaction();
                        try
                        {
                            if (InputPlanName.Text.Replace(" ", "") == "")
                            {
                                { throw new InvalidOperationException("Обязательные поля не заполнены"); }
                            }
                            if (CourseIDCB.SelectedIndex == -1) 
                            {
                                { throw new InvalidOperationException("Не выбран курс"); }
                            }
                            foreach (PlanItem item in DataLB.Items) 
                            {
                                SqliteCommand command = new SqliteCommand(@"INSERT INTO [Учебный План](Название, Курс_ID, Дисциплина_ID, Препод_ID, [Кол-во часов])
                                                                                         VALUES(@NamePlan,@Course,@DiscipId,@LectId,@hours)",connection,transaction);
                                command.Parameters.AddWithValue("@NamePlan", InputPlanName.Text);
                                command.Parameters.AddWithValue("@Course", Convert.ToInt32(((ComboBoxItemForAdd)CourseIDCB.SelectedItem).Id));
                                command.Parameters.AddWithValue("@DiscipId", Convert.ToInt32(item.DisciplineId));
                                command.Parameters.AddWithValue("@LectId", Convert.ToInt32(item.LectId));
                                command.Parameters.AddWithValue("@hours", item.Hours);
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            
                            MessageBox.Show("Запись добавлена");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Discip":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand(@"INSERT INTO Дисциплина(Название) VALUES(@Name)", connection);
                        command.Parameters.AddWithValue("@Name", InputNameDiscip.Text);
                        try
                        {
                            if(InputNameDiscip.Text.Replace(" ", "") == "") 
                            {
                                { throw new InvalidOperationException("Обязательные поля не заполнены"); }
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputNameDiscip.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }

                            command.ExecuteNonQuery();
                            MessageBox.Show("Запись добавлена");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Group":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        try
                        {
                            if (InputNameGroup.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputNameGroup.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }
                            if (CBCourseForGroup.SelectedIndex == -1)
                            {
                                throw new InvalidOperationException("Курс не выбран");
                            }
                            SqliteCommand command = new SqliteCommand(@"INSERT INTO Группа(Номер, Курс_ID) VALUES(@Group,@Course)", connection);
                            command.Parameters.AddWithValue("@Group", InputNameGroup.Text);
                            command.Parameters.AddWithValue("@Course", ((ComboBoxItemForAdd)CBCourseForGroup.SelectedItem).Id);
                            command.ExecuteNonQuery();
                            MessageBox.Show("Запись добавлена");
                            RestartForm();

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case "Auditor":
                    using (SqliteConnection connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        try
                        {
                            if (InputCorpus.Text.Replace(" ", "") == "" || InputAuditor.Text.Replace(" ", "") == "")
                            {
                                throw new InvalidOperationException("Обязательные поля не заполнены");
                            }
                            if (InputCorpus.Text.Replace(" ", "") == "-1" || InputAuditor.Text.Replace(" ", "") == "-1")
                            {
                                throw new InvalidOperationException("Резерв");
                            }
                            foreach (ComboBoxItemForAdd item in currentCB.Items)
                            {
                                if (item.Text == InputCorpus.Text + "-" + InputAuditor.Text)
                                {
                                    throw new InvalidOperationException("Уже есть такое");
                                }
                            }

                            SqliteCommand command = new SqliteCommand(@"INSERT INTO Аудитория VALUES(@Corp,@Auditor)", connection);
                            command.Parameters.AddWithValue("@Corp", InputCorpus.Text);
                            command.Parameters.AddWithValue("@Auditor", InputAuditor.Text);
                            command.ExecuteNonQuery();
                            MessageBox.Show("Запись добавлена");
                            RestartForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
            }
        }
        private void RestartForm()
        {
            double left = this.Left;
            double top = this.Top;
            double width = this.Width;
            double height = this.Height;
            Constructor newForm = new Constructor(currentTabName);
            newForm.Left = left;
            newForm.Top = top;
            newForm.Width = width;
            newForm.Height = height;
            newForm.Show();
            this.Close();
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            
            try 
            {
                if(CBPlanDiscip.SelectedIndex == -1) 
                {
                    throw new InvalidOperationException("Не выбрана дисциплина");
                }
                if (CBPlanLectur.SelectedIndex == -1) 
                {
                    throw new InvalidOperationException("Не выбран преподаватель");
                }
                float hours;
                if (!float.TryParse(HoursTB.Text,out hours) || HoursTB.Text.Replace(" ","") == "")
                {
                    throw new InvalidOperationException("Не верно введено кол-во часов");
                }
                if (hours % 1.5f != 0) 
                {
                    throw new InvalidOperationException("Должно быть кратно 1,5");
                }

                ComboBoxItemForAdd discip = (ComboBoxItemForAdd)CBPlanDiscip.SelectedItem;
                ComboBoxItemForAdd lect = (ComboBoxItemForAdd)CBPlanLectur.SelectedItem;
                string IdDiscip = discip.Id;
                string discipText = discip.Text;
                string IdLect = lect.Id;
                string lectTex = lect.Text;
                foreach(PlanItem item in DataLB.Items)
                {
                    if(item.LectId == IdLect && item.DisciplineId == IdDiscip) 
                    {
                        throw new InvalidOperationException("Уже есть такое");
                    }
                }
                AddItem(IdDiscip, discipText, IdLect, lectTex, hours);
                CBPlanDiscip.SelectedIndex = -1;
                CBPlanLectur.SelectedIndex = -1;
                HoursTB.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CBPlanDiscip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CBPlanLectur.IsEnabled = false;
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItemForAdd cbItem = (ComboBoxItemForAdd)comboBox.SelectedItem;
            if (cbItem == null) { return; }
            List<(string, string)> IDsLectAndDiscip = new List<(string, string)>();
            foreach (PlanItem i in DataLB.Items) 
            {
                IDsLectAndDiscip.Add((i.DisciplineId, i.LectId));
            }
            List<ComboBoxItemForAdd> result = new List<ComboBoxItemForAdd>();
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqliteCommand GetLect = new SqliteCommand(@"SELECT Преподаватель_ID, Преподаватель.Фамилия, Преподаватель.Имя, Преподаватель.Отчество
                                                FROM ПреподИДисциплина 
                                                INNER JOIN Преподаватель ON ПреподИДисциплина.Преподаватель_ID = Преподаватель.ID
                                                WHERE Дисциплина_ID = @DiscipId ORDER BY Преподаватель.Фамилия", connection);
                        GetLect.Parameters.AddWithValue("@DiscipId", Convert.ToInt32(cbItem.Id));
                    SqliteDataReader reader = GetLect.ExecuteReader();
                    while (reader.Read())
                    {
                        string id = reader.GetInt32(0).ToString();
                        string lastName = reader.GetString(1);
                        string firstName = reader.GetString(2);
                        string fatName = reader.IsDBNull(3) ? null : reader.GetString(3);
                        string fio = "(" + id + ")" + lastName + " " + firstName + (fatName == null ? null : " " + fatName);
                        if (!IDsLectAndDiscip.Contains((cbItem.Id, id)))
                        {
                            result.Add(new ComboBoxItemForAdd(id, fio));
                        }
                    }
                    CBPlanLectur.ItemsSource = result;
                    if (CBPlanLectur.Items.Count > 0)
                    {
                        CBPlanLectur.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            } 
        }
        public void AddItem(string idDiscipline, string discipline, string idLect, string lect, float hours)
        {
            PlanItems.Add(new PlanItem(idDiscipline, discipline, idLect, lect, hours));
        }
        public void ClearItemsFromPlan() 
        {
            PlanItems.Clear();
        }
        public void AddItem(int ID,string idDiscipline, string discipline, string idLect, string lect, float hours)
        {
            PlanItems.Add(new PlanItem(ID, idDiscipline, discipline, idLect, lect, hours));
        }
        // Перехват если на кнопку удалить тыкнули не левую
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                e.Handled = true;
                return;
            }
        }

        private void StackPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            ListBoxItem listBoxItem = FindVisualParent<ListBoxItem>(stackPanel);

            if (listBoxItem != null)
            {
                var planItem = listBoxItem.DataContext as PlanItem;
                if (planItem != null)
                {
                    if (planItem.id != null) 
                    {
                        List<PlanItem> planitems = new List<PlanItem>();
                        foreach(PlanItem item in DataLB.Items) 
                        {
                            planitems.Add(item);
                        }
                        var result = new Edit(planitems, planItem, ((ComboBoxItemForAdd)CourseIDCB.SelectedItem).Id, InputPlanName.Text).ShowDialog();
                        if (result == true) 
                        {
                            MessageBox.Show("Изменено");
                            RestartForm();
                        }
                        return;
                    }
                    MessageBox.Show("Редактирование только для уже добавленных в БД данных");
                }
            }
        }
        public class RemoveItemCommand : ICommand
        {
            private ObservableCollection<PlanItem> _planItems;
            private List<int> _bufferForPlan;



            public RemoveItemCommand(ObservableCollection<PlanItem> planItems, List<int> bufferForPlan)
            {
                _planItems = planItems;
                _bufferForPlan = bufferForPlan;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is PlanItem item)
                {
                    if (item.id != null)
                    {
                        _bufferForPlan.Add((int)item.id);
                    }
                    _planItems.Remove(item);
                }
            }
        }

        public class BufferForDelete
        {
            public List<int> BufferForPlan = new List<int>();
            public ObservableCollection<PlanItem> PlanItems = new ObservableCollection<PlanItem>();

            public BufferForDelete()
            {
                var removeItemCommand = new RemoveItemCommand(PlanItems, BufferForPlan);
            }
        }

        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
    }

}
