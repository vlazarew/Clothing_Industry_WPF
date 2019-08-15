using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Баланс_клиентов
{
    /// <summary>
    /// Логика взаимодействия для CustomerBalanceListWindow.xaml
    /// </summary>
    public partial class CustomerBalanceListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public CustomerBalanceListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            customersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string getQueryText()
        {
            string query_text = "select customers_balance.Customers_id_Customer, customers_balance.Accured, customers_balance.Paid, customers_balance.Debt, customers.Name, " +
                                "customers.Lastname, customers.Address, customers.Phone_Number, customers.Nickname, customers.Passport_Data " +
                                "from customers " +
                                "left join customers_balance on customers.id_Customer = customers_balance.Customers_id_Customer ; ";

            return query_text;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields();

            var findWindow = new FindWindow(currentFindDescription, listOfField);
            if (findWindow.ShowDialog().Value)
            {
                currentFindDescription = findWindow.Result;
            }
            else
            {
                return;
            }

            var field = listOfField.Where(kvp => kvp.application_name == currentFindDescription.field).First().db_name;
            string query = getQueryText();
            string edited_query;

            if (!currentFindDescription.isDate)
            {
                edited_query = query.Replace(";", " where " + field + " ");
                edited_query += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
            }
            else
            {
                edited_query = query.Replace(";", " where DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                edited_query += string.Format("= \'{0}\'", currentFindDescription.value);
            }

            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(edited_query, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            customersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
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

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe customers", connection, describe);
            DescribeHelper("describe customers_balance", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        private void DescribeHelper(string query, MySqlConnection connection, List<KeyValuePair<string, string>> pairs)
        {
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    pairs.Add(new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)));
                }
            }
        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");
            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();           
        }

        private void ButtonFilters_Click(object sender, RoutedEventArgs e)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields();
            var filterWindow = new FilterWindow(currentFilterDescription, listOfField);
            if (filterWindow.ShowDialog().Value)
            {
                currentFilterDescription = filterWindow.Result;
            }
            else
            {
                return;
            }

            string editedQuery = EditFilterQuery(currentFilterDescription, listOfField);

            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(editedQuery, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            customersGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string EditFilterQuery(List<FilterHandler.FilterDescription> filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = getQueryText();

            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result = result.Replace(";", " where ");
                    break;
                }
            }

            int index = 0;
            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result += AddСondition(filterRecord, listOfField);
                    index++;
                    if (index < filter.Count)
                    {
                        result += " and ";
                    }
                }
            }

            return result;
        }

        private string AddСondition(FilterHandler.FilterDescription filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = "";
            var field = listOfField.Where(kvp => kvp.application_name == filter.field).First().db_name;
            var typeFilter = FilterHandler.TakeFilter(filter.typeOfFilter);
            if (filter.typeOfFilter == TypeOfFilter.TypesOfFilter.isFilled)
            {
                result += "NOT ";
            }

            if (!filter.isDate)
            {
                result += string.Format(field + " " + typeFilter + "\"{0}\"", filter.value);
            }
            else
            {
                string day = filter.value.Substring(0, 2);
                string month = filter.value.Substring(3, 2);
                string year = filter.value.Substring(6, 4);
                result += string.Format(field + " " + typeFilter + " \'{0}-{1}-{2}\'", year, month, day);
                //result += string.Format(" DATE_FORMAT(" + field + ", '%d.%m.%Y') = \'{0}\'", filter.value);
            }

            /*if (filter.typeOfFilter == TypeOfFilter.TypesOfFilter.contains)
            {
                result += ") ";
            }*/

            return result;
        }
    }
}
