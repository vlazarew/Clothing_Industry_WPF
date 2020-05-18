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

namespace Clothing_Industry_WPF.Отпуска
{
    public class Holidays : IDataErrorInfo
    {
        public string employeeLogin { get; set; }
        public int year { get; set; }
        public int daysOfHolidays { get; set; }
        public int daysUsed { get; set; }
        public int restOfDays { get; set; }
        public DateTime plannedStart { get; set; }
        public DateTime inFactStart { get; set; }
        public DateTime inFactEnd { get; set; }
        public string notes { get; set; }

        public string this[string columnName] => "";

        public string Error => "";

        public static string getQueryText()
        {
            string query_text = "select employees.Login, holidays.Year, holidays.Days_Of_Holidays, holidays.Days_Used, holidays.Rest_Of_Days, DATE_FORMAT(holidays.Planned_Start, \"%d.%m.%Y\") as Planned_Start, DATE_FORMAT(holidays.In_Fact_Start, \"%d.%m.%Y\") as In_Fact_Start, " +
                                "DATE_FORMAT(holidays.In_Fact_End, \"%d.%m.%Y\") as In_Fact_End, holidays.Notes" +
                                " from holidays" +
                                " join employees on holidays.Employees_Login = employees.Login;";
            return query_text;
        }
        // Получить полное описание таблиц, по которым мы можем вести поиск
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe holidays", connection, describe);
            FindHandler.DescribeHelper("describe employees", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }
        // Список полей, по которым мы можем делать поиск
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Login", "Логин", describe.Where(key => key.Key == "Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Year", "Год", describe.Where(key => key.Key == "Year").First().Value));
            result.Add(new FindHandler.FieldParameters("Days_Of_Holidays", "Всего дней", describe.Where(key => key.Key == "Days_Of_Holidays").First().Value));
            result.Add(new FindHandler.FieldParameters("Days_Used", "Прошло", describe.Where(key => key.Key == "Days_Used").First().Value));
            result.Add(new FindHandler.FieldParameters("Rest_Of_Days", "Осталось", describe.Where(key => key.Key == "Rest_Of_Days").First().Value));
            result.Add(new FindHandler.FieldParameters("Planned_Start", "Запланировано", describe.Where(key => key.Key == "Planned_Start").First().Value));
            result.Add(new FindHandler.FieldParameters("In_Fact_Start", "Начинается с", describe.Where(key => key.Key == "In_Fact_Start").First().Value));
            result.Add(new FindHandler.FieldParameters("In_Fact_End", "Заканчивается", describe.Where(key => key.Key == "In_Fact_End").First().Value));
            result.Add(new FindHandler.FieldParameters("Notes", "Примечания", describe.Where(key => key.Key == "Notes").First().Value));

            return result;
        }
        // Поиск
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListHolidays(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        // Фильтр
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListHolidays(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }
        public static void DeleteFromDB(List<string> Codes, MySqlConnection connection)
        {
            connection.Open();

            foreach (string Code in Codes)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from holidays where Employees_Login = @login";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@login", Code);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление отпуска " + Code + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }
    }
}

