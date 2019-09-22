using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
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

namespace Clothing_Industry_WPF.Заказы
{
    /// <summary>
    /// Логика взаимодействия для OrderRecordWindow.xaml
    /// </summary>
    public partial class OrderRecordWindow : Window
    {
        private Order order;
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int idOrder;
        private string previousStatus;
        private float salaryToExecutor;

        public OrderRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int idOrder = -1)
        {
            order = new Order();
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            this.idOrder = idOrder;

            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                buttonListProducts.Visibility = Visibility.Hidden;
            }

            FillComboBoxes();

            if (idOrder != -1)
            {
                FillFields(idOrder);
            }
        }

        private void FillFields(int idOrder)
        {
            string query_text = "select orders.id_Order, orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Total_price, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible, orders.Added_Price_For_Complexity " +
                                "from orders " +
                                "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order =statuses_of_order.id_Status_Of_Order " +
                                "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                "left join customers on orders.Customers_id_Customer = customers.id_Customer  " +
                                "where orders.id_Order = @idOrder " +
                                "group by orders.id_Order ;";

            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@idOrder", idOrder);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    datePickerDateOfOrder.SelectedDate = DateTime.Parse(reader.GetString(1));
                    textBoxDiscount.Text = reader.IsDBNull(2) ? "0" : reader.GetString(2);
                    textBoxTotal_Price.Text = reader.IsDBNull(3) ? "0" : reader.GetString(3);
                    textBoxPaid.Text = reader.IsDBNull(4) ? "0" : reader.GetString(4);
                    textBoxDebt.Text = reader.IsDBNull(5) ? "0" : reader.GetString(5);
                    datePickerDateOfDelievery.SelectedDate = DateTime.Parse(reader.GetString(6));
                    if (reader.GetValue(7).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(7);
                    }
                    comboBoxTypeOfOrder.SelectedValue = reader.GetString(8);
                    comboBoxStatusOfOrder.SelectedValue = reader.GetString(9);
                    previousStatus = comboBoxStatusOfOrder.SelectedValue.ToString();
                    comboBoxCustomer.SelectedValue = reader.GetString(10);
                    comboBoxResponsible.SelectedValue = reader.GetString(11);
                    comboBoxExecutor.SelectedValue = reader.GetString(12);
                    if (reader.GetValue(13).ToString() != "")
                    {
                        textBoxAddedPrice.Text = reader.GetString(13);
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

            if (datePickerDateOfOrder.SelectedDate == null)
            {
                result += result == "" ? "Дата заказа" : ", Дата заказа";
            }
            if (datePickerDateOfDelievery.SelectedDate == null)
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

        private void ButtonListProducts_Click(object sender, RoutedEventArgs e)
        {

            var windowListProducts = new OrderProductListWindow(idOrder);
            windowListProducts.ShowDialog();
            {
                var Result = windowListProducts.Result;
                if (!Result)
                {
                    comboBoxStatusOfOrder.IsEnabled = false;
                    comboBoxStatusOfOrder.SelectedValue = "Отменён";
                }
            }
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = "select products.Name_Of_Product, products.Fixed_price, list_products_to_order.Count " +
                           "from list_products_to_order " +
                           "join products on products.id_product = list_products_to_order.Products_id_Product " +
                           "where list_products_to_order.Orders_id_Order = @idOrder; ";

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idOrder", idOrder);

            float total_price = 0;
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    total_price += (float)reader.GetValue(1) * (int)reader.GetValue(2);
                }
            }

            textBoxTotal_Price.Text = total_price.ToString();
            textBoxDebt.Text = (total_price - float.Parse(textBoxPaid.Text)).ToString();
            connection.Close();
        }

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
            if (warning == "")
            {
                FillEmptyTextBoxes();

                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                // Расчет зп
                if (comboBoxStatusOfOrder.SelectedValue.ToString() == "Готов" || comboBoxStatusOfOrder.SelectedValue.ToString() == "Отправлен" ||
                   comboBoxStatusOfOrder.SelectedValue.ToString() == "Сдан")
                {
                    salaryToExecutor = CalculateSalary(connection);
                }
                else
                {
                    salaryToExecutor = 0;
                }

                //Создать/изменить запись в таблице Заказы
                MySqlCommand command = actionInDBCommand(connection, transaction);
                // !!! ИЗМЕНЕНИЕ БАЛАНСА КЛИЕНТА !!!
                MySqlCommand commandSetBalance = EditCustomerBalance(connection, transaction);
                // !!! КОНЕЦ ИЗМЕНЕНИЯ БАЛАНСА КЛИЕНТА !!!

                // !!! НАЧИСЛЕНИЕ ЗП !!!
                MySqlCommand commandSetSalary = EditEmployeeSalary(connection, transaction);
                // !!! КОНЕЦ НАЧИСЛЕНИЯ ЗП !!!

                try
                {
                    command.ExecuteNonQuery();
                    commandSetBalance.ExecuteNonQuery();
                    if (commandSetSalary != null)
                    {
                        commandSetSalary.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    this.Close();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                connection.Close();
                //this.Close();
            }
            else
            {
                MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillEmptyTextBoxes()
        {
            if (textBoxDebt.Text == "")
            {
                textBoxDebt.Text = "0";
            }
            if (textBoxPaid.Text == "")
            {
                textBoxPaid.Text = "0";
            }
            if (textBoxTotal_Price.Text == "")
            {
                textBoxTotal_Price.Text = "0";
            }
        }

        private MySqlCommand EditCustomerBalance(MySqlConnection connection, MySqlTransaction transaction)
        {
            // Получение данных о балансе клиента
            string queryCheckBalance = "select Accured, Paid, Debt, customers_id_customer " +
                                       "from customers_balance " +
                                       "join customers on customers_balance.customers_id_customer = customers.id_customer " +
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
                string selectQuery = "select Paid, Debt, Total_Price from orders where id_order = @idOrder";
                MySqlCommand commandSelectCurrentOrder = new MySqlCommand(selectQuery, connection);
                commandSelectCurrentOrder.Parameters.AddWithValue("@idOrder", idOrder);

                // Состояние баланса клиента на текущий момент

                float cur_paid = 0;
                float cur_debt = 0;
                float cur_accured = 0;
                using (DbDataReader reader = commandSelectCurrentOrder.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cur_paid = (float)reader.GetValue(0);
                        cur_debt = (float)reader.GetValue(1);
                        cur_accured = (float)reader.GetValue(2);
                    }
                }

                paid -= cur_paid;
                debt -= cur_debt;
                accured -= cur_accured;
            }

            // Новые значения баланса клиента
            paid += float.Parse(textBoxPaid.Text);
            debt += float.Parse(textBoxDebt.Text);
            accured += float.Parse(textBoxTotal_Price.Text);


            string querySetBalance = "update customers_balance set Accured = @accured, Paid = @paid, Debt = @debt " +
                                     "where customers_balance.customers_id_customer = @id";
            commandSetBalance = new MySqlCommand(querySetBalance, connection, transaction);
            commandSetBalance.Parameters.AddWithValue("@id", id);
            commandSetBalance.Parameters.AddWithValue("@accured", accured);
            commandSetBalance.Parameters.AddWithValue("@paid", paid);
            commandSetBalance.Parameters.AddWithValue("@debt", debt);

            return commandSetBalance;
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO orders " +
                        "(Date_Of_Order, Discount_Per_Cent, Total_Price, Paid, Debt, Date_Of_Delievery, Added_Price_For_Complexity, Notes, " +
                        " Types_Of_Order_id_Type_Of_Order, Statuses_Of_Order_id_Status_Of_Order, Customers_id_Customer, Responsible, Executor, SalaryToExecutor)" +
                        " VALUES (@dateOrder, @discount, @totalPrice, @paid, @debt, @dateDelievery, @addedPrice, @notes, @typeOrder, @statusOrder, @customer, @responsible, @executor, @salary);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update orders set Date_of_Order = @dateOrder, Discount_Per_Cent = @discount, Total_Price = @totalPrice, Paid = @paid, Debt = @debt," +
                        " Date_Of_Delievery = @dateDelievery, Added_Price_For_Complexity = @addedPrice, Notes = @notes, Types_Of_Order_id_Type_Of_Order = @typeOrder, " +
                        " Statuses_Of_Order_id_Status_Of_Order = @statusOrder," +
                        " Customers_id_Customer = @customer, Responsible = @responsible, Executor = @executor, SalaryToExecutor = @salary " +
                        " where id_order = @idOrder;";

            }

            MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@dateOrder", datePickerDateOfOrder.SelectedDate.Value);
            command.Parameters.AddWithValue("@discount", textBoxDiscount.Text == "" ? 0 : float.Parse(textBoxDiscount.Text));
            command.Parameters.AddWithValue("@totalPrice", textBoxTotal_Price.Text == "" ? 0 : float.Parse(textBoxTotal_Price.Text));
            command.Parameters.AddWithValue("@paid", textBoxPaid.Text == "" ? 0 : float.Parse(textBoxPaid.Text));
            command.Parameters.AddWithValue("@debt", textBoxDebt.Text == "" ? 0 : float.Parse(textBoxDebt.Text));
            command.Parameters.AddWithValue("@dateDelievery", datePickerDateOfDelievery.SelectedDate.Value);
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);
            command.Parameters.AddWithValue("@salary", salaryToExecutor);
            command.Parameters.AddWithValue("@addedPrice", textBoxAddedPrice.Text == "" ? 0 : float.Parse(textBoxAddedPrice.Text));

            MySqlCommand commandType = new MySqlCommand("select id_Type_Of_Order from types_of_order where Name_Of_Type = @type", connection);
            commandType.Parameters.AddWithValue("@type", comboBoxTypeOfOrder.SelectedItem.ToString());
            int id_type = -1;
            using (DbDataReader reader = commandType.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_type = reader.GetInt32(0);
                }
            }

            MySqlCommand commandStatus = new MySqlCommand("select id_Status_Of_Order from statuses_of_order where Name_Of_Status = @status", connection);
            commandStatus.Parameters.AddWithValue("@status", comboBoxStatusOfOrder.SelectedItem.ToString());
            int id_status = -1;
            using (DbDataReader reader = commandStatus.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_status = reader.GetInt32(0);
                }
            }

            MySqlCommand commandCustomer = new MySqlCommand("select id_Customer from customers where nickname = @nickname", connection);
            commandCustomer.Parameters.AddWithValue("@nickname", comboBoxCustomer.SelectedItem.ToString());
            int id_customer = -1;
            using (DbDataReader reader = commandCustomer.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_customer = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@typeOrder", id_type);
            command.Parameters.AddWithValue("@statusOrder", id_status);
            command.Parameters.AddWithValue("@customer", id_customer);
            command.Parameters.AddWithValue("@responsible", comboBoxResponsible.SelectedItem.ToString());
            command.Parameters.AddWithValue("@executor", comboBoxExecutor.SelectedItem.ToString());

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@idOrder", idOrder);
            }

            return command;
        }

        private MySqlCommand EditEmployeeSalary(MySqlConnection connection, MySqlTransaction transaction)
        {
            // Необходимо получить, сколько у него сейчас доп зп
            string querySelectSalary = "select PieceWorkPayment, Total_Salary, To_Pay, period " +
                                       "from payrolls " +
                                       "where employees_login = @login and not PaidOff " +
                                       "order by period desc " +
                                       "limit 1 ;";

            MySqlCommand commandSelect = new MySqlCommand(querySelectSalary, connection);
            commandSelect.Parameters.AddWithValue("@login", comboBoxExecutor.SelectedValue.ToString());

            float prevPieceWork = 0;
            float prevTotalSalary = 0;
            float prevToPay = 0;
            string period = "";

            using (DbDataReader reader = commandSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    prevPieceWork = float.Parse(reader.GetString(0));
                    prevTotalSalary = float.Parse(reader.GetString(1));
                    prevToPay = float.Parse(reader.GetString(2));
                    period = reader.GetString(3);
                }
            }

            float deleteSalary = 0;

            if (previousStatus != comboBoxStatusOfOrder.SelectedValue.ToString())
            {
                float curPieceWork = prevPieceWork;
                float curTotalSalary = prevTotalSalary;
                float curToPay = prevToPay;

                // Если он ранее был сделан, то мы должны удалить прошлую зп и начислить новую
                // Старую удаляем в любом случае
                string deleteQuery = "select SalaryToExecutor from orders where id_Order = @idOrder";
                MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@idOrder", idOrder);

                using (DbDataReader reader = deleteCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deleteSalary = float.Parse(reader.GetString(0));
                    }
                }

                curPieceWork -= deleteSalary;
                curTotalSalary -= deleteSalary;
                curToPay -= deleteSalary;

                // Если заказ принят/готов и т.д. , то начисляем ему новую зп
                if (comboBoxStatusOfOrder.SelectedValue.ToString() == "Готов" || comboBoxStatusOfOrder.SelectedValue.ToString() == "Отправлен" ||
                   comboBoxStatusOfOrder.SelectedValue.ToString() == "Сдан")
                {
                    curPieceWork += salaryToExecutor;
                    curTotalSalary += salaryToExecutor;
                    curToPay += salaryToExecutor;
                }

                string queryEdit = "update payrolls set PieceWorkPayment = @curPieceWork, Total_Salary = @curTotalSalary, To_Pay = @curToPay " +
                              "where employees_login = @login and period = @period ";
                MySqlCommand commandEdit = new MySqlCommand(queryEdit, connection, transaction);
                commandEdit.Parameters.AddWithValue("@curPieceWork", curPieceWork);
                commandEdit.Parameters.AddWithValue("@curTotalSalary", curTotalSalary);
                commandEdit.Parameters.AddWithValue("@curToPay", curToPay);
                commandEdit.Parameters.AddWithValue("@login", comboBoxExecutor.SelectedValue.ToString());
                commandEdit.Parameters.AddWithValue("@period", period);

                return commandEdit;
            }

            return null;
        }

        private float CalculateSalary(MySqlConnection connection)
        {
            // Необходимо получить все изделия и их кол-во
            string query = "select products.MoneyToEmployee, list_products_to_order.Count, orders.Added_Price_For_Complexity as Added_Price, orders.SalaryToExecutor " +
                           "from orders " +
                           "join list_products_to_order on orders.id_Order = list_products_to_order.Orders_id_Order " +
                           "join products on list_products_to_order.Products_id_Product = products.id_Product " +
                           "where orders.id_Order = @idOrder ;";

            MySqlCommand commandSelect = new MySqlCommand(query, connection);
            commandSelect.Parameters.AddWithValue("@idOrder", idOrder);

            // Будем так вот данные хранить походу. Списки сдельной зп и прочего для расчета доп зп для сотрудника
            List<float> listMoneyToEmployee = new List<float>();
            List<int> listCount = new List<int>();
            List<float> listAddedPrice = new List<float>();

            using (DbDataReader reader = commandSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    listMoneyToEmployee.Add(float.Parse(reader.GetString(0)));
                    listCount.Add(int.Parse(reader.GetString(1)));
                    listAddedPrice.Add(float.Parse(reader.GetString(2)));
                }
            }
            float result = 0;

            for (int i = 0; i < listMoneyToEmployee.Count; i++)
            {
                result += listMoneyToEmployee[i] * listCount[i];
            }
            if (listAddedPrice.Count > 0)
            {
                result += listAddedPrice[0];
            }
            return result;
        }

        private void TextBoxTotal_Price_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshDebt();
        }

        private void RefreshDebt()
        {
            float.TryParse(textBoxTotal_Price.Text, out float totalPrice);
            float.TryParse(textBoxPaid.Text, out float paid);
            textBoxDebt.Text = (totalPrice - paid).ToString();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxValidator.IsFloatTextAllowed(e.Text);
        }

        private void TextBoxPaid_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshDebt();
        }
    }
}
