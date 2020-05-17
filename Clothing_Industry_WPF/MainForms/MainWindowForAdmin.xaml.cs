using Clothing_Industry_WPF.Баланс_клиентов;
using Clothing_Industry_WPF.Доходы;
using Clothing_Industry_WPF.Заказы;
using Clothing_Industry_WPF.Изделия;
using Clothing_Industry_WPF.Клиенты;
using Clothing_Industry_WPF.Материал;
using Clothing_Industry_WPF.Начисление_ЗП;
using Clothing_Industry_WPF.Отпуска;
using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using Clothing_Industry_WPF.Примерки;
using Clothing_Industry_WPF.Приход_материалов;
using Clothing_Industry_WPF.Расходы;
using Clothing_Industry_WPF.Состояние_склада;
using Clothing_Industry_WPF.Сотрудники;
using Clothing_Industry_WPF.Справочник.Должности;
using Clothing_Industry_WPF.Справочник.Единицы_измерения;
using Clothing_Industry_WPF.Справочник.Каналы_заказов;
using Clothing_Industry_WPF.Справочник.Категории_расходов;
using Clothing_Industry_WPF.Справочник.Периодичности;
using Clothing_Industry_WPF.Справочник.Поставщики;
using Clothing_Industry_WPF.Справочник.Статусы_клиентов;
using Clothing_Industry_WPF.Справочник.Статусы_оплаты;
using Clothing_Industry_WPF.Справочник.Страны;
using Clothing_Industry_WPF.Справочник.Типы_заказов;
using Clothing_Industry_WPF.Справочник.Типы_материалов;
using Clothing_Industry_WPF.Справочник.Типы_оплаты;
using Clothing_Industry_WPF.Справочник.Типы_примерок;
using Clothing_Industry_WPF.Справочник.Типы_транзакций;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static Clothing_Industry_WPF.Доходы.IncomeRecordWindow;

namespace Clothing_Industry_WPF.MainForms
{
    /// <summary>
    /// Логика взаимодействия для WindowExperimental.xaml
    /// </summary>
    public partial class WindowExperimental : Window
    {
        private string login;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        public static RoutedCommand RoutedCommands = new RoutedCommand();
        private string queryrefresh;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;
        private MySqlConnection connection;

        public WindowExperimental(string entry_login = "")
        {
            InitializeComponent();
            login = entry_login;
            FillUsername();
            currentFindDescription = new FindHandler.FindDescription();
            connection = new MySqlConnection(connectionString);
            currentFilterDescription = new List<FilterHandler.FilterDescription>();

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
                        Title += user_lastname + " " + user_name + " " + user_patronymic;
                        return;
                    }
                }
            }
            Title += "Гость";
        }
        private int CountForms()
        {
            int count = 0;
            foreach (Window w in App.Current.Windows)
                count++;
            return count;

        }

        //ДОБАВЛЕНИЕ СТОЛБЦА В НУЖНУЮ ТАБЛИЦУ
        private void AddColumnDataGrid(DataGrid datagrid, string header, string bind)
        {
            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = header;
            textColumn.Binding = new Binding(bind);
            datagrid.Columns.Add(textColumn);
        }
        //СБРОС МЕНЮ+ТАБЛИЦ
        private void ClearForm()
        {
            ClearIncome.Visibility = Visibility.Collapsed;
            DirtyIncome.Visibility = Visibility.Collapsed;
            Print.Visibility = Visibility.Collapsed;
            RefreshTable.Visibility = Visibility.Collapsed;
            SearchItem.Visibility = Visibility.Collapsed;
            StopSearchItem.Visibility = Visibility.Collapsed;
            UseFilter.Visibility = Visibility.Collapsed;
            DataGrid.ItemsSource = null;
            DataGrid.Columns.Clear();
            OneGrid.Visibility = Visibility.Collapsed;

            DataGrid1.ItemsSource = null;
            DataGrid1.Columns.Clear();

            DataGrid2.ItemsSource = null;
            DataGrid2.Columns.Clear();
            TwoGrids.Visibility = Visibility.Collapsed;
        }

        //ОБНОВЛЕНИЕ ТАБЛИЦЫ ПО ЗАПРОСУ
        private void RefreshList(string query_text, DataGrid d)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            d.ItemsSource = dataTable.DefaultView;
            connection.Close();
            d.SelectedIndex = 0;
        }

        //  ПАНЕЛЬ РАЗДЕЛОВ     
        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearForm();
            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                
                case "ItemIncome":

                    ClearIncome.Visibility = Visibility.Visible;
                    DirtyIncome.Visibility = Visibility.Visible;
                    Print.Visibility = Visibility.Visible;
                    TwoGrids.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid1, "ID", "id");
                    AddColumnDataGrid(DataGrid1, "Сумма", "Count");
                    AddColumnDataGrid(DataGrid2, "ID", "id");
                    AddColumnDataGrid(DataGrid2, "Сумма", "Count");

                    break;

                case "ItemBalance":
                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;
                    
                    AddColumnDataGrid(DataGrid, "Фамилия", "Lastname");
                    AddColumnDataGrid(DataGrid, "Имя", "Name");
                    AddColumnDataGrid(DataGrid, "Начислено на сумму", "Accured");
                    AddColumnDataGrid(DataGrid, "Оплачено заказчиком", "Paid");
                    AddColumnDataGrid(DataGrid, "Долг заказчика", "Debt");
                    AddColumnDataGrid(DataGrid, "Телефонный номер", "Phone_Number");
                    AddColumnDataGrid(DataGrid, "Паспорт", "Passport_data");
                    AddColumnDataGrid(DataGrid, "Никнейм", "Nickname");
                    AddColumnDataGrid(DataGrid, "Адрес", "Address");
                    queryrefresh = CustomerBalance.getQueryText();
                    RefreshList(queryrefresh, DataGrid);
                    break;

                default:
                    break;
            }
        }
        //  ПАНЕЛЬ РАЗДЕЛОВ(СПРАВОЧНИК+ВЫХОД)  
        private void ListViewMenu2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Window auth_window;
            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ItemDictionary":
                    GridList.Visibility = Visibility.Collapsed;
                    GridListDictionary.Visibility = Visibility.Visible;
                    ItemDictionary.Visibility = Visibility.Collapsed;
                    ItemExitFromDicitionary.Visibility = Visibility.Visible;
                    if (ButtonOpenMenu.Visibility == Visibility.Visible)
                    {
                        ButtonOpenMenu_Click(this, null);
                        var OpenMenu = (Storyboard)this.Resources["OpenMenu"];
                        OpenMenu.Begin();
                    }

                    break;
                case "ItemExitFromDicitionary":
                    GridListDictionary.Visibility = Visibility.Collapsed;
                    GridList.Visibility = Visibility.Visible;
                    ItemExitFromDicitionary.Visibility = Visibility.Collapsed;
                    ItemDictionary.Visibility = Visibility.Visible;

                    break;
                case "ItemExit":

                    auth_window = new AuthentificationForm();
                    auth_window.Show();
                    Close();

                    break;
                default:
                    break;
            }
        }
        ////////////////////////////
        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            Window windowhelp = new WindowHelp();
            windowhelp.ShowDialog();
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            Window windowinfo = new WindowInfo();
            windowinfo.ShowDialog();
        }
        ////////////////////////////
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }
        ////////////////////////////




        private void AddItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Добавление");
        }

        private void EditItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Изменение");
        }

        private void DeleteItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Удаление");
        }

        private void RefreshTable_Click(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshList(queryrefresh, DataGrid);                       
        }

        private void SearchItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = CustomerBalance.FindListCustomerBalance(currentFindDescription, connection);
            if (result.dataTable != null)
            {
                DataGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFindDescription = result.findDescription;

            StopSearchItem.Visibility = Visibility.Visible;
        }

        private void StopSearchItem_Click(object sender, RoutedEventArgs e)
        {
            RefreshList(queryrefresh, DataGrid);
            currentFindDescription = new FindHandler.FindDescription();
            StopSearchItem.Visibility = Visibility.Collapsed;
        }

        private void UseFilter_Click(object sender, ExecutedRoutedEventArgs e)
        {
            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = CustomerBalance.FilterListCustomerBalance(currentFilterDescription, connection);
            if (result.dataTable != null)
            {
                DataGrid.ItemsSource = result.dataTable.DefaultView;
            }
            currentFilterDescription = result.filterDescription;
        }

        //Печать(Не готово)
        private void Print_Click(object sender, ExecutedRoutedEventArgs e)
        {
            ///////////////////////////////////
            /*
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(storeGrid, "Распечатываем элемент Canvas");
            }
            */
            //////////////////////////////////
            /*
            Microsoft.Office.Interop.Excel.Application excel;

            // Вот если сделать именно так, то будет проверка на запуск Экселя, и если все ок, то идем дальше, иначе сразу вылетаем из метода
            try
            {
                excel = new Excel.Application();
            }
            catch
            {
                MessageBox.Show("На вашем компьютере не установлен Excel. Печать невозможна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            excel.Visible = true;
            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];

            Range myRange = (Range)sheet1.Cells[1, 1];
            sheet1.Cells[1, 1].Font.Bold = true;
            myRange.Value2 = "Прибыль";

            for (int j = 0; j < incomeGrid.Columns.Count; j++)
            {
                Range myRange1 = (Range)sheet1.Cells[2, j + 1];
                sheet1.Cells[2, j + 1].Font.Bold = true;
                sheet1.Columns[j + 1].ColumnWidth = 15;
                myRange1.Value2 = incomeGrid.Columns[j].Header;
            }

            for (int i = 0; i < incomeGrid.Columns.Count; i++)
            {
                for (int j = 0; j < incomeGrid.Items.Count; j++)
                {
                    TextBlock b = incomeGrid.Columns[i].GetCellContent(incomeGrid.Items[j]) as TextBlock;
                    Microsoft.Office.Interop.Excel.Range myRange2 = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 3, i + 1];
                    myRange2.Value2 = b.Text;
                }
            }

            int countitems = incomeGrid.Items.Count;
            int countclearitems = clearincomeGrid.Items.Count;

            myRange = (Range)sheet1.Cells[4 + countitems, 1];
            sheet1.Cells[4 + countitems, 1].Font.Bold = true;
            myRange.Value2 = "Прибыль:";
            myRange = (Range)sheet1.Cells[4 + countitems, 2];
            sheet1.Cells[4 + countitems, 2].Font.Bold = true;
            myRange.Value2 = textBoxPlus.Text;
            ////////////////////////////////
            if (clearincomeGrid.Visibility == Visibility.Visible)
            {
                myRange = (Range)sheet1.Cells[6 + countitems, 1];
                sheet1.Cells[6 + incomeGrid.Items.Count, 1].Font.Bold = true;
                myRange.Value2 = "Расходы";

                for (int j = 0; j < clearincomeGrid.Columns.Count; j++)
                {
                    Range myRange1 = (Range)sheet1.Cells[7 + countitems, j + 1];
                    sheet1.Cells[7 + countitems, j + 1].Font.Bold = true;
                    sheet1.Columns[j + 1].ColumnWidth = 15;
                    myRange1.Value2 = clearincomeGrid.Columns[j].Header;
                }

                for (int i = 0; i < clearincomeGrid.Columns.Count; i++)
                {
                    for (int j = 0; j < countclearitems; j++)
                    {
                        TextBlock b = clearincomeGrid.Columns[i].GetCellContent(clearincomeGrid.Items[j]) as TextBlock;
                        Microsoft.Office.Interop.Excel.Range myRange2 = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 8 + countitems, i + 1];
                        myRange2.Value2 = b.Text;
                    }
                }

                myRange = (Range)sheet1.Cells[9 + countclearitems + countitems, 1];
                sheet1.Cells[9 + countclearitems + countitems, 1].Font.Bold = true;
                myRange.Value2 = "Расходы:";

                myRange = (Range)sheet1.Cells[9 + countclearitems + countitems, 2];
                sheet1.Cells[9 + countclearitems + countitems, 2].Font.Bold = true;
                myRange.Value2 = textBoxMinus.Text;

                myRange = (Range)sheet1.Cells[11 + countclearitems + countitems, 1];
                sheet1.Cells[11 + countclearitems + countitems, 1].Font.Bold = true;
                myRange.Value2 = "Итого:";

                myRange = (Range)sheet1.Cells[11 + countclearitems + countitems, 2];
                sheet1.Cells[11 + countclearitems + countitems, 2].Font.Bold = true;
                myRange.Value2 = textBoxCount.Text;
            }
            */
        }

        private void MakeExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Excel");
        }

        private void OpenList_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Лист");
        }
        //ЧИСТЫЙ ДОХОД
        private void ClearIncome_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new IncomeRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            string mounth = (csMounth.MounthOfIncome + 1).ToString();
            string query_text1 = "select orders.id_Order as id, orders.Total_Price as Count  from orders " +
                                "where month(orders.Date_Of_Delievery) = " + mounth + " and year(orders.Date_Of_Delievery) = year(curdate()) and (orders.Statuses_Of_Order_id_Status_Of_Order = 2 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 3 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 4); ";
            RefreshList(query_text1, DataGrid1);
            string query_text2 = "select costs.id as id, costs.Amount as Count  from costs " +
                                "where month(costs.Date_Of_Cost) = " + mounth + " and year(costs.Date_Of_Cost) = year(curdate())";
            RefreshList(query_text2, DataGrid2);
            float countplus = 0;
            float countminus = 0;

            foreach (DataRowView row in DataGrid1.Items)
            {
                countplus = countplus + (float)row.Row.ItemArray[1];
            }

            foreach (DataRowView row in DataGrid2.Items)
            {
                countminus = countminus + (float)row.Row.ItemArray[1];
            }

            if ((countplus > 0) || (countminus > 0))
            {
                System.Windows.MessageBox.Show("Доходы: " + countplus.ToString() + "\n" + "Расходы: " + countminus.ToString() + "\n" + "\n" + "Итого: " + (countplus - countminus).ToString(), "Чистый доход", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        //ГРЯЗНЫЙ ДОХОД
        private void DirtyIncome_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new IncomeRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            string mounth = (csMounth.MounthOfIncome + 1).ToString();
            string query_text = "select orders.id_Order as id, orders.Total_Price as Count from orders " +
                                "where month(orders.Date_Of_Delievery) = " + mounth + " and (orders.Statuses_Of_Order_id_Status_Of_Order = 2 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 3 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 4) and year(orders.Date_Of_Delievery) = year(curdate()); ";
            RefreshList(query_text, DataGrid1);
            float count = 0;
            foreach (DataRowView row in DataGrid1.Items)
            {
                count += (float)row.Row.ItemArray[1];
            }
            if (count > 0)
            {
                System.Windows.MessageBox.Show("Доходы: " + count.ToString() + "\n" + "\n" + "Итого: " + count.ToString(), "Грязный доход", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
