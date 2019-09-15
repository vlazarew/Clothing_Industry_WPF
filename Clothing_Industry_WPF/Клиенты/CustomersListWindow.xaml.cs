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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Клиенты
{
    /// <summary>
    /// Логика взаимодействия для CustomersListWindow.xaml
    /// </summary>
    public partial class CustomersListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public struct HelpStructToDelete
        {
            public int id { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
        }

        public CustomersListWindow()
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
            var dataTable = Customer.getListCustomers(connection);
            customersGrid.ItemsSource = dataTable.DefaultView;

            // Если ничего не выделено, то стиль заблокированной кнопки
            if (customersGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int id = (int)((DataRowView)customersGrid.SelectedItem).Row.ItemArray[0];

            Window create_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<HelpStructToDelete> dataToDelete = new List<HelpStructToDelete>();
            foreach (DataRowView row in customersGrid.SelectedItems)
            {
                dataToDelete.Add(new HelpStructToDelete { id = (int)row.Row.ItemArray[0], Firstname = row.Row.ItemArray[1].ToString(), Lastname = row.Row.ItemArray[2].ToString() });
            }

            DeleteFromDB(dataToDelete);

        }

        private void DeleteFromDB(List<HelpStructToDelete> dataToDelete)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (HelpStructToDelete data in dataToDelete)
            {

                if (!IsReadyToDelete(data, connection))
                {
                    return;
                }

                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from customers where id_Customer = @id";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id", data.id);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка удаления клиента", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
            RefreshList();
        }

        // Костыль. Надо либо нам расковырять по-нормальному БД, чтоб PK могли быть NULL, либо мириться с такими сообщениями
        private bool IsReadyToDelete(HelpStructToDelete data, MySqlConnection connection)
        {
            string queryBalance = "select Customers_id_Customer from Customers_Balance where Customers_id_Customer = @id;";
            MySqlCommand commandBalance = new MySqlCommand(queryBalance, connection);
            commandBalance.Parameters.AddWithValue("id", data.id);

            using (DbDataReader reader = commandBalance.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    MessageBox.Show("Клиент " + data.Firstname + " " + data.Lastname + " находится в таблице Баланс Клиентов. Первоначально удалите записи о нем в указанной таблице.",
                                    "Невозможно удалить клиента", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }

            string queryFittings = "select Customers_id_Customer from Fittings where Customers_id_Customer = @id;";
            MySqlCommand commandFittings = new MySqlCommand(queryFittings, connection);
            commandFittings.Parameters.AddWithValue("id", data.id);

            using (DbDataReader reader = commandFittings.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    MessageBox.Show("Клиент " + data.Firstname + " " + data.Lastname + " находится в таблице Примерки. Первоначально удалите записи о нем в указанной таблице.",
                                    "Невозможно удалить клиента", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }

            return true;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToEdit = new List<int>();
            foreach (DataRowView row in customersGrid.SelectedItems)
            {
                idsToEdit.Add((int)row.Row.ItemArray[0]);
            }

            if (idsToEdit.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (idsToEdit.Count > 1)
                {
                    for (int i = 0; i < idsToEdit.Count - 1; i++)
                    {
                        edit_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[idsToEdit.Count - 1]);
                edit_window.ShowDialog();

                //Обновление списка
                RefreshList();
            }
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
            string query = Customer.getQueryText();
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
            customersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters("customers.Lastname", "Фамилия", describe.Where(key => key.Key == "Lastname").First().Value));
            result.Add(new FindHandler.FieldParameters("Name", "Имя", describe.Where(key => key.Key == "Name").First().Value));
            result.Add(new FindHandler.FieldParameters("Patronymic", "Отчество", describe.Where(key => key.Key == "Patronymic").First().Value));
            result.Add(new FindHandler.FieldParameters("Address", "Адрес", describe.Where(key => key.Key == "Address").First().Value));
            result.Add(new FindHandler.FieldParameters("Phone_Number", "Телефон", describe.Where(key => key.Key == "Phone_Number").First().Value));
            result.Add(new FindHandler.FieldParameters("Nickname", "Никнейм", describe.Where(key => key.Key == "Nickname").First().Value));
            result.Add(new FindHandler.FieldParameters("Birthday", "Дата рождения", describe.Where(key => key.Key == "Birthday").First().Value));
            result.Add(new FindHandler.FieldParameters("Passport_data", "Паспортные данные", describe.Where(key => key.Key == "Passport_data").First().Value));
            result.Add(new FindHandler.FieldParameters("Size", "Размер", describe.Where(key => key.Key == "Size").First().Value));
            result.Add(new FindHandler.FieldParameters("Parameters", "Параметры", describe.Where(key => key.Key == "Parameters").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_of_status", "Статус", describe.Where(key => key.Key == "Name_of_status").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_of_channel", "Канал связи", describe.Where(key => key.Key == "Name_of_channel").First().Value));
            result.Add(new FindHandler.FieldParameters("Login", "Логин", describe.Where(key => key.Key == "Login").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe customers", connection, describe);
            DescribeHelper("describe employees", connection, describe);
            DescribeHelper("describe customer_statuses", connection, describe);
            DescribeHelper("describe order_channels", connection, describe);
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
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");
            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();
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
            customersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string EditFilterQuery(List<FilterHandler.FilterDescription> filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = Customer.getQueryText();

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

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            List<int> ids = new List<int>();
            foreach (DataRowView row in customersGrid.SelectedItems)
            {
                ids.Add((int)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
    }
}
