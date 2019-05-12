﻿using MySql.Data.MySqlClient;
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

namespace Clothing_Industry_WPF.Изделия
{
    /// <summary>
    /// Логика взаимодействия для MaterialsForProductsRecordWindow.xaml
    /// </summary>
    public partial class MaterialsForProductsRecordWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9]");
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private int productId;

        public MaterialsForProductsRecordWindow(int productId)
        {
            InitializeComponent();
            this.productId = productId;
            FillComboBoxes();
        }

        private void FillComboBoxes()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            string query = "select Name_Of_Material from materials ;";
            MySqlCommand command = new MySqlCommand(query, connection);

            connection.Open();

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_Material.Items.Add(reader.GetString(0));
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

            string query = "insert into materials_for_product (Products_id_Product, Materials_Vendor_Code, Count) values (@productId, @Vendor_Code, @count) ";
            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@productId", productId);
            command.Parameters.AddWithValue("@count", int.Parse(textBoxCount.Text));


            string query_product = "select Vendor_Code from materials where Name_Of_Material = @name ";
            MySqlCommand command_product = new MySqlCommand(query_product, connection);
            command_product.Parameters.AddWithValue("@name", comboBoxName_Of_Material.SelectedItem.ToString());
            int Vendor_Code = -1;
            using (DbDataReader reader = command_product.ExecuteReader())
            {
                while (reader.Read())
                {
                    Vendor_Code = (int)reader.GetValue(0);
                }
            }

            command.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);

            try
            {
                command.ExecuteNonQuery();
                transaction.Commit();
                this.Close();
            }
            catch
            {
                transaction.Rollback();
                MessageBox.Show("Ошибка добвления");
            }

            connection.Close();
        }
    }
}
