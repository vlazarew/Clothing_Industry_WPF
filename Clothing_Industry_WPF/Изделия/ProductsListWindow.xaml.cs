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

namespace Clothing_Industry_WPF.Изделия
{
    /// <summary>
    /// Логика взаимодействия для ProductsListWindow.xaml
    /// </summary>
    public partial class ProductsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public ProductsListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
        }

        private string getQueryText()
        {
            string query_text = "select products.id_product, products.Name_Of_Product, products.Fixed_Price, products.MoneyToEmployee," +
                                " products.Description" +
                                " from products;";
            return query_text;
        }


        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new ProductsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> id_ProductsToDelete = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                id_ProductsToDelete.Add((int)row.Row.ItemArray[0]);
            }

            DeleteFromDB(id_ProductsToDelete);
        }

        private void DeleteFromDB(List<int> id_Products)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (int id_Product in id_Products)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from products where id_Product = @id_Product";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id_Product", id_Product);
               
                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление продукта " + id_Product + " не удалось");
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
            List<int> id_ProductsToDelete = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                id_ProductsToDelete.Add((int)row.Row.ItemArray[0]);
            }

            if (id_ProductsToDelete.Count > 0)
            {
                Window create_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (id_ProductsToDelete.Count > 1)
                {
                    for (int i = 0; i < id_ProductsToDelete.Count - 1; i++)
                    {
                        create_window = new ProductsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id_ProductsToDelete[i]);
                        create_window.Show();
                    }
                }
                //Заключительная форма
                create_window = new ProductsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id_ProductsToDelete[id_ProductsToDelete.Count - 1]);
                create_window.ShowDialog();

                //Обновление списка
                RefreshList();
            }
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
            productsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Name_Of_Product", "Название", describe.Where(key => key.Key == "Name_Of_Product").First().Value));
            result.Add(new FindHandler.FieldParameters("Fixed_Price", "Цена", describe.Where(key => key.Key == "Fixed_Price").First().Value));
            result.Add(new FindHandler.FieldParameters("Per_Cents", "Выплата сотруднику", describe.Where(key => key.Key == "MoneyToEmployee").First().Value));
            result.Add(new FindHandler.FieldParameters("Description", "Описание", describe.Where(key => key.Key == "Description").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe products", connection, describe);
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
            productsGrid.ItemsSource = dataTable.DefaultView;
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

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {
            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            //Думаю, что не очень хорошо, когда в списке пользователей виден пароль!!! /* employees.Password, */
            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            productsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            productsGrid.SelectedIndex = 0;
            List<int> ids = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                ids.Add((int)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonOpenList.Style = (Style)ButtonOpenList.FindResource("NoActive");
            }
        }


        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = productsGrid.SelectedIndex;
            int id_Product = -1;
            int current_row = 0;
            foreach (DataRowView row in productsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id_Product = (int)row.Row.ItemArray[0];
                break;
            }

            Window create_window = new ProductsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id_Product);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonOpenList_Click(object sender, RoutedEventArgs e)
        {
            int row_index = productsGrid.SelectedIndex;
            int id_Product = -1;
            int current_row = 0;
            foreach (DataRowView row in productsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id_Product = (int)row.Row.ItemArray[0];
                break;
            }
            Window listProduct = new MaterialsForProductsList(id_Product);
            listProduct.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonOpenList.Style = (Style)ButtonOpenList.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            List<int> ids = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                ids.Add((int)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonOpenList.Style = (Style)ButtonOpenList.FindResource("NoActive");
            }
        }
    }

        
}
