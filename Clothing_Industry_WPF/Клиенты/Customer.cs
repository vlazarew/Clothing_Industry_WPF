using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Clothing_Industry_WPF.Клиенты
{
    public class Customer : IDataErrorInfo
    {
        // Структура Клиента
        public int id { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string patronymic { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string nickname { get; set; }
        public DateTime birthday { get; set; }
        public string passportData { get; set; }
        public int size { get; set; }
        public string parameters { get; set; }
        public string notes { get; set; }
        public byte[] photo { get; set; }
        public int statusId { get; set; }
        public string statusName { get; set; }
        public int channelId { get; set; }
        public string channelName { get; set; }
        public string employeeLogin { get; set; }

        // Пустой конструктор
        public Customer()
        {
            id = -1;
            name = "";
            lastname = "";
            patronymic = "";
            address = "";
            phoneNumber = "";
            nickname = "";
            birthday = DateTime.Now;
            passportData = "";
            size = 0;
            parameters = "";
            notes = "";
            photo = null;
            statusName = "";
            channelName = "";
            employeeLogin = "";
        }

        // Конструктор по айди (запрос в базу)
        public Customer(int id, MySqlConnection connection)
        {
            string query_text = "SELECT customers.id_Customer, customers.Name, customers.Lastname, customers.Patronymic, customers.Address, customers.Phone_Number, customers.Nickname, " +
                                "DATE_FORMAT(customers.Birthday, \"%d.%m.%Y\") as Birthday, customers.Passport_data, customers.Size, customers.Parameters, customers.Notes, customer_statuses.Name_Of_Status, " +
                                "order_channels.Name_of_channel, employees.Login, customers.Photo, customer_statuses.id_Status, order_channels.id_Channel " +
                                "FROM customers " +
                                "join main_database.employees on main_database.employees.login = customers.Employees_Login " +
                                "join main_database.customer_statuses on main_database.customer_statuses.id_Status = customers.Customer_Statuses_id_Status " +
                                "join main_database.order_channels on main_database.order_channels.id_Channel = customers.Order_Channels_id_Channel " +
                                "where customers.id_Customer = @id";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    name = reader.GetString(1);
                    lastname = reader.GetString(2);
                    patronymic = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    address = reader.GetString(4);
                    phoneNumber = reader.GetString(5);
                    nickname = reader.GetString(6);
                    birthday = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7));
                    passportData = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    if (reader.GetValue(9).ToString() != "")
                    {
                        size = reader.GetInt32(9);
                    }
                    if (reader.GetValue(10).ToString() != "")
                    {
                        parameters = reader.GetString(10);
                    }
                    if (reader.GetValue(11).ToString() != "")
                    {
                        notes = reader.GetString(11);
                    }

                    statusName = reader.GetString(12);
                    channelName = reader.GetString(13);
                    employeeLogin = reader.GetString(14);

                    statusId = reader.GetInt32(16);
                    channelId = reader.GetInt32(17);

                    photo = null;
                    try
                    {
                        photo = (byte[])(reader[15]);
                    }
                    catch
                    {

                    }
                }
            }
            connection.Close();
        }

        // Проверка на заполненность полей
        public string CheckData()
        {
            string result = "";

            if (name == "")
            {
                result += result == "" ? "Имя" : ", Имя";
            }
            if (lastname == "")
            {
                result += result == "" ? "Фамилия" : ", Фамилия";
            }
            if (address == "")
            {
                result += result == "" ? "Адрес" : ", Адрес";
            }
            if (phoneNumber == "")
            {
                result += result == "" ? "Телефонный номер" : ", Телефонный номер";
            }
            if (nickname == "")
            {
                result += result == "" ? "Никнейм" : ", Никнейм";
            }
            if (size == 0)
            {
                result += result == "" ? "Размер" : ", Размер";
            }
            if (parameters == "")
            {
                result += result == "" ? "Параметры" : ", Параметры";
            }
            if (statusName == "")
            {
                result += result == "" ? "Статус клиента" : ", Статус клиента";
            }
            if (channelName == "")
            {
                result += result == "" ? " Канал связи" : ",  Канал связи";
            }
            if (employeeLogin == "")
            {
                result += result == "" ? "Обслуживающий сотрудник" : ", Обслуживающий сотрудник";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        // Вызов сохранения и прогон по всей логике
        public void Save(MySqlConnection connection, WaysToOpenForm.WaysToOpen way)
        {
            string warning = CheckData();
            if (warning == "")
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                // Создать/изменить запись в таблице Клиенты
                MySqlCommand command = SaveInDB(connection, way);
                command.Transaction = transaction;

                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (way == WaysToOpenForm.WaysToOpen.create)
                {
                    // Если создаем клиента, то добавляем его в таблицу Балансы клиентов 
                    transaction = connection.BeginTransaction();
                    string query_max_id = "SELECT max(customers.id_Customer) FROM main_database.customers";
                    MySqlCommand commandFindId = new MySqlCommand(query_max_id, connection, transaction);
                    int findId = -1;

                    using (DbDataReader reader = commandFindId.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            findId = reader.GetInt32(0);
                        }
                    }

                    string query_balance = "Insert into customers_balance (Customers_id_Customer, Accured, Paid, Debt) values (@id, 0, 0, 0)";
                    MySqlCommand commandFillBalance = new MySqlCommand(query_balance, connection, transaction);
                    commandFillBalance.Parameters.AddWithValue("@id", findId + 1);
                    // Конец блока, дальше само выполнение sql комманд

                    try
                    {
                        commandFillBalance.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        System.Windows.MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                connection.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Генерация команды сохранения в БД
        private MySqlCommand SaveInDB(MySqlConnection connection, WaysToOpenForm.WaysToOpen way)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO customers " +
                        "(Name, Lastname, Patronymic, Address, Phone_Number, Nickname," +
                        " Birthday, Passport_Data, Size, Parameters, Notes, Customer_Statuses_id_Status, Order_Channels_id_Channel, Employees_Login, Photo)" +
                        " VALUES (@name, @lastname, @patronymic, @address, @phone, @nickname, @birthday, @passport, @size, @parameters, @notes," +
                        "         @status, @channel, @login, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update customers set Name = @name, Lastname = @lastname, Patronymic = @patronymic, Address = @address, Phone_Number = @phone, Nickname = @nickname, " +
                        "Birthday = @birthday, Passport_Data = @passport, Size = @size, Parameters = @parameters, Notes = @notes, Customer_Statuses_id_Status = @status, " +
                        "Order_Channels_id_Channel = @channel, Employees_Login = @login, Photo = @image" +
                        " where id_Customer = @id;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@lastname", lastname);
            command.Parameters.AddWithValue("@patronymic", patronymic);
            command.Parameters.AddWithValue("@address", address);
            command.Parameters.AddWithValue("@phone", phoneNumber);
            command.Parameters.AddWithValue("@nickname", nickname);
            command.Parameters.AddWithValue("@birthday", birthday);
            command.Parameters.AddWithValue("@passport", passportData);
            command.Parameters.AddWithValue("@size", size);
            command.Parameters.AddWithValue("@parameters", parameters);
            command.Parameters.AddWithValue("@notes", notes);

            MySqlCommand commandStatus = new MySqlCommand("select id_Status from customer_statuses where name_of_status = @status", connection);
            commandStatus.Parameters.AddWithValue("status", statusName);
            int id_status = -1;
            using (DbDataReader reader = commandStatus.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_status = reader.GetInt32(0);
                }
            }

            MySqlCommand commandChannel = new MySqlCommand("select id_Channel from order_channels where name_of_channel = @channel", connection);
            commandChannel.Parameters.AddWithValue("channel", channelName);
            int id_channel = -1;
            using (DbDataReader reader = commandChannel.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_channel = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@status", id_status);
            command.Parameters.AddWithValue("@channel", id_channel);
            command.Parameters.AddWithValue("@login", employeeLogin);


            // Обработка фото 
            command.Parameters.AddWithValue("@image", photo);

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@id", id);
            }

            return command;
        }

        // Запросец на всех клиентов
        public static string getQueryText()
        {
            string query_text = "SELECT customers.id_Customer, customers.Name, customers.Lastname, customers.Patronymic, customers.Address, customers.Phone_Number, customers.Nickname, " +
                "DATE_FORMAT(customers.Birthday, \"%d.%m.%Y\") as Birthday, customers.Passport_data, customers.Size, customers.Parameters, customers.Notes, customer_statuses.Name_Of_Status, " +
                "order_channels.Name_of_channel, employees.Login " +
                "FROM customers " +
                "join main_database.employees on main_database.employees.login = customers.Employees_Login " +
                "join main_database.customer_statuses on main_database.customer_statuses.id_Status = customers.Customer_Statuses_id_Status " +
                "join main_database.order_channels on main_database.order_channels.id_Channel = customers.Order_Channels_id_Channel ;";

            return query_text;
        }

        // Получение данных обо всех клиентах
        public static DataTable getListCustomers(MySqlConnection connection)
        {
            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            connection.Close();

            return dataTable;
        }

        // Валидация данных
        public string this[string columnName]
        {
            get
            {
                string error = "";
                /*switch (columnName)
                {
                    case "name":
                        if (name == "")
                        {
                            error = "Имя должно быть заполнено";
                        }
                        break;
                    case "lastname":
                        if (lastname == "")
                        {
                            error = "Фамилия должна быть заполнена";
                        }
                        break;
                    case "address":
                        if (address == "")
                        {
                            error = "Адрес должен быть заполнен";
                        }
                        break;
                    case "phoneNumber":
                        if (phoneNumber == "")
                        {
                            error = "Телефон должен быть заполнен";
                        }
                        break;
                    case "nickname":
                        if (nickname == "")
                        {
                            error = "Никнейм должен быть заполнен";
                        }
                        break;
                    case "size":
                        if (size == 0)
                        {
                            error = "Размер должен быть заполнен";
                        }
                        break;
                    case "parameters":
                        if (parameters == "")
                        {
                            error = "Параметры должны быть заполнены";
                        }
                        break;
                    case "statusId":
                        if (statusId == 0)
                        {
                            error = "Статус должен быть заполнен";
                        }
                        break;
                    case "channelId":
                        if (channelId == 0)
                        {
                            error = "Канал связи должен быть заполнен";
                        }
                        break;
                    case "employeeId":
                        if (employeeId == 0)
                        {
                            error = "Обслуживающий сотрудник должен быть заполнен";
                        }
                        break;
                }*/
                return error;
            }
        }

        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
