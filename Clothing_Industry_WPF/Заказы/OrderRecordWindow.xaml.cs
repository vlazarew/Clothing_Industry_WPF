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

namespace Clothing_Industry_WPF.Заказы
{
    /// <summary>
    /// Логика взаимодействия для OrderRecordWindow.xaml
    /// </summary>
    public partial class OrderRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int idOrder;

        public OrderRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int idOrder = -1)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            this.idOrder = idOrder;

            FillComboBoxes();

            if (idOrder != -1)
            {
                FillFields(idOrder);
            }
        }

        private void FillFields(int idOrder)
        {
            string query_text = "select orders.id_Order, orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible " +
                                "from orders " +
                                "join types_of_order on types_of_order.id_Type_Of_Order = orders.Types_Of_Order_id_Type_Of_Order " +
                                "join statuses_of_order on statuses_of_order.id_Status_Of_Order = orders.Statuses_Of_Order_id_Status_Of_Order " +
                                "join list_products_to_order on list_products_to_order.Orders_id_Order = orders.id_order " +
                                "join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                "join employees on(employees.Login = orders.Responsible) " +
                                "where orders.id_Order = @idOrder " +
                                "group by orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible, list_products_to_order.Orders_id_Order ;";

            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@idOrder", idOrder);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    datePickerDateOfOrder.SelectedDate = DateTime.Parse(reader.GetString(1));
                    textBoxDiscount.Text = reader.GetString(2);
                    textBoxPaid.Text = reader.GetString(3);
                    textBoxDebt.Text = reader.GetString(4);
                    datePickerDateOfDelievery.SelectedDate = DateTime.Parse(reader.GetString(5));
                    if (reader.GetValue(6).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(6);
                    }
                    comboBoxTypeOfOrder.SelectedValue = reader.GetString(7);
                    comboBoxStatusOfOrder.SelectedValue = reader.GetString(8);
                    comboBoxCustomer.SelectedValue = reader.GetString(9);
                    comboBoxResponsible.SelectedValue = reader.GetString(10);
                    comboBoxExecutor.SelectedValue = reader.GetString(11);

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
                    datePickerDateOfOrder.Text = DateTime.Now.ToLongDateString();
                    datePickerDateOfDelievery.Text = DateTime.Now.ToLongDateString();
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

            string query = "select name_of_type from types_of_order";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxTypeOfOrder.Items.Add(reader.GetString(0));
                }
            }

            query = "select name_of_status from statuses_of_order";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxStatusOfOrder.Items.Add(reader.GetString(0));
                }
            }

            query = "select nickname from customers";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxCustomer.Items.Add(reader.GetString(0));
                }
            }

            query = "select login from employees";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxExecutor.Items.Add(reader.GetString(0));
                    comboBoxResponsible.Items.Add(reader.GetString(0));
                }
            }

            connection.Close();
        }

        private string CheckData()
        {
            string result = "";

            if (datePickerDateOfOrder.SelectedDate.Value.ToString() == "")
            {
                result += result == "" ? "Дата заказа" : ", Дата заказа";
            }
            if (textBoxDiscount.Text == "")
            {
                result += result == "" ? "Скидка" : ", Скидка";
            }
            if (textBoxPaid.Text == "")
            {
                result += result == "" ? "Оплачено" : ", Оплачено";
            }
            if (textBoxDebt.Text == "")
            {
                result += result == "" ? "Долг" : ", Долг";
            }
            if (datePickerDateOfDelievery.SelectedDate.Value.ToString() == "")
            {
                result += result == "" ? "Дата доставки" : ", Дата доставки";
            }
            if (comboBoxTypeOfOrder.SelectedValue == null)
            {
                result += result == "" ? "Тип заказа" : ", Тип заказа";
            }
            if (comboBoxStatusOfOrder.SelectedValue == null)
            {
                result += result == "" ? "Статус заказа" : ", Статус заказа";
            }
            if (comboBoxCustomer.SelectedValue == null)
            {
                result += result == "" ? " Заказчик" : ",  Заказчик";
            }
            if (comboBoxResponsible.SelectedValue == null)
            {
                result += result == "" ? "Ответственный" : ", Ответственный";
            }
            if (comboBoxExecutor.SelectedValue == null)
            {
                result += result == "" ? "Исполнитель" : ", Исполнитель";
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

                //Создать/изменить запись в таблице Заказы
                //MySqlCommand command = actionInDBCommand(connection);
                MySqlCommand command = new MySqlCommand("",connection);
                command.Transaction = transaction;

                // Получение данных о балансе клиента
                string queryCheckBalance = "select Accured, Paid, Debt, customers_id_customer " +
                                               "from customer_balance " +
                                               "join customers on customer_balance.customers_id_customer = customers.id_customer" +
                                               "where customers.nickname = @nickname";
                MySqlCommand commandCheckBalance = new MySqlCommand(queryCheckBalance, connection);
                commandCheckBalance.Parameters.AddWithValue("@nickname", comboBoxCustomer.SelectedValue.ToString());
                // Состояние баланса клиента на текущий момент
                float accured = 0;
                float paid = 0;
                float debt = 0;
                int id = -1;
                using (DbDataReader reader = commandCheckBalance.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accured = (float)reader.GetValue(0);
                        paid = (float)reader.GetValue(1);
                        debt = (float)reader.GetValue(2);
                        id = (int)reader.GetValue(3);
                    }
                }
                //

                MySqlCommand commandSetBalance;
                // Если мы создаем заказ, то у нас в балансе он еще не учитывается и надо бы записать это дело
                // Иначе мы для начала должны вычесть то, что имеем на текущий момент и прибавить новые значения
                if (way == WaysToOpenForm.WaysToOpen.edit)
                {
                    string selectQuery = "select Paid, Debt from orders where id_order = @idOrder";
                    MySqlCommand commandSelectCurrentOrder = new MySqlCommand(selectQuery, connection);
                    commandSelectCurrentOrder.Parameters.AddWithValue("@idOrder", idOrder);

                    // Состояние баланса клиента на текущий момент
                    float cur_paid = 0;
                    float cur_debt = 0;
                    using (DbDataReader reader = commandSelectCurrentOrder.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cur_paid = (float)reader.GetValue(0);
                            cur_debt = (float)reader.GetValue(1);
                        }
                    }

                    paid -= cur_paid;
                    debt -= cur_debt;
                    accured -= (paid + debt);
                }

                // Новые значения баланса клиента
                paid += float.Parse(textBoxPaid.Text);
                debt += float.Parse(textBoxDebt.Text);
                accured += float.Parse(textBoxPaid.Text) + float.Parse(textBoxDebt.Text);


                string querySetBalance = "update customer_balance set Accured = @accured, Paid = @paid, Debt = @debt" +
                                         "where customer_balance.customers_id_customer = @id";
                commandSetBalance = new MySqlCommand(querySetBalance, connection, transaction);
                commandSetBalance.Parameters.AddWithValue("@id", id);

                //Читаемости ради                    
                /*if (way == WaysToOpenForm.WaysToOpen.create)
                {
                    queryUser = string.Format("CREATE USER '{0}'@'%' IDENTIFIED BY '{1}';", textBoxLogin.Text,
                    CheckBoxPassword.IsChecked.Value ? textBoxPassword.Text : PasswordBoxCurrent.Password);
                }
                if (way == WaysToOpenForm.WaysToOpen.edit && old_login != textBoxLogin.Text)
                {
                    queryUser = string.Format("Rename user '{0}'@'%' To '{1}'@'%';", old_login, textBoxLogin.Text);
                }*/

                //MySqlCommand commandUser = new MySqlCommand(queryUser, connection, transaction);

                try
                {
                    command.ExecuteNonQuery();
                    commandSetBalance.ExecuteNonQuery();
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
            }
        }

       /*private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
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
