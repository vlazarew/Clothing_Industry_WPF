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
            string query_text = "select orders.id_Order, orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible, " +
                                "list_products_to_order.Orders_id_Order, group_concat(products.Name_Of_Product separator ', ') as Products " +
                                "from orders " +
                                "join types_of_order on types_of_order.id_Type_Of_Order = orders.Types_Of_Order_id_Type_Of_Order " +
                                "join statuses_of_order on statuses_of_order.id_Status_Of_Order = orders.Statuses_Of_Order_id_Status_Of_Order " +
                                "join list_products_to_order on list_products_to_order.Orders_id_Order = orders.id_order " +
                                "join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                "join employees on(employees.Login = orders.Responsible) " +
                                "join products on products.id_product = list_products_to_order.Products_id_Product " +
                                "group by orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible, list_products_to_order.Orders_id_Order ;";

            return query_text;
        }

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

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonFilters_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
