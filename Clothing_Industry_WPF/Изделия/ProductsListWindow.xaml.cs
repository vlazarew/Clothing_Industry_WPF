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
            string query_text = "select products.Name_Of_Product, table_materials_to_product.Count, products.Fixed_Price, products.Per_Cents," +
                                "products.Added_Price_For_Complexity, products.Description" +
                                " from employees" +
                                " join employee_positions on employees.Employee_Positions_id_Employee_Position = employee_positions.id_Employee_Position" +
                                " join employee_roles on employees.Employee_Roles_id_Employee_Role = employee_roles.id_Employee_Role;";
            return query_text;
        }


        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            //Window create_window = new ProductsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            //create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<string> id_ProductsToDelete = new List<string>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                id_ProductsToDelete.Add(row.Row.ItemArray[0].ToString());
            }

            DeleteFromDB(id_ProductsToDelete);
        }

        private void DeleteFromDB(List<string> id_Products)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (string id_Product in id_Products)
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

        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {

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
        }

        // Список полей, по которым мы можем делать поиск
        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("Login", "Логин", describe.Where(key => key.Key == "Login").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Role", "Роль", describe.Where(key => key.Key == "Name_Of_Role").First().Value));
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

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe employees", connection, describe);
            DescribeHelper("describe employee_roles", connection, describe);
            DescribeHelper("describe employee_positions", connection, describe);
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

        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {

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
        }


        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = productsGrid.SelectedIndex;
            string id_Product = "";
            int current_row = 0;
            foreach (DataRowView row in productsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id_Product = row.Row.ItemArray[0].ToString();
                break;
            }

            //Window create_window = new ProductsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            //create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonOpenList_Click(object sender, RoutedEventArgs e)
        {

        }
    }

        
}
