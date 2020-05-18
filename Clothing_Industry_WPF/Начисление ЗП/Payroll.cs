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

namespace Clothing_Industry_WPF.Начисление_ЗП
{
    public class Payroll : IDataErrorInfo
    {
        public string employeeLogin { get; set; }
        public string period { get; set; }
        public DateTime dateOfPay { get; set; }
        public float salary { get; set; }
        public float pieceWorkPayment { get; set; }
        public float totalSalary { get; set; }
        public float penalty { get; set; }
        public float toPay { get; set; }
        public string notes { get; set; }
        public bool PaidOff { get; set; }


        public string this[string columnName] => "";

        public string Error => "";

        public static string getQueryText()
        {
            string query_text = "SELECT Employees_Login as Login, Period , DATE_FORMAT(Date_Of_Pay, \"%d.%m.%Y\") as Date_Of_Pay, Salary, PieceWorkPayment, " +
                                "Total_Salary, Penalty, To_Pay, Notes, PaidOff " +
                                "FROM payrolls ; ";
            return query_text;
        }

        // Получить полное описание таблиц, по которым мы можем вести поиск
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

            result.Add(new FindHandler.FieldParameters("Employees_Login", "Логин", describe.Where(key => key.Key == "Employees_Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Period", "Период", describe.Where(key => key.Key == "Period").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Pay", "Дата выплаты", describe.Where(key => key.Key == "Date_Of_Pay").First().Value));
            result.Add(new FindHandler.FieldParameters("Salary", "Зарплата", describe.Where(key => key.Key == "Salary").First().Value));
            result.Add(new FindHandler.FieldParameters("PieceWorkPayment", "Сдельная Зарплата", describe.Where(key => key.Key == "PieceWorkPayment").First().Value));
            result.Add(new FindHandler.FieldParameters("Total_Salary", "Общая зп", describe.Where(key => key.Key == "Total_Salary").First().Value));
            result.Add(new FindHandler.FieldParameters("Penalty", "Штраф", describe.Where(key => key.Key == "Penalty").First().Value));
            result.Add(new FindHandler.FieldParameters("To_Pay", "К Выплате", describe.Where(key => key.Key == "To_Pay").First().Value));
            result.Add(new FindHandler.FieldParameters("PaidOff", "Выплачено", describe.Where(key => key.Key == "PaidOff").First().Value));
            result.Add(new FindHandler.FieldParameters("Notes", "Заметки", describe.Where(key => key.Key == "Notes").First().Value));


            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListPayrolls(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }


        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListPayrolls(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }

        public static void DeleteFromDB(List<(string login, string period)> dataToDelete, MySqlConnection connection)
        {
            connection.Open();

            foreach (var data in dataToDelete)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from payrolls where Employees_Login = @login and period = @period";
                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@login", data.login);
                commandTable.Parameters.AddWithValue("@period", data.period);

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
        }
    }
}
