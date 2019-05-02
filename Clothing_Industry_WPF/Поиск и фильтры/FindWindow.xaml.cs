using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Сотрудники;
using System;
using System.Collections.Generic;
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

namespace Clothing_Industry_WPF.Поиск_и_фильтры
{
    /// <summary>
    /// Логика взаимодействия для FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        private List<KeyValuePair<string, string>> listOfFields;
        private bool isNowTime;
        private bool isNowNumber;
        public FindHandler.FindDescription Result { get; set; }

        public FindWindow(List<KeyValuePair<string, string>> listOfFields = null)
        {
            InitializeComponent();
            this.listOfFields = listOfFields;
            datePicker.Visibility = Visibility.Hidden;
            isNowTime = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboBox();
        }

        private void FillComboBox()
        {
            foreach (var pair in listOfFields)
            {
                comboBoxField.Items.Add(pair.Value);
            }
        }

        private void ComboBoxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedValue = comboBoxField.SelectedItem.ToString();

            if (selectedValue.IndexOf("Дата") != -1 && !isNowTime)
            {
                datePicker.SelectedDate = DateTime.Now;
                datePicker.Visibility = Visibility.Visible;
                textBoxValue.Text = "";
                textBoxValue.Visibility = Visibility.Hidden;
                isNowTime = true;
            }
            else
            {
                if (isNowTime)
                {
                    textBoxValue.Text = "";
                    textBoxValue.Visibility = Visibility.Visible;
                    datePicker.SelectedDate = DateTime.Now;
                    datePicker.Visibility = Visibility.Hidden;
                    isNowTime = false;
                }
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            FindHandler.FindDescription result = new FindHandler.FindDescription();
            result.field = comboBoxField.SelectedItem.ToString();
            result.typeOfFind = radioButtonExact.IsChecked.Value ? TypeOfFind.TypesOfFind.byExactCoincidence : TypeOfFind.TypesOfFind.byPart;
            result.value = isNowTime ? datePicker.SelectedDate.Value.ToShortDateString() : textBoxValue.Text;
            result.isDate = isNowTime;
            Result = result;

            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
