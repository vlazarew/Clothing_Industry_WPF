using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Примерки
{
    /// <summary>
    /// Логика взаимодействия для FittingsRecordWindow.xaml
    /// </summary>
    public partial class FittingsRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;

        private int idOrder;
        private string nickname;

        public FittingsRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int idOrder = -1, string nickname = null)
        {
            InitializeComponent();
            way = waysToOpen;
            this.idOrder = idOrder;
            this.nickname = nickname;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();

            if (idOrder != -1 && nickname != null)
            {
                FillFields(idOrder, nickname);
            }
        }

        private void FillFields(int idOrder, string nickname)
        {
            string query_text = "select customers.nickname as Customer, orders.id_order OrderId, types_of_fitting.Name_Of_type as Type_Of_Fitting, " +
                                "fittings.date as Date, fittings.time as Time, fittings.notes as Notes " +
                                "from fittings " +
                                "join orders on fittings.orders_id_order = orders.id_order " +
                                "join customers on fittings.customers_id_customer = customers.id_customer " +
                                "join types_of_fitting on fittings.types_of_fitting_id_type_of_fitting = types_of_fitting.id_type_of_fitting " +
                                "where customers.nickname = @nickname and orders.id_order = @idOrder ; ";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@nickname", nickname);
            command.Parameters.AddWithValue("@idOrder", idOrder);
            connection.Open();

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxCustomer.Text = reader.GetString(0);
                    textBoxOrder.Text = reader.GetString(1);
                    comboBoxTypeOfFitting.SelectedValue = reader.GetString(2);
                    datePickerDate.SelectedDate = DateTime.Parse(reader.GetString(3));
                    //textBoxTime.Text = reader.GetString(4);
                    if (reader.GetValue(5).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(5);
                    }
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
                    Header.Content += " (Создание)";
                    datePickerDate.Text = DateTime.Now.ToLongDateString();
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

            string query = "select name_of_type from types_of_fitting";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxTypeOfFitting.Items.Add(reader.GetString(0));
                }
            }

            connection.Close();
        }

        private string CheckData()
        {
            string result = "";

            if (datePickerDate.SelectedDate == null)
            {
                result += result == "" ? "Дата примерки" : ", Дата добавленпримеркиия";
            }
            if (comboBoxTypeOfFitting.SelectedValue == null)
            {
                result += result == "" ? "Тип примерки" : ",  Тип примерки";
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

                //Создать/изменить запись в таблице Примерки
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                //try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    this.Hide();
                }
                /*catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка сохранения!");
                }*/

                connection.Close();
                //this.Hide();
            }
            else
            {
                MessageBox.Show(warning);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO fittings " +
                        "(Customers_id_Customer, Orders_id_Order, Date," +
                        " Time, Notes, Types_Of_Fitting_id_Type_Of_Fitting) " +
                        " VALUES (@customer, @orderId, @date, @time, @notes, @type_of_Fitting) ; ";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update fittings set Customers_id_Customer = @customer, Orders_id_Order = @orderId, Date = @date, " +
                        "Time = @time, Notes = @notes, Types_Of_Fitting_id_Type_Of_Fitting = @type_of_Fitting " +
                        " where Orders_id_Order = @orderId and Customers_id_Customer = @customer ;";
            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", datePickerDate.SelectedDate.Value);
            command.Parameters.AddWithValue("@time", null);
            //command.Parameters.AddWithValue("@time", DateTime.Now.ToString("HH:MM:ss "));
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);

            MySqlCommand commandType = new MySqlCommand("select id_Type_Of_Fitting from types_of_fitting where Name_Of_type = @name_of_Fitting", connection);
            commandType.Parameters.AddWithValue("@name_of_Fitting", comboBoxTypeOfFitting.SelectedValue.ToString());
            int id_type = -1;
            using (DbDataReader reader = commandType.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_type = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@type_of_Fitting", id_type);
            command.Parameters.AddWithValue("@orderId", int.Parse(textBoxOrder.Text));

            MySqlCommand commandCustomer = new MySqlCommand("select id_Customer from customers where nickname = @nickname", connection);
            commandCustomer.Parameters.AddWithValue("@nickname", textBoxCustomer.Text);
            int id_customer = -1;
            using (DbDataReader reader = commandCustomer.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_customer = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@customer", id_customer);

            return command;
        }

    }
}

