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
    /// Логика взаимодействия для OrderRecordMaterialForProduct.xaml
    /// </summary>
   
    public partial class OrderRecordMaterialForProduct : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9]");
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private int idmaterialproduct;
        private int productId;
        private int groupofmaterial;
        private float count;

        public OrderRecordMaterialForProduct(int productId, int groupofmaterial, int idmaterialproduct, float count)
        {
            InitializeComponent();
            this.idmaterialproduct = idmaterialproduct;
            this.productId = productId;
            this.groupofmaterial = groupofmaterial;
            this.count = count;
            FillComboBoxes();
        }

        private void FillComboBoxes()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            string query = "select Name_Of_Material, Vendor_Code from Materials where Groups_Of_Material_id_Group_Of_Material = @groupofmaterial;";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@groupofmaterial", groupofmaterial);
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
                result += result == "" ? " Название" : ", Название";
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

                string query = "Update materials_for_product set Materials_Vendor_Code = @Vendor_Code " +
                        " where Products_id_Product = @productId and Groups_Of_Material_id_Group_Of_Material = @groupofmaterial and Count = @count; ";
                MySqlCommand command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@groupofmaterial", groupofmaterial);
                command.Parameters.AddWithValue("@count", count);

                MySqlCommand commandvendor = new MySqlCommand("select vendor_code from materials where Name_Of_Material = @Name_Of_Material", connection);
                commandvendor.Parameters.AddWithValue("@Name_Of_Material", comboBoxName_Of_Material.SelectedItem.ToString());
                int vendor_code = -1;
                using (DbDataReader reader = commandvendor.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vendor_code = reader.GetInt32(0);
                    }
                }
                command.Parameters.AddWithValue("@Vendor_Code", vendor_code);
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
