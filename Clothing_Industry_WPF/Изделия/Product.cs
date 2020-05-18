using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clothing_Industry_WPF.Изделия
{
    public class Product : IDataErrorInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public float fixedPrice { get; set; }
        public float moneyToEmployee { get; set; }
        public string description { get; set; }
        public byte[] photo { get; set; }

        public string this[string columnName] => "";

        public string Error => "";

        public static string getQueryText()
        {
            string query_text = "select products.id_product, products.Name_Of_Product, products.Fixed_Price, products.MoneyToEmployee," +
                                " products.Description" +
                                " from products;";

            return query_text;
        }

        public static void DeleteFromDB(List<int> ids, MySqlConnection connection)
        {
            connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from products where id_Product = @id_Product";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id_Product", id);
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление не удалось", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            connection.Close();
        }
        // Получить полное описание таблиц, по которым мы можем вести поиск
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe products", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        // Список полей, по которым мы можем делать поиск
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters("Name_Of_Product", "Название", describe.Where(key => key.Key == "Name_Of_Product").First().Value));
            result.Add(new FindHandler.FieldParameters("Fixed_Price", "Цена", describe.Where(key => key.Key == "Fixed_Price").First().Value));
            result.Add(new FindHandler.FieldParameters("Per_Cents", "Выплата сотруднику", describe.Where(key => key.Key == "MoneyToEmployee").First().Value));
            result.Add(new FindHandler.FieldParameters("Description", "Описание", describe.Where(key => key.Key == "Description").First().Value));

            return result;
        }

        // Поиск
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListProducts(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }

        // Фильтр
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListProducts(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }
    }
}