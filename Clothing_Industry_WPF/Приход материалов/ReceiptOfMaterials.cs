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

namespace Clothing_Industry_WPF.Приход_материалов
{
    public class ReceiptOfMaterials : IDataErrorInfo
    {
        public int id { get; set; }
        public string defaulFolder { get; set; }
        public string nameOfDocument { get; set; }
        public DateTime dateOfEntry { get; set; }
        public string notes { get; set; }
        public float totalPrice { get; set; }
        public string paymentStateName { get; set; }
        public string typeOfTransactionName { get; set; }
        public string supplierName { get; set; }

        public string this[string columnName] => "";

        public string Error => "";

        public static string getQueryText()
        {
            string query_text = "select receipt_of_materials.id_Document_Of_Receipt, receipt_of_materials.Default_Folder, receipt_of_materials.Name_Of_Document, " +
                                 "DATE_FORMAT(receipt_of_materials.Date_Of_Entry, '%d.%m.%Y') as Date_Of_Entry, receipt_of_materials.Notes, " +
                                 "suppliers.Name_Of_Supplier, payment_states.Name_Of_State, " +
                                 "type_of_transactions.Name_Of_Type, receipt_of_materials.Notes, receipt_of_materials.Total_Price " +
                                 "from receipt_of_materials " +
                                 "join payment_states on receipt_of_materials.Payment_States_id_Payment_States = payment_states.id_Payment_States " +
                                 "join type_of_transactions on receipt_of_materials.Type_Of_Transactions_id_Type_Of_Transaction = type_of_transactions.id_Type_Of_Transaction " +
                                 "join suppliers on receipt_of_materials.Suppliers_id_Supplier = suppliers.id_Supplier;";
            return query_text;
        }
        // Получить полное описание таблиц, по которым мы можем вести поиск
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
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListReceipt(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListReceipt(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
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

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from receipt_of_materials where id_Document_Of_Receipt = @id_Document_Of_Receipt";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id_Document_Of_Receipt", id);
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление документа " + id + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }
    }
}

