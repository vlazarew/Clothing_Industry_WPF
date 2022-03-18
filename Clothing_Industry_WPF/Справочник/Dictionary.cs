using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace Clothing_Industry_WPF.Справочник
{
    class Dictionary
    {
        public static string getQueryText(string id, string Name, string Dictionary)
        {
            string query_text = "select " + id + " , " + Name + " as Name from " + Dictionary + " ; ";
            return query_text;
        }

        public static string getQueryEmployeePositions()
        {
            string query_text = "select id_Employee_Position, Name_Of_Position as Name, IsAdministrator from employee_positions;";
            return query_text;
        }
        // Получить полное описание таблиц, по которым мы можем вести поиск
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe employees", connection, describe);
            FindHandler.DescribeHelper("describe employee_positions", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }
        private static List<FindHandler.FieldParameters> FillFindFieldsEmployee(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters("id_Employee_Position", "Код", describe.Where(key => key.Key == "id_Employee_Position").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Position", "Наименование", describe.Where(key => key.Key == "Name_Of_Position").First().Value));
            result.Add(new FindHandler.FieldParameters("IsAdministrator", "Это администратор", describe.Where(key => key.Key == "IsAdministrator").First().Value));


            return result;
        }
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListEmployee(FindHandler.FindDescription currentFindDescription,  MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFieldsEmployee(connection);
            var query = getQueryEmployeePositions();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListEmployee(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFieldsEmployee(connection);
            var query = getQueryEmployeePositions();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }

        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection, string id, string Name)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters(id, "Код", describe.Where(key => key.Key == id).First().Value));
            result.Add(new FindHandler.FieldParameters(Name, "Наименование", describe.Where(key => key.Key == Name).First().Value));



            return result;
        }
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindList(FindHandler.FindDescription currentFindDescription, MySqlConnection connection, string id, string Name)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection,id,Name);
            var query = getQueryEmployeePositions();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterList(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection, string id, string Name)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection, id, Name);
            var query = getQueryEmployeePositions();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }
        public static void DeleteFromDBEmployee(List<int> ids, MySqlConnection connection)
        {
            connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from employee_positions where id_Employee_Position = @id";

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
                    System.Windows.MessageBox.Show("Удаление " + id + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }
        public static void DeleteFromDB(List<int> ids, MySqlConnection connection, string Id, string Dictionary)
        {

        connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

        string queryTable = "delete from " + Dictionary + " where " + Id + " = @id;";

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
                    System.Windows.MessageBox.Show("Удаление  " + id + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();

        }
    }
}