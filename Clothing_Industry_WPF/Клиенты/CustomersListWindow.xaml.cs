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

namespace Clothing_Industry_WPF.Клиенты
{
    /// <summary>
    /// Логика взаимодействия для CustomersListWindow.xaml
    /// </summary>
    public partial class CustomersListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;
        private MySqlConnection connection;

        public CustomersListWindow()
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
            var dataTable = Customer.getListCustomers(connection);
            customersGrid.ItemsSource = dataTable.DefaultView;

            // Если ничего не выделено, то стиль заблокированной кнопки
            if (customersGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int id = (int)((DataRowView)customersGrid.SelectedItem).Row.ItemArray[0];

            Window create_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<(int id, string firstname, string lastname)> dataToDelete = new List<(int id, string firstname, string lastname)>();
            foreach (DataRowView row in customersGrid.SelectedItems)
            {
                dataToDelete.Add((id: (int)row.Row.ItemArray[0], firstname: row.Row.ItemArray[1].ToString(), lastname: row.Row.ItemArray[2].ToString()));
            }

            DeleteFromDB(dataToDelete);
        }

        private void DeleteFromDB(List<(int id, string firstname, string lastname)> dataToDelete)
        {
            Customer.DeleteFromDB(dataToDelete, connection);
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToEdit = new List<int>();
            foreach (DataRowView row in customersGrid.SelectedItems)
            {
                idsToEdit.Add((int)row.Row.ItemArray[0]);
            }

            if (idsToEdit.Count > 0)
            {
                Window edit_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (idsToEdit.Count > 1)
                {
                    for (int i = 0; i < idsToEdit.Count - 1; i++)
                    {
                        edit_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[i]);
                        edit_window.Show();
                    }
                }
                //Заключительная форма
                edit_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToEdit[idsToEdit.Count - 1]);
                edit_window.ShowDialog();

                //Обновление списка
                RefreshList();
            }
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = Customer.FindListCustomers(currentFindDescription, connection);
            if (result.dataTable != null)
            {
                customersGrid.ItemsSource = result.dataTable.DefaultView;
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
            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = Customer.FilterListCustomers(currentFilterDescription, connection);
            if (result.dataTable != null)
            {
                customersGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFilterDescription = result.filterDescription;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            // Если ничего не выделено, то стиль заблокированной кнопки
            if (customersGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
    }
}
