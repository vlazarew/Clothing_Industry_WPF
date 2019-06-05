using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using Clothing_Industry_WPF.Примерки;
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

namespace Clothing_Industry_WPF.Заказы
{
    /// <summary>
    /// Логика взаимодействия для OrdersListWindow.xaml
    /// </summary>
    public partial class OrdersListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public OrdersListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
        }


        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            string query_text = getQueryText();
            //query_text = query_text.Replace(";", getGroupBy());
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            ordersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            ordersGrid.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private string getQueryText()
        {
            /*string query_text = "select orders.id_Order, orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Total_Price, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                    "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, " +
                                    "group_concat(products.Name_Of_Product separator ', ') as Products " +
                                    "from orders " +
                                    "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                    "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                    "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                    "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                    "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                "union " +
                                "select orders.id_Order, orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Total_Price, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                    "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, " +
                                    "   'Не указано' as Products " +
                                    "from orders " +
                                    "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                    "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                    "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                    "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                    "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                    "where products.Name_Of_Product is null ; ";

            return query_text;*/

            string result = getNotNullQueryText();
            result = result.Replace(";", getGroupBy());
            result = result.Replace(";", " union ");
            result += getNullQueryText();
            result = result.Replace(";", getGroupBy());

            return result;
        }

        private string getNotNullQueryText()
        {
            string query_text = "select orders.id_Order, DATE_FORMAT(orders.Date_Of_Order, \"%d.%m.%Y\") as Date_Of_Order, orders.Discount_Per_Cent, " +
                "orders.Total_Price, orders.Paid, orders.Debt, DATE_FORMAT(orders.Date_Of_Delievery, \"%d.%m.%Y\") as Date_Of_Delievery, orders.Notes, " +
                                    "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, " +
                                    "group_concat(products.Name_Of_Product separator ', ') as Products, orders.SalaryToExecutor " +
                                    "from orders " +
                                    "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                    "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                    "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                    "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                    "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                    "where not products.Name_Of_Product is null ;";

            return query_text;
        }

        private string getNullQueryText()
        {
            string query_text = "select orders.id_Order, DATE_FORMAT(orders.Date_Of_Order, \"%d.%m.%Y\") as Date_Of_Order, orders.Discount_Per_Cent, orders.Total_Price, " +
                                "orders.Paid, orders.Debt, DATE_FORMAT(orders.Date_Of_Delievery, \"%d.%m.%Y\") as Date_Of_Delievery, orders.Notes, " +
                                    "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, " +
                                    "'Не указано' as Products, orders.SalaryToExecutor " +
                                    "from orders " +
                                    "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                    "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                    "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                    "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                    "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                    "where products.Name_Of_Product is null ;";

            return query_text;
        }

        private string getGroupBy()
        {
            string groupBy = "  group by orders.id_Order ; ";
            return groupBy;
        }

        /*private string getGroupBy()
        {
            string group_by = "group by orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible, list_products_to_order.Orders_id_Order ;";

            return group_by;
        }*/


        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = ordersGrid.SelectedIndex;
            int orderId = -1;
            int current_row = 0;
            foreach (DataRowView row in ordersGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                orderId = int.Parse(row.Row.ItemArray[0].ToString());
                break;
            }

            Window edit_window = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, orderId);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToDelete = new List<int>();
            foreach (DataRowView row in ordersGrid.SelectedItems)
            {
                idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
            }

            DeleteFromDB(idsToDelete);
        }

        private void DeleteFromDB(List<int> ids)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from orders where id_order = @id";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id", id);
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление не удалось");
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
            List<int> idsToEdit = new List<int>();
            foreach (DataRowView row in ordersGrid.SelectedItems)
            {
                idsToEdit.Add(int.Parse(row.Row.ItemArray[0].ToString()));
            }

            if (idsToEdit.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (idsToEdit.Count > 1)
                {
                    for (int i = 0; i < idsToEdit.Count - 1; i++)
                    {
                        edit_window = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[idsToEdit.Count - 1]);
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
            string queryNotNull = getNotNullQueryText();
            string edited_query;

            if (!currentFindDescription.isDate)
            {
                edited_query = queryNotNull.Replace(";", " and " + field + " ");
                edited_query += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
            }
            else
            {
                edited_query = queryNotNull.Replace(";", " and DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                edited_query += string.Format("= \'{0}\'", currentFindDescription.value);
            }

            edited_query += " " + getGroupBy();
            edited_query = edited_query.Replace(";", " union ");
            edited_query += getNullQueryText();

            if (!currentFindDescription.isDate)
            {
                edited_query = queryNotNull.Replace(";", " and " + field + " ");
                edited_query += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
            }
            else
            {
                edited_query = queryNotNull.Replace(";", " and DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                edited_query += string.Format("= \'{0}\'", currentFindDescription.value);
            }

            edited_query += " " + getGroupBy();

            //edited_query += " " + getGroupBy();

            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(edited_query, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            ordersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Date_Of_Order", "Дата заказа", describe.Where(key => key.Key == "Date_Of_Order").First().Value));
            result.Add(new FindHandler.FieldParameters("Discount_Per_Cent", "Скидка", describe.Where(key => key.Key == "Discount_Per_Cent").First().Value));
            result.Add(new FindHandler.FieldParameters("Total_Price", "Итоговая стоимость", describe.Where(key => key.Key == "Total_Price").First().Value));
            result.Add(new FindHandler.FieldParameters("Paid", "Оплачено", describe.Where(key => key.Key == "Paid").First().Value));
            result.Add(new FindHandler.FieldParameters("Debt", "Долг", describe.Where(key => key.Key == "Debt").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Delievery", "Дата доставки", describe.Where(key => key.Key == "Date_Of_Delievery").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Type", "Тип заказа", describe.Where(key => key.Key == "Name_Of_Type").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Status", "Статус заказа", describe.Where(key => key.Key == "Name_Of_Status").First().Value));
            result.Add(new FindHandler.FieldParameters("Nickname", "Никнейм заказчика", describe.Where(key => key.Key == "Nickname").First().Value));
            result.Add(new FindHandler.FieldParameters("Executor", "Исполнитель", describe.Where(key => key.Key == "Executor").First().Value));
            result.Add(new FindHandler.FieldParameters("Responsible", "Ответственный", describe.Where(key => key.Key == "Responsible").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe orders", connection, describe);
            DescribeHelper("describe types_of_order", connection, describe);
            DescribeHelper("describe statuses_of_order", connection, describe);
            DescribeHelper("describe customers", connection, describe);
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
            ordersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string EditFilterQuery(List<FilterHandler.FilterDescription> filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = getNotNullQueryText();

            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result = result.Replace(";", " and ");
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
                        result += " or ";
                    }
                }
            }

            result += " " + getGroupBy();
            result = result.Replace(";", " union ");
            result += getNullQueryText();

            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result = result.Replace(";", " and ");
                    break;
                }
            }

            index = 0;
            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result += AddСondition(filterRecord, listOfField);
                    index++;
                    if (index < filter.Count)
                    {
                        result += " or ";
                    }
                }
            }

            result += " " + getGroupBy();
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

        private void ButtonListProducts_Click(object sender, RoutedEventArgs e)
        {
            int row_index = ordersGrid.SelectedIndex;
            int idOrder = -1;
            int current_row = 0;
            foreach (DataRowView row in ordersGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                idOrder = (int)row.Row.ItemArray[0];
                break;
            }
            Window listProduct = new OrderProductsListWindow(idOrder);
            listProduct.ShowDialog();
            RefreshList();
        }

        private void ButtonAddFitting_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
        }

        private void OrdersGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            string status = ((DataRowView)e.Row.DataContext).Row.ItemArray[9].ToString();
            switch (status)
            {
                case "Принят":
                    e.Row.Background = Brushes.White;
                    break;
                case "Сдан":
                    e.Row.Background = Brushes.Green;
                    break;
                case "Отправлен":
                    e.Row.Background = Brushes.Orange;
                    break;
                case "Готов":
                    e.Row.Background = Brushes.Aqua;
                    break;
                case "Отменён":
                    e.Row.Background = Brushes.Red;
                    break;
                default:
                    break;
            }
        }
    }
}
