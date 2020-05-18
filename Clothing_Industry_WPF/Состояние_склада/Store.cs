using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Состояние_склада
{
    class Store
    {
        public static string getQueryText()
        {
            string query_text = "select materials.Vendor_Code, materials.Name_Of_Material, store.Count, units.Name_Of_Unit" +
                                " from store" +
                                " join materials on store.Materials_Vendor_Code = materials.Vendor_Code" +
                                " join units on materials.Units_id_Unit = units.id_Unit;";
            return query_text;
        }
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe store", connection, describe);
            FindHandler.DescribeHelper("describe materials", connection, describe);
            FindHandler.DescribeHelper("describe units", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Materials_Vendor_Code", "Артикул", describe.Where(key => key.Key == "Materials_Vendor_Code").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Material", "Название материала", describe.Where(key => key.Key == "Name_Of_Material").First().Value));
            result.Add(new FindHandler.FieldParameters("Count", "Количество", describe.Where(key => key.Key == "Count").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Unit", "Единица измерения", describe.Where(key => key.Key == "Name_Of_Unit").First().Value));

            return result;
        }
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListStore(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListStore(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }
    }
}
