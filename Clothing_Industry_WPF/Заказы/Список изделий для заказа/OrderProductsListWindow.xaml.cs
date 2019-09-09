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
        public float MoneyToEmployee { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для OrderProductsListWindow.xaml
    /// </summary>
    public partial class OrderProductsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private ObservableCollection<HelpStruct> collection;
        private int orderId;

        public bool Result { get; set; }

        public OrderProductsListWindow(int orderId)
        {
            InitializeComponent();
            collection = new ObservableCollection<HelpStruct>();
            this.orderId = orderId;
            listOfProductsGrid.ItemsSource = collection;
            RefreshList();
        }

        private void RefreshList()
        {
            collection = new ObservableCollection<HelpStruct>();
            string query = "select id_Product, Name_Of_Product, Fixed_Price, MoneyToEmployee, Description, Count " +
                           "from products " +
                           "join list_products_to_order on list_products_to_order.Products_id_Product = products.id_Product " +
                           "where list_products_to_order.orders_id_order = @orderID " +
                           "group by Name_Of_Product, Fixed_Price, MoneyToEmployee, Description ;";

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
                        MoneyToEmployee = (float)reader.GetValue(3),
                        Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Count = (int)reader.GetValue(5),
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
                    ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка удаления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Result = true;
            // Тут вроде все окей, но надо переделать, я ничего не понимаю
            //ОК вот блоки
            //Здесь блок с запросом продукта и его количества
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            string query_product = "select Products_id_Product, Count from list_products_to_order where  Orders_id_Order = @orderId; ";
            MySqlCommand command_product = new MySqlCommand(query_product, connection);
            command_product.Parameters.AddWithValue("@orderId", orderId);
            List<int> product_id = new List<int>();
            List<int> countproduct = new List<int>();
            using (DbDataReader reader = command_product.ExecuteReader())
            {
                while (reader.Read())
                {
                    product_id.Add((int)reader.GetValue(0));
                    countproduct.Add((int)reader.GetValue(1));
                }
            }

            //Ищем материал и его количество для заказа
            int i = 0;
            List<int> Materials_Vendor_Code = new List<int>();
            List<float> CountProduct = new List<float>();
            while (i != product_id.Count)
            {
                string query_material = "select materials_for_product.Materials_Vendor_Code,materials_for_product.Count from materials_for_product where materials_for_product.Products_id_Product = @productId; ";
                MySqlCommand command_material = new MySqlCommand(query_material, connection);
                command_material.Parameters.AddWithValue("@productId", product_id[i]);


                using (DbDataReader reader2 = command_material.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        Materials_Vendor_Code.Add((int)reader2.GetValue(0));
                        CountProduct.Add((float)reader2.GetValue(1) * countproduct[i]);
                    }
                }
                i++;
            }
            //Сколько на складе
            i = 0;
            string query_store = "select store.Count from store where store.Materials_Vendor_Code = @Materials_Vendor_Code; ";
            List<float> CountStore = new List<float>();
            string cnt;
            while (i != Materials_Vendor_Code.Count)
            {
                MySqlCommand command_store = new MySqlCommand(query_store, connection);
                command_store.Parameters.AddWithValue("@Materials_Vendor_Code", Materials_Vendor_Code[i]);
                using (DbDataReader reader3 = command_store.ExecuteReader())
                {
                    while (reader3.Read())
                    {
                        cnt = reader3.GetValue(0).ToString();
                        CountStore.Add(float.Parse(cnt));
                        i++;
                    }
                }
            }
            i = 0;
            bool check = true;
            while ((i != CountStore.Count) && (check))
            {
                if ((CountProduct[i] > CountStore[i]))
                    check = false;
                i++;
            }
            //вычет со склада
            if (check)
            {
                i = 0;
                while (i != Materials_Vendor_Code.Count)
                {
                    MySqlTransaction transaction2 = connection.BeginTransaction();
                    string totalquery = "Update store set Count = Count - @CountProduct" +
                                " where Materials_Vendor_Code = @Materials_Vendor_Code;";
                    MySqlCommand command_total = new MySqlCommand(totalquery, connection, transaction2);
                    command_total.Parameters.AddWithValue("@Materials_Vendor_Code", Materials_Vendor_Code[i]);
                    command_total.Parameters.AddWithValue("@CountProduct", CountProduct[i]);

                    try
                    {

                        command_total.ExecuteNonQuery();
                        transaction2.Commit();
                        this.Close();
                    }
                    catch
                    {
                        transaction2.Rollback();
                        MessageBox.Show("Ошибка добавления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    i++;
                }
            }
            //либо чего нет
            else
            {
                List<float> NeedMaterial = new List<float>();
                for (i = 0; i != CountStore.Count; i++)
                {
                    if (CountProduct[i] > CountStore[i])
                    {
                        NeedMaterial.Add(CountProduct[i] - CountStore[i]);
                    }
                }
                //отменяем заказ 
                MySqlTransaction transaction3 = connection.BeginTransaction();
                string queryorder = "update orders set Statuses_Of_Order_id_Status_Of_Order = (select id_Status_Of_Order from statuses_of_order where Name_Of_Status = 'Отменён')" +
                    " where id_Order = @orderId;";
                MySqlCommand command_order = new MySqlCommand(queryorder, connection, transaction3);
                command_order.Parameters.AddWithValue("@orderId", orderId);


                try
                {
                    command_order.ExecuteNonQuery();
                    transaction3.Commit();
                    this.Close();
                    i = 0;
                    string MessageMaterialsNeed = "";
                    string query_materials_name = "select materials.Name_Of_Material,units.Name_Of_Unit  from materials join units on materials.Units_id_Unit = units.id_Unit where  Vendor_Code = @Vendor_Code; ";
                    while (i != Materials_Vendor_Code.Count)
                    {
                        MySqlCommand command_materials_name = new MySqlCommand(query_materials_name, connection);
                        command_materials_name.Parameters.AddWithValue("@Vendor_Code", Materials_Vendor_Code[i]);

                        using (DbDataReader reader3 = command_materials_name.ExecuteReader())
                        {
                            while (reader3.Read())
                            {
                                MessageMaterialsNeed = MessageMaterialsNeed + reader3.GetValue(0).ToString() + " " + NeedMaterial[i].ToString() + " " + reader3.GetValue(1).ToString() + "\n";
                                i++;
                            }
                        }
                    }
                    Result = false;
                    //this.DialogResult = false;
                    MessageBox.Show("Материалов не хватает на изделие, заказ временно отменён!" + "\n" + "Не хватает:" + "\n" + MessageMaterialsNeed, "Недостаточно материалов на скаде", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    transaction3.Rollback();
                    MessageBox.Show("Ошибка добавления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

    }
}
