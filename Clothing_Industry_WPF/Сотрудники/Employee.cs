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

namespace Clothing_Industry_WPF.Сотрудники
{
    public class Employee : IDataErrorInfo
    {
        public string login { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string patronymic { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string passportData { get; set; }
        public string notes { get; set; }
        public byte[] photo { get; set; }
        public DateTime added { get; set; }
        public float lastSalary { get; set; }
        public string positionName { get; set; }

        public string this[string columnName] => "";

        public string Error => "";

        public static string getQueryText()
        {
            string query_text = "select employees.Login, employees.Name, employees.Lastname, employees.Patronymic, employees.Phone_Number, employees.Passport_Data, employees.Email," +
                                "employees.Notes, DATE_FORMAT(employees.Added, \"%d.%m.%Y\") as Added, employees.Last_Salary, employee_positions.Name_Of_Position" +
                                " from employees" +
                                " join employee_positions on employees.Employee_Positions_id_Employee_Position = employee_positions.id_Employee_Position;";

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
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters("Login", "Логин", describe.Where(key => key.Key == "Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Lastname", "Фамилия", describe.Where(key => key.Key == "Lastname").First().Value));
            result.Add(new FindHandler.FieldParameters("Name", "Имя", describe.Where(key => key.Key == "Name").First().Value));
            result.Add(new FindHandler.FieldParameters("Patronymic", "Отчество", describe.Where(key => key.Key == "Patronymic").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Position", "Должность", describe.Where(key => key.Key == "Name_Of_Position").First().Value));
            result.Add(new FindHandler.FieldParameters("Phone_Number", "Телефон", describe.Where(key => key.Key == "Phone_Number").First().Value));
            result.Add(new FindHandler.FieldParameters("Email", "Email", describe.Where(key => key.Key == "Email").First().Value));
            result.Add(new FindHandler.FieldParameters("Passport_Data", "Паспортные данные", describe.Where(key => key.Key == "Passport_Data").First().Value));
            result.Add(new FindHandler.FieldParameters("Added", "Дата добавления", describe.Where(key => key.Key == "Added").First().Value));
            result.Add(new FindHandler.FieldParameters("Last_Salary", "З/п за прошлый месяц", describe.Where(key => key.Key == "Last_Salary").First().Value));

            return result;
        }
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListEmployee(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListEmployee(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }

        public static void DeleteFromDB(List<string> logins, MySqlConnection connection)
        {
            connection.Open();

            foreach (string login in logins)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from employees where Login = @login";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@login", login);

                string queryUser = string.Format("drop user '{0}'@'%'", login);
                MySqlCommand commandUser = new MySqlCommand(queryUser, connection, transaction);
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление документа " + login + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }
    }
}

