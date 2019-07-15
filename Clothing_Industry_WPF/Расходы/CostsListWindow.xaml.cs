using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using Clothing_Industry_WPF.Поиск_файла;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Clothing_Industry_WPF.Расходы
{
    /// <summary>
    /// Логика взаимодействия для CostsListWindow.xaml
    /// </summary>
    public partial class CostsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public CostsListWindow()
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
            costsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            costsGrid.SelectedIndex = 0;
        }

        private string getQueryText()
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

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = costsGrid.SelectedIndex;
            int id = -1;
            int current_row = 0;
            foreach (DataRowView row in costsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id = (int)row.Row.ItemArray[0];
                break;
            }

            Window edit_window = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToDelete = new List<int>();
            foreach (DataRowView row in costsGrid.SelectedItems)
            {
                idsToDelete.Add((int)row.Row.ItemArray[0]);
            }

            DeleteFromDB(idsToDelete);

        }

        private void DeleteFromDB(List<int> ids)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
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
                    System.Windows.MessageBox.Show("Удаление " + id + " не удалось");
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
            List<int> idsToDelete = new List<int>();
            foreach (DataRowView row in costsGrid.SelectedItems)
            {
                idsToDelete.Add((int)row.Row.ItemArray[0]);
            }

            if (idsToDelete.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (idsToDelete.Count > 1)
                {
                    for (int i = 0; i < idsToDelete.Count - 1; i++)
                    {
                        edit_window = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToDelete[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToDelete[idsToDelete.Count - 1]);
                edit_window.Show();

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
            costsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("id", "Номер документа", describe.Where(key => key.Key == "id").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Document", "Название документа", describe.Where(key => key.Key == "Name_Of_Document").First().Value));
            result.Add(new FindHandler.FieldParameters("Default_Folder", "Путь документа", describe.Where(key => key.Key == "Default_Folder").First().Value));
            result.Add(new FindHandler.FieldParameters("Date_Of_Cost", "Дата расхода", describe.Where(key => key.Key == "Date_Of_Cost").First().Value));
            result.Add(new FindHandler.FieldParameters("Amount", "Сумма", describe.Where(key => key.Key == "Amount").First().Value));
            result.Add(new FindHandler.FieldParameters("Notes", "Заметки", describe.Where(key => key.Key == "Notes").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Category", "Категория расхода", describe.Where(key => key.Key == "Name_Of_Category").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Type", "Тип оплаты", describe.Where(key => key.Key == "Name_Of_Type").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Periodicity", "Периодичность", describe.Where(key => key.Key == "Name_Of_Periodicity").First().Value));
            result.Add(new FindHandler.FieldParameters("From", "От", describe.Where(key => key.Key == "To").First().Value));
            result.Add(new FindHandler.FieldParameters("To", "Кому", describe.Where(key => key.Key == "From").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe costs", connection, describe);
            DescribeHelper("describe consumption_categories", connection, describe);
            DescribeHelper("describe types_of_payment", connection, describe);
            DescribeHelper("describe periodicities", connection, describe);
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
            costsGrid.ItemsSource = dataTable.DefaultView;
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

        private void ButtonOpenDocument_Click(object sender, RoutedEventArgs e)
        {
            int row_index = costsGrid.SelectedIndex;
            int id = -1;
            string path = "";
            int current_row = 0;
            foreach (DataRowView row in costsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id = (int)row.Row.ItemArray[0];
                path = row.Row.ItemArray[2].ToString();
                break;
            }
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("Файл не найден");
            }
        }
    }
}
