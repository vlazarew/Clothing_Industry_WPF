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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Приход_материала
{
    /// <summary>
    /// Логика взаимодействия для ReceiptsOfMaterials.xaml
    /// </summary>
    public partial class ReceiptsOfMaterialsWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public ReceiptsOfMaterialsWindow()
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
            receiptsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string getQueryText()
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

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new ReceiptsRecordDocumentWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = receiptsGrid.SelectedIndex;
            int id_Document_Of_Receipt = -1;
            int current_row = 0;
            foreach (DataRowView row in receiptsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id_Document_Of_Receipt = (int)row.Row.ItemArray[0];
                break;
            }

            Window edit_window = new ReceiptsRecordDocumentWindow(WaysToOpenForm.WaysToOpen.edit, id_Document_Of_Receipt);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<string> id_Document_Of_ReceiptToDelete = new List<string>();
            foreach (DataRowView row in receiptsGrid.SelectedItems)
            {
                id_Document_Of_ReceiptToDelete.Add(row.Row.ItemArray[0].ToString());
            }

            DeleteFromDB(id_Document_Of_ReceiptToDelete);

        }

        private void DeleteFromDB(List<string> id_Document_Of_Receipts)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (string id_Document_Of_Receipt in id_Document_Of_Receipts)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from receipt_of_materials where id_Document_Of_Receipt = @id_Document_Of_Receipt";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id_Document_Of_Receipt", id_Document_Of_Receipt);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление документа " + id_Document_Of_Receipt + " не удалось");
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
            List<int> id_Document_Of_Receipts = new List<int>();
            foreach (DataRowView row in receiptsGrid.SelectedItems)
            {
                id_Document_Of_Receipts.Add((int)row.Row.ItemArray[0]);
            }

            if (id_Document_Of_Receipts.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (id_Document_Of_Receipts.Count > 1)
                {
                    for (int i = 0; i < id_Document_Of_Receipts.Count - 1; i++)
                    {
                        edit_window = new ReceiptsRecordDocumentWindow(WaysToOpenForm.WaysToOpen.edit, id_Document_Of_Receipts[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new ReceiptsRecordDocumentWindow(WaysToOpenForm.WaysToOpen.edit, id_Document_Of_Receipts[id_Document_Of_Receipts.Count - 1]);
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
            receiptsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
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

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe receipt_of_materials", connection, describe);
            DescribeHelper("describe payment_states", connection, describe);
            DescribeHelper("describe type_of_transactions", connection, describe);
            DescribeHelper("describe suppliers", connection, describe);
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
            receiptsGrid.ItemsSource = dataTable.DefaultView;
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

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            int row_index = receiptsGrid.SelectedIndex;
            int id_Document_Of_Receipts = -1;
            int current_row = 0;
            foreach (DataRowView row in receiptsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id_Document_Of_Receipts = (int)row.Row.ItemArray[0];
                break;
            }
            Window receiptsmaterial = new ReceiptsRecordWindow(id_Document_Of_Receipts);
            receiptsmaterial.ShowDialog();
            RefreshList();
        }
    }
}
