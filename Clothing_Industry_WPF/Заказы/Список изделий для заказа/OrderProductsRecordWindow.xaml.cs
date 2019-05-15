using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа
{
    /// <summary>
    /// Логика взаимодействия для OrderProductsRecordWindow.xaml
    /// </summary>
    public partial class OrderProductsRecordWindow : Window
    {
        // Ввод только букв в численные поля 
        private static readonly Regex _regex = new Regex("[^0-9]");
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private int orderId;


        public OrderProductsRecordWindow(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            FillComboBoxes();
        }

        private void FillComboBoxes()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            string query = "select Name_Of_Product from products ;";
            MySqlCommand command = new MySqlCommand(query, connection);

            connection.Open();

            using (DbDataReader reader = command.ExecuteReader())
            {
                while(reader.Read())
                {
                    comboBoxProducts.Items.Add(reader.GetString(0));
                }
            }

            connection.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBoxCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            e.Handled = !IsTextAllowed(e.Text);

        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();

            string query = "insert into list_products_to_order (Orders_id_Order, Products_id_Product, Count) values (@orderId, @productId, @count); ";
            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@orderId", orderId);
            command.Parameters.AddWithValue("@count", int.Parse(textBoxCount.Text));


            string query_product = "select id_product from products where  Name_Of_Product = @name; ";
            MySqlCommand command_product = new MySqlCommand(query_product, connection);
            command_product.Parameters.AddWithValue("@name", comboBoxProducts.SelectedItem.ToString());
            int product_id = -1;
            using (DbDataReader reader = command_product.ExecuteReader())
            {
                while (reader.Read())
                {
                    product_id = (int)reader.GetValue(0);
                }
            }

            command.Parameters.AddWithValue("@productId", product_id);

            try
            {
                command.ExecuteNonQuery();
                transaction.Commit();
                this.Close();
            }
            catch
            {
                transaction.Rollback();
                MessageBox.Show("Ошибка добавления");
            }
            ////////////////////////
            string query_material = "select materials_for_product.Materials_Vendor_Code,materials_for_product.Count from materials_for_product where materials_for_product.Products_id_Product = @productId; ";
            MySqlCommand command_material = new MySqlCommand(query_material, connection);
            command_material.Parameters.AddWithValue("@productId", product_id);
            int[] Materials_Vendor_Code = new int[30];
            int[] CountProduct = new int[30];
            int i = 0;
            using (DbDataReader reader2 = command_material.ExecuteReader())
            {
                while (reader2.Read())
                {
                    Materials_Vendor_Code[i] = (int)reader2.GetValue(0);
                    CountProduct[i] = (int)reader2.GetValue(1) * int.Parse(textBoxCount.Text);
                    i++;
                }
            }
            //// 
            i = 0;
            string query_store = "select store.Count from store where store.Materials_Vendor_Code = @Materials_Vendor_Code; ";
            int[] CountStore = new int[30];
            string cnt;
            while (Materials_Vendor_Code[i] != 0)
            {
                MySqlCommand command_store = new MySqlCommand(query_store, connection);
                command_store.Parameters.AddWithValue("@Materials_Vendor_Code", Materials_Vendor_Code[i]);
                using (DbDataReader reader3 = command_store.ExecuteReader())
                {
                    while (reader3.Read())
                    {
                        cnt = reader3.GetValue(0).ToString();
                        CountStore[i] = Int32.Parse(cnt);
                        i++;                      
                    }
                }
            }
            i = 0;
            bool check = true;
            while ((CountProduct[i] <= CountStore[i])&&(i!=29))
                i++;
            if (i != 29)
                check = false;
            if (check)
            {
                i = 0;
                while (Materials_Vendor_Code[i] != 0)
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
                        MessageBox.Show("Ошибка добавления");
                    }
                    i++;
                }
            }
            else
            {
                int[] NeedMaterial = new int[30];
                for (i = 0; i<30;i++)
                {
                    if (CountProduct[i] > CountStore[i])
                    {
                        NeedMaterial[i] = CountProduct[i] - CountStore[i];
                    }
                }
                //отменяем заказ 
                MySqlTransaction transaction3 = connection.BeginTransaction();
                string queryorder = "update orders set Statuses_Of_Order_id_Status_Of_Order = (select id_Status_Of_Order from statuses_of_order where Name_Of_Status = 'Отменён')" +
                    " where id_Order = @orderId;";
                MySqlCommand command_order= new MySqlCommand(queryorder, connection, transaction);
                command_order.Parameters.AddWithValue("@orderId", orderId);


                try
                {
                    command_order.ExecuteNonQuery();
                    transaction3.Commit();
                    this.Close();
                    i = 0;
                    string MessageMaterialsNeed = "";
                    string query_materials_name = "select materials.Name_Of_Material,units.Name_Of_Unit  from materials join units on materials.Units_id_Unit = units.id_Unit where  Vendor_Code = @Vendor_Code; ";                   
                    while (Materials_Vendor_Code[i] != 0)
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
                    MessageBox.Show("Материалов не хватает на изделие, заказ временно отменён!" + "\n" + "Не хватает:" + "\n" + MessageMaterialsNeed);
                }
                catch
                {
                    transaction3.Rollback();
                    MessageBox.Show("Ошибка добавления");
                }
            }
            /////////////////////
            connection.Close();
        }
    }
}
