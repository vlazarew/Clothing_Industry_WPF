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

namespace Clothing_Industry_WPF.Отпуска
{
    /// <summary>
    /// Логика взаимодействия для HolidaysRecordWindow.xaml
    /// </summary>
    public partial class HolidaysRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string old_login = "";

        public HolidaysRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, string login = "")
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            if (WaysToOpenForm.WaysToOpen.edit != way)
            {
                FillComboBoxes();
            }
            else
            {
                comboBoxLogin.Items.Add(login);
            }

            if (login != "")
            {
                old_login = login;
                FillFields(login);
            }
        }

        private void FillFields(string login)
        {
            string query_text = "select employees.Login, holidays.Year, holidays.Days_Of_Holidays, holidays.Days_Used, holidays.Rest_Of_Days, DATE_FORMAT(holidays.Planned_Start, \"%d.%m.%Y\") as Planned_Start, DATE_FORMAT(holidays.In_Fact_Start, \"%d.%m.%Y\") as In_Fact_Start, " +
                                " DATE_FORMAT(holidays.In_Fact_End, \"%d.%m.%Y\") as In_Fact_End, holidays.Notes" +
                                " from holidays" +
                                " join employees on holidays.Employees_Login = employees.Login" +
                                " where employees.Login = @login;";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@login", login);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxLogin.SelectedValue = reader.GetString(0);
                    datePickerIn_Fact_Start.SelectedDate = DateTime.Parse(reader.GetString(6));
                    datePickerIn_Fact_End.SelectedDate = DateTime.Parse(reader.GetString(7));
                    if (reader.GetValue(8).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(8);
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

            string query = "select Login from employees";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxLogin.Items.Add(reader.GetString(0));
                }
            }
            connection.Close();
        }

        // Не знаю, полей как-то маловато, может ты сделал, что все считается, но мне прям страшно вот тут!
        private string CheckData()
        {
            string result = "";

            if (comboBoxLogin.Text == "")
            {
                result += result == "" ? "Логин" : ", Логин";
            }
            if (datePickerIn_Fact_Start.SelectedDate == null)
            {
                result += result == "" ? "Начало отпуска" : ", Начало отпуска";
            }
            if (datePickerIn_Fact_End.SelectedDate == null)
            {
                result += result == "" ? "Конец отпуска" : ", Конец отпуска";
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

                //Создать/изменить запись в таблице Материалы
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    this.Close();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                connection.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO  holidays " +
                        " (Employees_Login, Year, Days_Of_Holidays, Days_Used, Rest_Of_Days, Planned_Start, In_Fact_Start, " +
                        " In_Fact_End, Notes) " +
                        " VALUES (@login, @Year, @Days_Of_Holidays, @Days_Used, " +
                        " @Rest_Of_Days, @In_Fact_Start, @In_Fact_Start, @In_Fact_End, @Notes); ";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update holidays set  Year = @Year, Days_Of_Holidays = @Days_Of_Holidays, Days_Used = @Days_Used, Rest_Of_Days = @Rest_Of_Days," +
                        " In_Fact_Start = @In_Fact_Start, In_Fact_End = @In_Fact_End, Notes = @Notes" +
                        " where Employees_Login = @oldLogin;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@In_Fact_Start", datePickerIn_Fact_Start.SelectedDate.Value);
            command.Parameters.AddWithValue("@In_Fact_End", datePickerIn_Fact_End.SelectedDate.Value);
            command.Parameters.AddWithValue("@Notes", textBoxNotes.Text);
            command.Parameters.AddWithValue("@login", comboBoxLogin.SelectedItem.ToString());
            //cумма дней 
            MySqlCommand commandsumdays = new MySqlCommand("select timestampdiff(DAY,@In_Fact_Start,@In_Fact_End);", connection);
            commandsumdays.Parameters.AddWithValue("@In_Fact_Start", datePickerIn_Fact_Start.SelectedDate.Value);
            commandsumdays.Parameters.AddWithValue("@In_Fact_End", datePickerIn_Fact_End.SelectedDate.Value);
            int days_sum = -1;
            using (DbDataReader reader = commandsumdays.ExecuteReader())
            {
                while (reader.Read())
                {
                    days_sum = reader.GetInt32(0);
                }
            }
            if (days_sum < 0)
            {
                System.Windows.MessageBox.Show("Дата начала отпуска меньше чем конец отпуска!", "Ошибка в данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                command.Parameters.AddWithValue("@Days_Of_Holidays", days_sum);
            }
            //вычет дней остаток и ушло
            MySqlCommand commanddays = new MySqlCommand("select timestampdiff(DAY,@In_Fact_Start,CURDATE());", connection);
            commanddays.Parameters.AddWithValue("@In_Fact_Start", datePickerIn_Fact_Start.SelectedDate.Value);
            int days_used = -1;
            using (DbDataReader reader = commanddays.ExecuteReader())
            {
                while (reader.Read())
                {
                    days_used = reader.GetInt32(0);
                }
            }
            if (days_used < 0)
            {
                command.Parameters.AddWithValue("@Days_Used", 0);
                command.Parameters.AddWithValue("@Rest_Of_Days", days_sum);
            }
            else
            {
                if (days_used >= days_sum)
                {
                    command.Parameters.AddWithValue("@Days_Used", days_sum);
                    command.Parameters.AddWithValue("@Rest_Of_Days", 0);
                }
                else
                {
                    command.Parameters.AddWithValue("@Days_Used", days_used);
                    command.Parameters.AddWithValue("@Rest_Of_Days", days_sum - days_used);
                }
            }
            //Год
            MySqlCommand commandyear = new MySqlCommand(" select Year(@In_Fact_Start); ", connection);
            commandyear.Parameters.AddWithValue("@In_Fact_Start", datePickerIn_Fact_Start.SelectedDate.Value);
            int year = -1;
            using (DbDataReader reader = commandyear.ExecuteReader())
            {
                while (reader.Read())
                {
                    year = reader.GetInt32(0);
                }
            }
            command.Parameters.AddWithValue("@Year", year);
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldLogin", old_login);
            }

            return command;
        }
    }
}
