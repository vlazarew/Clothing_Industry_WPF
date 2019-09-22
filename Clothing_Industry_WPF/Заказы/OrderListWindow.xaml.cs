using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using Clothing_Industry_WPF.Примерки;
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
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;
using PrintDialog = System.Windows.Controls.PrintDialog;
using System.Windows.Forms;

namespace Clothing_Industry_WPF.Заказы
{
    /// <summary>
    /// Логика взаимодействия для OrderListWindow.xaml
    /// </summary>
    public partial class OrderListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;
        private MySqlConnection connection;

        public OrderListWindow()
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
            var dataTable = Order.getListOrders(connection);
            ordersGrid.ItemsSource = dataTable.DefaultView;

            // Если ничего не выделено, то стиль заблокированной кнопки
            if (ordersGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (System.Windows.Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (System.Windows.Style)ButtonDelete.FindResource("NoActive");
                ButtonListProducts.Style = (System.Windows.Style)ButtonListProducts.FindResource("NoActive");
            }
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int id = (int)((DataRowView)ordersGrid.SelectedItem).Row.ItemArray[0];

            Window editWindow = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
            editWindow.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToDelete = new List<int>();
            foreach (DataRowView row in ordersGrid.SelectedItems)
            {
                idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
            }

            DeleteFromDB(idsToDelete);
        }

        private void DeleteFromDB(List<int> ids)
        {
            Order.DeleteFromDB(ids, connection);
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToEdit = new List<int>();
            foreach (DataRowView row in ordersGrid.SelectedItems)
            {
                idsToEdit.Add(int.Parse(row.Row.ItemArray[0].ToString()));
            }

            if (idsToEdit.Count > 0)
            {
                Window editWindow;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (idsToEdit.Count > 1)
                {
                    for (int i = 0; i < idsToEdit.Count - 1; i++)
                    {
                        editWindow = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[i]);
                        editWindow.Show();
                    }
                }
                //Заключительная форма
                editWindow = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[idsToEdit.Count - 1]);
                editWindow.ShowDialog();

                //Обновление списка
                RefreshList();
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = Order.FindListOrders(currentFindDescription, connection);
            if (result.dataTable != null)
            {
                ordersGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFindDescription = result.findDescription;

            buttonCancelFind.Style = (System.Windows.Style)buttonCancelFind.FindResource("Active");
        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {
            buttonCancelFind.Style = (System.Windows.Style)buttonCancelFind.FindResource("NoActive");

            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();
        }

        private void ButtonFilters_Click(object sender, RoutedEventArgs e)
        {
            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = Order.FilterListOrders(currentFilterDescription, connection);
            if (result.dataTable != null)
            {
                ordersGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFilterDescription = result.filterDescription;
        }

        private void ButtonListProducts_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((DataRowView)ordersGrid.SelectedItem).Row.ItemArray[0];

            Window listProduct = new OrderProductListWindow(id);
            listProduct.ShowDialog();
            RefreshList();
        }

        private void ButtonAddFitting_Click(object sender, RoutedEventArgs e)
        {
            Window createWindow = new FittingRecordWindow(WaysToOpenForm.WaysToOpen.create);
            createWindow.ShowDialog();
        }

        // Раскраска по статусу заказа
        private void OrdersGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            string status = ((DataRowView)e.Row.DataContext).Row.ItemArray[9].ToString();
            switch (status)
            {
                case "Принят":
                    e.Row.Background = Brushes.White;
                    break;
                case "Сдан":
                    e.Row.Background = Brushes.Green;
                    break;
                case "Отправлен":
                    e.Row.Background = Brushes.Orange;
                    break;
                case "Готов":
                    e.Row.Background = Brushes.Aqua;
                    break;
                case "Отменён":
                    e.Row.Background = Brushes.Red;
                    break;
                default:
                    break;
            }
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            string idToPrint = "";
            foreach (DataRowView row in ordersGrid.SelectedItems)
            {
                idToPrint += row.Row.ItemArray[0].ToString() + ", ";
            }
            idToPrint = idToPrint.Substring(0, idToPrint.Length - 2);

            Order.PrintOrders(idToPrint, connection);
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonDelete.Style = (System.Windows.Style)ButtonDelete.FindResource("Active");
            ButtonEdit.Style = (System.Windows.Style)ButtonEdit.FindResource("Active");
            ButtonListProducts.Style = (System.Windows.Style)ButtonListProducts.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            // Если ничего не выделено, то стиль заблокированной кнопки
            if (ordersGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (System.Windows.Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (System.Windows.Style)ButtonDelete.FindResource("NoActive");
                ButtonListProducts.Style = (System.Windows.Style)ButtonListProducts.FindResource("NoActive");
            }
        }
    }
}
