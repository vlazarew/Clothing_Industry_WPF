using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Баланс_клиентов
{
    public class CustomerBalance : IDataErrorInfo
    {
        public int idCustomer { get; set; }
        public float accured { get; set; }
        public float paid { get; set; }
        public float debt { get; set; }

        // Запросец на все балансы клиентов
        public static string getQueryText()
        {
            string query_text = "select customers_balance.Customers_id_Customer, customers_balance.Accured, customers_balance.Paid, customers_balance.Debt, customers.Name, " +
                                "customers.Lastname, customers.Address, customers.Phone_Number, customers.Nickname, customers.Passport_Data " +
                                "from customers " +
                                "left join customers_balance on customers.id_Customer = customers_balance.Customers_id_Customer ; ";

            return query_text;
        }

        // Получение данных обо всех балансах 
        public static DataTable getListCustomerBalance(MySqlConnection connection)
        {
            string queryText = getQueryText();

            connection.Open();
            var dataTable = FormLoader.ExecuteQuery(queryText, connection);
            connection.Close();

            return dataTable;
        }

        // Список полей, по которым мы можем делать поиск
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters("Accured", "Начислено на сумму", describe.Where(key => key.Key == "Accured").First().Value));
            result.Add(new FindHandler.FieldParameters("Paid", "Оплачено заказчиком", describe.Where(key => key.Key == "Paid").First().Value));
            result.Add(new FindHandler.FieldParameters("Debt", "Долг", describe.Where(key => key.Key == "Debt").First().Value));
            result.Add(new FindHandler.FieldParameters("Name", "Имя заказчика", describe.Where(key => key.Key == "Name").First().Value));
            result.Add(new FindHandler.FieldParameters("Lastname", "Фамилия заказчика", describe.Where(key => key.Key == "Lastname").First().Value));
            result.Add(new FindHandler.FieldParameters("Address", "Адрес", describe.Where(key => key.Key == "Address").First().Value));
            result.Add(new FindHandler.FieldParameters("Phone_Number", "Телефонный номер", describe.Where(key => key.Key == "Phone_Number").First().Value));
            result.Add(new FindHandler.FieldParameters("Nickname", "Никнейм", describe.Where(key => key.Key == "Nickname").First().Value));
            result.Add(new FindHandler.FieldParameters("Passport_data", "Паспортные данные", describe.Where(key => key.Key == "Passport_data").First().Value));

            return result;
        }

        // Получить полное описание таблиц, по которым мы можем вести поиск
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe customers", connection, describe);
            FindHandler.DescribeHelper("describe customers_balance", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        // Поиск
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListCustomerBalance(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }

        // Фильтр
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListCustomerBalance(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }

        public string this[string columnName] => "";

        public string Error => "";
    }
}
