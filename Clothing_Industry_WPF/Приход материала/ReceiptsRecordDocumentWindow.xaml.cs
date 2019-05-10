using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Приход_материала
{
    /// <summary>
    /// Логика взаимодействия для ReceiptRecordDocumentWindow.xaml
    /// </summary>
    public partial class ReceiptsRecordDocumentWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string image_path;
        private byte[] image_bytes;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string old_Name_Of_Document = "";
        public ReceiptsRecordDocumentWindow(WaysToOpenForm.WaysToOpen waysToOpen, string Name_Of_Document = "")
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();

            if (Name_Of_Document != "")
            {
                old_Name_Of_Document = Name_Of_Document;
                FillFields(Name_Of_Document);
            }
        }

        private void FillFields(string Name_Of_Document)
        {
            {
                string query_text = "select  documents_of_receipts.Name_Of_Document," +
                                        "DATE_FORMAT( documents_of_receipts.Date_Of_Entry, \"%d.%m.%Y\") as Date_Of_Entry, suppliers.Name_Of_Supplier, documents_of_receipts.Notes, payment_states.Name_Of_State, type_of_transactions.Name_Of_Type" +
                                        " from documents_of_receipts" +
                                        " join receipt_of_materials on documents_of_receipts.id_Document_Of_Receipt = receipt_of_materials.Documents_Of_Receipts_id_Document_Of_Receipt" +
                                        " join payment_states on receipt_of_materials.Payment_States_id_Payment_States = payment_states.id_Payment_States" +
                                        " join type_of_transactions on receipt_of_materials.Type_Of_Transactions_id_Type_Of_Transaction = type_of_transactions.id_Type_Of_Transaction" +
                                        " join suppliers on receipt_of_materials.Suppliers_id_Supplier = suppliers.id_Supplier" +
                                        " where documents_of_receipts.id_Document_Of_Receipt = @id_Document_Of_Receipt;";
                MySqlCommand command = new MySqlCommand(query_text, connection);
                command.Parameters.AddWithValue("@Name_Of_Document", Name_Of_Document);
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        textBoxName_Of_Document.Text = reader.GetString(0);
                        datePickerComing_Date.SelectedDate = DateTime.Parse(reader.GetString(1));
                        comboBoxName_Of_Supplier.Text = reader.GetString(2);
                        if (reader.GetValue(3).ToString() != "")
                        {
                            textBoxNotes.Text = reader.GetString(3);
                        }
                        comboBoxName_Of_State.SelectedValue = reader.GetString(4);
                        comboBoxName_Of_Type.SelectedValue = reader.GetString(5);                       
                    }
                }
                connection.Close();
            }
        }

        private void setNewTitle()
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    this.Title += " (Создание)";
                    Header.Content += " (Создание)";
                    datePickerComing_Date.Text = DateTime.Now.ToLongDateString();
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";
                    Header.Content += " (Изменение)";
                    break;
                default:
                    break;

            }
        }
        private void FillComboBoxes()
        {
            connection.Open();

            string query = "select Name_Of_Supplier from suppliers";

            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_Supplier.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_State from payment_states";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_State.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_Type from type_of_transactions";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_Type.Items.Add(reader.GetString(0));
                }
            }
            connection.Close();
        }

        private string CheckData()
        {
            string result = "";

            if (textBoxName_Of_Document.Text == "")
            {
                result += result == "" ? "Название документа" : ", Название документа";
            }
            if (datePickerComing_Date.SelectedDate.Value.ToString() == "")
            {
                result += result == "" ? "Дата прихода" : ", Дата прихода";
            }
            if (comboBoxName_Of_Supplier.SelectedValue == null)
            {
                result += result == "" ? " Поставщик" : ",  Поставщик";
            }
            if (textBoxNotes.Text == "")
            {
                result += result == "" ? "Примечания" : ", Примечания";
            }

            if (comboBoxName_Of_State.SelectedValue == null)
            {
                result += result == "" ? " Статус" : ",  Статус";
            }
            if (comboBoxName_Of_Type.SelectedValue == null)
            {
                result += result == "" ? "Тип транзакции" : ", Тип транзакции";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
            if (warning == "")
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                //Создать/изменить запись в таблице Пользователи
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;
               
                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!");
                }

                connection.Close();
                this.Hide();
            }
            else
            {
                System.Windows.MessageBox.Show(warning);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            //не оч default_folder
            string query1 = "";
            string query2 = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                //document
                query1 = "INSERT INTO documents_of_receipts " +
                                       "(Default_Folder, Name_of_Document, Date_Of_Entry)" +
                                       " VALUES ('1', @Name_of_Document, @Date_Of_Entry);";
                //receipt
                query2 = "INSERT INTO receipt_of_materials " +
                                       "(Documents_Of_Receipts_id_Document_Of_Receipt, Coming_Date, Summ, Notes, Payment_States_id_Payment_States, Type_Of_Transactions_id_Type_Of_Transaction, Suppliers_id_Supplier)" +
                                       " VALUES (@id_Document_Of_Receipt, @Date_Of_Entry, 0, @Notes, @States, @Type, @Supplier);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query1 = "Update documents_of_receipts set Default_Folder = 1, Name_of_Document = @Name_of_Document,  Date_Of_Entry = @Date_Of_Entry" +
                        " where Name_Of_Document = @old_Name_Of_Document;";
                query2 = "Update receipt_of_materials set Documents_Of_Receipts_id_Document_Of_Receipt = @id_Document_Of_Receipt, Coming_Date = @Date_Of_Entry, Notes = @Notes," +
                        "Payment_States_id_Payment_States = @States, Type_Of_Transactions_id_Type_Of_Transaction = @Type, Suppliers_id_Supplier = @Supplier, " +
                        " where Login = @old_id_Document_Of_Receipt;";

            }

            MySqlCommand command = new MySqlCommand(query1, connection);
            command.Parameters.AddWithValue("@Name_Of_Document", textBoxName_Of_Document.Text);
            command.Parameters.AddWithValue("@Date_Of_Entry", datePickerComing_Date.SelectedDate.Value);           
            command.Parameters.AddWithValue("@Notes", textBoxNotes.Text);

            MySqlCommand commandsupplier = new MySqlCommand("select id_Supplier from suppliers where Name_Of_Supplier = @Name_Of_Supplier", connection);
            commandsupplier.Parameters.AddWithValue("Name_Of_Supplier", comboBoxName_Of_Supplier.SelectedItem.ToString());
            int id_Supplier = -1;
            using (DbDataReader reader = commandsupplier.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Supplier = reader.GetInt32(0);
                }
            }

            MySqlCommand commandpayment = new MySqlCommand("select id_Payment_States from payment_states where Name_Of_State = @Name_Of_State", connection);
            commandpayment.Parameters.AddWithValue("Name_Of_State", comboBoxName_Of_State.SelectedItem.ToString());
            int id_Payment_States = -1;
            using (DbDataReader reader = commandpayment.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Payment_States = reader.GetInt32(0);
                }
            }

            MySqlCommand commandtype = new MySqlCommand("select id_Type_Of_Transaction from type_of_transactions where Name_Of_Type = @Name_Of_Type", connection);
            commandtype.Parameters.AddWithValue("Name_Of_State", comboBoxName_Of_Type.SelectedItem.ToString());
            int id_Type_Of_Transaction = -1;
            using (DbDataReader reader = commandtype.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Type_Of_Transaction = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@Name_Of_Supplier", id_Supplier);
            command.Parameters.AddWithValue("@Name_Of_State", id_Payment_States);
            command.Parameters.AddWithValue("@Type_Of_Transaction", id_Payment_States);

           

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldName_Of_Document", old_Name_Of_Document);
            }

            return command;
        }
    }
}
