using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int employeeId { get; set; }
        public string employeeName { get; set; }

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
            employeeName = "";
        }

        public Customer(int id, string name, string lastname, string patronymic, string address, string phoneNumber, string nickname, DateTime birthday, string passportData, int size, 
                            string parameters, string notes, byte[] photo, string statusName, string channelName, string employeeName)
        {
            this.id = id;
            this.name = name;
            this.lastname = lastname;
            this.patronymic = patronymic;
            this.address = address;
            this.phoneNumber = phoneNumber;
            this.nickname = nickname;
            this.birthday = birthday;
            this.passportData = passportData;
            this.size = size;
            this.parameters = parameters;
            this.notes = notes;
            this.photo = photo;
            this.statusName = statusName;
            this.channelName = channelName;
            this.employeeName = employeeName;
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
                    employeeName = reader.GetString(14);

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
            if (employeeName == "")
            {
                result += result == "" ? "Обслуживающий сотрудник" : ", Обслуживающий сотрудник";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
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
