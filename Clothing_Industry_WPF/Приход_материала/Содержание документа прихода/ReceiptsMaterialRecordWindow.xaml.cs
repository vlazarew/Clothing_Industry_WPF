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


namespace Clothing_Industry_WPF.Приход_материала
{
    /// <summary>
    /// Логика взаимодействия для ReceiptsMaterialRecordWindow.xaml
    /// </summary>
    public partial class ReceiptsMaterialRecordWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9]");
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private int DocumentId;
        public ReceiptsMaterialRecordWindow(int DocumentId)
        {
            InitializeComponent();
            this.DocumentId = DocumentId;
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

        private void TextBoxCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            e.Handled = !IsTextAllowed(e.Text);

        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            //Добавление в список документа
            string query = "insert into documents_of_receipts (Receipt_Of_Materials_id_Document_Of_Receipt, Materials_Vendor_Code, Count) values (@DocumentID, @Vendor_Code, @count) ";
            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@DocumentID", DocumentId);
            command.Parameters.AddWithValue("@count", int.Parse(textBoxCount.Text));

            ////
            string query_document = "select Vendor_Code from materials where Name_Of_Material = @name ";
            MySqlCommand command_document = new MySqlCommand(query_document, connection);
            command_document.Parameters.AddWithValue("@name", comboBoxName_Of_Material.SelectedItem.ToString());
            int Vendor_Code = -1;
            using (DbDataReader reader = command_document.ExecuteReader())
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
                MessageBox.Show("Ошибка добавления");
            }

            //Увеличение суммы
            string query_price = "select materials.Cost_Of_Material from materials where materials.Name_Of_Material = @name ";
            MySqlCommand command_price = new MySqlCommand(query_price, connection);
            command_price.Parameters.AddWithValue("@name", comboBoxName_Of_Material.SelectedItem.ToString());
            float Cost_Of_Material = -1;
            using (DbDataReader reader2 = command_price.ExecuteReader())
            {
                while (reader2.Read())
                {
                    Cost_Of_Material = (float)reader2.GetValue(0);
                }
            }
            ////      
            MySqlTransaction transaction2 = connection.BeginTransaction();
            string totalquery = "Update receipt_of_materials set Total_Price = Total_Price + @Total_Price" +
                        " where id_Document_Of_Receipt = @DocumentId;";
            MySqlCommand command_total = new MySqlCommand(totalquery, connection, transaction2);
            command_total.Parameters.AddWithValue("@DocumentID", DocumentId);
            command_total.Parameters.AddWithValue("@Total_Price", int.Parse(textBoxCount.Text) * Cost_Of_Material);
            
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

            //добавление в склад

            ////      
            MySqlTransaction transaction3 = connection.BeginTransaction();
            string storequery = "Update store set Count = Count + @Count" +
                        " where Materials_Vendor_Code = @Vendor_Code;";
            MySqlCommand command_store = new MySqlCommand(storequery, connection, transaction3);
            command_store.Parameters.AddWithValue("@Count", int.Parse(textBoxCount.Text));
            command_store.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);
            try
            {
                command_store.ExecuteNonQuery();
                transaction3.Commit();
                this.Close();
            }
            catch
            {
                transaction3.Rollback();
                MessageBox.Show("Ошибка добавления");
            }




            connection.Close();
        }
    }
}
