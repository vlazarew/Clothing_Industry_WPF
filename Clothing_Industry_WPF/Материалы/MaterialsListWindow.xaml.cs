using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace Clothing_Industry_WPF.Материал
{
    /// <summary>
    /// Логика взаимодействия для MaterialsListWindow.xaml
    /// </summary>
    public partial class MaterialsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public MaterialsListWindow()
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
            materialsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private string getQueryText()
        {
            string query_text = "select materials.Vendor_Code, materials.Name_Of_Material, materials.Cost_Of_Material, materials.Notes, units.Name_Of_Unit,groups_of_material.Name_Of_Group,types_of_material.Name_Of_Type,countries.Name_Of_Country" +
                                " from materials" +
                                " join units on materials.Units_id_Unit = units.id_Unit" +
                                " join groups_of_material on materials.Groups_Of_Material_id_Group_Of_Material = groups_of_material.id_Group_Of_Material" +
                                " join types_of_material on materials.Types_Of_Material_id_Type_Of_Material = types_of_material.id_Type_Of_Material" +
                                " join countries on materials.Countries_id_Country = countries.id_Country;";
            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new MaterialsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = materialsGrid.SelectedIndex;
            string vendor_code = "";
            int current_row = 0;
            foreach (DataRowView row in materialsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                vendor_code = row.Row.ItemArray[0].ToString();
                break;
            }

            Window create_window = new MaterialsRecordWindow(WaysToOpenForm.WaysToOpen.edit, vendor_code);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            
            List<string> vendor_codesToDelete = new List<string>();
            foreach (DataRowView row in materialsGrid.SelectedItems)
            {
                vendor_codesToDelete.Add(row.Row.ItemArray[0].ToString());
            }
            
            DeleteFromDB(vendor_codesToDelete);

        }

        private void DeleteFromDB(List<string> vendor_codes)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (string vendor_code in vendor_codes)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from materials where vendor_code = @Vendor_Code";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@Vendor_Code", vendor_code);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление материала " + vendor_code + " не удалось");
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
            List<string> vendor_codesToDelete = new List<string>();
            foreach (DataRowView row in materialsGrid.SelectedItems)
            {
                vendor_codesToDelete.Add(row.Row.ItemArray[0].ToString());
            }

            if (vendor_codesToDelete.Count > 0)
            {
                Window create_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (vendor_codesToDelete.Count > 1)
                {
                    for (int i = 0; i < vendor_codesToDelete.Count - 1; i++)
                    {
                        create_window = new MaterialsRecordWindow(WaysToOpenForm.WaysToOpen.edit, vendor_codesToDelete[i]);
                        create_window.Show();
                    }
                }
                //Заключительная форма
                create_window = new MaterialsRecordWindow(WaysToOpenForm.WaysToOpen.edit, vendor_codesToDelete[vendor_codesToDelete.Count - 1]);
                create_window.ShowDialog();

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
            edited_query = query.Replace(";", " where " + field + " ");
            edited_query += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(edited_query, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            materialsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
        }

        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Vendor_Code", "Артикул", describe.Where(key => key.Key == "Vendor_Code").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Material", "Название", describe.Where(key => key.Key == "Name_Of_Material").First().Value));
            result.Add(new FindHandler.FieldParameters("Cost_Of_Material", "Стоимость", describe.Where(key => key.Key == "Cost_Of_Material").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Unit", "Система измерения", describe.Where(key => key.Key == "Name_Of_Unit").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Group", "Вид", describe.Where(key => key.Key == "Name_Of_Group").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Type", "Тип", describe.Where(key => key.Key == "Name_Of_Type").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Country", "Страна производитель", describe.Where(key => key.Key == "Name_Of_Country").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe materials", connection, describe);
            DescribeHelper("describe units", connection, describe);
            DescribeHelper("describe groups_of_material", connection, describe);
            DescribeHelper("describe types_of_material", connection, describe);
            DescribeHelper("describe countries", connection, describe);
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
            materialsGrid.ItemsSource = dataTable.DefaultView;
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
                        result += " or ";
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
