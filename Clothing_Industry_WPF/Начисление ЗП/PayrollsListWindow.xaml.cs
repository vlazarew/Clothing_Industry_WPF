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

namespace Clothing_Industry_WPF.Начисление_ЗП
{
    public struct HelpStruct
    {
        public string login { get; set; }
        public string period { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для PayrollsListWindow.xaml
    /// </summary>
    public partial class PayrollsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public PayrollsListWindow()
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
            payrollsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            payrollsGrid.SelectedIndex = 0;

            List<HelpStruct> ids = new List<HelpStruct>();
            foreach (DataRowView row in payrollsGrid.SelectedItems)
            {
                ids.Add(new HelpStruct { login = row.Row.ItemArray[0].ToString(), period = row.Row.ItemArray[1].ToString() });
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private string getQueryText()
        {
            string query_text = "SELECT Employees_Login as Login, Period , DATE_FORMAT(Date_Of_Pay, \"%d.%m.%Y\") as Date_Of_Pay, Salary, PieceWorkPayment, " +
                                "Total_Salary, Penalty, To_Pay, Notes, PaidOff " +
                                "FROM payrolls ; ";
            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new PayrollsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = payrollsGrid.SelectedIndex;
            string login = "";
            string period = "";
            int current_row = 0;
            foreach (DataRowView row in payrollsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                login = row.Row.ItemArray[0].ToString();
                period = row.Row.ItemArray[1].ToString();
                break;
            }

            Window edit_window = new PayrollsRecordWindow(WaysToOpenForm.WaysToOpen.edit, login, period);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<HelpStruct> rowsToDelete = new List<HelpStruct>();
            foreach (DataRowView row in payrollsGrid.SelectedItems)
            {
                rowsToDelete.Add(new HelpStruct { login = row.Row.ItemArray[0].ToString(), period = row.Row.ItemArray[1].ToString() });
            }

            DeleteFromDB(rowsToDelete);

        }

        private void DeleteFromDB(List<HelpStruct> myStruct)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (HelpStruct record in myStruct)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from payrolls where Employees_Login = @login and period = @period";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@login", record.login);
                commandTable.Parameters.AddWithValue("@period", record.period);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Удаление не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<HelpStruct> rowsToEdit = new List<HelpStruct>();
            foreach (DataRowView row in payrollsGrid.SelectedItems)
            {
                rowsToEdit.Add(new HelpStruct { login = row.Row.ItemArray[0].ToString(), period = row.Row.ItemArray[1].ToString() });
            }

            if (rowsToEdit.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (rowsToEdit.Count > 1)
                {
                    for (int i = 0; i < rowsToEdit.Count - 1; i++)
                    {
                        edit_window = new PayrollsRecordWindow(WaysToOpenForm.WaysToOpen.edit, rowsToEdit[i].login, rowsToEdit[i].period);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new PayrollsRecordWindow(WaysToOpenForm.WaysToOpen.edit, rowsToEdit[rowsToEdit.Count - 1].login, rowsToEdit[rowsToEdit.Count - 1].period);
                edit_window.ShowDialog();

                //Обновление списка
                RefreshList();
            }
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
            payrollsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Employees_Login", "Логин", describe.Where(key => key.Key == "Employees_Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Pay", "Дата выплаты", describe.Where(key => key.Key == "Date_Of_Pay").First().Value));
            result.Add(new FindHandler.FieldParameters("Salary", "Зарплата", describe.Where(key => key.Key == "Salary").First().Value));
            result.Add(new FindHandler.FieldParameters("PieceWorkPayment", "Сдельная Зарплата", describe.Where(key => key.Key == "PieceWorkPayment").First().Value));
            result.Add(new FindHandler.FieldParameters("Total_Salary", "Общая зп", describe.Where(key => key.Key == "Total_Salary").First().Value));
            result.Add(new FindHandler.FieldParameters("Penalty", "Штраф", describe.Where(key => key.Key == "Penalty").First().Value));
            result.Add(new FindHandler.FieldParameters("To_Pay", "К Выплате", describe.Where(key => key.Key == "To_Pay").First().Value));
            result.Add(new FindHandler.FieldParameters("Period", "Месяц и год", describe.Where(key => key.Key == "Period").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe payrolls", connection, describe);
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
            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");
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
            payrollsGrid.ItemsSource = dataTable.DefaultView;
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

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            List<HelpStruct> ids = new List<HelpStruct>();
            foreach (DataRowView row in payrollsGrid.SelectedItems)
            {
                ids.Add(new HelpStruct { login = row.Row.ItemArray[0].ToString(), period = row.Row.ItemArray[1].ToString() });
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
    }
}
