using Clothing_Industry_WPF.Перечисления;
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

namespace Clothing_Industry_WPF.Начисление_ЗП
{
    /// <summary>
    /// Логика взаимодействия для PayrollsRecordWindow.xaml
    /// </summary>
    public partial class PayrollsRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string old_login = "";
        private DateTime old_payrollDate;

        // Ввод только букв в численные поля 
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        public PayrollsRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, string login = "", DateTime? payrollDate = null)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();

            if (login != "")
            {
                old_login = login;
                old_payrollDate = payrollDate.Value;
                FillFields(login, old_payrollDate);
            }
        }

        private void FillFields(string login, DateTime payrollDate)
        {
            string query_text = "SELECT Employees_Login as Login, Period, Date_Of_Pay, Salary, PieceWorkPayment, Total_Salary, Penalty, To_Pay, Notes " +
                                "FROM payrolls " +
                                " where payrolls.Employees_Login = @login and payrolls.Date_Of_Pay = @payrollDate";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@payrollDate", payrollDate);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxLogin.SelectedValue = reader.GetString(0);
                    textBoxPeriod.Text = reader.GetString(1);
                    datePickerPayrollDate.SelectedDate = DateTime.Parse(reader.GetString(2));
                    textBoxSalary.Text = reader.GetString(3);
                    if (reader.GetValue(4).ToString() != "")
                    {
                        textBoxPieceWork.Text = reader.GetString(4);
                    }
                    textBoxTotal_Salary.Text = reader.GetString(5);
                    if (reader.GetValue(6).ToString() != "")
                    {
                        textBoxPenalty.Text = reader.GetString(6);
                    }
                    textBoxTo_Pay.Text = reader.GetString(7);
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
                    datePickerPayrollDate.Text = DateTime.Now.ToLongDateString();
                    textBoxPeriod.Text = DateTime.Now.Month + "." + DateTime.Now.Year;
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

            string query = "select login from employees";
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

        private string CheckData()
        {
            string result = "";

            if (comboBoxLogin.SelectedValue == null)
            {
                result += result == "" ? "Логин" : ", Логин";
            }
            if (textBoxPeriod.Text == "")
            {
                result += result == "" ? "Период" : ", Период";
            }
            if (datePickerPayrollDate.SelectedDate.Value.ToString() == "")
            {
                result += result == "" ? "Дата выплаты" : ", Дата выплаты";
            }
            if (textBoxSalary.Text == "")
            {
                result += result == "" ? "Зарплата" : ", Зарплата";
            }
            if (textBoxTotal_Salary.Text == "")
            {
                result += result == "" ? "Общая зп" : ", Общая зп";
            }
            if (textBoxTo_Pay.Text == "")
            {
                result += result == "" ? "К выплате" : ", К выплате";
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

                //Создать/изменить запись в таблице Начисления ЗП
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                //try
                //{
                command.ExecuteNonQuery();
                transaction.Commit();
                this.Hide();
                /*}
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!");
                }*/

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
                query = "INSERT INTO payrolls " +
                                       "(Employees_Login, Period, Date_Of_Pay, Salary, PieceWorkPayment, Total_Salary, Penalty, To_Pay, Notes)" +
                                       " VALUES (@login, @period, @dateOfPay, @salary, @pieceWorkPayment, @totalSalary, @penalty, @toPay, @notes);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update payrolls set Employees_Login = @login, Period = @period, Date_Of_Pay = @dateOfPay, Salary = @salary, PieceWorkPayment = @pieceWorkPayment, " +
                        "Total_Salary = @totalSalary, Penalty = @penalty, To_Pay = @toPay, Notes = @notes " +
                        " where Employees_Login = @oldLogin and Date_Of_Pay = @oldDateOfPay ;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@login", comboBoxLogin.Text);
            command.Parameters.AddWithValue("@period", textBoxPeriod.Text);
            command.Parameters.AddWithValue("@dateOfPay", datePickerPayrollDate.SelectedDate.Value);
            command.Parameters.AddWithValue("@salary", textBoxSalary.Text);
            command.Parameters.AddWithValue("@pieceWorkPayment", textBoxPieceWork.Text == "" ? 0.ToString() : textBoxPieceWork.Text);
            command.Parameters.AddWithValue("@totalSalary", textBoxTotal_Salary.Text);
            command.Parameters.AddWithValue("@penalty", textBoxPenalty.Text == "" ? 0.ToString() : textBoxPenalty.Text);
            command.Parameters.AddWithValue("@toPay", textBoxTo_Pay.Text);
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);


            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldLogin", old_login);
                command.Parameters.AddWithValue("@oldDateOfPay", old_payrollDate);
            }

            return command;
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TextBoxSalary_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBoxSalary_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshTotal();
        }

        private void TextBoxPieceWork_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshTotal();
        }

        private void TextBoxPenalty_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshTotal();
        }

        private void RefreshTotal()
        {
            float salary = textBoxSalary.Text != "" ? float.Parse(textBoxSalary.Text) : 0;
            float piecework = textBoxPieceWork.Text != "" ? float.Parse(textBoxPieceWork.Text) : 0;

            float totalSalary = salary + piecework;
            textBoxTotal_Salary.Text = totalSalary.ToString();

            float penalty = textBoxPenalty.Text != "" ? float.Parse(textBoxPenalty.Text) : 0;

            float toPay = totalSalary - penalty;

            textBoxTo_Pay.Text = toPay.ToString();
        }


    }
}
