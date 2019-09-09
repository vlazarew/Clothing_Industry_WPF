using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public EmployeesListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
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

            employeesGrid.SelectedIndex = 0;

            List<string> ids = new List<string>();
            foreach (DataRowView row in employeesGrid.SelectedItems)
            {
                ids.Add((string)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private string getQueryText()
        {
            string query_text = "select employees.Login, employees.Name, employees.Lastname, employees.Patronymic, employees.Phone_Number, employees.Passport_Data, employees.Email," +
                                "employees.Notes, DATE_FORMAT(employees.Added, \"%d.%m.%Y\") as Added, employees.Last_Salary, employee_positions.Name_Of_Position" +
                                " from employees" +
                                " join employee_positions on employees.Employee_Positions_id_Employee_Position = employee_positions.id_Employee_Position;";

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

            Window edit_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, login);
            edit_window.ShowDialog();
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
                    System.Windows.MessageBox.Show("Удаление пользователя " + login + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
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
            List<string> loginsToEdit = new List<string>();
            foreach (DataRowView row in employeesGrid.SelectedItems)
            {
                loginsToEdit.Add(row.Row.ItemArray[0].ToString());
            }

            if (loginsToEdit.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (loginsToEdit.Count > 1)
                {
                    for (int i = 0; i < loginsToEdit.Count - 1; i++)
                    {
                        edit_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, loginsToEdit[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, loginsToEdit[loginsToEdit.Count - 1]);
                edit_window.ShowDialog();

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
            List<FindHandler.FieldParameters> listOfField = FillFindFields();

            var findWindow = new FindWindow(currentFindDescription, listOfField);
            if (findWindow.ShowDialog().Value)
            {
                currentFindDescription = findWindow.Result;
            }
            else
            {
                return;
            }

            var field = listOfField.Where(kvp => kvp.application_name == currentFindDescription.field).First().db_name;
            string query = getQueryText();
            string edited_query;

            if (!currentFindDescription.isDate)
            {
                edited_query = query.Replace(";", " where " + field + " ");
                edited_query += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
            }
            else
            {
                edited_query = query.Replace(";", " where DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                edited_query += string.Format("= \'{0}\'", currentFindDescription.value);
            }

            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(edited_query, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            employeesGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Login", "Логин", describe.Where(key => key.Key == "Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Lastname", "Фамилия", describe.Where(key => key.Key == "Lastname").First().Value));
            result.Add(new FindHandler.FieldParameters("Name", "Имя", describe.Where(key => key.Key == "Name").First().Value));
            result.Add(new FindHandler.FieldParameters("Patronymic", "Отчество", describe.Where(key => key.Key == "Patronymic").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Position", "Должность", describe.Where(key => key.Key == "Name_Of_Position").First().Value));
            result.Add(new FindHandler.FieldParameters("Phone_Number", "Телефон", describe.Where(key => key.Key == "Phone_Number").First().Value));
            result.Add(new FindHandler.FieldParameters("Email", "Email", describe.Where(key => key.Key == "Email").First().Value));
            result.Add(new FindHandler.FieldParameters("Passport_Data", "Паспортные данные", describe.Where(key => key.Key == "Passport_Data").First().Value));
            result.Add(new FindHandler.FieldParameters("Added", "Дата добавления", describe.Where(key => key.Key == "Added").First().Value));
            result.Add(new FindHandler.FieldParameters("Last_Salary", "З/п за прошлый месяц", describe.Where(key => key.Key == "Last_Salary").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe employees", connection, describe);
            DescribeHelper("describe employee_positions", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        private void DescribeHelper(string query, MySqlConnection connection, List<KeyValuePair<string, string>> pairs)
        {
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    pairs.Add(new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)));
                }
            }
        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {
            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");
        }

        private void ButtonFilters_Click(object sender, RoutedEventArgs e)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields();
            var filterWindow = new FilterWindow(currentFilterDescription, listOfField);
            if (filterWindow.ShowDialog().Value)
            {
                currentFilterDescription = filterWindow.Result;
            }
            else
            {
                return;
            }

            string editedQuery = EditFilterQuery(currentFilterDescription, listOfField);

            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(editedQuery, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            employeesGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string EditFilterQuery(List<FilterHandler.FilterDescription> filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = getQueryText();

            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result = result.Replace(";", " where ");
                    break;
                }
            }

            int index = 0;
            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result += AddСondition(filterRecord, listOfField);
                    index++;
                    if (index < filter.Count)
                    {
                        result += " and ";
                    }
                }
            }

            return result;
        }

        private string AddСondition(FilterHandler.FilterDescription filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = "";
            var field = listOfField.Where(kvp => kvp.application_name == filter.field).First().db_name;
            var typeFilter = FilterHandler.TakeFilter(filter.typeOfFilter);
            if (filter.typeOfFilter == TypeOfFilter.TypesOfFilter.isFilled)
            {
                result += "NOT ";
            }

            if (!filter.isDate)
            {
                result += string.Format(field + " " + typeFilter + "\"{0}\"", filter.value);
            }
            else
            {
                string day = filter.value.Substring(0, 2);
                string month = filter.value.Substring(3, 2);
                string year = filter.value.Substring(6, 4);
                result += string.Format(field + " " + typeFilter + " \'{0}-{1}-{2}\'", year, month, day);
                //result += string.Format(" DATE_FORMAT(" + field + ", '%d.%m.%Y') = \'{0}\'", filter.value);
            }

            /*if (filter.typeOfFilter == TypeOfFilter.TypesOfFilter.contains)
            {
                result += ") ";
            }*/

            return result;
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            List<string> ids = new List<string>();
            foreach (DataRowView row in employeesGrid.SelectedItems)
            {
                ids.Add((string)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }


        }
    }
}
