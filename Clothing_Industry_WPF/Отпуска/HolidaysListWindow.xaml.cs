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

namespace Clothing_Industry_WPF.Отпуска
{
    /// <summary>
    /// Логика взаимодействия для HolidaysListWindow.xaml
    /// </summary>
    public partial class HolidaysListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public HolidaysListWindow()
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
            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            holidaysGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();           
            holidaysGrid.SelectedIndex = 0;

            List<string> ids = new List<string>();
            foreach (DataRowView row in holidaysGrid.SelectedItems)
            {
                ids.Add(row.Row.ItemArray[0].ToString());
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
        private void RefreshDates()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            //Login
            MySqlCommand commanddays = new MySqlCommand("select Employees_login, timestampdiff(DAY,In_Fact_Start,curdate()), Days_Of_Holidays from holidays;", connection);
            List<string> log = new List<string>();
            List<int> days_used = new List<int>();
            List<int> days_sum = new List<int>();
            using (DbDataReader reader = commanddays.ExecuteReader())
            {
                while (reader.Read())
                {
                    log.Add(reader.GetValue(0).ToString());
                    days_used.Add(reader.GetInt32(1));
                    days_sum.Add(reader.GetInt32(2));             
                }
            }
            // Что такое i я так и не понял, Леха, если сломается, будешь фиксить ты!!!
            int i = 0;
            while (i != log.Count)
            {
                
                MySqlTransaction transaction = connection.BeginTransaction();
                MySqlCommand commandchangedays = new MySqlCommand("update holidays set Days_Used = @Days_Used, Rest_Of_Days = @Rest_Of_Days where Employees_login = @login", connection, transaction);
                commandchangedays.Parameters.AddWithValue("@login", log[i]);
                if (days_used[i] < 0)
                {
                    commandchangedays.Parameters.AddWithValue("@Days_Used", 0);
                    commandchangedays.Parameters.AddWithValue("@Rest_Of_Days", days_sum[i]);
                }
                else
                {
                    if (days_used[i] >= days_sum[i])
                    {
                        commandchangedays.Parameters.AddWithValue("@Days_Used", days_sum[i]);
                        commandchangedays.Parameters.AddWithValue("@Rest_Of_Days", 0);
                    }
                    else
                    {
                        commandchangedays.Parameters.AddWithValue("@Days_Used", days_used[i]);
                        commandchangedays.Parameters.AddWithValue("@Rest_Of_Days", days_sum[i] - days_used[i]);
                    }
                }
                commandchangedays.ExecuteNonQuery();
                transaction.Commit();
                this.Close();
                i++;
            }
            connection.Close();

        }
        
        private string getQueryText()
        {
            string query_text = "select employees.Login, holidays.Year, holidays.Days_Of_Holidays, holidays.Days_Used, holidays.Rest_Of_Days, DATE_FORMAT(holidays.Planned_Start, \"%d.%m.%Y\") as Planned_Start, DATE_FORMAT(holidays.In_Fact_Start, \"%d.%m.%Y\") as In_Fact_Start, " +
                                "DATE_FORMAT(holidays.In_Fact_End, \"%d.%m.%Y\") as In_Fact_End, holidays.Notes" +
                                " from holidays" +
                                " join employees on holidays.Employees_Login = employees.Login;";
            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = holidaysGrid.SelectedIndex;
            string login = "";
            int current_row = 0;
            foreach (DataRowView row in holidaysGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                login = row.Row.ItemArray[0].ToString();
                break;
            }

            Window edit_window = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.edit, login);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<string> loginsToDelete = new List<string>();
            foreach (DataRowView row in holidaysGrid.SelectedItems)
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

                string queryTable = "delete from holidays where Employees_Login = @login";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@login", login);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление отпуска " + login + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDates();
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {         
            List<string> loginsToEdit = new List<string>();
            foreach (DataRowView row in holidaysGrid.SelectedItems)
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
                        edit_window = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.edit, loginsToEdit[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.edit, loginsToEdit[loginsToEdit.Count - 1]);
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
            holidaysGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Login", "Логин", describe.Where(key => key.Key == "Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Year", "Год", describe.Where(key => key.Key == "Year").First().Value));
            result.Add(new FindHandler.FieldParameters("Days_Of_Holidays", "Всего дней", describe.Where(key => key.Key == "Days_Of_Holidays").First().Value));
            result.Add(new FindHandler.FieldParameters("Days_Used", "Прошло", describe.Where(key => key.Key == "Days_Used").First().Value));
            result.Add(new FindHandler.FieldParameters("Rest_Of_Days", "Осталось", describe.Where(key => key.Key == "Rest_Of_Days").First().Value));
            result.Add(new FindHandler.FieldParameters("Planned_Start", "Запланировано", describe.Where(key => key.Key == "Planned_Start").First().Value));
            result.Add(new FindHandler.FieldParameters("In_Fact_Start", "Начинается с", describe.Where(key => key.Key == "In_Fact_Start").First().Value));
            result.Add(new FindHandler.FieldParameters("In_Fact_End", "Заканчивается", describe.Where(key => key.Key == "In_Fact_End").First().Value));
            result.Add(new FindHandler.FieldParameters("Notes", "Примечания", describe.Where(key => key.Key == "Notes").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe holidays", connection, describe);
            DescribeHelper("describe employees", connection, describe);
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
            holidaysGrid.ItemsSource = dataTable.DefaultView;
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
            foreach (DataRowView row in holidaysGrid.SelectedItems)
            {
                ids.Add(row.Row.ItemArray[0].ToString());
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
    }
}
