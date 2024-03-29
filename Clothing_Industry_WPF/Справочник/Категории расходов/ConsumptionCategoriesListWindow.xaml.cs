﻿using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace Clothing_Industry_WPF.Справочник.Категории_расходов
{
    /// <summary>
    /// Логика взаимодействия для ConsumptionCategoriesListWindow.xaml
    /// </summary>
    public partial class ConsumptionCategoriesListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private FindHandler.FindDescription currentFindDescription;
        private List<FilterHandler.FilterDescription> currentFilterDescription;

        public ConsumptionCategoriesListWindow()
        {
            InitializeComponent();
            currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();

        }

        private void RefreshList()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            string query_text = getQueryText();
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            productsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            List<int> ids = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                ids.Add((int)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }

        private string getQueryText()
        {
            string query_text = "select id_Consumption_Category, Name_Of_Category from consumption_categories;";


            return query_text;
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new ConsumptionCategoriesRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            RefreshList();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = productsGrid.SelectedIndex;
            int id = -1;
            int current_row = 0;
            foreach (DataRowView row in productsGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                id = (int)row.Row.ItemArray[0];
                break;
            }

            Window create_window = new ConsumptionCategoriesRecordWindow(WaysToOpenForm.WaysToOpen.edit, id);
            create_window.ShowDialog();
            RefreshList();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {

            List<int> idsToDelete = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                idsToDelete.Add((int)row.Row.ItemArray[0]);
            }

            DeleteFromDB(idsToDelete);

        }

        private void DeleteFromDB(List<int> ids)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (int id in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from consumption_categories where id_Consumption_Category = @id";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id", id);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Удаление  " + id + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
            RefreshList();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            List<int> idsToDelete = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                idsToDelete.Add((int)row.Row.ItemArray[0]);
            }

            if (idsToDelete.Count > 0)
            {
                Window create_window;

                //Первые окна мы открываем немодально, последнее модально, чтоб потом сразу обновились данные на форме
                if (idsToDelete.Count > 1)
                {
                    for (int i = 0; i < idsToDelete.Count - 1; i++)
                    {
                        create_window = new ConsumptionCategoriesRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToDelete[i]);
                        create_window.Show();
                    }
                }
                //Заключительная форма
                create_window = new ConsumptionCategoriesRecordWindow(WaysToOpenForm.WaysToOpen.edit, idsToDelete[idsToDelete.Count - 1]);
                create_window.ShowDialog();

                //Обновление списка
                RefreshList();
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields();

            var findWindow = new FindWindow(currentFindDescription, listOfField);
            if (findWindow.ShowDialog().Value)
            {
                currentFindDescription = findWindow.Result;
            }
            else
            {
                return;
            }

            var field = listOfField.Where(kvp => kvp.application_name == currentFindDescription.field).First().db_name;
            string query = getQueryText();
            string edited_query;
            edited_query = query.Replace(";", " where " + field + " ");
            edited_query += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
            MySqlConnection connection = new MySqlConnection(connectionString);
            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(edited_query, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            productsGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();

            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("Active");
        }

        private List<FindHandler.FieldParameters> FillFindFields()
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe();
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();
            result.Add(new FindHandler.FieldParameters("id_Consumption_Category", "Код", describe.Where(key => key.Key == "id_Consumption_Category").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Category", "Наименование", describe.Where(key => key.Key == "Name_Of_Category").First().Value));

            return result;
        }

        private List<KeyValuePair<string, string>> TakeDescribe()
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            DescribeHelper("describe consumption_categories", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        private void DescribeHelper(string query, MySqlConnection connection, List<KeyValuePair<string, string>> pairs)
        {
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    pairs.Add(new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)));
                }
            }
        }

        private void ButtonCancelFind_Click(object sender, RoutedEventArgs e)
        {
            currentFindDescription = new FindHandler.FindDescription();
            RefreshList();
            buttonCancelFind.Style = (Style)buttonCancelFind.FindResource("NoActive");
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonEdit.Style = (Style)ButtonEdit.FindResource("Active");
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }

        private void DataGridCell_LostFocus(object sender, RoutedEventArgs e)
        {
            List<int> ids = new List<int>();
            foreach (DataRowView row in productsGrid.SelectedItems)
            {
                ids.Add((int)row.Row.ItemArray[0]);
            }
            if (ids.Count == 0)
            {
                ButtonEdit.Style = (Style)ButtonEdit.FindResource("NoActive");
                ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
            }
        }
    }
}
