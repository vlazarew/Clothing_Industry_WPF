using Clothing_Industry_WPF.Баланс_клиентов;
using Clothing_Industry_WPF.Заказы;
using Clothing_Industry_WPF.Изделия;
using Clothing_Industry_WPF.Клиенты;
using Clothing_Industry_WPF.Материал;
using Clothing_Industry_WPF.Начисление_ЗП;
using Clothing_Industry_WPF.Примерки;
//using Clothing_Industry_WPF.Примерки;
using Clothing_Industry_WPF.Приход_материала;
using Clothing_Industry_WPF.Состояние_склада;
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

        // При нажатии на вкладку "Материалы"
        private void Materials_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_materials = new MaterialsListWindow();
            window_materials.Show();
        }

        // При нажатии на вкладку "Состояние склада"
        private void Store_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_store = new StoreListWindow();
            window_store.Show();
        }

        // При нажатии на вкладку "Клиенты"
        private void Clients_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_customers = new CustomersListWindow();
            window_customers.Show();
        }

        // При нажатии на вкладку "Баланс клиентов"
        private void BalanceOfClients_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_balance = new CustomerBalanceListWindow();
            window_balance.Show();
        }

        // При нажатии на вкладку "Заказы"
        private void Orders_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_orders = new OrdersListWindow();
            window_orders.Show();
        }

        // При нажатии на вкладку "Приход материалов"
        private void Receipt_Materials_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_receipt = new ReceiptsOfMaterialsWindow();
            window_receipt.Show();
        }

        // При нажатии на вкладку "Изделия"
        private void Products_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_products = new ProductsListWindow();
            window_products.Show();
        }

        // При нажатии на вкладку "Примерки"
        private void Fittings_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           Window window_fittings = new FittingsListWindow();
           window_fittings.Show();
        }

        // При нажатии на вкладку "Начисление ЗП"
        private void MoneyForWorkers_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window window_payrolls = new PayrollsListWindow();
            window_payrolls.Show();
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
