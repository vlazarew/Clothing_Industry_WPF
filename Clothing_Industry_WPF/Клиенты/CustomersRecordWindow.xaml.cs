using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string image_path;
        private byte[] image_bytes;
        private int idRecord;

        public CustomersRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id = -1)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            this.idRecord = id;
            setNewTitle();
            FillComboBoxes();

            if (idRecord != -1)
            {
                FillFields(id);
            }
        }

        private void FillFields(int id)
        {
            string query_text = "SELECT customers.id_Customer, customers.Name, customers.Lastname, customers.Patronymic, customers.Address, customers.Phone_Number, customers.Nickname, " +
                                "DATE_FORMAT(customers.Birthday, \"%d.%m.%Y\") as Birthday, customers.Passport_data, customers.Size, customers.Parameters, customers.Notes, customer_statuses.Name_Of_Status, " +
                                "order_channels.Name_of_channel, employees.Login, customers.Photo " +
                                "FROM customers " +
                                "join main_database.employees on main_database.employees.login = customers.Employees_Login " +
                                "join main_database.customer_statuses on main_database.customer_statuses.id_Status = customers.Customer_Statuses_id_Status " +
                                "join main_database.order_channels on main_database.order_channels.id_Channel = customers.Order_Channels_id_Channel " +
                                "where customers.id_Customer = @id";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxName.Text = reader.GetString(1);
                    textBoxLastname.Text = reader.GetString(2);
                    textBoxPatronymic.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    textBoxAdress.Text = reader.GetString(4);
                    textBoxPhone_Number.Text = reader.GetString(5);
                    textBoxNickname.Text = reader.GetString(6);
                    datePickerBirthday.SelectedDate = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7));
                    textBoxPassportData.Text = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    if (reader.GetValue(9).ToString() != "")
                    {
                        textBoxSize.Text = reader.GetString(9);
                    }
                    if (reader.GetValue(10).ToString() != "")
                    {
                        textBoxParameters.Text = reader.GetString(10);
                    }
                    if (reader.GetValue(11).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(11);
                    }

                    comboBoxStatus.SelectedValue = reader.GetString(12);
                    comboBoxChannel.SelectedValue = reader.GetString(13);
                    comboBoxEmployee.SelectedValue = reader.GetString(14);

                    image_bytes = null;
                    try
                    {
                        image_bytes = (byte[])(reader[15]);
                    }
                    catch
                    {

                    }

                    if (image_bytes == null)
                    {
                        imagePhoto.Source = null;
                    }
                    else
                    {
                        MemoryStream stream = new MemoryStream(image_bytes);
                        imagePhoto.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
            }
            connection.Close();

        }

        private void setNewTitle()
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    this.Title += " (Создание)";
                    Header.Content += " (Создание)";
                    datePickerBirthday.Text = DateTime.Now.ToLongDateString();
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";
                    Header.Content += " (Изменение)";
                    break;
                default:
                    break;

            }
        }

        private void ButtonAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Картинки (*.JPG; *.GIF; *.JPEG; *.PNG)|*.JPG; *.GIF; *.JPEG; *.PNG" + "|Все файлы (*.*)|*.* ";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                image_path = openFileDialog.FileName;
                imagePhoto.Source = new BitmapImage(new Uri(image_path));
            }
        }

        private void FillComboBoxes()
        {
            connection.Open();

            string query = "select name_of_status from customer_statuses";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxStatus.Items.Add(reader.GetString(0));
                }
            }

            query = "select name_of_channel from order_channels";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxChannel.Items.Add(reader.GetString(0));
                }
            }

            query = "select Login from employees";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxEmployee.Items.Add(reader.GetString(0));
                }
            }
            connection.Close();
        }

        private string CheckData()
        {
            string result = "";

            if (textBoxName.Text == "")
            {
                result += result == "" ? "Имя" : ", Имя";
            }
            if (textBoxLastname.Text == "")
            {
                result += result == "" ? "Фамилия" : ", Фамилия";
            }
            if (textBoxAdress.Text == "")
            {
                result += result == "" ? "Адрес" : ", Адрес";
            }
            if (textBoxPhone_Number.Text == "")
            {
                result += result == "" ? "Телефонный номер" : ", Телефонный номер";
            }
            if (textBoxNickname.Text == "")
            {
                result += result == "" ? "Никнейм" : ", Никнейм";
            }
            if (textBoxSize.Text == "")
            {
                result += result == "" ? "Размер" : ", Размер";
            }
            if (textBoxParameters.Text == "")
            {
                result += result == "" ? "Параметры" : ", Параметры";
            }
            if (comboBoxStatus.SelectedValue == null)
            {
                result += result == "" ? "Статус клиента" : ", Статус клиента";
            }
            if (comboBoxChannel.SelectedValue == null)
            {
                result += result == "" ? " Канал связи" : ",  Канал связи";
            }
            if (comboBoxEmployee.SelectedValue == null)
            {
                result += result == "" ? "Обслуживающий сотрудник" : ", Обслуживающий сотрудник";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
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
                    MessageBox.Show("Ошибка сохранения!");
                }

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
                    MessageBox.Show("Ошибка сохранения!");
                }

                connection.Close();
            }
            else
            {
                MessageBox.Show(warning);
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
            command.Parameters.AddWithValue("@address", textBoxAdress.Text);
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
            if (image_path != null)
            {
                FileStream fileStream = new FileStream(image_path, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                byte[] imageData = binaryReader.ReadBytes((int)fileStream.Length);
                binaryReader.Close();
                fileStream.Close();
                command.Parameters.AddWithValue("@image", imageData);
            }
            else
            {
                if (image_bytes != null)
                {
                    command.Parameters.AddWithValue("@image", image_bytes);
                }
                else
                {
                    command.Parameters.AddWithValue("@image", null);
                }
            }

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@id", idRecord);
            }

            return command;
        }
    }
}
