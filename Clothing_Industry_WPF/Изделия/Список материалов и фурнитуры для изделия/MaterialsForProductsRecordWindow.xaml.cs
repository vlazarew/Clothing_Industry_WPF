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
            string query = "select Name_Of_Group from Groups_Of_Material;";
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

        private string CheckData()
        {
            string result = "";

            if (comboBoxName_Of_Material.SelectedValue == null)
            {
                result += result == "" ? " Группа" : ", Группа";
            }
            if (textBoxCount.Text == "")
            {
                result += result == "" ? " Количество" : ", Количество";
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

                string query = "insert into materials_for_product (Products_id_Product, Groups_Of_Material_id_Group_Of_Material, Count) values (@productId, @name, @count) ";
                MySqlCommand command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@count", float.Parse(textBoxCount.Text));
                command.Parameters.AddWithValue("@name", comboBoxName_Of_Material.SelectedIndex+1);

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

                connection.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(warning);
            }
        }
    }
}
