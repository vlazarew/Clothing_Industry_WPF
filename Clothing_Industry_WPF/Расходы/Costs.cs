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

namespace Clothing_Industry_WPF.Расходы
{
    public class Costs : IDataErrorInfo
    {
        public int id { get; set; }
        public string defaultFolder { get; set; }
        public DateTime dateOfCost { get; set; }
        public string nameOfDocument { get; set; }
        public float amount { get; set; }
        public string notes { get; set; }
        public string to { get; set; }
        public string from { get; set; }
        public string consumptionCategoriesName { get; set; }
        public string typeOfPaymentName { get; set; }
        public string periodicityName { get; set; }


        public string this[string columnName] => "";

        public string Error => "";
        public static string getQueryText()
        {
            string query_text = "select costs.id, " +
                                "costs.Name_Of_Document, costs.Default_Folder, DATE_FORMAT(costs.Date_Of_Cost, \"%d.%m.%Y\") as Date_Of_Cost, costs.Amount," +
                                " costs.Notes, consumption_categories.Name_Of_Category, types_of_payment.Name_Of_Type, periodicities.Name_Of_Periodicity, costs.From, costs.To" +
                                " from costs" +
                                " join consumption_categories on costs.Consumption_Categories_id_Consumption_Category = consumption_categories.id_Consumption_Category" +
                                " join types_of_payment on costs.Types_Of_Payment_id_Of_Type = types_of_payment.id_Of_Type" +
                                " join periodicities on costs.Periodicities_id_Periodicity = periodicities.id_Periodicity;";
            return query_text;
        }
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe receipt_of_materials", connection, describe);
            FindHandler.DescribeHelper("describe payment_states", connection, describe);
            FindHandler.DescribeHelper("describe type_of_transactions", connection, describe);
            FindHandler.DescribeHelper("describe suppliers", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Name_Of_Document", "Название документа", describe.Where(key => key.Key == "Name_Of_Document").First().Value));
            result.Add(new FindHandler.FieldParameters("Default_Folder", "Путь документа", describe.Where(key => key.Key == "Default_Folder").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Entry", "Дата прихода", describe.Where(key => key.Key == "Date_Of_Entry").First().Value));
            result.Add(new FindHandler.FieldParameters("Notes", "Доп. сведения", describe.Where(key => key.Key == "Notes").First().Value));
            result.Add(new FindHandler.FieldParameters("Total_Price", "Сумма прихода", describe.Where(key => key.Key == "Total_Price").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Supplier", "Поставщик", describe.Where(key => key.Key == "Name_Of_Supplier").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_State", "Статус", describe.Where(key => key.Key == "Name_Of_State").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Type", "Тип транзакции", describe.Where(key => key.Key == "Name_Of_Type").First().Value));

            return result;
        }
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListCosts(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListCosts(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }

        public static void DeleteFromDB(List<int> ids, MySqlConnection connection)
        {

            connection.Open();

            foreach (var id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from costs where id = @id";

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

    }
}

