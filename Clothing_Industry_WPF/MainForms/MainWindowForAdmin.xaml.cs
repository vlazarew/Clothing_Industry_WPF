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
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static Clothing_Industry_WPF.Доходы.IncomeRecordWindow;
using Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа;
using Clothing_Industry_WPF.Материалы;
using System.IO;
using System.Diagnostics;
using Clothing_Industry_WPF.Справочник;

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
        private string textmenu = "ItemOrders";
        private string Id;
        private string name;

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
            AddItem.Visibility = Visibility.Collapsed;
            EditItem.Visibility = Visibility.Collapsed;
            DeleteItem.Visibility = Visibility.Collapsed;
            ClearIncome.Visibility = Visibility.Collapsed;
            DirtyIncome.Visibility = Visibility.Collapsed;
            Print.Visibility = Visibility.Collapsed;
            RefreshTable.Visibility = Visibility.Collapsed;
            SearchItem.Visibility = Visibility.Collapsed;
            StopSearchItem.Visibility = Visibility.Collapsed;
            UseFilter.Visibility = Visibility.Collapsed;
            OpenList.Visibility = Visibility.Collapsed;
            MakeExcel.Visibility = Visibility.Collapsed;
            DataGrid.ItemsSource = null;
            DataGrid.Columns.Clear();
            DataGrid.IsEnabled = true;
            OneGrid.Visibility = Visibility.Collapsed;

            DataGrid1.ItemsSource = null;
            DataGrid1.Columns.Clear();

            DataGrid2.ItemsSource = null;
            DataGrid2.Columns.Clear();
            TwoGrids.Visibility = Visibility.Collapsed;
        }
        //ЗАКРАСКА СТРОКИ ЗАКАЗОВ
        private void OrdersGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (textmenu == "ItemOrders")
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
        }
        //ОПРЕДЕЛЕНИЕ ФОКУСА
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            DeleteItem.Visibility = Visibility.Visible;
            EditItem.Visibility = Visibility.Visible;
            if ((textmenu == "ItemProducts") || (textmenu == "ItemOrders") || (textmenu == "ItemReceipt"))
                OpenList.Visibility = Visibility.Visible;
            if (textmenu == "ItemCosts")
                MakeExcel.Visibility = Visibility.Visible;
        }
        //ПОТЕРЯ ФОКУСА
        private void FocusLost()
        {
            if (DataGrid.SelectedItems.Count == 0)
            {
                DeleteItem.Visibility = Visibility.Collapsed;
                EditItem.Visibility = Visibility.Collapsed;
                OpenList.Visibility = Visibility.Collapsed;
                MakeExcel.Visibility = Visibility.Collapsed;
            }
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
            FocusLost();
            // d.SelectedIndex = 0;
        }
        //ЗАКАЗЫ ИНИЦИАЛИЗАЦИЯ
        private void Ordersload()
        {
            
            AddItem.Visibility = Visibility.Visible;

            RefreshTable.Visibility = Visibility.Visible;
            SearchItem.Visibility = Visibility.Visible;
            UseFilter.Visibility = Visibility.Visible;
            Print.Visibility = Visibility.Visible;
            OpenList.Visibility = Visibility.Visible;
            OneGrid.Visibility = Visibility.Visible;

            AddColumnDataGrid(DataGrid, "Номер заказа", "id_Order");
            AddColumnDataGrid(DataGrid, "Дата заказа", "Date_Of_Order");
            AddColumnDataGrid(DataGrid, "Изделия", "Products");
            AddColumnDataGrid(DataGrid, "% скидки", "Discount_Per_Cent");
            AddColumnDataGrid(DataGrid, "Полная стоимость", "Total_Price");
            AddColumnDataGrid(DataGrid, "Оплачено", "Paid");
            AddColumnDataGrid(DataGrid, "Долг", "Debt");
            AddColumnDataGrid(DataGrid, "Дата доставки", "Date_Of_Delievery");
            AddColumnDataGrid(DataGrid, "Тип заказа", "Name_Of_type");
            AddColumnDataGrid(DataGrid, "Статус заказа", "Name_Of_Status");
            AddColumnDataGrid(DataGrid, "Заказчик", "Nickname");
            AddColumnDataGrid(DataGrid, "Ответственный", "Responsible");
            AddColumnDataGrid(DataGrid, "Исполнитель", "Executor");
            AddColumnDataGrid(DataGrid, "Выплата за доп сложность", "Added_Price_For_Complexity");
            AddColumnDataGrid(DataGrid, "Заметки", "Notes");
            queryrefresh = Order.getQueryText();
            RefreshList(queryrefresh, DataGrid);
            string status = "";
            

        }
        // ПАНЕЛЬ РАЗДЕЛОВ     
        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearForm();
            textmenu = ((ListViewItem)((ListView)sender).SelectedItem).Name;
            switch (textmenu)
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
                    DataGrid.IsEnabled = false;
                    queryrefresh = CustomerBalance.getQueryText();
                    RefreshList(queryrefresh, DataGrid);

                    break;

                case "ItemOrders":

                    Ordersload();

                    break;

                case "ItemProducts":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;
                    OpenList.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Название", "Name_Of_Product");
                    AddColumnDataGrid(DataGrid, "Цена", "Fixed_Price");
                    AddColumnDataGrid(DataGrid, "Выплата сотруднику", "MoneyToEmployee");
                    AddColumnDataGrid(DataGrid, "Описание", "Description");

                    queryrefresh = Product.getQueryText();
                    RefreshList(queryrefresh, DataGrid);
                    break;

                case "ItemClients":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Фамилия", "Lastname");
                    AddColumnDataGrid(DataGrid, "Имя", "Name");
                    AddColumnDataGrid(DataGrid, "Телефонный номер", "Phone_Number");
                    AddColumnDataGrid(DataGrid, "Паспорт", "Passport_data");
                    AddColumnDataGrid(DataGrid, "Размер", "Size");
                    AddColumnDataGrid(DataGrid, "Параметры", "Parameters");
                    AddColumnDataGrid(DataGrid, "Никнейм", "Nickname");
                    AddColumnDataGrid(DataGrid, "Дата рождения", "Birthday");
                    AddColumnDataGrid(DataGrid, "Статус клиента", "Name_Of_Status");
                    AddColumnDataGrid(DataGrid, "Способ заказа", "Name_of_channel");
                    AddColumnDataGrid(DataGrid, "Обсуживающий сотрудник", "Login");
                    AddColumnDataGrid(DataGrid, "Заметки", "Notes");

                    queryrefresh = Customer.getQueryText();
                    RefreshList(queryrefresh, DataGrid);
                    break;

                case "ItemMaterials":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Артикул", "Vendor_Code");
                    AddColumnDataGrid(DataGrid, "Название материала", "Name_Of_Material");
                    AddColumnDataGrid(DataGrid, "Стоимость", "Cost_Of_Material");
                    AddColumnDataGrid(DataGrid, "Единица измерениия", "Name_Of_Unit");
                    AddColumnDataGrid(DataGrid, "Вид", "Name_Of_Group");
                    AddColumnDataGrid(DataGrid, "Тип", "Name_Of_Type");
                    AddColumnDataGrid(DataGrid, "Страна", "Name_Of_Country");
                    AddColumnDataGrid(DataGrid, "Заметки", "Notes");

                    queryrefresh = Material.getQueryText();
                    RefreshList(queryrefresh, DataGrid);
                    break;

                case "ItemPayrolls":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Логин", "Login");
                    AddColumnDataGrid(DataGrid, "Период", "Period");
                    AddColumnDataGrid(DataGrid, "Дата выплаты", "Date_Of_Pay");
                    AddColumnDataGrid(DataGrid, "Зарплата", "Salary");
                    AddColumnDataGrid(DataGrid, "Сдельная Зарплата", "PieceWorkPayment");
                    AddColumnDataGrid(DataGrid, "Общая зп", "Total_Salary");
                    AddColumnDataGrid(DataGrid, "Штраф", "Penalty");
                    AddColumnDataGrid(DataGrid, "К выплате", "To_Pay");
                    AddColumnDataGrid(DataGrid, "Выплачено", "PaidOff");
                    AddColumnDataGrid(DataGrid, "Заметки", "Notes");

                    queryrefresh = Payroll.getQueryText();
                    RefreshList(queryrefresh, DataGrid);
                    break;

                case "ItemHolidays":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Логин", "Login");
                    AddColumnDataGrid(DataGrid, "Год", "Year");
                    AddColumnDataGrid(DataGrid, "Всего дней", "Days_Of_Holidays");
                    AddColumnDataGrid(DataGrid, "Прошло", "Days_Used");
                    AddColumnDataGrid(DataGrid, "Осталось", "Rest_Of_Days");
                    AddColumnDataGrid(DataGrid, "Запланировано", "Planned_Start");
                    AddColumnDataGrid(DataGrid, "Начинается с", "In_Fact_Start");
                    AddColumnDataGrid(DataGrid, "Заканчивается", "In_Fact_End");
                    AddColumnDataGrid(DataGrid, "Примечание", "Notes");

                    queryrefresh = Holidays.getQueryText();
                    RefreshList(queryrefresh, DataGrid);

                    break;

                case "ItemFittings":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Никнейм заказчкика", "Customer");
                    AddColumnDataGrid(DataGrid, "Номер заказа", "OrderId");
                    AddColumnDataGrid(DataGrid, "Тип примерки", "Type_Of_Fitting");
                    AddColumnDataGrid(DataGrid, "Дата примерки", "Date");
                    AddColumnDataGrid(DataGrid, "Заметки", "Notes");


                    queryrefresh = Fitting.getQueryText();
                    RefreshList(queryrefresh, DataGrid);

                    break;
                case "ItemReceipt":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;
                    OpenList.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Название документа", "Name_Of_Document");
                    AddColumnDataGrid(DataGrid, "Сумма прихода", "Total_Price");
                    AddColumnDataGrid(DataGrid, "Дата прихода", "Date_Of_Entry");
                    AddColumnDataGrid(DataGrid, "Поставщик", "Name_Of_Supplier");
                    AddColumnDataGrid(DataGrid, "Статус", "Name_Of_State");
                    AddColumnDataGrid(DataGrid, "Тип транзакции", "Name_Of_Type");
                    AddColumnDataGrid(DataGrid, "Папка документа", "Default_Folder");
                    AddColumnDataGrid(DataGrid, "Доп. сведения", "Notes");
                    queryrefresh = ReceiptOfMaterials.getQueryText();
                    RefreshList(queryrefresh, DataGrid);
                    break;
                case "ItemCosts":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;
                    MakeExcel.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Номер документа", "id");
                    AddColumnDataGrid(DataGrid, "Название документа", "Name_Of_Document");
                    AddColumnDataGrid(DataGrid, "Дата расхода", "Date_Of_Cost");
                    AddColumnDataGrid(DataGrid, "Сумма", "Amount");
                    AddColumnDataGrid(DataGrid, "От", "From");
                    AddColumnDataGrid(DataGrid, "Кому", "To");
                    AddColumnDataGrid(DataGrid, "Категория расхода", "Name_Of_Category");
                    AddColumnDataGrid(DataGrid, "Тип оплаты", "Name_Of_Type");
                    AddColumnDataGrid(DataGrid, "Периодичность", "Name_Of_Periodicity");
                    AddColumnDataGrid(DataGrid, "Путь документа", "Default_Folder");
                    AddColumnDataGrid(DataGrid, "Заметки", "Notes");
                    queryrefresh = Costs.getQueryText();
                    RefreshList(queryrefresh, DataGrid);

                    break;
                case "ItemStore":
                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Артикул", "Vendor_Code");
                    AddColumnDataGrid(DataGrid, "Название материала", "Name_Of_Material");
                    AddColumnDataGrid(DataGrid, "Количество", "Count");
                    AddColumnDataGrid(DataGrid, "Единица измерения", "Name_Of_Unit");

                    DataGrid.IsEnabled = false;
                    queryrefresh = Store.getQueryText();
                    RefreshList(queryrefresh, DataGrid);

                    break;
                case "ItemWorker":
                    AddItem.Visibility = Visibility.Visible;

                    RefreshTable.Visibility = Visibility.Visible;
                    SearchItem.Visibility = Visibility.Visible;
                    UseFilter.Visibility = Visibility.Visible;
                    OneGrid.Visibility = Visibility.Visible;

                    AddColumnDataGrid(DataGrid, "Логин", "Login");
                    AddColumnDataGrid(DataGrid, "Фамилия", "Lastname");
                    AddColumnDataGrid(DataGrid, "Имя", "Name");
                    AddColumnDataGrid(DataGrid, "Отчество", "Patronymic");
                    AddColumnDataGrid(DataGrid, "Должность", "Name_Of_Position");
                    AddColumnDataGrid(DataGrid, "Телефонный номер", "Phone_Number");
                    AddColumnDataGrid(DataGrid, "E-mail", "Email");
                    AddColumnDataGrid(DataGrid, "Паспорт", "Passport_Data");
                    AddColumnDataGrid(DataGrid, "Дата добавления", "Added");
                    AddColumnDataGrid(DataGrid, "Последняя зарплата", "Last_Salary");
                    AddColumnDataGrid(DataGrid, "Заметки", "Notes");


                    queryrefresh = Employee.getQueryText();
                    RefreshList(queryrefresh, DataGrid);

                    break;
                default:
                    break;
            }
        }
        //ПАНЕЛЬ РАЗДЕЛОВ(СПРАВОЧНИКА)   
        private void ListViewMenuDictionary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearForm();
            AddItem.Visibility = Visibility.Visible;

            RefreshTable.Visibility = Visibility.Visible;
            SearchItem.Visibility = Visibility.Visible;
            UseFilter.Visibility = Visibility.Visible;
            OneGrid.Visibility = Visibility.Visible;

            AddColumnDataGrid(DataGrid, "Наименование", "Name");
            textmenu = ((ListViewItem)((ListView)sender).SelectedItem).Name;
            switch (textmenu)
            {
                case "EmployeePositions":
                    DataGridCheckBoxColumn textColumn = new DataGridCheckBoxColumn();
                    textColumn.Header = "Администратор";
                    textColumn.Binding = new Binding("IsAdministrator");
                    DataGrid.Columns.Add(textColumn);

                    queryrefresh = Dictionary.getQueryEmployeePositions();

                    break;
                case "Units":
                    Id = "id_Unit";
                    name = "Name_Of_Unit";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Order_channels":
                    Id = "id_Channel";
                    name = "Name_Of_Channel";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Consumption_categories":
                    Id = "id_Consumption_Category";
                    name = "Name_Of_Category";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Periodicities":
                    Id = "id_Periodicity";
                    name = "Name_Of_Periodicity";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Suppliers":
                    Id = "id_Supplier";
                    name = "Name_Of_Supplier";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Customer_statuses":
                    Id = "id_Status";
                    name = "Name_Of_status";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Payment_states":
                    Id = "id_Payment_States";
                    name = "Name_Of_State";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Countries":
                    Id = "id_Country";
                    name = "Name_Of_Country";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Types_of_order":
                    Id = "id_Type_Of_Order";
                    name = "Name_Of_Type";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Types_of_material":
                    Id = "id_Type_Of_Material";
                    name = "Name_Of_Type";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Types_of_payment":
                    Id = "id_Of_Type";
                    name = "Name_Of_Type";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Types_of_fitting":
                    Id = "id_Type_Of_Fitting";
                    name = "Name_Of_type";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
                case "Types_of_transactions":
                    Id = "id_Type_Of_Transaction";
                    name = "Name_Of_Type";
                    queryrefresh = Dictionary.getQueryText(Id, name, textmenu);

                    break;
            }

                    RefreshList(queryrefresh, DataGrid);
        }
        //ИЗМЕНЕНИЕ ПО ДВОЙНОМУ КЛИКУ
        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Window editWindow;
            switch (textmenu)
            {
                case "ItemOrders":
                    int id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemProducts":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new ProductRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemClients":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemMaterials":
                    editWindow = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.edit, (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0]);
                    editWindow.ShowDialog();
                    break;
                case "ItemPayrolls":
                    string login = (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    string period = (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[1];

                    editWindow = new PayrollRecordWindow(WaysToOpenForm.WaysToOpen.edit, login, period);
                    editWindow.ShowDialog();
                    break;
                case "ItemHolidays":
                    editWindow = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.edit, (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0]);
                    editWindow.ShowDialog();
                    break;
                case "ItemFittings":
                    string nickname = (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[1];
                    editWindow = new FittingRecordWindow(WaysToOpenForm.WaysToOpen.edit, id, nickname);
                    editWindow.ShowDialog();
                    break;
                case "ItemReceipt":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new ReceiptOfMaterialsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemCosts":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemWorker":
                    editWindow = new EmployeeRecordWindow(WaysToOpenForm.WaysToOpen.edit, (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0]);
                    editWindow.ShowDialog();
                    break;
                    //Справочник
                case "EmployeePositions":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new EmployeePositionsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                default:
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new DictionaryRecordWindow(WaysToOpenForm.WaysToOpen.edit,Id,name,textmenu,id);
                    editWindow.ShowDialog();
                    break;

            }
            RefreshList(queryrefresh, DataGrid);
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
        //ПОЛЕЗНАЯ ИНФОРМАЦИЯ
        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            Window windowhelp = new WindowHelp();
            windowhelp.ShowDialog();
        }
        //ИНФО О ПРОЕКТЕ
        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            Window windowinfo = new WindowInfo();
            windowinfo.ShowDialog();
        }
        //ОТКРЫТЬ МЕНЮ РАЗДЕЛОВ
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }
        //ЗАКРЫТЬ МЕНЮ РАЗДЕЛОВ
        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }
        //ДОБАВЛЕНИЕ ЗАПИСИ
        private void AddItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            Window create_window;
            switch (textmenu)
            {
                case "ItemOrders":
                    create_window = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemProducts":
                    create_window = new ProductRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemClients":
                    create_window = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemMaterials":
                    create_window = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemPayrolls":
                    create_window = new PayrollRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemHolidays":
                    create_window = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemFittings":
                    create_window = new FittingRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemReceipt":
                    create_window = new ReceiptOfMaterialsRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemCosts":
                    create_window = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                case "ItemWorker":
                    create_window = new EmployeeRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                    //Справочник
                case "EmployeePositions":
                    create_window = new EmployeePositionsRecordWindow(WaysToOpenForm.WaysToOpen.create);
                    create_window.ShowDialog();
                    break;
                default:
                    create_window = new DictionaryRecordWindow(WaysToOpenForm.WaysToOpen.create,Id, name, textmenu);
                    create_window.ShowDialog();
                    break;

            }
            RefreshList(queryrefresh, DataGrid);

        }
        //ИЗМЕНЕНИЕ ЗАПИСИ
        private void EditItem_Click(object sender, ExecutedRoutedEventArgs e)
        {

            Window editWindow;

            switch (textmenu)
            {
                case "ItemOrders":
                    int id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new OrderRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();

                    break;
                case "ItemProducts":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new ProductRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();

                    break;
                case "ItemClients":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new CustomersRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();

                    break;
                case "ItemMaterials":
                    editWindow = new MaterialRecordWindow(WaysToOpenForm.WaysToOpen.edit, (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0]);
                    editWindow.ShowDialog();
                    break;
                case "ItemPayrolls":
                    string login = (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    string period = (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[1];
                    editWindow = new PayrollRecordWindow(WaysToOpenForm.WaysToOpen.edit, login, period);
                    editWindow.ShowDialog();
                    break;
                case "ItemHolidays":
                    editWindow = new HolidaysRecordWindow(WaysToOpenForm.WaysToOpen.edit, (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0]);
                    editWindow.ShowDialog();
                    break;
                case "ItemFittings":
                    string nickname = (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[1];
                    editWindow = new FittingRecordWindow(WaysToOpenForm.WaysToOpen.edit, id, nickname);
                    editWindow.ShowDialog();
                    break;
                case "ItemReceipt":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new ReceiptOfMaterialsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemCosts":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new CostsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                case "ItemWorker":
                    editWindow = new EmployeeRecordWindow(WaysToOpenForm.WaysToOpen.edit, (string)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0]);
                    editWindow.ShowDialog();
                    break;
                case "EmployeePositions":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new EmployeePositionsRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
                    editWindow.ShowDialog();
                    break;
                default:
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];
                    editWindow = new DictionaryRecordWindow(WaysToOpenForm.WaysToOpen.edit, Id, name, textmenu, id);
                    editWindow.ShowDialog();
                    break;
            }
            RefreshList(queryrefresh, DataGrid);
        }
        //УДАЛЕНИЕ
        private void DeleteItem_Click(object sender, ExecutedRoutedEventArgs e)
        {

            switch (textmenu)
            {
                case "ItemOrders":
                    List<int> idsToDelete = new List<int>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
                    }
                    Order.DeleteFromDB(idsToDelete, connection);

                    break;

                case "ItemProducts":
                    idsToDelete = new List<int>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
                    }
                    Product.DeleteFromDB(idsToDelete, connection);
                    break;

                case "ItemClients":
                    List<(int id, string firstname, string lastname)> dataToDelete = new List<(int id, string firstname, string lastname)>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        dataToDelete.Add((id: (int)row.Row.ItemArray[0], firstname: row.Row.ItemArray[1].ToString(), lastname: row.Row.ItemArray[2].ToString()));
                    }
                    Customer.DeleteFromDB(dataToDelete, connection);
                    break;

                case "ItemMaterials":
                    List<string> CodesToDelete = new List<string>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        CodesToDelete.Add(row.Row.ItemArray[0].ToString());
                    }
                    Material.DeleteFromDB(CodesToDelete, connection);
                    break;

                case "ItemPayrolls":
                    List<(string login, string period)> dataToDeletePayroll = new List<(string login, string period)>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        dataToDeletePayroll.Add((login: row.Row.ItemArray[0].ToString(), period: row.Row.ItemArray[1].ToString()));
                    }
                    Payroll.DeleteFromDB(dataToDeletePayroll, connection);
                    break;

                case "ItemHolidays":
                    CodesToDelete = new List<string>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        CodesToDelete.Add(row.Row.ItemArray[0].ToString());
                    }
                    Holidays.DeleteFromDB(CodesToDelete, connection);
                    break;
                case "ItemFittings":
                    List<(string customerNickname, int idOrder)> dataToDeleteFittings = new List<(string customerNickname, int idOrder)>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        dataToDeleteFittings.Add((customerNickname: row.Row.ItemArray[0].ToString(), idOrder: (int)row.Row.ItemArray[1]));
                    }
                    Fitting.DeleteFromDB(dataToDeleteFittings, connection);
                    break;
                case "ItemReceipt":
                    idsToDelete = new List<int>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
                    }
                    ReceiptOfMaterials.DeleteFromDB(idsToDelete, connection);
                    break;
                case "ItemCosts":
                    idsToDelete = new List<int>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
                    }
                    Costs.DeleteFromDB(idsToDelete, connection);
                    break;
                case "ItemWorker":
                    CodesToDelete = new List<string>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        CodesToDelete.Add(row.Row.ItemArray[0].ToString());
                    }
                    Employee.DeleteFromDB(CodesToDelete, connection);
                    break;
                case "EmployeePositions":
                    idsToDelete = new List<int>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
                    }
                    Dictionary.DeleteFromDBEmployee(idsToDelete, connection);
                    break;
                default:
                    idsToDelete = new List<int>();
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idsToDelete.Add(int.Parse(row.Row.ItemArray[0].ToString()));
                    }
                    Dictionary.DeleteFromDB(idsToDelete, connection,Id, textmenu);
                    break;


            }
            RefreshList(queryrefresh, DataGrid);
        }


        //ОБНОВЛЕНИЕ ТАБЛИЦЫ
        private void RefreshTable_Click(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshList(queryrefresh, DataGrid);
        }
        //ПОИСК
        private void SearchItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            (DataTable dataTable, FindHandler.FindDescription findDescription) result;
            switch (textmenu)
            {
                case "ItemBalance":
                    result = CustomerBalance.FindListCustomerBalance(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;
                    break;
                case "ItemStore":
                    result = Store.FindListStore(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;
                    break;
                case "ItemOrders":
                    result = Order.FindListOrders(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemProducts":
                    result = Product.FindListProducts(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemClients":
                    result = Customer.FindListCustomers(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemMaterials":
                    result = Material.FindListMaterials(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemPayrolls":
                    result = Payroll.FindListPayrolls(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemHolidays":
                    result = Holidays.FindListHolidays(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemFittings":
                    result = Fitting.FindListFittings(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemReceipt":
                    result = ReceiptOfMaterials.FindListReceipt(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemCosts":
                    result = Costs.FindListCosts(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "ItemWorker":
                    result = Employee.FindListEmployee(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                case "EmployeePositions":
                    result = Dictionary.FindListEmployee(currentFindDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
                default:
                    result = Dictionary.FindList(currentFindDescription, connection, Id, name);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                        StopSearchItem.Visibility = Visibility.Visible;
                    }
                    currentFindDescription = result.findDescription;

                    break;
            }



        }
        //ОСТАНОВКА СПИСКА
        private void StopSearchItem_Click(object sender, RoutedEventArgs e)
        {
            RefreshList(queryrefresh, DataGrid);
            currentFindDescription = new FindHandler.FindDescription();
            StopSearchItem.Visibility = Visibility.Collapsed;
        }
        //ФИЛЬТРЫ
        private void UseFilter_Click(object sender, ExecutedRoutedEventArgs e)
        {
            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result;
            switch (textmenu)
            {
                case "ItemBalance":
                    result = CustomerBalance.FilterListCustomerBalance(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;
                    break;
                case "ItemStore":
                    result = Store.FilterListStore(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemOrders":
                    result = Order.FilterListOrders(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemProducts":
                    result = Product.FilterListProducts(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemClients":
                    result = Customer.FilterListCustomers(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;

                case "ItemMaterials":
                    result = Material.FilterListMaterials(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;

                case "ItemPayrolls":
                    result = Payroll.FilterListPayrolls(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemHolidays":
                    result = Holidays.FilterListHolidays(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemFittings":
                    result = Fitting.FilterListFittings(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemReceipt":
                    result = ReceiptOfMaterials.FilterListReceipt(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemCosts":
                    result = Costs.FilterListCosts(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "ItemWorker":
                    result = Employee.FilterListEmployee(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                case "EmployeePositions":
                    result = Dictionary.FilterListEmployee(currentFilterDescription, connection);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;
                default:
                    result = Dictionary.FilterList(currentFilterDescription, connection,Id,name);
                    if (result.dataTable != null)
                    {
                        DataGrid.ItemsSource = result.dataTable.DefaultView;
                    }
                    currentFilterDescription = result.filterDescription;

                    break;


            }

        }
        //ПЕЧАТЬ
        private void Print_Click(object sender, ExecutedRoutedEventArgs e)
        {

            string idToPrint = "";
            switch (textmenu)
            {
                case "ItemOrders":
                    foreach (DataRowView row in DataGrid.SelectedItems)
                    {
                        idToPrint += row.Row.ItemArray[0].ToString() + ", ";
                    }
                    idToPrint = idToPrint.Substring(0, idToPrint.Length - 2);

                    Order.PrintOrders(idToPrint, connection);

                    break;
            }


        }
        //СОСТАВЛЕНИЕ ДОКУМЕНТА
        private void MakeExcel_Click(object sender, RoutedEventArgs e)
        {

            string path = ((DataRowView)DataGrid.SelectedItem).Row.ItemArray[2].ToString();
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Файл не найден", "Выплаты", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
        //ОТКРЫТЬ СПИСОК
        private void OpenList_Click(object sender, RoutedEventArgs e)
        {
            int id;
            Window list;
            switch (textmenu)
            {
                case "ItemOrders":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];

                    list = new OrderProductListWindow(id);
                    list.ShowDialog();


                    break;
                case "ItemProducts":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];

                    list = new MaterialsForProductsListWindow(id);
                    list.ShowDialog();

                    break;
                case "ItemReceipt":
                    id = (int)((DataRowView)DataGrid.SelectedItem).Row.ItemArray[0];

                    list = new ContentOfReceiptOfMaterialsListWindow(id);
                    list.ShowDialog();

                    break;
            }
            RefreshList(queryrefresh, DataGrid);
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
        //ЗАГРУЗКА ЗАКАЗОВ
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Ordersload();
        }

    }
}
