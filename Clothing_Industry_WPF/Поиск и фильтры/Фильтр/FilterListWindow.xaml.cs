using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры.Фильтр;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using static Clothing_Industry_WPF.Перечисления.TypeOfFilter;

namespace Clothing_Industry_WPF.Поиск_и_фильтры
{


    /// <summary>
    /// Логика взаимодействия для FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        // Список полей, по которым мы можем делать отбор!
        private List<FindHandler.FieldParameters> listOfFields;


        // Результат
        public List<FilterHandler.FilterDescription> Result { get; set; }

        // Прошлый фильтр
        private List<FilterHandler.FilterDescription> filterDescription;

        // Типы фильтров
        private List<KeyValuePair<TypeOfFilter.TypesOfFilter, string>> typesOfFilter;

        private ObservableCollection<FilterHandler.FilterDescription> collection;

        public FilterWindow(List<FilterHandler.FilterDescription> filterDescription, List<FindHandler.FieldParameters> listOfFields = null)
        {
            InitializeComponent();
            this.listOfFields = listOfFields;
            this.filterDescription = filterDescription;
            collection = new ObservableCollection<FilterHandler.FilterDescription>();
            dataGridFilters.ItemsSource = collection;
            this.typesOfFilter = FilterHandler.FillTypesOfFilter();

            if (filterDescription.Count > 0)
            {
                FillPreviousFilter();
            }
        }

        private void FillPreviousFilter()
        {
            collection = new ObservableCollection<FilterHandler.FilterDescription>(filterDescription);
            dataGridFilters.ItemsSource = collection;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonAddFilter_Click(object sender, RoutedEventArgs e)
        {
            var addFilter = new FilterRecordWindow(Перечисления.WaysToOpenForm.WaysToOpen.create, listOfFields, new FilterHandler.FilterDescription());
            if (addFilter.ShowDialog().Value)
            {
                var result = addFilter.Result;
                collection.Add(new FilterHandler.FilterDescription()
                {
                    active = result.active,
                    field = result.field,
                    typeOfFilter = result.typeOfFilter,
                    filterValue = result.filterValue,
                    value = result.value,
                    isDate = result.isDate,
                    isNumber = result.isNumber
                });
                dataGridFilters.Items.Refresh();
            }
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = dataGridFilters.SelectedIndex;
            var editWindow = new FilterRecordWindow(Перечисления.WaysToOpenForm.WaysToOpen.edit, listOfFields, collection[rowIndex]);
            if (editWindow.ShowDialog().Value)
            {
                var result = editWindow.Result;
                collection[rowIndex] = new FilterHandler.FilterDescription()
                {
                    active = result.active,
                    field = result.field,
                    typeOfFilter = result.typeOfFilter,
                    filterValue = result.filterValue,
                    value = result.value,
                    isDate = result.isDate,
                    isNumber = result.isNumber
                };
                dataGridFilters.Items.Refresh();
            }

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            List<FilterHandler.FilterDescription> result = new List<FilterHandler.FilterDescription>(collection);
            Result = result;
            Close();
        }

        private void ButtonDeleteFilter_Click(object sender, RoutedEventArgs e)
        {
            int rowIndex = dataGridFilters.SelectedIndex;
            if (rowIndex == -1)
            {
                MessageBox.Show("Вы не выбрали элемент для удаления. Выделите строку!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                collection.RemoveAt(rowIndex);
                dataGridFilters.Items.Refresh();
            }
        }
    }
}
