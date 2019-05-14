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
                                    "fittings.date as Date, fittings.time as Time, fittings.notes as Notes" +
                                "from fittings " +
                                "join orders on fittings.orders_id_order = orders.id_order " +
                                "join customers on fittings.customers_id_customer = customers.id_customer " +
                                "join types_of_fitting on fittings.types_of_fitting_id_type_of_fitting = types_of_fitting.id_type_of_fitting; " +
                                "where customers.nickname = @nickname and orders.id_order = @idOrder";
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
                    comboBoxTypeOfFitting.SelectedValue = reader.GetString(3);
                    datePickerDate.SelectedDate = DateTime.Parse(reader.GetString(4));
                    textBoxTime.Text = reader.GetString(5);
                    if (reader.GetValue(6).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(6);
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

            if (textBoxTime.Text == "")
            {
                result += result == "" ? "Время примерки" : ", Время примерки";
            }
            if (datePickerDate.SelectedDate.Value.ToString() == "")
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
            /*string warning = CheckData();
            if (warning == "")
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                //Создать/изменить запись в таблице Пользователи
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                //Создание/изменение пользователя в БД
                string queryUser = "";
                //Читаемости ради                    
                if (way == WaysToOpenForm.WaysToOpen.create)
                {
                    queryUser = string.Format("CREATE USER '{0}'@'%' IDENTIFIED BY '{1}';", textBoxLogin.Text,
                    CheckBoxPassword.IsChecked.Value ? textBoxPassword.Text : PasswordBoxCurrent.Password);
                }
                if (way == WaysToOpenForm.WaysToOpen.edit && old_login != textBoxLogin.Text)
                {
                    queryUser = string.Format("Rename user '{0}'@'%' To '{1}'@'%';", old_login, textBoxLogin.Text);
                }
                MySqlCommand commandUser = new MySqlCommand(queryUser, connection, transaction);

                try
                {
                    command.ExecuteNonQuery();
                    if (queryUser != "")
                    {
                        commandUser.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    this.Hide();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!");
                }

                connection.Close();
                //this.Hide();
            }
            else
            {
                System.Windows.MessageBox.Show(warning);
            }*/
        }

       /*private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            /*string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO employees " +
                                       "(Login, Password, Name, Lastname, Patronymic, Phone_Number, Email," +
                                       " Passport_Data, Notes, Added, Last_Salary, Employee_Roles_id_Employee_Role, Employee_Positions_id_Employee_Position, Photo)" +
                                       " VALUES (@login, @password, @name, @lastname, @patronymic, @phone, @email, @passport, @notes, @added, @lastSalary, @role, @position, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update employees set Login = @login, Password = @password, Name = @name, Lastname = @lastname, Patronymic = @patronymic, Phone_Number = @phone, " +
                        "Email = @email, Passport_Data = @passport, Notes = @notes, Added = @added, Last_Salary = @lastSalary, " +
                        "Employee_Roles_id_Employee_Role = @role, Employee_Positions_id_Employee_Position = @position, Photo = @image" +
                        " where Login = @oldLogin;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@login", textBoxLogin.Text);
            command.Parameters.AddWithValue("@password", CheckBoxPassword.IsChecked.Value ? textBoxPassword.Text : PasswordBoxCurrent.Password);
            command.Parameters.AddWithValue("@name", textBoxName.Text);
            command.Parameters.AddWithValue("@lastname", textBoxLastname.Text);
            command.Parameters.AddWithValue("@patronymic", textBoxPatronymic.Text);
            command.Parameters.AddWithValue("@phone", textBoxPhone_Number.Text);
            command.Parameters.AddWithValue("@email", textBoxEmail.Text);
            command.Parameters.AddWithValue("@passport", textBoxPassportData.Text);
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);
            command.Parameters.AddWithValue("@added", datePickerAdded.SelectedDate.Value);
            command.Parameters.AddWithValue("@lastSalary", float.Parse(textBoxLastSalary.Text == "" ? "0" : textBoxLastSalary.Text));

            MySqlCommand commandRole = new MySqlCommand("select id_Employee_Role from employee_roles where Name_Of_Role = @role", connection);
            commandRole.Parameters.AddWithValue("role", comboBoxRole.SelectedItem.ToString());
            int id_role = -1;
            using (DbDataReader reader = commandRole.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_role = reader.GetInt32(0);
                }
            }

            MySqlCommand commandPosition = new MySqlCommand("select id_Employee_Position from employee_positions where Name_Of_position = @position", connection);
            commandPosition.Parameters.AddWithValue("position", comboBoxPosition.SelectedItem.ToString());
            int id_position = -1;
            using (DbDataReader reader = commandRole.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_position = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@role", id_role);
            command.Parameters.AddWithValue("@position", id_position);
           
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldLogin", old_login);
            }

            return command;
        }*/

    }
}

