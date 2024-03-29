﻿using Clothing_Industry_WPF.Перечисления;

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

namespace Clothing_Industry_WPF.Расходы
{
    /// <summary>
    /// Логика взаимодействия для CostsRecordWindow.xaml
    /// </summary>
    public partial class CostsRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int old_login = -1;

        public CostsRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int login = -1)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();

            if (login != -1)
            {
                old_login = login;
                FillFields(login);
            }
        }

        private void FillFields(int login)
        {
            string query_text = "select costs.Default_Folder, " +
                                " DATE_FORMAT(costs.Date_Of_Cost, \"%d.%m.%Y\") as Date_Of_Cost, costs.Name_Of_Document, costs.Amount, costs.Notes, " +
                                " consumption_categories.Name_Of_Category, types_of_payment.Name_Of_Type, periodicities.Name_Of_Periodicity, costs.To, costs.From" +
                                " from costs" +
                                " join consumption_categories on costs.Consumption_Categories_id_Consumption_Category = consumption_categories.id_Consumption_Category" +
                                " join types_of_payment on costs.Types_Of_Payment_id_Of_Type = types_of_payment.id_Of_Type" +
                                " join periodicities on costs.Periodicities_id_Periodicity = periodicities.id_Periodicity" +
                                " where costs.id = @login";

            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@login", login);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxFileFolder.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    datePickerDate_Of_Cost.SelectedDate = DateTime.Parse(reader.GetString(1));
                    if (reader.GetValue(2).ToString() != "")
                    {
                        textBoxFileName.Text = reader.GetString(2);
                    }
                    textBoxAmount.Text = reader.GetString(3);
                    if (reader.GetValue(4).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(4);
                    }
                    comboBoxName_Of_Category.SelectedValue = reader.GetString(5);
                    comboBoxName_Of_Type.SelectedValue = reader.GetString(6);
                    comboBoxName_Of_Periodicity.SelectedValue = reader.GetString(7);
                    textBoxTo.Text = reader.GetString(8);
                    textBoxFrom.Text = reader.GetString(9);
                }
            }
            connection.Close();

        }

        private void setNewTitle()
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    this.Title += " (Создание)";

                    datePickerDate_Of_Cost.Text = DateTime.Now.ToLongDateString();
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";

                    break;
                default:
                    break;
            }
        }

        private void FillComboBoxes()
        {
            connection.Open();

            string query = "select Name_Of_Category from consumption_categories";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_Category.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_Type from types_of_payment";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_Type.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_Periodicity from periodicities";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxName_Of_Periodicity.Items.Add(reader.GetString(0));
                }
            }
            connection.Close();
        }

        private string CheckData()
        {
            string result = "";

            if (datePickerDate_Of_Cost.SelectedDate == null)
            {
                result += result == "" ? "Дата расхода" : ", Дата расхода";
            }
            if (textBoxAmount.Text == "")
            {
                result += result == "" ? "Сумма" : ", Сумма";
            }
            if (comboBoxName_Of_Category.SelectedValue == null)
            {
                result += result == "" ? " Категория расхода" : ",  Категория расхода";
            }
            if (comboBoxName_Of_Type.SelectedValue == null)
            {
                result += result == "" ? "Тип оплаты" : ", Тип оплаты";
            }
            if (comboBoxName_Of_Periodicity.SelectedValue == null)
            {
                result += result == "" ? "Периодичность" : ", Периодичность";
            }
            if (textBoxFrom.Text == "")
            {
                result += result == "" ? "От" : ", От";
            }
            if (textBoxTo.Text == "")
            {
                result += result == "" ? "Кому" : ", Кому";
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

                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    this.Close();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                connection.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO costs (Default_Folder, Date_Of_Cost, Name_Of_Document, Amount, Notes, Consumption_Categories_id_Consumption_Category," +
                        " Types_Of_Payment_id_Of_Type, Periodicities_id_Periodicity, costs.From, costs.To) " +
                        "VALUES (@Default_Folder, @Date_Of_Cost, @Name_Of_Document, @Amount, @Notes, @Consumption_Category, @Name_Of_Type, @Name_Of_Periodicity, @From, @To);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update costs set Default_Folder = @Default_Folder, Name_Of_Document = @Name_Of_Document,  Date_Of_Cost = @Date_Of_Cost, Amount = @Amount, Notes = @Notes," +
                        " Consumption_Categories_id_Consumption_Category = @Consumption_Category, " +
                        "Types_Of_Payment_id_Of_Type = @Name_Of_Type, Periodicities_id_Periodicity = @Name_Of_Periodicity, costs.From = @From, costs.To = @To " +
                        " where id = @oldLogin ; ";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Default_Folder", textBoxFileFolder.Text);
            command.Parameters.AddWithValue("@Date_Of_Cost", datePickerDate_Of_Cost.SelectedDate.Value);
            command.Parameters.AddWithValue("@Name_Of_Document", textBoxFileName.Text);
            command.Parameters.AddWithValue("@Amount", textBoxAmount.Text);
            command.Parameters.AddWithValue("@Notes", textBoxNotes.Text);       
            command.Parameters.AddWithValue("@From", textBoxFrom.Text);
            command.Parameters.AddWithValue("@To", textBoxTo.Text);

            MySqlCommand commandConsumptionCategory = new MySqlCommand("select id_Consumption_Category from consumption_categories where Name_Of_Category = @Consumption_Category", connection);
            commandConsumptionCategory.Parameters.AddWithValue("Consumption_Category", comboBoxName_Of_Category.SelectedItem.ToString());
            int id_consumption_category = -1;
            using (DbDataReader reader = commandConsumptionCategory.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_consumption_category = reader.GetInt32(0);
                }
            }

            MySqlCommand commandType = new MySqlCommand("select id_Of_Type from types_of_payment where Name_Of_Type = @Name_Of_Type", connection);
            commandType.Parameters.AddWithValue("Name_Of_Type", comboBoxName_Of_Type.SelectedItem.ToString());
            int id_Of_Type = -1;
            using (DbDataReader reader = commandType.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Of_Type = reader.GetInt32(0);
                }
            }

            MySqlCommand commandPeriod = new MySqlCommand("select id_Periodicity from periodicities where Name_Of_Periodicity = @Name_Of_Periodicity", connection);
            commandPeriod.Parameters.AddWithValue("Name_Of_Periodicity", comboBoxName_Of_Periodicity.SelectedItem.ToString());
            int id_Periodicity = -1;
            using (DbDataReader reader = commandPeriod.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_Periodicity = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@Consumption_Category", id_consumption_category);
            command.Parameters.AddWithValue("@Name_Of_Type", id_Of_Type);
            command.Parameters.AddWithValue("@Name_Of_Periodicity", id_Periodicity);

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldLogin", old_login);
            }

            return command;
        }

        private void ButtonFileSearch_Click(object sender, RoutedEventArgs e)
        {
            /*
            Window create_filesearch = new FileWindow();
            create_filesearch.ShowDialog();
            textBoxFileFolder.Text = csPathToFolder.PathOfSelectedFolder;
            textBoxFileName.Text = System.IO.Path.GetFileNameWithoutExtension(csPathToFolder.PathOfSelectedFolder);
            */
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Документы (*.DOC; *.DOCX; *.TXT; *.XLS)|*.DOC; *.DOCX; *.TXT; *.XLS" + "|Все файлы (*.*)|*.* ";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                textBoxFileFolder.Text = openFileDialog.FileName;
                textBoxFileName.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);

            }
            
        }
    }
}
