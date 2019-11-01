using Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Clothing_Industry_WPF.Заказы
{
    /// <summary>
    /// Логика взаимодействия для OrderProductListWindow.xaml
    /// </summary>
    public partial class OrderProductListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private int orderId;
        private MySqlConnection connection;

        // Результат модального вызова формы
        public bool Result { get; set; }

        public OrderProductListWindow(int orderId)
        {
            InitializeComponent();
            connection = new MySqlConnection(connectionString);
            this.orderId = orderId;

            RefreshList();
        }

        private void RefreshList()
        {
            var dataTable = OrderProducts.getListOrderProducts(orderId, connection);
            listOfProductsGrid.ItemsSource = dataTable.DefaultView;

            // Если ничего не выделено, то стиль заблокированной кнопки
            if (listOfProductsGrid.SelectedItems.Count == 0)
            {
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexesToDelete = new List<int>();
            foreach (DataRowView row in listOfProductsGrid.SelectedItems)
            {
                indexesToDelete.Add((int)row.Row.ItemArray[0]);
            }

            DeleteFromDB(indexesToDelete);
        }

        private void DeleteFromDB(List<int> ids)
        {
            OrderProducts.DeleteFromDB(ids, orderId, connection);
            RefreshList();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Добавление изделия в заказ
        private void ButtonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            Window windowAdd = new OrderProductsRecordWindow(orderId);
            windowAdd.ShowDialog();
            RefreshList();
            Result = OrderProducts.CalcualteMaterials(orderId, connection);
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            // Если ничего не выделено, то стиль заблокированной кнопки
            if (listOfProductsGrid.SelectedItems.Count == 0)
            {
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

    }
}
