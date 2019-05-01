using Clothing_Industry_WPF.Сотрудники;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string login;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;

        #region Загрузка формы
        public MainWindow(string entry_login = "")
        {
            InitializeComponent();
            login = entry_login;
            FillUsername();
        }

        // Заполнить ФИО в формочек
        private void FillUsername()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            string query_text = "select Name, Lastname, Patronymic from employees where login = @login";
            connection.Open();

            MySqlCommand query = new MySqlCommand(query_text, connection);
            query.Parameters.AddWithValue("@login", login);

            using (DbDataReader reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    string user_name = reader.GetString(0);
                    string user_lastname = reader.GetString(1);
                    string user_patronymic = reader.GetString(2);

                    if (user_name != "" && user_lastname != "")
                    {
                        textBlockUserName.Text = user_lastname + " " + user_name + " " + user_patronymic;
                        return;
                    }
                }
            }
            textBlockUserName.Text = "Гость";
        }
        #endregion

        // При нажатии на вкладку "Сотрудники"
        private void Workers_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_workers = new EmployeesListWindow();
            window_workers.Show();
        }

        // ОТКЛЮЧЕНИЕ СИСТЕМЫ
        #region Отключение системы
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
