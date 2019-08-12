using Clothing_Industry_WPF.MainForms;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        private static bool isLocalHost = false;
        private static bool isServer = false;

        public AuthentificationForm()
        {
            InitializeComponent();

            textboxLogin.Focus();
            // Вызываем асинхронный методы, в которых будут одновременно проверятся доступность БД на сервере и на локальной машине
            CheckServerDB();
            CheckLocalHostDB();

            // Если у нас все недоступно, то выключаем прогу с сообщением
            CrashApplication();
        }

        // Ну в этих двух методах думаю все понятно
        private async void CheckServerDB()
        {
            isServer = await Task.Run(() => CheckServer(Properties.Settings.Default.server_ip)).ConfigureAwait(false);
        }

        private async void CheckLocalHostDB()
        {
            isLocalHost = await Task.Run(() => CheckServer("localhost")).ConfigureAwait(false);
        }

        private async void CrashApplication()
        {
            int count = 0;
            int maxCount = 10;
            bool edited = false;
            // Допустим максимальный таймаут - 10. можно будет прибавить если что
            while (count < maxCount)
            {
                await Task.Delay(1000).ConfigureAwait(true);
                if ((isServer || isLocalHost))
                {
                    this.Visibility = Visibility.Visible;
                    maxCount = count;
                    edited = true;
                }
                count++;
            }
            // Если уже поставленное время прошло и мы не меняли время вылета, значит все плохо, отрубаем приложение
            if (!edited)
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
            /*var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    int s = 1;
                    //return ip.ToString();
                }
            }*/

            string host = Dns.GetHostName();
            foreach (var ip in Dns.GetHostByName(host).AddressList)
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

            string connString = connectionString + "Server=" + (isLocalHost ? "localhost" : Properties.Settings.Default.server_ip)
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

            bool isAdministrator = CheckAdministrator(connection, username);
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

        private bool CheckAdministrator(MySqlConnection connection, string username)
        {
            string query = "select employee_positions.IsAdministrator " +
                           "from employee_positions " +
                           "join employees on employee_positions.id_Employee_Position = employees.Employee_Positions_id_Employee_Position " +
                           "where employees.login = @login";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@login", username);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Взамен ушедшим ролям сделал булево поле в должностях. Оно и решает, кто наш пользователь
                    return (reader.GetBoolean(0));
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
