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
        private List<FindHandler.FieldParameters> listOfFields;
        private bool isNowTime;
        private bool isNowNumber;

        public FindHandler.FindDescription Result { get; set; }
        private FindHandler.FindDescription findDescription;

        public FindWindow(FindHandler.FindDescription findDescription, List<FindHandler.FieldParameters> listOfFields = null)
        {
            InitializeComponent();
            this.listOfFields = listOfFields;
            datePicker.Visibility = findDescription.isDate ? Visibility.Visible : Visibility.Hidden;
            textBoxValue.Visibility = findDescription.isDate ? Visibility.Hidden : Visibility.Visible;
            isNowTime = findDescription.isDate;
            isNowNumber = findDescription.isNumber;
            this.findDescription = findDescription;
        }

        private void FillPreviousFind(FindHandler.FindDescription findDescription)
        {
            comboBoxField.SelectedItem = findDescription.field;
            if (findDescription.isDate)
            {
                datePicker.SelectedDate = Convert.ToDateTime(findDescription.value);
            }
            else
            {
                textBoxValue.Text = findDescription.value;
            }

            textBoxValue.Visibility = findDescription.isDate ? Visibility.Hidden : Visibility.Visible;
            datePicker.Visibility = findDescription.isDate ? Visibility.Visible : Visibility.Hidden;

            // Фишка - минутка
            radioButtonExact.IsEnabled = !(isNowNumber || isNowTime);
            radioButtonPart.IsEnabled = !(isNowNumber || isNowTime);
            //
            if (!radioButtonExact.IsEnabled)
            {
                radioButtonExact.IsChecked = true;
            }
            else
            {
                radioButtonExact.IsChecked = findDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence;
                radioButtonPart.IsChecked = !(findDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboBox();
            if (findDescription.field != null)
            {
                isNowTime = findDescription.isDate;
                FillPreviousFind(findDescription);
            }
        }

        private void FillComboBox()
        {
            foreach (var pair in listOfFields)
            {
                comboBoxField.Items.Add(pair.application_name);
            }
            comboBoxField.SelectedIndex = 0;
        }

        private void ComboBoxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedValue = comboBoxField.SelectedItem.ToString();
            string selectedType = listOfFields.Where(value => value.application_name == selectedValue).First().type;

            // ЭТОТ КУСОК МНЕ НУЖНО ДОВЕСТИ ДО УМА, ТУТ МОЖНО СДЕЛАТЬ НЕСКОЛЬКО КРАСИВЕЕ И ВВЕСТИ ПРОВЕРКУ НА ЧИСЛА!!!
            if (selectedType.IndexOf("date") != -1 && !isNowTime)
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
            //
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
