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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Баланс_клиентов
{
    /// <summary>
    /// Логика взаимодействия для CustomerBalanceListWindow.xaml
    /// </summary>
    public partial class CustomerBalanceListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;
        private MySqlConnection connection;

        public CustomerBalanceListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
            connection = new MySqlConnection(connectionString);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            var dataTable = CustomerBalance.getListCustomerBalance(connection);
            customerBalanceGrid.ItemsSource = dataTable.DefaultView;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = CustomerBalance.FindListCustomerBalance(currentFindDescription, connection);
            if (result.dataTable != null)
            {
                customerBalanceGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFindDescription = result.findDescription;

            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");

            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();           
        }

        private void ButtonFilters_Click(object sender, RoutedEventArgs e)
        {
            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = CustomerBalance.FilterListCustomerBalance(currentFilterDescription, connection);
            if (result.dataTable != null)
            {
                customerBalanceGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFilterDescription = result.filterDescription;
        }
    }
}
