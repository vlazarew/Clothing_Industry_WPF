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
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            idRecord = id;

            // Заполнение шапки, полей даты 
            Title = FormLoader.setNewTitle(way, Title);
            Header.Content = Title;
            datePickerBirthday.Text = DateTime.Now.ToLongDateString();

            FillComboBoxes();
            customer = new Customer();

            if (idRecord != -1)
            {
                customer = new Customer(idRecord, connection);
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
            comboBoxEmployee.SelectedValue = customer.employeeName;
        }

        private void ButtonAddPhoto_Click(object sender, RoutedEventArgs e)
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
            var outgoingData = new List<Tuple<string, string, string>>();
            var comboboxStatusData = new Tuple<string, string, string>("name_of_status", "customer_statuses", "comboBoxStatus");
            var comboboxChannelData = new Tuple<string, string, string>("name_of_channel", "order_channels", "comboBoxChannel");
            var comboboxEmployeeData = new Tuple<string, string, string>("Login", "employees", "comboBoxEmployee");

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


        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = customer.CheckData();
            if (warning == "")
            {
                MySqlConnection connection = new MySqlConnection(connectionString);

                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                // Создать/изменить запись в таблице Клиенты
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    this.Hide();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (way == WaysToOpenForm.WaysToOpen.create)
                {
                    transaction = connection.BeginTransaction();
                    string query_max_id = "SELECT max(customers.id_Customer) FROM main_database.customers";
                    MySqlCommand commandFindId = new MySqlCommand(query_max_id, connection, transaction);
                    int findId = -1;

                    using (DbDataReader reader = commandFindId.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            findId = reader.GetInt32(0);
                        }
                    }

                    string query_balance = "Insert into customers_balance (Customers_id_Customer, Accured, Paid, Debt) values (@id, 0, 0, 0)";
                    MySqlCommand commandFillBalance = new MySqlCommand(query_balance, connection, transaction);
                    commandFillBalance.Parameters.AddWithValue("@id", findId);

                    try
                    {
                        commandFillBalance.ExecuteNonQuery();
                        transaction.Commit();
                        this.Hide();
                    }
                    catch
                    {
                        transaction.Rollback();
                        MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                connection.Close();
            }
            else
            {
                MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO customers " +
                        "(Name, Lastname, Patronymic, Address, Phone_Number, Nickname," +
                        " Birthday, Passport_Data, Size, Parameters, Notes, Customer_Statuses_id_Status, Order_Channels_id_Channel, Employees_Login, Photo)" +
                        " VALUES (@name, @lastname, @patronymic, @address, @phone, @nickname, @birthday, @passport, @size, @parameters, @notes," +
                        "         @status, @channel, @login, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update customers set Name = @name, Lastname = @lastname, Patronymic = @patronymic, Address = @address, Phone_Number = @phone, Nickname = @nickname, " +
                        "Birthday = @birthday, Passport_Data = @passport, Size = @size, Parameters = @parameters, Notes = @notes, Customer_Statuses_id_Status = @status, " +
                        "Order_Channels_id_Channel = @channel, Employees_Login = @login, Photo = @image" +
                        " where id_Customer = @id;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", textBoxName.Text);
            command.Parameters.AddWithValue("@lastname", textBoxLastname.Text);
            command.Parameters.AddWithValue("@patronymic", textBoxPatronymic.Text);
            command.Parameters.AddWithValue("@address", textBoxAddress.Text);
            command.Parameters.AddWithValue("@phone", textBoxPhone_Number.Text);
            command.Parameters.AddWithValue("@nickname", textBoxNickname.Text);
            command.Parameters.AddWithValue("@birthday", datePickerBirthday.SelectedDate.Value);
            command.Parameters.AddWithValue("@passport", textBoxPassportData.Text);
            command.Parameters.AddWithValue("@size", textBoxSize.Text);
            command.Parameters.AddWithValue("@parameters", textBoxParameters.Text);
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);

            MySqlCommand commandStatus = new MySqlCommand("select id_Status from customer_statuses where name_of_status = @status", connection);
            commandStatus.Parameters.AddWithValue("status", comboBoxStatus.SelectedItem.ToString());
            int id_status = -1;
            using (DbDataReader reader = commandStatus.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_status = reader.GetInt32(0);
                }
            }

            MySqlCommand commandChannel = new MySqlCommand("select id_Channel from order_channels where name_of_channel = @channel", connection);
            commandChannel.Parameters.AddWithValue("channel", comboBoxChannel.SelectedItem.ToString());
            int id_channel = -1;
            using (DbDataReader reader = commandChannel.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_channel = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@status", id_status);
            command.Parameters.AddWithValue("@channel", id_channel);
            command.Parameters.AddWithValue("@login", comboBoxEmployee.SelectedItem.ToString());


            // Обработка фото 
            command.Parameters.AddWithValue("@image", customer.photo);

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@id", idRecord);
            }

            return command;
        }

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
                    customer.patronymic = value;
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

        private void DateTimePicker_TextChanged(object sender, RoutedEventArgs e)
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

        private void DateTimePicker_TextChanged(object sender, KeyEventArgs e)
        {
            var dateTimePicker = sender as DatePicker;
            var dateTimePickerName = dateTimePicker.Name;
            var value = DateTime.Parse(dateTimePicker.Text);
            switch (dateTimePickerName)
            {
                case "datePickerBirthday":
                    customer.birthday = value;
                    break;
            }
        }

        private void ComboBox_TextChanged(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var comboBoxrName = comboBox.Name;
            var value = comboBox.Text;
            switch (comboBoxrName)
            {
                case "datePickerBirthday":
                    // customer.birthday = value;
                    break;
            }
        }

        
    }
}
