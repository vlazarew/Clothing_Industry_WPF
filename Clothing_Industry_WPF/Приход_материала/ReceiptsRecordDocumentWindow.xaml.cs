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
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int old_id_Document_Of_Receipt = -1;
        private int idDocument_Of_Receipt;
        public ReceiptsRecordDocumentWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id_Document_Of_Receipt = -1)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();

            if (id_Document_Of_Receipt != -1)
            {
                idDocument_Of_Receipt = id_Document_Of_Receipt;
                FillFields(id_Document_Of_Receipt);
            }
        }

        private void FillFields(int id_Document_Of_Receipt)
        {
            {
                string query_text = "select documents_of_receipts.Name_Of_Document," +
                                " DATE_FORMAT(documents_of_receipts.Date_Of_Entry, '%d.%m.%Y') as Date_Of_Entry," +
                                " suppliers.Name_Of_Supplier, documents_of_receipts.Default_Folder, payment_states.Name_Of_State," +
                                " type_of_transactions.Name_Of_Type, documents_of_receipts.Materials_Vendor_Code" +
                                " from documents_of_receipts" +
                                " join receipt_of_materials on documents_of_receipts.id_Document_Of_Receipt = receipt_of_materials.Documents_Of_Receipts_id_Document_Of_Receipt" +
                                " join payment_states on receipt_of_materials.Payment_States_id_Payment_States = payment_states.id_Payment_States" +
                                " join type_of_transactions on receipt_of_materials.Type_Of_Transactions_id_Type_Of_Transaction = type_of_transactions.id_Type_Of_Transaction" +
                                " join suppliers on receipt_of_materials.Suppliers_id_Supplier = suppliers.id_Supplier" +
                                " where documents_of_receipts.id_Document_Of_Receipt = @id_Document_Of_Receipt;";

                MySqlCommand command = new MySqlCommand(query_text, connection);
                command.Parameters.AddWithValue("@id_Document_Of_Receipt", id_Document_Of_Receipt);
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        textBoxName_Of_Document.Text = reader.GetString(0);
                        datePickerComing_Date.SelectedDate = DateTime.Parse(reader.GetString(1));
                        comboBoxName_Of_Supplier.Text = reader.GetString(2);
                        textBoxDefault_Folder.Text = reader.GetString(3);                       
                        comboBoxName_Of_State.SelectedValue = reader.GetString(4);
                        comboBoxName_Of_Type.SelectedValue = reader.GetString(5);
                        comboBoxVendor_Code.SelectedValue = reader.GetString(6);
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

            query = "select Vendor_Code from materials";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxVendor_Code.Items.Add(reader.GetString(0));
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
            if (textBoxDefault_Folder.Text == "")
            {
                result += result == "" ? "Путь документа" : ", Путь документа";
            }

            if (comboBoxName_Of_State.SelectedValue == null)
            {
                result += result == "" ? " Статус" : ",  Статус";
            }
            if (comboBoxName_Of_Type.SelectedValue == null)
            {
                result += result == "" ? "Тип транзакции" : ", Тип транзакции";
            }
            if (comboBoxVendor_Code.SelectedValue == null)
            {
                result += result == "" ? "Артикул материала" : ", Артикул материала";
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
                //doc
                MySqlCommand command = actionInDBCommand1(connection);
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
                
                //rec
                MySqlConnection connection2 = new MySqlConnection(connectionString);
                MySqlTransaction transaction2;

                connection2.Open();
                transaction2 = connection2.BeginTransaction();

                MySqlCommand command2 = actionInDBCommand2(connection2);
                command2.Transaction = transaction2;

                //try
                //{
                    //command.ExecuteNonQuery();
                    transaction2.Commit();
                //}
                //catch
                //{
                //    transaction2.Rollback();
                //    System.Windows.MessageBox.Show("Ошибка сохранения!");
                //}

                connection2.Close();
                this.Hide();
            }
            else
            {
                System.Windows.MessageBox.Show(warning);
            }
        }
        //doc
        private MySqlCommand actionInDBCommand1(MySqlConnection connection)
        {
            //не оч default_folder
            string query = "";

            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                //document
                query = "INSERT INTO documents_of_receipts " +
                                       "(Default_Folder, Name_of_Document, Date_Of_Entry, Amount, Price_For_One, Total_Price, Materials_Vendor_Code)" +
                                       " VALUES (@Default_Folder, @Name_of_Document, @Date_Of_Entry,0,0,0, @Materials_Vendor_Code);";
                

            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update documents_of_receipts set Default_Folder = @Default_Folder, Name_of_Document = @Name_of_Document,  Date_Of_Entry = @Date_Of_Entry, Materials_Vendor_Code = @Materials_Vendor_Code" +
                        " where id_Document_Of_Receipt = @old_id_Document_Of_Receipt;";              
            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name_Of_Document", textBoxName_Of_Document.Text);
            command.Parameters.AddWithValue("@Date_Of_Entry", datePickerComing_Date.SelectedDate.Value);           
            command.Parameters.AddWithValue("@Default_Folder", textBoxDefault_Folder.Text);

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
            commandtype.Parameters.AddWithValue("Name_Of_Type", comboBoxName_Of_Type.SelectedItem.ToString());
            int id_Type_Of_Transaction = -1;
            using (DbDataReader reader = commandtype.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Type_Of_Transaction = reader.GetInt32(0);
                }
            }
            /*
            MySqlCommand commandvendor = new MySqlCommand("select Materials_Vendor_Code from documents_of_receipts where Materials_Vendor_Code = @Materials_Vendor_Code", connection);
            commandtype.Parameters.AddWithValue("Materials_Vendor_Code", (int)comboBoxVendor_Code.SelectedItem);
            int Materials_Vendor_Code = -1;
            using (DbDataReader reader = commandtype.ExecuteReader())
            {
                while (reader.Read())
                {
                    Materials_Vendor_Code = reader.GetInt32(0);
                }
            }
            */
            command.Parameters.AddWithValue("@Name_Of_Supplier", id_Supplier);
            command.Parameters.AddWithValue("@Name_Of_State", id_Payment_States);
            command.Parameters.AddWithValue("@Type_Of_Transaction", id_Type_Of_Transaction);
            command.Parameters.AddWithValue("@Materials_Vendor_Code", int.Parse(comboBoxVendor_Code.SelectedItem.ToString()));



            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@old_id_Document_Of_Receipt", old_id_Document_Of_Receipt);
            }

            return command;
        }

        //rec
        private MySqlCommand actionInDBCommand2(MySqlConnection connection)
        {
            string query = "";

            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO receipt_of_materials " +
                                       "(Documents_Of_Receipts_id_Document_Of_Receipt, Summ, Notes, Payment_States_id_Payment_States, Type_Of_Transactions_id_Type_Of_Transaction, Suppliers_id_Supplier)" +
                                       " VALUES (@id_Document_Of_Receipt, 0, '', @States, @Type, @Supplier);";


            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update receipt_of_materials set Documents_Of_Receipts_id_Document_Of_Receipt = @id_Document_Of_Receipt," +
                        "Payment_States_id_Payment_States = @States, Type_Of_Transactions_id_Type_Of_Transaction = @Type, Suppliers_id_Supplier = @Supplier, " +
                        "where id_Document_Of_Receipt = @old_id_Document_Of_Receipt;";
            }

            MySqlCommand command2 = new MySqlCommand(query, connection);

            command2.Parameters.AddWithValue("@id_Document_Of_Receipt", idDocument_Of_Receipt);
            command2.Parameters.AddWithValue("@Date_Of_Entry", datePickerComing_Date.SelectedDate.Value);
            command2.Parameters.AddWithValue("@Default_Folder", textBoxDefault_Folder.Text);

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
            commandtype.Parameters.AddWithValue("Name_Of_Type", comboBoxName_Of_Type.SelectedItem.ToString());
            int id_Type_Of_Transaction = -1;
            using (DbDataReader reader = commandtype.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Type_Of_Transaction = reader.GetInt32(0);
                }
            }

            MySqlCommand commandvendor = new MySqlCommand("select Materials_Vendor_Code from documents_of_receipts where Materials_Vendor_Code = @Materials_Vendor_Code", connection);
            commandtype.Parameters.AddWithValue("Materials_Vendor_Code", comboBoxVendor_Code.SelectedItem.ToString());
            int Materials_Vendor_Code = -1;
            using (DbDataReader reader = commandtype.ExecuteReader())
            {
                while (reader.Read())
                {
                    Materials_Vendor_Code = reader.GetInt32(0);
                }
            }

            command2.Parameters.AddWithValue("@Name_Of_Supplier", id_Supplier);
            command2.Parameters.AddWithValue("@Name_Of_State", id_Payment_States);
            command2.Parameters.AddWithValue("@Type_Of_Transaction", id_Payment_States);
            command2.Parameters.AddWithValue("@Materials_Vendor_Code", Materials_Vendor_Code);



            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command2.Parameters.AddWithValue("@old_id_Document_Of_Receipt", old_id_Document_Of_Receipt);
            }

            return command2;
        }             
    }
}
