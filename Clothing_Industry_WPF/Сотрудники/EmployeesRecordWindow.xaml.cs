using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clothing_Industry_WPF.Сотрудники
{
    /// <summary>
    /// Логика взаимодействия для EmployeesRecordWindow.xaml
    /// </summary>
    public partial class EmployeesRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string image_path;
        private byte[] image_bytes;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string old_login = "";

        public EmployeesRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, string login = "")
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            ShowPassword(false);
            FillComboBoxes();

            if (login != "")
            {
                old_login = login;
                FillFields(login);
            }
        }

        private void FillFields(string login)
        {
            string query_text = "select employees.Login, employees.Password, employees.Name, employees.Lastname, employees.Patronymic, employees.Phone_Number, employees.Passport_Data, employees.Email," +
                               "employees.Notes, DATE_FORMAT(employees.Added, \"%d.%m.%Y\") as Added, employees.Last_Salary, employee_roles.Name_Of_Role, employee_positions.Name_Of_Position, employees.Photo" +
                               " from employees" +
                               " join employee_positions on employees.Employee_Positions_id_Employee_Position = employee_positions.id_Employee_Position" +
                               " join employee_roles on employees.Employee_Roles_id_Employee_Role = employee_roles.id_Employee_Role" +
                               " where employees.Login = @login";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@login", login);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxLogin.Text = reader.GetString(0);
                    PasswordBoxCurrent.Password = reader.GetString(1);
                    textBoxPassword.Text = reader.GetString(1);
                    textBoxName.Text = reader.GetString(2);
                    textBoxLastname.Text = reader.GetString(3);
                    textBoxPatronymic.Text = reader.GetString(4);
                    textBoxPhone_Number.Text = reader.GetString(5);
                    textBoxPassportData.Text = reader.GetString(6);
                    textBoxEmail.Text = reader.GetString(7);
                    if (reader.GetValue(8).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(8);
                    }
                    datePickerAdded.SelectedDate = DateTime.Parse(reader.GetString(9));
                    if (reader.GetValue(10).ToString() != "")
                    {
                        textBoxLastSalary.Text = reader.GetString(10);
                    }
                    comboBoxRole.SelectedValue = reader.GetString(11);
                    comboBoxPosition.SelectedValue = reader.GetString(12);

                    image_bytes = null;
                    try
                    {
                        image_bytes = (byte[])(reader[13]);
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
                    datePickerAdded.Text = DateTime.Now.ToLongDateString();
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";
                    Header.Content += " (Изменение)";
                    break;
                default:
                    break;

            }
        }

        private void FillComboBoxes()
        {
            connection.Open();

            string query = "select name_of_position from employee_positions";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxPosition.Items.Add(reader.GetString(0));
                }
            }

            query = "select name_of_role from employee_roles";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxRole.Items.Add(reader.GetString(0));
                }
            }
            connection.Close();
        }

        private void ShowPassword(bool showPassword)
        {
            switch (showPassword)
            {
                case true:
                    textBoxPassword.Text = PasswordBoxCurrent.Password;
                    textBoxPassword.TabIndex = 2;
                    textBoxPassword.Visibility = Visibility.Visible;
                    PasswordBoxCurrent.Visibility = Visibility.Hidden;
                    break;
                default:
                    PasswordBoxCurrent.Password = textBoxPassword.Text;
                    PasswordBoxCurrent.TabIndex = 2;
                    PasswordBoxCurrent.Visibility = Visibility.Visible;
                    textBoxPassword.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void CheckBoxPassword_Click(object sender, RoutedEventArgs e)
        {
            ShowPassword(CheckBoxPassword.IsChecked.Value);
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

        private string CheckData()
        {
            string result = "";

            if (textBoxLogin.Text == "")
            {
                result += result == "" ? "Логин" : ", Логин";
            }
            if (textBoxPassword.Text == "" && PasswordBoxCurrent.Password == "")
            {
                result += result == "" ? "Пароль" : ", Пароль";
            }
            if (textBoxName.Text == "")
            {
                result += result == "" ? "Имя" : ", Имя";
            }
            if (textBoxLastname.Text == "")
            {
                result += result == "" ? "Фамилия" : ", Фамилия";
            }
            if (textBoxPhone_Number.Text == "")
            {
                result += result == "" ? "Телефонный номер" : ", Телефонный номер";
            }
            if (textBoxEmail.Text == "")
            {
                result += result == "" ? "E-mail" : ", E-mail";
            }
            if (textBoxPassportData.Text == "")
            {
                result += result == "" ? "Паспортные данные" : ", Паспортные данные";
            }
            if (datePickerAdded.SelectedDate == null)
            {
                result += result == "" ? "Дата добавления" : ", Дата добавления";
            }
            if (comboBoxPosition.SelectedValue == null)
            {
                result += result == "" ? " Должность" : ",  Должность";
            }
            if (comboBoxRole.SelectedValue == null)
            {
                result += result == "" ? "Роль" : ", Роль";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }



        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
            if (warning == "")
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                //Создать/изменить запись в таблице Пользователи
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                //Создание/изменение пользователя в БД
                string queryUser = "";
                //Выдача прав пользователю
                string queryGrants = "";
                //Читаемости ради                    
                if (way == WaysToOpenForm.WaysToOpen.create)
                {
                    queryUser = string.Format("CREATE USER '{0}'@'%' IDENTIFIED BY '{1}';", textBoxLogin.Text,
                    CheckBoxPassword.IsChecked.Value ? textBoxPassword.Text : PasswordBoxCurrent.Password);
                    queryGrants = string.Format("GRANT ALL PRIVILEGES ON main_database.* TO '{0}'@'%'", textBoxLogin.Text);
                }
                if (way == WaysToOpenForm.WaysToOpen.edit && old_login != textBoxLogin.Text)
                {
                    queryUser = string.Format("Rename user '{0}'@'%' To '{1}'@'%';", old_login, textBoxLogin.Text);
                }
                MySqlCommand commandUser = new MySqlCommand(queryUser, connection, transaction);
                MySqlCommand commandGrants = new MySqlCommand(queryGrants, connection, transaction);

                try
                {
                    command.ExecuteNonQuery();
                    if (queryUser != "")
                    {
                        commandUser.ExecuteNonQuery();
                    }
                    if (queryGrants != "")
                    {
                        commandGrants.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    this.Hide();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!");
                }

                connection.Close();
                //this.Hide();
            }
            else
            {
                System.Windows.MessageBox.Show(warning);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO employees " +
                                       "(Login, Password, Name, Lastname, Patronymic, Phone_Number, Email," +
                                       " Passport_Data, Notes, Added, Last_Salary, Employee_Roles_id_Employee_Role, Employee_Positions_id_Employee_Position, Photo)" +
                                       " VALUES (@login, @password, @name, @lastname, @patronymic, @phone, @email, @passport, @notes, @added, @lastSalary, @role, @position, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update employees set Login = @login, Password = @password, Name = @name, Lastname = @lastname, Patronymic = @patronymic, Phone_Number = @phone, " +
                        "Email = @email, Passport_Data = @passport, Notes = @notes, Added = @added, Last_Salary = @lastSalary, " +
                        "Employee_Roles_id_Employee_Role = @role, Employee_Positions_id_Employee_Position = @position, Photo = @image" +
                        " where Login = @oldLogin;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@login", textBoxLogin.Text);
            command.Parameters.AddWithValue("@password", CheckBoxPassword.IsChecked.Value ? textBoxPassword.Text : PasswordBoxCurrent.Password);
            command.Parameters.AddWithValue("@name", textBoxName.Text);
            command.Parameters.AddWithValue("@lastname", textBoxLastname.Text);
            command.Parameters.AddWithValue("@patronymic", textBoxPatronymic.Text);
            command.Parameters.AddWithValue("@phone", textBoxPhone_Number.Text);
            command.Parameters.AddWithValue("@email", textBoxEmail.Text);
            command.Parameters.AddWithValue("@passport", textBoxPassportData.Text);
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);
            command.Parameters.AddWithValue("@added", datePickerAdded.SelectedDate.Value);
            command.Parameters.AddWithValue("@lastSalary", float.Parse(textBoxLastSalary.Text == "" ? "0" : textBoxLastSalary.Text));

            MySqlCommand commandRole = new MySqlCommand("select id_Employee_Role from employee_roles where Name_Of_Role = @role", connection);
            commandRole.Parameters.AddWithValue("role", comboBoxRole.SelectedItem.ToString());
            int id_role = -1;
            using (DbDataReader reader = commandRole.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_role = reader.GetInt32(0);
                }
            }

            MySqlCommand commandPosition = new MySqlCommand("select id_Employee_Position from employee_positions where Name_Of_position = @position", connection);
            commandPosition.Parameters.AddWithValue("position", comboBoxPosition.SelectedItem.ToString());
            int id_position = -1;
            using (DbDataReader reader = commandRole.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_position = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@role", id_role);
            command.Parameters.AddWithValue("@position", id_position);


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
                command.Parameters.AddWithValue("@oldLogin", old_login);
            }

            return command;
        }

    }
}
