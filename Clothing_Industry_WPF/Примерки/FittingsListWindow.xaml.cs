using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Clothing_Industry_WPF.Примерки
{
    /// <summary>
    /// Логика взаимодействия для FittingsListWindow.xaml
    /// </summary>
    public partial class FittingsListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public FittingsListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            fittingsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            fittingsGrid.SelectedIndex = 0;
        }

        private string getQueryText()
        {
            string query_text = "select customers.nickname as Customer, orders.id_order OrderId, types_of_fitting.Name_Of_type as Type_Of_Fitting, " +
                                    "fittings.date as Date, fittings.time as Time, fittings.notes as Notes" +
                                "from fittings " +
                                "join orders on fittings.orders_id_order = orders.id_order " +
                                "join customers on fittings.customers_id_customer = customers.id_customer " +
                                "join types_of_fitting on fittings.types_of_fitting_id_type_of_fitting = types_of_fitting.id_type_of_fitting; ";
            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = fittingsGrid.SelectedIndex;
            string nickname = "";
            int idOrder = -1;
            int current_row = 0;
            foreach (DataRowView row in fittingsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                nickname = row.Row.ItemArray[0].ToString();
                idOrder = int.Parse(row.Row.ItemArray[1].ToString());
                break;
            }

            Window create_window = new FittingsRecordWindow(WaysToOpenForm.WaysToOpen.edit, idOrder, nickname);
            create_window.ShowDialog();
            RefreshList();
        }
    }
}
