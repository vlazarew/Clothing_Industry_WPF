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

namespace Clothing_Industry_WPF.Примерки
{
    public struct HelpStruct
    {
        public string customerNickname { get; set; }
        public int idOrder { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для FittingsListWindow.xaml
    /// </summary>
    public partial class FittingsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public FittingsListWindow()
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
            fittingsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            fittingsGrid.SelectedIndex = 0;
        }

        private string getQueryText()
        {
            string query_text = "select customers.nickname as Customer, orders.id_order OrderId, types_of_fitting.Name_Of_type as Type_Of_Fitting, " +
                                "DATE_FORMAT(fittings.date, \"%d.%m.%Y\") as Date, fittings.time as Time, fittings.notes as Notes " +
                                "from fittings " +
                                "join orders on fittings.orders_id_order = orders.id_order " +
                                "join customers on fittings.customers_id_customer = customers.id_customer " +
                                "join types_of_fitting on fittings.types_of_fitting_id_type_of_fitting = types_of_fitting.id_type_of_fitting; ";
            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = fittingsGrid.SelectedIndex;
            string nickname = "";
            int idOrder = -1;
            int current_row = 0;
            foreach (DataRowView row in fittingsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                nickname = row.Row.ItemArray[0].ToString();
                idOrder = int.Parse(row.Row.ItemArray[1].ToString());
                break;
            }

            Window edit_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.edit, idOrder, nickname);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<HelpStruct> rowsToDelete = new List<HelpStruct>();
            foreach (DataRowView row in fittingsGrid.SelectedItems)
            {
                rowsToDelete.Add(new HelpStruct { customerNickname = row.Row.ItemArray[0].ToString(), idOrder = int.Parse(row.Row.ItemArray[1].ToString()) });
            }

            DeleteFromDB(rowsToDelete);

        }

        private void DeleteFromDB(List<HelpStruct> myStruct)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (HelpStruct record in myStruct)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                MySqlCommand commandCustomer = new MySqlCommand("select id_Customer from customers where nickname = @nickname", connection);
                commandCustomer.Parameters.AddWithValue("@nickname", record.customerNickname);
                int id_customer = -1;
                using (DbDataReader reader = commandCustomer.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_customer = reader.GetInt32(0);
                    }
                }

                string queryTable = "delete from fittings where Orders_id_Order = @idOrder and Customers_id_Customer = @idCustomer";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@idOrder", record.idOrder);
                commandTable.Parameters.AddWithValue("@idCustomer", id_customer);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Удаление не удалось");
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
            List<HelpStruct> rowsToEdit = new List<HelpStruct>();
            foreach (DataRowView row in fittingsGrid.SelectedItems)
            {
                rowsToEdit.Add(new HelpStruct { customerNickname = row.Row.ItemArray[0].ToString(), idOrder = int.Parse(row.Row.ItemArray[1].ToString()) });
            }

            if (rowsToEdit.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (rowsToEdit.Count > 1)
                {
                    for (int i = 0; i < rowsToEdit.Count - 1; i++)
                    {
                        edit_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.edit, rowsToEdit[i].idOrder, rowsToEdit[i].customerNickname);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.edit, rowsToEdit[rowsToEdit.Count - 1].idOrder, rowsToEdit[rowsToEdit.Count - 1].customerNickname);
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
            fittingsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Nickname", "Никнейм", describe.Where(key => key.Key == "Nickname").First().Value));
            result.Add(new FindHandler.FieldParameters("Orders_id_Order", "Номер заказа", describe.Where(key => key.Key == "Orders_id_Order").First().Value));
            result.Add(new FindHandler.FieldParameters("Date", "Дата примерки", describe.Where(key => key.Key == "Date").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_type", "Тип примерки", describe.Where(key => key.Key == "Name_Of_type").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe fittings", connection, describe);
            DescribeHelper("describe customers", connection, describe);
            DescribeHelper("describe types_of_fitting", connection, describe);
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
            fittingsGrid.ItemsSource = dataTable.DefaultView;
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
    }
}
