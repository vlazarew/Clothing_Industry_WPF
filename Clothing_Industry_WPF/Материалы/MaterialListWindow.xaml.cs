using Clothing_Industry_WPF.Материалы;
using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace Clothing_Industry_WPF.Материал
{
    /// <summary>
    /// Логика взаимодействия для MaterialListWindow.xaml
    /// </summary>
    public partial class MaterialListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public MaterialListWindow()
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
            var dataTable = Material.getListMaterials(connection);
            materialsGrid.ItemsSource = dataTable.DefaultView;

            // Если ничего не выделено, то стиль заблокированной кнопки
            if (materialsGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string vendorCode = (string)((DataRowView)materialsGrid.SelectedItem).Row.ItemArray[0];

            Window edit_window = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.edit, vendorCode);
            edit_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<string> vendoCodesToDelete = new List<string>();
            foreach (DataRowView row in materialsGrid.SelectedItems)
            {
                vendoCodesToDelete.Add(row.Row.ItemArray[0].ToString());
            }

            DeleteFromDB(vendoCodesToDelete);
        }

        private void DeleteFromDB(List<string> vendorCodes)
        {
            Material.DeleteFromDB(vendorCodes, connection);
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<string> vendorCodesToEdit = new List<string>();
            foreach (DataRowView row in materialsGrid.SelectedItems)
            {
                vendorCodesToEdit.Add(row.Row.ItemArray[0].ToString());
            }

            if (vendorCodesToEdit.Count > 0)
            {
                Window editWindow;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (vendorCodesToEdit.Count > 1)
                {
                    for (int i = 0; i < vendorCodesToEdit.Count - 1; i++)
                    {
                        editWindow = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.edit, vendorCodesToEdit[i]);
                        editWindow.Show();
                    }
                }
                //Заключительная форма
                editWindow = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.edit, vendorCodesToEdit[vendorCodesToEdit.Count - 1]);
                editWindow.ShowDialog();

                //Обновление списка
                RefreshList();
            }
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = Material.FindListMaterials(currentFindDescription, connection);
            if (result.dataTable != null)
            {
                materialsGrid.ItemsSource = result.dataTable.DefaultView;
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
            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = Material.FilterListMaterials(currentFilterDescription, connection);
            if (result.dataTable != null)
            {
                materialsGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFilterDescription = result.filterDescription;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            // Если ничего не выделено, то стиль заблокированной кнопки
            if (materialsGrid.SelectedItems.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
    }
}
