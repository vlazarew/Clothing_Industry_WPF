using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
