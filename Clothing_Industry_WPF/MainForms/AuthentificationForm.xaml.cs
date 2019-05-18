using Clothing_Industry_WPF.MainForms;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AuthentificationForm.xaml
    /// </summary>
    public partial class AuthentificationForm : Window
    {
        private string username;
        private string password;
        private string connectionString = "database=main_database;characterset=utf8;Database=main_database;port=" + 3306 + ";";
        private bool localhost;

        public AuthentificationForm()
        {
            InitializeComponent();
            textboxLogin.Focus();

            localhost = IsLocalhost();

            if (!CheckServer(localhost ? "localhost" : Properties.Settings.Default.server_ip))
            {
                MessageBox.Show(this, "Нет подключения к серверу. Вероятно последний находится в выключенном состоянии",
                    "Ошибка соединения с сервером", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

        }

        private bool CheckServer(string server_ip)
        {
            string connString = connectionString + "Server=" + server_ip + ";user id=test_server;password=123";
            MySqlConnection connection = new MySqlConnection(connString);

            try
            {
                connection.Open();
                connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Проверяем, с какой машины мы заходим в сеть (localhost не сможет к себе обращаться, как остальные машины в локальной системе)
        [Obsolete]
        private bool IsLocalhost()
        {
            string host = System.Net.Dns.GetHostName();
            foreach (var ip in System.Net.Dns.GetHostByName(host).AddressList)
            {
                if (ip.ToString() == Properties.Settings.Default.server_ip)
                {
                    return true;
                }
            }
            return false;
        }

        [Obsolete]
        private void Button_LogIn_Click(object sender, RoutedEventArgs e)
        {
            username = textboxLogin.Text;
            password = PasswordBoxPassword.Password;

            string connString = connectionString + "Server=" + (localhost ? "localhost" : Properties.Settings.Default.server_ip)
                    + ";user id=" + username + ";password=" + password; ;

            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                Properties.Settings.Default.main_databaseConnectionString = connString;
            }
            catch
            {
                MessageBox.Show("Неверные логин или пароль!");
                PasswordBoxPassword.Clear();
                return;
            }

            UpdateSalaryTable(connection);

            bool isAdministrator = CheckRole(connection, username);
            Window mainWindow;
            // Тернарный оператор тут бессилен, разные типы говорит
            if (isAdministrator)
            {
                mainWindow = new MainWindow(username);
            }
            else
            {
                mainWindow = new MainWindowForUser(username);
            }

            Close();
            mainWindow.Show();
            connection.Close();
        }

        // Обновляем(добавляем) строчки в Начислениях ЗП
        private void UpdateSalaryTable(MySqlConnection connection)
        {
            // Посмотреть последний период зп
            string querySelect = "select period " +
                                 "from payrolls " +
                                 "order by period desc " +
                                 "limit 1 ;";

            string periodNow = DateTime.Now.Month + "." + DateTime.Now.Year;
            MySqlCommand commandSelect = new MySqlCommand(querySelect, connection);

            string lastPeriod = "";
            using (DbDataReader reader = commandSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    lastPeriod = reader.GetString(0);
                }
            }

            if (lastPeriod != periodNow)
            {
                List<KeyValuePair<string, float>> listSalary = new List<KeyValuePair<string, float>>();
                string queryLoginSalary = "select * " +
                                          "from (select Login, Salary " +
                                          "from employees " +
                                          "join payrolls on employees.Login = payrolls.Employees_Login " +
                                          "order by period desc " +
                                          "limit 18446744073709551615) as testTable " +
                                          "group by login ;";
                MySqlCommand commandLoginSalary = new MySqlCommand(queryLoginSalary, connection);

                using (DbDataReader reader = commandLoginSalary.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listSalary.Add(new KeyValuePair<string, float>(reader.GetString(0), float.Parse(reader.GetString(1))));
                    }
                }

                foreach (var record in listSalary)
                {
                    MySqlTransaction transaction = connection.BeginTransaction();
                    string queryInsert = "insert into payrolls " +
                                         "(Employees_Login, Period, Date_Of_Pay, Salary, PieceWorkPayment, Total_Salary, Penalty, To_Pay, Notes, PaidOff)" +
                                         " VALUES (@login, @period, @dateOfPay, @salary, 0, @salary, 0, @salary, '', false) ";

                    MySqlCommand commandInsert = new MySqlCommand(queryInsert, connection, transaction);
                    commandInsert.Parameters.AddWithValue("@login", record.Key);
                    commandInsert.Parameters.AddWithValue("@period", periodNow);
                    commandInsert.Parameters.AddWithValue("@dateOfPay", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15));
                    commandInsert.Parameters.AddWithValue("@salary", record.Value);

                    try
                    {
                        commandInsert.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

        }

        private bool CheckRole(MySqlConnection connection, string username)
        {
            string query = "select employee_roles.name_of_role from employee_roles " +
                           "join employees on employee_roles.id_Employee_role = employee_roles_id_Employee_role " +
                           "where employees.login = @login";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@login", username);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Если в названии роли у нас встречается слово "Администратор", значит у него есть доступ ко всему
                    return (reader.GetString(0).IndexOf("Администратор") != -1);
                }
            }

            return false;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
