using Clothing_Industry_WPF.Перечисления;
using System;
using System.Collections.Generic;
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
using static Clothing_Industry_WPF.Перечисления.WaysToOpenForm;

namespace Clothing_Industry_WPF.Поиск_и_фильтры.Фильтр
{
    /// <summary>
    /// Логика взаимодействия для FilterRecordWindow.xaml
    /// </summary>
    public partial class FilterRecordWindow : Window
    {
        // Результат
        public FilterHandler.FilterDescription Result { get; set; }

        // Ввод только букв в численные поля 
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        // Доступные поля для фильтрации
        private List<FindHandler.FieldParameters> listOfFields;
        // Типы фильтров
        private List<KeyValuePair<TypeOfFilter.TypesOfFilter, string>> typesOfFilter;

        private WaysToOpen way;

        private bool isNumber;
        private bool isDate;

        private FilterHandler.FilterDescription lastFilter;

        public FilterRecordWindow(WaysToOpen way, List<FindHandler.FieldParameters> listOfFields, FilterHandler.FilterDescription lastFilter)
        {
            InitializeComponent();
            isDate = false;
            isNumber = false;

            this.way = way;
            this.listOfFields = listOfFields;
            this.typesOfFilter = FilterHandler.FillTypesOfFilter();
            this.lastFilter = lastFilter;
            setNewTitle();
            FillComboBoxes();

            if (lastFilter.value != null)
            {
                isDate = lastFilter.isDate;
                isNumber = lastFilter.isNumber;
                FillPreviousFilter(lastFilter);
            }
        }

        private void FillPreviousFilter(FilterHandler.FilterDescription lastFilter)
        {
            comboBoxField.SelectedItem = lastFilter.field;
            if (lastFilter.isDate)
            {
                datePickerValue.SelectedDate = Convert.ToDateTime(lastFilter.value);
            }
            else
            {
                textBoxValue.Text = lastFilter.value;
            }

            checkBoxActive.IsChecked = lastFilter.active;
            comboBoxTypeOfFilter.SelectedItem = lastFilter.filterValue;
            textBoxValue.Visibility = lastFilter.isDate ? Visibility.Hidden : Visibility.Visible;
            datePickerValue.Visibility = lastFilter.isDate ? Visibility.Visible : Visibility.Hidden;
        }

        private void FillComboBoxes()
        {
            // Заполнение полей
            foreach (var field in listOfFields)
            {
                comboBoxField.Items.Add(field.application_name);
            }

            comboBoxField.SelectedIndex = 0;
            var firstType = listOfFields.ElementAt(0).type;
            isNumber = firstType.IndexOf("float") != -1 || firstType.IndexOf("int") != -1;
            isDate = firstType.IndexOf("date") != -1;
            if (isDate)
            {
                datePickerValue.SelectedDate = DateTime.Now;
                datePickerValue.Visibility = Visibility.Visible;
                textBoxValue.Visibility = Visibility.Hidden;
            }

            // Заполнение видов сравнения
            foreach (var typeFilter in typesOfFilter)
            {
                comboBoxTypeOfFilter.Items.Add(typeFilter.Value);
            }

            comboBoxTypeOfFilter.SelectedIndex = 0;
            textBoxValue.Focus();
        }

        private void setNewTitle()
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    this.Title += " (Создание)";

                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";

                    break;
                default:
                    break;

            }
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TextBoxValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (isNumber)
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
            else
            {
                e.Handled = false;
            }
        }

        private void ComboBoxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedValue = comboBoxField.SelectedItem.ToString();
            string selectedType = listOfFields.Where(value => value.application_name == selectedValue).First().type;


            if (selectedType.IndexOf("date") != -1)
            {
                datePickerValue.SelectedDate = DateTime.Now;
                datePickerValue.Visibility = Visibility.Visible;
                textBoxValue.Visibility = Visibility.Hidden;
                isDate = true;
                isNumber = false;
                datePickerValue.Focus();
            }
            else
            {
                if (selectedType.IndexOf("float") != -1 || selectedType.IndexOf("int") != -1)
                {
                    textBoxValue.Text = "";
                    textBoxValue.Visibility = Visibility.Visible;
                    datePickerValue.Visibility = Visibility.Hidden;
                    isDate = false;
                    isNumber = true;
                }
                else
                {
                    textBoxValue.Text = "";
                    textBoxValue.Visibility = Visibility.Visible;
                    datePickerValue.Visibility = Visibility.Hidden;
                    isDate = false;
                    isNumber = false;
                }
                textBoxValue.Focus();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            FilterHandler.FilterDescription result = new FilterHandler.FilterDescription();
            result.field = comboBoxField.SelectedItem.ToString();
            result.typeOfFilter = typesOfFilter.Where(key => key.Value == comboBoxTypeOfFilter.SelectedItem.ToString()).First().Key;
            result.value = isDate ? datePickerValue.SelectedDate.Value.ToShortDateString() : textBoxValue.Text;
            result.filterValue = comboBoxTypeOfFilter.SelectedItem.ToString();
            result.isDate = isDate;
            result.active = checkBoxActive.IsChecked.Value;
            Result = result;

            Close();
        }
    }
}
