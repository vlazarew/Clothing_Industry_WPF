using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Clothing_Industry_WPF.Сотрудники
{
    /// <summary>
    /// Логика взаимодействия для EmployeesListWindow.xaml
    /// </summary>
    public partial class EmployeesListWindow : Window
    {

        //private string connectionString;

        public EmployeesListWindow(string entry_connectionString)
        {
            InitializeComponent();
            //connectionString = entry_connectionString;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(Properties.Settings.Default.main_databaseConnectionString);
            string query_text = "select employees.Login, employees.Password, employees.Name, employees.Lastname, employees.Patronymic, employees.Phone_Number, employees.Passport_Data, employees.Email," +
                                "employees.Notes, employees.Added, employees.Last_Salary, employee_roles.Name_Of_Role, employee_positions.Name_Of_Position" +
                                " from employees" +
                                " join employee_positions on employees.Employee_Positions_id_Employee_Position = employee_positions.id_Employee_Position" +
                                " join employee_roles on employees.Employee_Roles_id_Employee_Role = employee_roles.id_Employee_Role;";
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            employeesGrid.ItemsSource = dataTable.DefaultView;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = employeesGrid.SelectedIndex;
            string login = "";
            int current_row = 0;
            foreach (DataRowView row in employeesGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                login = row.Row.ItemArray[0].ToString();
                break;
            }

            Window create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.edit, login);
            create_window.ShowDialog();
            RefreshList();
        }
    }
}
