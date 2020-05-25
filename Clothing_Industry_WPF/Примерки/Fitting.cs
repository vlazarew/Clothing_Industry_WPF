using Clothing_Industry_WPF.Поиск_и_фильтры;
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

namespace Clothing_Industry_WPF.Примерки
{
    public class Fitting : IDataErrorInfo
    {
        public int orderId { get; set; }
        public DateTime date { get; set; }
        public string notes { get; set; }
        public string typeOfFitting { get; set; }

        public string this[string columnName] => "";

        public string Error => "";

        public static string getQueryText()
        {
            string query_text = "select customers.nickname as Customer, orders.id_order OrderId, types_of_fitting.Name_Of_type as Type_Of_Fitting, " +
                        "DATE_FORMAT(fittings.date, \"%d.%m.%Y\") as Date, fittings.time as Time, fittings.notes as Notes " +
                        "from fittings " +
                        "join orders on fittings.orders_id_order = orders.id_order " +
                        "join customers on orders.customers_id_customer = customers.id_customer " +
                        "join types_of_fitting on fittings.types_of_fitting_id_type_of_fitting = types_of_fitting.id_type_of_fitting; ";
            return query_text;
        }

        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe fittings", connection, describe);
            FindHandler.DescribeHelper("describe customers", connection, describe);
            FindHandler.DescribeHelper("describe types_of_fitting", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        // Список полей, по которым мы можем делать поиск
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Nickname", "Никнейм", describe.Where(key => key.Key == "Nickname").First().Value));
            result.Add(new FindHandler.FieldParameters("Orders_id_Order", "Номер заказа", describe.Where(key => key.Key == "Orders_id_Order").First().Value));
            result.Add(new FindHandler.FieldParameters("Date", "Дата примерки", describe.Where(key => key.Key == "Date").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_type", "Тип примерки", describe.Where(key => key.Key == "Name_Of_type").First().Value));

            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListFittings(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }


        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListFittings(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }

        public static void DeleteFromDB(List<(string customerNickname, int idOrder)> dataToDelete, MySqlConnection connection)
        {
            connection.Open();

            foreach (var data in dataToDelete)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                MySqlCommand commandCustomer = new MySqlCommand("select id_Customer from customers where nickname = @nickname", connection);
                commandCustomer.Parameters.AddWithValue("@nickname", data.customerNickname);
                int id_customer = -1;
                using (DbDataReader reader = commandCustomer.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id_customer = reader.GetInt32(0);
                    }
                }

                string queryTable = "delete from fittings where Orders_id_Order = @idOrder and Customers_id_Customer = @idCustomer";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@idOrder", data.idOrder);
                commandTable.Parameters.AddWithValue("@idCustomer", id_customer);
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }
      
    }
}

