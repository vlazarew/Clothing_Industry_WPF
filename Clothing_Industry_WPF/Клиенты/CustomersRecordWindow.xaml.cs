using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
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
    /// Логика взаимодействия для CustomersRecordWindow.xaml
    /// </summary>
    public partial class CustomersRecordWindow : Window
    {
        private Customer customer;
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int idRecord;

        public CustomersRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id = -1)
        {
            customer = new Customer();
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            idRecord = id;

            // Заполнение шапки, полей даты 
            Title = FormLoader.setNewTitle(way, Title);
            Header.Content = Title;
            datePickerBirthday.Text = DateTime.Now.ToLongDateString();

            FillComboBoxes();

            if (idRecord != -1)
            {
                customer = new Customer(idRecord, connection);
                Border.Visibility = (customer.photo != null) ? Visibility.Hidden : Visibility.Visible;
                FillFields();
            }
        }

        private void FillFields()
        {
            textBoxName.Text = customer.name;
            textBoxLastname.Text = customer.lastname;
            textBoxPatronymic.Text = customer.patronymic;
            textBoxAddress.Text = customer.address;
            textBoxPhone_Number.Text = customer.phoneNumber;
            textBoxNickname.Text = customer.nickname;
            datePickerBirthday.SelectedDate = customer.birthday;
            textBoxPassportData.Text = customer.passportData;
            textBoxSize.Text = customer.size.ToString();
            textBoxParameters.Text = customer.parameters;
            textBoxNotes.Text = customer.notes;

            if (customer.photo == null)
            {
                imagePhoto.Source = null;
            }
            else
            {
                MemoryStream stream = new MemoryStream(customer.photo);
                imagePhoto.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            comboBoxStatus.SelectedValue = customer.statusName;
            comboBoxChannel.SelectedValue = customer.channelName;
            comboBoxEmployee.SelectedValue = customer.employeeLogin;
        }

        private void ButtonAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            ButtonAddPhoto_Click();
        }

        private void ButtonAddPhoto_Click(object sender, MouseButtonEventArgs e)
        {
            ButtonAddPhoto_Click();
        }

        private void ButtonAddPhoto_Click()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.JPG; *.GIF; *.JPEG; *.PNG)|*.JPG; *.GIF; *.JPEG; *.PNG" + "|Все файлы (*.*)|*.* ";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                // Отображение на форме
                string image_path = openFileDialog.FileName;
                imagePhoto.Source = new BitmapImage(new Uri(image_path));
                // Запоминаем бинарные данные в объекте 
                FileStream fileStream = new FileStream(image_path, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                customer.photo = binaryReader.ReadBytes((int)fileStream.Length);
                binaryReader.Close();
                fileStream.Close();
                Border.Visibility = Visibility.Hidden;
            }
        }

        private void FillComboBoxes()
        {
            var outgoingData = new List<(string field, string table, string comboBox)>();
            var comboboxStatusData = (field: "name_of_status", table: "customer_statuses", comboBox: "comboBoxStatus");
            var comboboxChannelData = (field: "name_of_channel", table: "order_channels", comboBox: "comboBoxChannel");
            var comboboxEmployeeData = (field: "Login", table: "employees", comboBox: "comboBoxEmployee");

            outgoingData.Add(comboboxChannelData);
            outgoingData.Add(comboboxEmployeeData);
            outgoingData.Add(comboboxStatusData);

            var receivedData = FormLoader.FillComboBoxes(outgoingData, connection);

            foreach (var data in receivedData)
            {
                var combobox = (ComboBox)FindName(data.Key);
                combobox.ItemsSource = data.Value;
            }
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
                case "textBoxName":
                    customer.name = value;
                    break;
                case "textBoxLastname":
                    customer.lastname = value;
                    break;
                case "textBoxPatronymic":
                    customer.patronymic = value;
                    break;
                case "textBoxAddress":
                    customer.address = value;
                    break;
                case "textBoxPhone_Number":
                    customer.phoneNumber = value;
                    break;
                case "textBoxNickname":
                    customer.nickname = value;
                    break;
                case "textBoxPassportData":
                    customer.passportData = value;
                    break;
                case "textBoxSize":
                    int newSize;
                    bool canParse = int.TryParse(value, out newSize);
                    customer.size = canParse ? newSize : 0;
                    break;
                case "textBoxParameters":
                    customer.parameters = value;
                    break;
                case "textBoxNotes":
                    customer.notes = value;
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
                case "datePickerBirthday":
                    customer.birthday = value;
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
                case "comboBoxStatus":
                    if (comboBoxStatus.Items.IndexOf(value) != -1)
                    {
                        customer.statusName = value;
                    }
                    break;
                case "comboBoxChannel":
                    if (comboBoxChannel.Items.IndexOf(value) != -1)
                    {
                        customer.channelName = value;
                    }
                    break;
                case "comboBoxEmployee":
                    if (comboBoxEmployee.Items.IndexOf(value) != -1)
                    {
                        customer.employeeLogin = value;
                    }
                    break;
            }
        }

        #endregion

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            // Истина - сохранение прошло успешно, ложь - если проблемы
            if (customer.Save(connection, way))
            {
                this.Close();
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            customer.Save(connection, way);
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
