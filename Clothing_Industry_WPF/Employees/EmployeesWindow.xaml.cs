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

namespace Clothing_Industry_WPF
{
    /// <summary>
    /// Логика взаимодействия для EmployeesWindow.xaml
    /// </summary>
    public partial class EmployeesWindow : Window
    {
        private string connectionString;

        public EmployeesWindow(string entry_connectionString)
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
            //MySqlCommand query = new MySqlCommand(query_text, connection);
            //MySqlDataAdapter MyData = new MySqlDataAdapter();
            //MyData.SelectCommand = new MySqlCommand(query_text, connection);
           // MyData.Fill(dataTable);
            //DataGridEmployees.ItemsSource = dataTable.DefaultView;
            //DataGridEmployees.



            /* using (DbDataReader reader = query.ExecuteReader())
             {
                 while (reader.Read())
                 {
                     string login = reader.GetString(0);
                     string password = reader.GetString(1);
                     string name = reader.GetString(2);
                     string lastname = reader.GetString(3);
                     string patronymic = reader.GetString(4);
                     string phone_number = reader.GetString(2);
                     string passport_data = reader.GetString(2);
                     string email = reader.GetString(2);
                     string notes = reader.GetString(2);
                     string added = reader.GetString(2);
                     string last_salary = reader.GetString(2);
                     string name_of_role = reader.GetString(2);
                     string name_of_position = reader.GetString(2);

                     /*if (user_name != "" && user_lastname != "")
                     {
                         textBlockUserName.Text = user_lastname + " " + user_name + " " + user_patronymic;
                         return;
                     }*/
            //     }
            //}
            //query.Parameters.AddWithValue("@login", login);
        }
    }
}
