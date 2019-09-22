using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace Clothing_Industry_WPF.Заказы
{
    public class Order : IDataErrorInfo
    {
        public int id { get; set; }
        public DateTime dateOfOrder { get; set; }
        public float discountPerCent { get; set; }
        public float totalPrice { get; set; }
        public float paid { get; set; }
        public float debt { get; set; }
        public DateTime dateOfDelievery { get; set; }
        public float addedPriceForComplexity { get; set; }
        public string notes { get; set; }
        public float salaryToExecutor { get; set; }
        public string responsible { get; set; }
        public string executor { get; set; }
        public string customerLogin { get; set; }
        public string statusName { get; set; }
        // При начислении сдельной ЗП нужно учитывать статус заказа
        public string previousStatus { get; set; }
        public string typeOfOrderName { get; set; }

        // Пустой констуктор
        public Order()
        {
            id = -1;
            dateOfOrder = DateTime.Now;
            discountPerCent = 0;
            totalPrice = 0;
            paid = 0;
            debt = 0;
            dateOfDelievery = DateTime.Now;
            addedPriceForComplexity = 0;
            notes = "";
            salaryToExecutor = 0;
            responsible = "";
            executor = "";
            customerLogin = "";
            statusName = "";
            typeOfOrderName = "";
        }

        // Конструктор по айди (запрос в базу)
        public Order(int id, MySqlConnection connection)
        {
            this.id = id;
            string queryText = "select orders.id_Order, orders.Date_Of_Order, orders.Discount_Per_Cent, orders.Total_price, orders.Paid, orders.Debt, orders.Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor, orders.Responsible, orders.Added_Price_For_Complexity " +
                                "from orders " +
                                "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order =statuses_of_order.id_Status_Of_Order " +
                                "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                "left join customers on orders.Customers_id_Customer = customers.id_Customer  " +
                                "where orders.id_Order = @idOrder ;";

            MySqlCommand command = new MySqlCommand(queryText, connection);
            command.Parameters.AddWithValue("@idOrder", id);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    dateOfOrder = reader.GetDateTime(1);
                    discountPerCent = reader.IsDBNull(2) ? 0 : reader.GetFloat(2);
                    totalPrice = reader.IsDBNull(3) ? 0 : reader.GetFloat(3);
                    paid = reader.IsDBNull(4) ? 0 : reader.GetFloat(4);
                    debt = reader.IsDBNull(5) ? 0 : reader.GetFloat(5);
                    dateOfDelievery = reader.GetDateTime(6);
                    notes = reader.IsDBNull(7) ? "" : reader.GetString(7);

                    typeOfOrderName = reader.GetString(8);
                    statusName = reader.GetString(9);
                    previousStatus = statusName;
                    customerLogin = reader.GetString(10);
                    responsible = reader.GetString(11);
                    executor = reader.GetString(12);
                    addedPriceForComplexity = reader.IsDBNull(13) ? 0 : reader.GetFloat(13);
                }
            }
            connection.Close();
        }

        // Проверка на заполненность полей
        private string CheckData()
        {
            string result = "";

            if (dateOfOrder == null)
            {
                result += result == "" ? "Дата заказа" : ", Дата заказа";
            }
            if (dateOfDelievery == null)
            {
                result += result == "" ? "Дата доставки" : ", Дата доставки";
            }
            if (typeOfOrderName == "")
            {
                result += result == "" ? "Тип заказа" : ", Тип заказа";
            }
            if (statusName == "")
            {
                result += result == "" ? "Статус заказа" : ", Статус заказа";
            }
            if (customerLogin == "")
            {
                result += result == "" ? " Заказчик" : ",  Заказчик";
            }
            if (responsible == "")
            {
                result += result == "" ? "Ответственный" : ", Ответственный";
            }
            if (executor == "")
            {
                result += result == "" ? "Исполнитель" : ", Исполнитель";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        // После того, как мы установили список изделий для заказа, то обновляем общую стоимость заказа
        public void UpdateTotalPrice(MySqlConnection connection)
        {
            connection.Open();

            string query = "select products.Name_Of_Product, products.Fixed_price, list_products_to_order.Count " +
                           "from list_products_to_order " +
                           "join products on products.id_product = list_products_to_order.Products_id_Product " +
                           "where list_products_to_order.Orders_id_Order = @idOrder; ";

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idOrder", id);

            float totalPrice = 0;
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    totalPrice += reader.GetFloat(1) * reader.GetInt32(2);
                }
            }

            this.totalPrice = totalPrice;
            debt = this.totalPrice = paid;
            connection.Close();
        }

        // Вызов сохранения и прогон по всей логике
        public bool Save(MySqlConnection connection, WaysToOpenForm.WaysToOpen way)
        {
            string warning = CheckData();
            if (warning == "")
            {
                //FillEmptyTextBoxes();
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                // Расчет зп
                if (statusName == "Готов" || statusName == "Отправлен" || statusName == "Сдан")
                {
                    salaryToExecutor = CalculateSalary(connection);
                }
                else
                {
                    salaryToExecutor = 0;
                }

                //Создать/изменить запись в таблице Заказы
                MySqlCommand command = SaveInDB(connection, transaction, way);

                // !!! ИЗМЕНЕНИЕ БАЛАНСА КЛИЕНТА !!!
                MySqlCommand commandSetBalance = EditCustomerBalance(connection, transaction, way);
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
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                connection.Close();
            }
            else
            {
                MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        // Генерация команды сохранения в БД
        private MySqlCommand SaveInDB(MySqlConnection connection, MySqlTransaction transaction, WaysToOpenForm.WaysToOpen way)
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
            command.Parameters.AddWithValue("@dateOrder", dateOfOrder);
            command.Parameters.AddWithValue("@discount", discountPerCent);
            command.Parameters.AddWithValue("@totalPrice", totalPrice);
            command.Parameters.AddWithValue("@paid", paid);
            command.Parameters.AddWithValue("@debt", debt);
            command.Parameters.AddWithValue("@dateDelievery", dateOfDelievery);
            command.Parameters.AddWithValue("@notes", notes);
            command.Parameters.AddWithValue("@salary", salaryToExecutor);
            command.Parameters.AddWithValue("@addedPrice", addedPriceForComplexity);

            // ГОВНОКОД НАЧАЛО
            MySqlCommand commandType = new MySqlCommand("select id_Type_Of_Order from types_of_order where Name_Of_Type = @type", connection);
            commandType.Parameters.AddWithValue("@type", typeOfOrderName);
            int id_type = -1;
            using (DbDataReader reader = commandType.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_type = reader.GetInt32(0);
                }
            }

            MySqlCommand commandStatus = new MySqlCommand("select id_Status_Of_Order from statuses_of_order where Name_Of_Status = @status", connection);
            commandStatus.Parameters.AddWithValue("@status", statusName);
            int id_status = -1;
            using (DbDataReader reader = commandStatus.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_status = reader.GetInt32(0);
                }
            }

            MySqlCommand commandCustomer = new MySqlCommand("select id_Customer from customers where nickname = @nickname", connection);
            commandCustomer.Parameters.AddWithValue("@nickname", customerLogin);
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
            command.Parameters.AddWithValue("@responsible", responsible);
            command.Parameters.AddWithValue("@executor", executor);
            // ГОВНОКОД КОНЕЦ

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@idOrder", id);
            }

            return command;
        }

        // Обновление состояние общего баланса клиента после движений над заказом
        private MySqlCommand EditCustomerBalance(MySqlConnection connection, MySqlTransaction transaction, WaysToOpenForm.WaysToOpen way)
        {
            // Получение данных о балансе клиента
            string queryCheckBalance = "select Accured, Paid, Debt, customers_id_customer " +
                                       "from customers_balance " +
                                       "join customers on customers_balance.customers_id_customer = customers.id_customer " +
                                       "where customers.nickname = @nickname";
            MySqlCommand commandCheckBalance = new MySqlCommand(queryCheckBalance, connection);
            commandCheckBalance.Parameters.AddWithValue("@nickname", customerLogin);

            // Состояние общего баланса клиента на текущий момент
            (float accured, float paid, float debt, int id) result = (0, 0, 0, -1);
            using (DbDataReader reader = commandCheckBalance.ExecuteReader())
            {
                while (reader.Read())
                {
                    result = (accured: reader.GetFloat(0), paid: reader.GetFloat(1), debt: reader.GetFloat(2), id: reader.GetInt32(3));
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
                commandSelectCurrentOrder.Parameters.AddWithValue("@idOrder", id);

                // Состояние баланса клиента по текущему заказу на текущий момент
                (float currentPaid, float currentDebt, float currentTotalPrice) current = (0, 0, 0);
                using (DbDataReader reader = commandSelectCurrentOrder.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        current = (currentPaid: reader.GetFloat(0), currentDebt: reader.GetFloat(1), currentTotalPrice: reader.GetFloat(2));
                    }
                }

                result.paid -= current.currentPaid;
                result.debt -= current.currentDebt;
                result.accured -= current.currentTotalPrice;
            }

            // Новые значения баланса клиента
            result.paid += this.paid;
            result.debt += this.debt;
            result.accured += this.totalPrice;


            string querySetBalance = "update customers_balance set Accured = @accured, Paid = @paid, Debt = @debt " +
                                     "where customers_balance.customers_id_customer = @id";
            commandSetBalance = new MySqlCommand(querySetBalance, connection, transaction);
            commandSetBalance.Parameters.AddWithValue("@id", id);
            commandSetBalance.Parameters.AddWithValue("@accured", result.accured);
            commandSetBalance.Parameters.AddWithValue("@paid", result.paid);
            commandSetBalance.Parameters.AddWithValue("@debt", result.debt);

            return commandSetBalance;
        }

        // Обновление состояния накопительной(сдельной) зп исполнителя заказа
        private MySqlCommand EditEmployeeSalary(MySqlConnection connection, MySqlTransaction transaction)
        {
            // Необходимо получить, сколько у него сейчас доп зп
            string querySelectSalary = "select PieceWorkPayment, Total_Salary, To_Pay, period " +
                                       "from payrolls " +
                                       "where employees_login = @login and not PaidOff " +
                                       "order by period desc " +
                                       "limit 1 ;";

            MySqlCommand commandSelect = new MySqlCommand(querySelectSalary, connection);
            commandSelect.Parameters.AddWithValue("@login", executor);

            (float previousPieceWork, float previousTotalSalary, float previousToPay, string period) previous = (0, 0, 0, "");
            using (DbDataReader reader = commandSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    previous = (previousPieceWork: reader.GetFloat(0), previousTotalSalary: reader.GetFloat(1), previousToPay: reader.GetFloat(2), period: reader.GetString(3));
                }
            }

            float deleteSalary = 0;

            if (previousStatus != statusName)
            {

                // Если он ранее был сделан, то мы должны удалить прошлую зп и начислить новую
                // Старую удаляем в любом случае
                string deleteQuery = "select SalaryToExecutor from orders where id_Order = @idOrder";
                MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@idOrder", id);

                using (DbDataReader reader = deleteCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deleteSalary = reader.GetFloat(0);
                    }
                }

                (float currentPieceWork, float currentTotalSalary, float currentToPay) current = (currentPieceWork: previous.previousPieceWork - deleteSalary,
                                                                    currentTotalSalary: previous.previousTotalSalary - deleteSalary, currentToPay: previous.previousToPay - deleteSalary);

                // Если заказ принят/готов и т.д. , то начисляем ему новую зп
                if (statusName == "Готов" || statusName == "Отправлен" || statusName == "Сдан")
                {
                    current.currentPieceWork += salaryToExecutor;
                    current.currentTotalSalary += salaryToExecutor;
                    current.currentToPay += salaryToExecutor;
                }

                string queryEdit = "update payrolls set PieceWorkPayment = @currentPieceWork, Total_Salary = @currentTotalSalary, To_Pay = @currentToPay " +
                                   "where employees_login = @login and period = @period ";
                MySqlCommand commandEdit = new MySqlCommand(queryEdit, connection, transaction);
                commandEdit.Parameters.AddWithValue("@currentPieceWork", current.currentPieceWork);
                commandEdit.Parameters.AddWithValue("@currentTotalSalary", current.currentTotalSalary);
                commandEdit.Parameters.AddWithValue("@currentToPay", current.currentToPay);
                commandEdit.Parameters.AddWithValue("@login", executor);
                commandEdit.Parameters.AddWithValue("@period", previous.period);

                return commandEdit;
            }

            return null;
        }

        // Расчет сдельной ЗП
        private float CalculateSalary(MySqlConnection connection)
        {
            // Необходимо получить все изделия и их кол-во
            string query = "select products.MoneyToEmployee, list_products_to_order.Count, orders.Added_Price_For_Complexity as Added_Price, orders.SalaryToExecutor " +
                           "from orders " +
                           "join list_products_to_order on orders.id_Order = list_products_to_order.Orders_id_Order " +
                           "join products on list_products_to_order.Products_id_Product = products.id_Product " +
                           "where orders.id_Order = @idOrder ;";

            MySqlCommand commandSelect = new MySqlCommand(query, connection);
            commandSelect.Parameters.AddWithValue("@idOrder", id);

            // Будем так вот данные хранить походу. Списки сдельной зп и прочего для расчета доп зп для сотрудника
            List<float> listMoneyToEmployee = new List<float>();
            List<int> listCount = new List<int>();
            List<float> listAddedPrice = new List<float>();

            using (DbDataReader reader = commandSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    listMoneyToEmployee.Add(reader.GetFloat(0));
                    listCount.Add(reader.GetInt32(1));
                    listAddedPrice.Add(reader.GetFloat(2));
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

        // Запросец на все заказы
        public static string getQueryText()
        {
            string result = getNotNullQueryText();
            result = result.Replace(";", getGroupBy());
            result = result.Replace(";", " union ");
            result += getNullQueryText();
            result = result.Replace(";", getGroupBy());

            return result;
        }

        private static string getNotNullQueryText()
        {
            string query_text = "select orders.id_Order, DATE_FORMAT(orders.Date_Of_Order, \"%d.%m.%Y\") as Date_Of_Order, orders.Discount_Per_Cent, " +
                                "orders.Total_Price, orders.Paid, orders.Debt, DATE_FORMAT(orders.Date_Of_Delievery, \"%d.%m.%Y\") as Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, " +
                                "group_concat(products.Name_Of_Product separator ', ') as Products, orders.Added_Price_For_Complexity " +
                                "from orders " +
                                "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                "where not products.Name_Of_Product is null ;";

            return query_text;
        }

        private static string getNullQueryText()
        {
            string query_text = "select orders.id_Order, DATE_FORMAT(orders.Date_Of_Order, \"%d.%m.%Y\") as Date_Of_Order, orders.Discount_Per_Cent, orders.Total_Price, " +
                                "orders.Paid, orders.Debt, DATE_FORMAT(orders.Date_Of_Delievery, \"%d.%m.%Y\") as Date_Of_Delievery, orders.Notes, " +
                                "types_of_order.Name_Of_type, statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, " +
                                "'Не указано' as Products, orders.Added_Price_For_Complexity " +
                                "from orders " +
                                "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                "where products.Name_Of_Product is null ;";

            return query_text;
        }

        private static string getNull()
        {
            string query_text = "select orders.id_Order, DATE_FORMAT(orders.Date_Of_Order, \"%d.%m.%Y\") as Date_Of_Order, orders.Discount_Per_Cent, orders.Total_Price, " +
                                "orders.Paid, orders.Debt, DATE_FORMAT(orders.Date_Of_Delievery, \"%d.%m.%Y\") as Date_Of_Delievery, orders.Notes, types_of_order.Name_Of_type, " +
                                "statuses_of_order.Name_Of_Status, customers.Nickname, orders.Executor,orders.Responsible, 'Не указано' as Products, orders.Added_Price_For_Complexity " +
                                "from orders left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                "where products.Name_Of_Product is not null and id_Order is null ;";

            return query_text;
        }

        private static string getGroupBy()
        {
            string groupBy = " group by orders.id_Order ; ";
            return groupBy;
        }

        // Получение данных обо всех заказах
        public static System.Data.DataTable getListOrders(MySqlConnection connection)
        {
            string queryText = getQueryText();

            connection.Open();
            var dataTable = FormLoader.ExecuteQuery(queryText, connection);
            connection.Close();

            return dataTable;
        }

        // Удаление заказа
        public static void DeleteFromDB(List<int> ids, MySqlConnection connection)
        {
            connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from orders where id_order = @id";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id", id);
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Удаление не удалось", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }

        // Список полей, по которым мы можем делать поиск
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("id_Order", "Номер заказа", describe.Where(key => key.Key == "id_Order").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Order", "Дата заказа", describe.Where(key => key.Key == "Date_Of_Order").First().Value));
            result.Add(new FindHandler.FieldParameters("Discount_Per_Cent", "Скидка", describe.Where(key => key.Key == "Discount_Per_Cent").First().Value));
            result.Add(new FindHandler.FieldParameters("Total_Price", "Итоговая стоимость", describe.Where(key => key.Key == "Total_Price").First().Value));
            result.Add(new FindHandler.FieldParameters("Paid", "Оплачено", describe.Where(key => key.Key == "Paid").First().Value));
            result.Add(new FindHandler.FieldParameters("Debt", "Долг", describe.Where(key => key.Key == "Debt").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Delievery", "Дата доставки", describe.Where(key => key.Key == "Date_Of_Delievery").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Type", "Тип заказа", describe.Where(key => key.Key == "Name_Of_Type").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Status", "Статус заказа", describe.Where(key => key.Key == "Name_Of_Status").First().Value));
            result.Add(new FindHandler.FieldParameters("Nickname", "Никнейм заказчика", describe.Where(key => key.Key == "Nickname").First().Value));
            result.Add(new FindHandler.FieldParameters("Executor", "Исполнитель", describe.Where(key => key.Key == "Executor").First().Value));
            result.Add(new FindHandler.FieldParameters("Responsible", "Ответственный", describe.Where(key => key.Key == "Responsible").First().Value));

            return result;
        }

        // Получить полное описание таблиц, по которым мы можем вести поиск
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe orders", connection, describe);
            FindHandler.DescribeHelper("describe types_of_order", connection, describe);
            FindHandler.DescribeHelper("describe statuses_of_order", connection, describe);
            FindHandler.DescribeHelper("describe customers", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        // Поиск
        public static (System.Data.DataTable dataTable, FindHandler.FindDescription findDescription) FindListOrders(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            var queryNotNull = getNotNullQueryText();
            var queryNull = getNullQueryText();
            var queryGroupBy = getGroupBy();
            var queryNullAlex = getNull();

            (System.Data.DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query,
                                                                                                                                 queryNotNull, queryNull, queryGroupBy, queryNullAlex);
            return result;
        }

        // Фильтр
        public static (System.Data.DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListOrders(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            var queryNotNull = getNotNullQueryText();
            var queryNull = getNullQueryText();
            var queryGroupBy = getGroupBy();

            (System.Data.DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query,
                                                                                                                                        queryNotNull, queryNull, queryGroupBy);
            return result;
        }

        // Запрос для печать
        private static string getQueryForPrint(string idToPrint)
        {
            // Костыль, но правильный
            string query_text = "select orders.id_Order as 'Номер заказа', DATE_FORMAT(orders.Date_Of_Order, \"%d.%m.%Y\") as 'Дата заказа', " +
                                " DATE_FORMAT(orders.Date_Of_Delievery, \"%d.%m.%Y\") as 'Дата доставки', customers.Parameters as 'Параметры', customers.Size 'Размер', orders.Notes as 'Заметки', " +
                                "types_of_order.Name_Of_type as 'Тип заказа', statuses_of_order.Name_Of_Status as 'Статус заказа', customers.Nickname 'Заказчик', orders.Executor as 'Исполнитель', " +
                                "orders.Responsible as 'Ответственный', products.Name_Of_Product as 'Изделие', list_products_to_order.Count as 'Количество изделий', " +
                                "materials.Name_Of_Material 'Материал', materials_for_product.Count as 'Количество материала', units.Name_Of_Unit as 'Единица измерения', " +
                                "orders.Added_Price_For_Complexity as 'Надбавка за сложность'" +
                                "from orders " +
                                "left join types_of_order on orders.Types_Of_Order_id_Type_Of_Order = types_of_order.id_Type_Of_Order " +
                                "left join statuses_of_order on orders.Statuses_Of_Order_id_Status_Of_Order = statuses_of_order.id_Status_Of_Order " +
                                "left join list_products_to_order on orders.id_order = list_products_to_order.Orders_id_Order " +
                                "left join customers on customers.id_Customer = orders.Customers_id_Customer " +
                                "left join products on list_products_to_order.Products_id_Product = products.id_product " +
                                "left join materials_for_product on materials_for_product.Products_id_Product = products.id_Product " +
                                "left join materials on materials.Vendor_Code = materials_for_product.Materials_Vendor_Code " +
                                "left join units on  materials.Units_id_unit = units.id_Unit " +
                                "where not products.Name_Of_Product is null and orders.id_Order in (" + idToPrint + ") " +
                                "order by orders.id_Order,products.Name_Of_Product; ";

            return query_text;
        }

        // Печать выделенных заказов, у которых указан перечень изделий
        public static void PrintOrders(string idToPrint, MySqlConnection connection)
        {
            string queryText = getQueryForPrint(idToPrint);

            connection.Open();
            var dataTable = FormLoader.ExecuteQuery(queryText, connection);
            connection.Close();

            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show("У выделенных заказов не заполнена таблица изделий", "Печать заказов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MakeExcel(dataTable);
            }
        }

        // Заполенение таблицы в Экселе
        private static void MakeExcel(System.Data.DataTable dataTable)
        {
            Excel.Application excel;
            // Вот если сделать именно так, то будет проверка на запуск Экселя, и если все ок, то идем дальше, иначе сразу вылетаем из метода
            try
            {
                excel = new Excel.Application();
            }
            catch
            {
                MessageBox.Show("На вашем компьютере не установлен Excel. Печать невозможна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet = (Worksheet)workbook.Sheets[1];

            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                Range myRange = (Range)sheet.Cells[1, j + 1];
                sheet.Cells[1, j + 1].Font.Bold = true;
                sheet.Columns[j + 1].ColumnWidth = 15;
                myRange.Value2 = dataTable.Columns[j].ColumnName;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    string field = dataTable.Rows[i].ItemArray[j].ToString();
                    TextBlock TextBlockWithField = new TextBlock();
                    TextBlockWithField.Text = field;
                    Range myRange = (Range)sheet.Cells[i + 2, j + 1];
                    myRange.Value2 = TextBlockWithField.Text;
                }
            }

            excel.Visible = true;
        }

        public string this[string columnName] => "";

        public string Error => "";
    }
}
