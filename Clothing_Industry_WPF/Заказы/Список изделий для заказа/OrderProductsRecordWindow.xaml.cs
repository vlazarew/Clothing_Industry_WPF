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
                while (reader.Read())
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

        private string CheckData()
        {
            string result = "";

            if (comboBoxProducts.SelectedValue == null)
            {
                result += result == "" ? " Изделие" : ",  Изделие";
            }


            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
            if (warning == "")
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                string query = "insert into list_products_to_order (Orders_id_Order, Products_id_Product, Count) values (@orderId, @productId, @count); ";
                MySqlCommand command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@orderId", orderId);


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

                /////////////////////
                connection.Close();
            }
            else
            {
                MessageBox.Show(warning);
            }
        }
    }
}
