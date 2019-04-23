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

        public AuthentificationForm()
        {
            InitializeComponent();
            textboxLogin.Focus();
        }


        private bool CheckServer(string server_ip)
        {
            string connString = "Server=localhost;database=main_database;characterset=utf8;Database=main_database;port="
                + 3306 + ";user id=test_server;password=123";
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

        private void Button_LogIn_Click(object sender, RoutedEventArgs e)
        {
            bool check = IsLocalhost();
            if (!CheckServer(Properties.Settings.Default.server_ip))
            {
                MessageBox.Show(this, "Нет подключения к серверу. Вероятно последний находится в выключенном состоянии",
                    "Ошибка соединения с сервером", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            username = textboxLogin.Text;
            password = textboxPassword.Text;

            string connString = "Server=localhost;database=main_database;characterset=utf8;Database=main_database;port="
               + 3306 + ";user id=" + username + ";password=" + password;

            MySqlConnection connection = new MySqlConnection(connString);
            try
            {
                connection.Open();
                Properties.Settings.Default.main_databaseConnectionString = connString;
                Window mainWindow = new MainWindow();
                Hide();
                mainWindow.Show();
            }
            catch
            {
                MessageBox.Show("Неверные логин или пароль!");
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
