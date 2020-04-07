using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для OrderRecordWindow.xaml
    /// </summary>
    public partial class OrderRecordWindow : Window
    {
        private Order order;
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int idRecord;

        public OrderRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id = -1)
        {
            order = new Order();
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            idRecord = id;

            // При создании они не могут накинуть сразу и список изделий в заказ
            buttonListProducts.Visibility = (way == WaysToOpenForm.WaysToOpen.create) ? Visibility.Hidden : Visibility.Visible;

            // Заполнение шапки, полей даты 
            Title = FormLoader.setNewTitle(way, Title);
            datePickerDateOfDelievery.Text = DateTime.Now.ToLongDateString();
            datePickerDateOfOrder.Text = DateTime.Now.ToLongDateString();

            FillComboBoxes();

            if (idRecord != -1)
            {
                order = new Order(idRecord, connection);
                FillFields();
            }
        }

        private void FillFields()
        {
            datePickerDateOfOrder.SelectedDate = order.dateOfOrder;
            textBoxDiscount.Text = order.discountPerCent.ToString();
            textBoxTotal_Price.Text = order.totalPrice.ToString();
            textBoxPaid.Text = order.paid.ToString();
            textBoxDebt.Text = order.debt.ToString();
            datePickerDateOfDelievery.SelectedDate = order.dateOfDelievery;
            textBoxNotes.Text = order.notes;
            textBoxAddedPrice.Text = order.addedPriceForComplexity.ToString();

            comboBoxTypeOfOrder.SelectedValue = order.typeOfOrderName;
            comboBoxStatusOfOrder.SelectedValue = order.statusName;
            comboBoxCustomer.SelectedValue = order.customerLogin;
            comboBoxResponsible.SelectedValue = order.responsible;
            comboBoxExecutor.SelectedValue = order.executor;
        }

        private void FillComboBoxes()
        {
            var outgoingData = new List<(string field, string table, string comboBox)>();
            var comboboxTypeData = (field: "name_of_type", table: "types_of_order", comboBox: "comboBoxTypeOfOrder");
            var comboboxStatusData = (field: "name_of_status", table: "statuses_of_order", comboBox: "comboBoxStatusOfOrder");
            var comboboxCustomerData = (field: "nickname", table: "customers", comboBox: "comboBoxCustomer");
            // Сделал плохо, надо без второго запроса
            var comboboxExecutorData = (field: "login", table: "employees", comboBox: "comboBoxExecutor");
            var comboboxResponsibleData = (field: "login", table: "employees", comboBox: "comboBoxResponsible");

            outgoingData.Add(comboboxTypeData);
            outgoingData.Add(comboboxStatusData);
            outgoingData.Add(comboboxCustomerData);
            outgoingData.Add(comboboxExecutorData);
            outgoingData.Add(comboboxResponsibleData);

            var receivedData = FormLoader.FillComboBoxes(outgoingData, connection);

            foreach (var data in receivedData)
            {
                var combobox = (ComboBox)FindName(data.Key);
                combobox.ItemsSource = data.Value;
            }
        }

        private void ButtonListProducts_Click(object sender, RoutedEventArgs e)
        {
            var windowListProducts = new OrderProductListWindow(idRecord);
            if (windowListProducts.ShowDialog().Value)
            {
                var result = windowListProducts.Result;
                if (!result)
                {
                    comboBoxStatusOfOrder.IsEnabled = false;
                    comboBoxStatusOfOrder.SelectedValue = "Отменён";
                }
            }
            order.UpdateTotalPrice(connection);

            textBoxTotal_Price.Text = order.totalPrice.ToString();
            textBoxDebt.Text = order.debt.ToString();
        }

        #region Обработка изменений данных в полях

        // Обработка всех текстовых полей
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var textBoxName = textBox.Name;
            var value = textBox.Text;
            switch (textBoxName)
            {
                case "textBoxDiscount":
                    float.TryParse(value, out float discountPerCent);
                    order.discountPerCent = discountPerCent;
                    break;
                case "textBoxTotal_Price":
                    float.TryParse(value, out float totalPrice);
                    order.totalPrice = totalPrice;
                    RefreshDebt();
                    break;
                case "textBoxPaid":
                    float.TryParse(value, out float paid);
                    order.paid = paid;
                    RefreshDebt();
                    break;
                case "textBoxDebt":
                    float.TryParse(value, out float debt);
                    order.debt = debt;
                    break;
                case "textBoxNotes":
                    order.notes = value;
                    break;
                case "textBoxAddedPrice":
                    float.TryParse(value, out float addedPriceForComplexity);
                    order.addedPriceForComplexity = addedPriceForComplexity;
                    break;
            }
        }

        // Обработка календарей по кнопке выбора
        private void DateTimePicker_TextChanged(object sender, RoutedEventArgs e)
        {
            DateTimePicker_TextChanged(sender);
        }

        // Обработка календарей вводом текста
        private void DateTimePicker_TextChanged(object sender, KeyEventArgs e)
        {
            DateTimePicker_TextChanged(sender);
        }

        private void DateTimePicker_TextChanged(object sender)
        {
            var dateTimePicker = sender as DatePicker;
            var dateTimePickerName = dateTimePicker.Name;
            var value = dateTimePicker.SelectedDate.Value;
            switch (dateTimePickerName)
            {
                case "datePickerDateOfOrder":
                    order.dateOfOrder = value;
                    break;
                case "datePickerDateOfDelievery":
                    order.dateOfDelievery = value;
                    break;
            }
        }

        // Обработка комбобоксов вводом текста
        private void ComboBox_TextChanged(object sender, KeyEventArgs e)
        {
            ComboBox_TextChanged(sender);
        }

        // Обработка комбобоксов выбором позиции
        private void ComboBox_TextChanged(object sender, EventArgs e)
        {
            ComboBox_TextChanged(sender);
        }

        private void ComboBox_TextChanged(object sender)
        {
            var comboBox = sender as ComboBox;
            var comboBoxName = comboBox.Name;
            var value = comboBox.Text;
            switch (comboBoxName)
            {
                case "comboBoxTypeOfOrder":
                    if (comboBoxTypeOfOrder.Items.IndexOf(value) != -1)
                    {
                        order.typeOfOrderName = value;
                    }
                    break;
                case "comboBoxStatusOfOrder":
                    if (comboBoxStatusOfOrder.Items.IndexOf(value) != -1)
                    {
                        order.statusName = value;
                    }
                    break;
                case "comboBoxCustomer":
                    if (comboBoxCustomer.Items.IndexOf(value) != -1)
                    {
                        order.customerLogin = value;
                    }
                    break;
                case "comboBoxResponsible":
                    if (comboBoxResponsible.Items.IndexOf(value) != -1)
                    {
                        order.responsible = value;
                    }
                    break;
                case "comboBoxExecutor":
                    if (comboBoxExecutor.Items.IndexOf(value) != -1)
                    {
                        order.executor = value;
                    }
                    break;
            }
        }

        #endregion

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            // Истина - сохранение прошло успешно, ложь - если проблемы
            if (order.Save(connection, way))
            {
                Close();
            }
        }

        private void RefreshDebt()
        {
            float.TryParse(textBoxTotal_Price.Text, out float totalPrice);
            float.TryParse(textBoxPaid.Text, out float paid);
            textBoxDebt.Text = (totalPrice - paid).ToString();
            order.debt = totalPrice - paid;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxValidator.IsFloatTextAllowed(e.Text);
        }


    }
}
