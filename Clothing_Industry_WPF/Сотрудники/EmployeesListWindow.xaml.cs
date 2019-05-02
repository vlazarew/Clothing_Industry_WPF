using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Сотрудники
{
    /// <summary>
    /// Логика взаимодействия для EmployeesListWindow.xaml
    /// </summary>
    public partial class EmployeesListWindow : Window
    {

        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        public string param { get; set; }

        public EmployeesListWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            //Думаю, что не очень хорошо, когда в списке пользователей виден пароль!!! /* employees.Password, */
            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            employeesGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string getQueryText()
        {
            string query_text = "select employees.Login, employees.Name, employees.Lastname, employees.Patronymic, employees.Phone_Number, employees.Passport_Data, employees.Email," +
                                "employees.Notes, DATE_FORMAT(employees.Added, \"%d.%m.%Y\") as Added, employees.Last_Salary, employee_roles.Name_Of_Role, employee_positions.Name_Of_Position" +
                                " from employees" +
                                " join employee_positions on employees.Employee_Positions_id_Employee_Position = employee_positions.id_Employee_Position" +
                                " join employee_roles on employees.Employee_Roles_id_Employee_Role = employee_roles.id_Employee_Role;";
            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = employeesGrid.SelectedIndex;
            string login = "";
            int current_row = 0;
            foreach (DataRowView row in employeesGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                login = row.Row.ItemArray[0].ToString();
                break;
            }

            Window create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, login);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<string> loginsToDelete = new List<string>();
            foreach (DataRowView row in employeesGrid.SelectedItems)
            {
                loginsToDelete.Add(row.Row.ItemArray[0].ToString());
            }

            DeleteFromDB(loginsToDelete);

        }

        private void DeleteFromDB(List<string> logins)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (string login in logins)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from employees where Login = @login";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@login", login);

                string queryUser = string.Format("drop user '{0}'@'%'", login);
                MySqlCommand commandUser = new MySqlCommand(queryUser, connection, transaction);

                try
                {
                    commandTable.ExecuteNonQuery();
                    commandUser.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление пользователя " + login + " не удалось");
                }
            }

            connection.Close();
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<string> loginsToDelete = new List<string>();
            foreach (DataRowView row in employeesGrid.SelectedItems)
            {
                loginsToDelete.Add(row.Row.ItemArray[0].ToString());
            }

            if (loginsToDelete.Count > 0)
            {
                Window create_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (loginsToDelete.Count > 1)
                {
                    for (int i = 0; i < loginsToDelete.Count - 1; i++)
                    {
                        create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, loginsToDelete[i]);
                        create_window.Show();
                    }
                }
                //Заключительная форма
                create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, loginsToDelete[loginsToDelete.Count - 1]);
                create_window.Show();

                //Обновление списка
                RefreshList();
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<string, string>> listOfField = FillFindFields();

            var findWindow = new FindWindow(listOfField);
            FindHandler.FindDescription result = new FindHandler.FindDescription();
            if (findWindow.ShowDialog().Value)
            {
                result = findWindow.Result;
            }
            else
            {
                return;
            }

            var field = listOfField.Where(kvp => kvp.Value == result.field).First().Key;
            string query = getQueryText();
            string edited_query; 

            ///!!!!!!!!!!!!!!! ДАТА НЕ ИЩЕТСЯ
            if (!result.isDate)
            {
                edited_query = query.Replace(";", " where " + field.ToString() + " ");
                edited_query += string.Format(result.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", result.value);
            }
            else
            {
                edited_query = query.Replace(";", " where DATE_FORMAT(employees.Added, \"%d.%m.%Y\")  ");
                //string date = result.value.Replace(".", "");
                edited_query += string.Format("= date(\'{0}\')", result.value);
            }

            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(edited_query, connection);
            command.Parameters.AddWithValue("@value", result.value);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            employeesGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();


        }

        // Список полей, по которым мы можем делать поиск
        private List<KeyValuePair<string, string>> FillFindFields()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("Login", "Логин"));
            result.Add(new KeyValuePair<string, string>("Name_Of_Role", "Роль"));
            result.Add(new KeyValuePair<string, string>("Lastname", "Фамилия"));
            result.Add(new KeyValuePair<string, string>("Name", "Имя"));
            result.Add(new KeyValuePair<string, string>("Patronymic", "Отчество"));
            result.Add(new KeyValuePair<string, string>("Name_Of_Position", "Должность"));
            result.Add(new KeyValuePair<string, string>("Phone_Number", "Телефон"));
            result.Add(new KeyValuePair<string, string>("Email", "Email"));
            result.Add(new KeyValuePair<string, string>("Passport_Data", "Паспортные данные"));
            result.Add(new KeyValuePair<string, string>("Added", "Дата добавления"));
            result.Add(new KeyValuePair<string, string>("Last_Salary", "З/п за прошлый месяц"));

            return result;
        }
    }
}
