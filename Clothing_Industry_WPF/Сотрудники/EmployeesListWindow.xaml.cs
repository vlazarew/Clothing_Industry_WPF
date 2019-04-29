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

        private string connectionString;

        public EmployeesListWindow(string entry_connectionString)
        {
            InitializeComponent();
            connectionString = entry_connectionString;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
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
            //Window create_window = new EmployeesRecordWindow();
            Window create_window = new EmployeesRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.Show();
        }
    }
}
