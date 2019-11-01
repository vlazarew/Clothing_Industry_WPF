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

namespace Clothing_Industry_WPF.Доходы
{
    /// <summary>
    /// Логика взаимодействия для IncomeRecordWindow.xaml
    /// </summary>
    public partial class IncomeRecordWindow : Window
    {
        private Income income;
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;

        public IncomeRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();
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
                    comboBoxMounth.Items.Add("Январь");
                    comboBoxMounth.Items.Add("Февраль");
                    comboBoxMounth.Items.Add("Март");
                    comboBoxMounth.Items.Add("Апрель");
                    comboBoxMounth.Items.Add("Май");
                    comboBoxMounth.Items.Add("Июнь");
                    comboBoxMounth.Items.Add("Июль");
                    comboBoxMounth.Items.Add("Август");
                    comboBoxMounth.Items.Add("Сентябрь");
                    comboBoxMounth.Items.Add("Октябрь");
                    comboBoxMounth.Items.Add("Ноябрь");
                    comboBoxMounth.Items.Add("Декабрь");
        }

        private string CheckData()
        {
            string result = "";

            if (comboBoxMounth.SelectedValue == null)
            {
                result += result == "" ? "Месяц" : ", Месяц";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        public static class csMounth
        {
            public static int MounthOfIncome { get; set; }

        }

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
            if (warning == "")
            {
                csMounth.MounthOfIncome = comboBoxMounth.SelectedIndex;
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
