using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

            Window mainWindow = new MainWindow(username);
            Close();
            mainWindow.Show();
            connection.Close();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
