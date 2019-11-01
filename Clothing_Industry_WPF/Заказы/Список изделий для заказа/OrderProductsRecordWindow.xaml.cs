using Clothing_Industry_WPF.Общее.Работа_с_формами;
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

namespace Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа
{
    /// <summary>
    /// Логика взаимодействия для OrderProductsRecordWindow.xaml
    /// </summary>
    public partial class OrderProductsRecordWindow : Window
    {
        OrderProducts orderProducts;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private int orderId;
        private MySqlConnection connection;

        public OrderProductsRecordWindow(int orderId)
        {
            orderProducts = new OrderProducts(orderId);
            InitializeComponent();
            connection = new MySqlConnection(connectionString);
            this.orderId = orderId;
            FillComboBoxes();
        }

        private void FillComboBoxes()
        {
            // Заполнение только тех изделий, которых еще нет в заказе
            var result = orderProducts.TakeFreeProducts(connection);

            foreach (var data in result)
            {
                comboBoxProducts.Items.Add(data);
            }
        }

        private void TextBoxCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxValidator.IsIntTextAllowed(e.Text);
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
                case "textBoxCount":
                    int.TryParse(value, out int newCount);
                    orderProducts.count = newCount;
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
                case "comboBoxProducts":
                    if (comboBoxProducts.Items.IndexOf(value) != -1)
                    {
                        orderProducts.product = value;
                    }
                    break;
            }
        }

        #endregion

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Истина - сохранение прошло успешно, ложь - если проблемы
            if (orderProducts.Save(connection))
            {
                Close();
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
