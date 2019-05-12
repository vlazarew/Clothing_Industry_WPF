using Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public struct HelpStruct
    {
        public int id { get; set; }
        public string Name_Of_Product { get; set; }
        public float Fixed_Price { get; set; }
        public float Per_Cents { get; set; }
        public float Added_Price_For_Complexity { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для OrderProductsListWindow.xaml
    /// </summary>
    public partial class OrderProductsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;

        /*private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        List<int> resultProductId;
        */
        private ObservableCollection<HelpStruct> collection;
        private int orderId;

        public OrderProductsListWindow(int orderId)
        {
            InitializeComponent();
            /*currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
            resultProductId = new List<int>();*/
            collection = new ObservableCollection<HelpStruct>();
            this.orderId = orderId;
            listOfProductsGrid.ItemsSource = collection;
            RefreshList();
        }

        private void RefreshList()
        {
            collection = new ObservableCollection<HelpStruct>();
            string query = "select id_Product, Name_Of_Product, Fixed_Price, Per_Cents, Added_Price_For_Complexity, Description, Count " +
                            "from products " +
                            "join list_products_to_order  on list_products_to_order.Products_id_Product = products.id_Product " +
                            "where list_products_to_order.orders_id_order = @orderID " +
                            "group by Name_Of_Product, Fixed_Price, Per_Cents, Added_Price_For_Complexity, Description ;";

            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@orderID", orderId);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    collection.Add(new HelpStruct()
                    {
                        id = (int)reader.GetValue(0),
                        Name_Of_Product = reader.GetString(1),
                        Fixed_Price = (float)reader.GetValue(2),
                        Per_Cents = (float)reader.GetValue(3),
                        Added_Price_For_Complexity = (float)reader.GetValue(4),
                        Description = reader.GetString(5),
                        Count = (int)reader.GetValue(6),
                    });
                }
            }

            listOfProductsGrid.ItemsSource = collection;
            connection.Close();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexesToDelete = new List<int>();
            foreach (HelpStruct row in listOfProductsGrid.SelectedItems)
            {
                indexesToDelete.Add(row.id);
            }

            DeleteFromDB(indexesToDelete);
        }

        private void DeleteFromDB(List<int> ids)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from list_products_to_order where Products_id_Product = @id_Products and Orders_id_Order = @id_order";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id_Products", id);
                commandTable.Parameters.AddWithValue("@id_order", orderId);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка удаления");
                }
            }

            connection.Close();
            RefreshList();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            Window window_add = new OrderProductsRecordWindow(orderId);
            window_add.ShowDialog();
            RefreshList();
        }
    }
}
