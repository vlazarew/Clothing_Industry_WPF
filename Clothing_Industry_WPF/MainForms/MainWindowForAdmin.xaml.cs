using Clothing_Industry_WPF.Баланс_клиентов;
using Clothing_Industry_WPF.Доходы;
using Clothing_Industry_WPF.Заказы;
using Clothing_Industry_WPF.Изделия;
using Clothing_Industry_WPF.Клиенты;
using Clothing_Industry_WPF.Материал;
using Clothing_Industry_WPF.Начисление_ЗП;
using Clothing_Industry_WPF.Отпуска;
using Clothing_Industry_WPF.Примерки;
using Clothing_Industry_WPF.Приход_материалов;
using Clothing_Industry_WPF.Расходы;
using Clothing_Industry_WPF.Состояние_склада;
using Clothing_Industry_WPF.Сотрудники;
using Clothing_Industry_WPF.Справочник.Должности;
using Clothing_Industry_WPF.Справочник.Единицы_измерения;
using Clothing_Industry_WPF.Справочник.Каналы_заказов;
using Clothing_Industry_WPF.Справочник.Категории_расходов;
using Clothing_Industry_WPF.Справочник.Периодичности;
using Clothing_Industry_WPF.Справочник.Поставщики;
using Clothing_Industry_WPF.Справочник.Статусы_клиентов;
using Clothing_Industry_WPF.Справочник.Статусы_оплаты;
using Clothing_Industry_WPF.Справочник.Страны;
using Clothing_Industry_WPF.Справочник.Типы_заказов;
using Clothing_Industry_WPF.Справочник.Типы_материалов;
using Clothing_Industry_WPF.Справочник.Типы_оплаты;
using Clothing_Industry_WPF.Справочник.Типы_примерок;
using Clothing_Industry_WPF.Справочник.Типы_транзакций;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Clothing_Industry_WPF.MainForms
{
    /// <summary>
    /// Логика взаимодействия для WindowExperimental.xaml
    /// </summary>
    public partial class WindowExperimental : Window
    {
        private string login;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        public static RoutedCommand RoutedCommands = new RoutedCommand();

        public WindowExperimental(string entry_login = "")
        {
            InitializeComponent();
            login = entry_login;
            FillUsername();

        }

        // Заполнить ФИО в формочек
        private void FillUsername()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            string query_text = "select Name, Lastname, Patronymic from employees where login = @login";
            connection.Open();

            MySqlCommand query = new MySqlCommand(query_text, connection);
            query.Parameters.AddWithValue("@login", login);

            using (DbDataReader reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    string user_name = reader.GetString(0);
                    string user_lastname = reader.GetString(1);
                    string user_patronymic = reader.GetString(2);

                    if (user_name != "" && user_lastname != "")
                    {
                        Title += user_lastname + " " + user_name + " " + user_patronymic;
                        return;
                    }
                }
            }
            Title += "Гость";
        }
        private int CountForms()
        {
            int count = 0;
            foreach (Window w in App.Current.Windows)
                count++;
            return count;

        }
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void ListViewMenu2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Window auth_window;
            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ItemDictionary":
                    GridList.Visibility = Visibility.Collapsed;
                    GridListDictionary.Visibility = Visibility.Visible;
                    ItemDictionary.Visibility = Visibility.Collapsed;
                    ItemExitFromDicitionary.Visibility = Visibility.Visible;
                    if (ButtonOpenMenu.Visibility == Visibility.Visible)
                    {
                        ButtonOpenMenu_Click(this, null);
                        var OpenMenu = (Storyboard)this.Resources["OpenMenu"];
                        OpenMenu.Begin();
                    }

                    break;
                case "ItemExitFromDicitionary":
                    GridListDictionary.Visibility = Visibility.Collapsed;
                    GridList.Visibility = Visibility.Visible;
                    ItemExitFromDicitionary.Visibility = Visibility.Collapsed;
                    ItemDictionary.Visibility = Visibility.Visible;

                    break;
                case "ItemExit":

                    auth_window = new AuthentificationForm();
                    auth_window.Show();
                    Close();
                    
                    break;
                default:
                    break;
            }
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            Window windowhelp = new WindowHelp();
            windowhelp.ShowDialog();
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            Window windowinfo = new WindowInfo();
            windowinfo.ShowDialog();
        }

 



        private void AddItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Добавление");
        }

        private void EditItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Изменение");
        }

        private void DeleteItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Удаление");
        }

        private void RefreshTable_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Обновление таблы");
        }

        private void SearchItem_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Поиск элемента");
        }

        private void UseFilter_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Фильтры");
        }

        private void Print_Click(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Печать");
        }

        private void MakeExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Excel");
        }

        private void OpenList_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Лист");
        }

        private void ClearIncome_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DirtyIncome_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
