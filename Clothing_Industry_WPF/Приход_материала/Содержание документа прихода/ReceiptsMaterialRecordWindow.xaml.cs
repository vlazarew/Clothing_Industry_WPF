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

        private string CheckData()
        {
            string result = "";

            if (comboBoxName_Of_Material.SelectedValue == null)
            {
                result += result == "" ? " Название материала" : ",  Название материала";
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

                //Добавление в список документа
                string query = "insert into documents_of_receipts (Receipt_Of_Materials_id_Document_Of_Receipt, Materials_Vendor_Code, Count) values (@DocumentID, @Vendor_Code, @count) ";
                MySqlCommand command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@DocumentID", DocumentId);
                command.Parameters.AddWithValue("@count", int.Parse(textBoxCount.Text));

                // Выборка Артикула и стоимости материала для главной команды
                string query_document = "select Vendor_Code, Cost_Of_Material from materials where Name_Of_Material = @name ";
                MySqlCommand command_document = new MySqlCommand(query_document, connection);
                command_document.Parameters.AddWithValue("@name", comboBoxName_Of_Material.SelectedItem.ToString());
                int Vendor_Code = -1;
                float Cost_Of_Material = -1;
                using (DbDataReader reader = command_document.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Vendor_Code = (int)reader.GetValue(0);
                        Cost_Of_Material = (float)reader.GetValue(0);
                    }
                }

                command.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);

                // Во вступлениях материалов обновляем данные
                string totalquery = "Update receipt_of_materials set Total_Price = Total_Price + @Total_Price" +
                                    " where id_Document_Of_Receipt = @DocumentId;";
                MySqlCommand command_total = new MySqlCommand(totalquery, connection, transaction);
                command_total.Parameters.AddWithValue("@DocumentID", DocumentId);
                command_total.Parameters.AddWithValue("@Total_Price", int.Parse(textBoxCount.Text) * Cost_Of_Material);

                // Добавление в склад
                string storequery = "Update store set Count = Count + @Count" +
                                    " where Materials_Vendor_Code = @Vendor_Code;";
                MySqlCommand command_store = new MySqlCommand(storequery, connection, transaction);
                command_store.Parameters.AddWithValue("@Count", int.Parse(textBoxCount.Text));
                command_store.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);

                try
                {
                    command.ExecuteNonQuery();
                    command_total.ExecuteNonQuery();
                    command_store.ExecuteNonQuery();
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
                MessageBox.Show(warning);
            }
        }
    }
}
