using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using Clothing_Industry_WPF.Поиск_файла;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
using MessageBox = System.Windows.Forms.MessageBox;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;
using PrintDialog = System.Windows.Controls.PrintDialog;
using static Clothing_Industry_WPF.Доходы.IncomeRecordWindow;

namespace Clothing_Industry_WPF.Доходы
{
    /// <summary>
    /// Логика взаимодействия для IncomeListWindow.xaml
    /// </summary>
    public partial class IncomeListWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;

        public IncomeListWindow()
        {
            InitializeComponent();
            clearincomeGrid.Visibility = Visibility.Hidden;
            LabelCosts.Visibility = Visibility.Hidden;
            LabelMinus.Visibility = Visibility.Hidden;
            textBoxMinus.Visibility = Visibility.Hidden;
            LabelMinus.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //RefreshList();
        }

        private void RefreshListDirty(string query_text)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            incomeGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            incomeGrid.SelectedIndex = 0;
        }

        private void RefreshListClear(string query_text)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            DataTable dataTable = new DataTable();
            MySqlCommand command = new MySqlCommand(query_text, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataTable);
            clearincomeGrid.ItemsSource = dataTable.DefaultView;
            connection.Close();
            clearincomeGrid.SelectedIndex = 0;
        }

        private void ButtonDirtyIncome_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new IncomeRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            string mounth = (csMounth.MounthOfIncome + 1).ToString();
            string query_text = "select orders.id_Order as id,orders.Total_Price as Count  from orders " +
                                "where month(orders.Date_Of_Delievery) = " + mounth + " and (orders.Statuses_Of_Order_id_Status_Of_Order = 2 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 3 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 4) and year(orders.Date_Of_Delievery) = year(curdate()); ";
            RefreshListDirty(query_text);
            float count = 0;
            foreach (DataRowView row in incomeGrid.Items)
            {
                count = count + (float)row.Row.ItemArray[1];
            }
            textBoxPlus.Text = count.ToString();
            textBoxCount.Text = count.ToString();
            clearincomeGrid.Visibility = Visibility.Hidden;
            LabelCosts.Visibility = Visibility.Hidden;
            LabelMinus.Visibility = Visibility.Hidden;
            textBoxMinus.Visibility = Visibility.Hidden;
            LabelMinus.Visibility = Visibility.Hidden;

        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            ///////////////////////////////////
            /*
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(storeGrid, "Распечатываем элемент Canvas");
            }
            */
            //////////////////////////////////
            
            Excel.Application excel;

            // Вот если сделать именно так, то будет проверка на запуск Экселя, и если все ок, то идем дальше, иначе сразу вылетаем из метода
            try
            {
                excel = new Excel.Application();
            }
            catch
            {
                MessageBox.Show("На вашем компьютере не установлен Excel. Печать невозможна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            excel.Visible = true;
            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];

            Range myRange = (Range)sheet1.Cells[1, 1];
            sheet1.Cells[1, 1].Font.Bold = true;
            myRange.Value2 = "Прибыль";

            for (int j = 0; j < incomeGrid.Columns.Count; j++)
            {
                Range myRange1 = (Range)sheet1.Cells[2, j + 1];
                sheet1.Cells[2, j + 1].Font.Bold = true;
                sheet1.Columns[j + 1].ColumnWidth = 15;
                myRange1.Value2 = incomeGrid.Columns[j].Header;
            }

            for (int i = 0; i < incomeGrid.Columns.Count; i++)
            {
                for (int j = 0; j < incomeGrid.Items.Count; j++)
                {
                    TextBlock b = incomeGrid.Columns[i].GetCellContent(incomeGrid.Items[j]) as TextBlock;
                    Microsoft.Office.Interop.Excel.Range myRange2 = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 3, i + 1];
                    myRange2.Value2 = b.Text;
                }
            }

            int countitems = incomeGrid.Items.Count;
            int countclearitems = clearincomeGrid.Items.Count;

            myRange = (Range)sheet1.Cells[4 + countitems, 1];
            sheet1.Cells[4 + countitems, 1].Font.Bold = true;
            myRange.Value2 = "Прибыль:";
            myRange = (Range)sheet1.Cells[4 + countitems, 2];
            sheet1.Cells[4 + countitems, 2].Font.Bold = true;
            myRange.Value2 = textBoxPlus.Text;
            ////////////////////////////////
            if (clearincomeGrid.Visibility == Visibility.Visible)
            {
                myRange = (Range)sheet1.Cells[6 + countitems, 1];
                sheet1.Cells[6 + incomeGrid.Items.Count, 1].Font.Bold = true;
                myRange.Value2 = "Расходы";

                for (int j = 0; j < clearincomeGrid.Columns.Count; j++)
                {
                    Range myRange1 = (Range)sheet1.Cells[7 + countitems, j + 1];
                    sheet1.Cells[7 + countitems, j + 1].Font.Bold = true;
                    sheet1.Columns[j + 1].ColumnWidth = 15;
                    myRange1.Value2 = clearincomeGrid.Columns[j].Header;
                }

                for (int i = 0; i < clearincomeGrid.Columns.Count; i++)
                {
                    for (int j = 0; j < countclearitems; j++)
                    {
                        TextBlock b = clearincomeGrid.Columns[i].GetCellContent(clearincomeGrid.Items[j]) as TextBlock;
                        Microsoft.Office.Interop.Excel.Range myRange2 = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 8 + countitems, i + 1];
                        myRange2.Value2 = b.Text;
                    }
                }

                myRange = (Range)sheet1.Cells[9 + countclearitems + countitems, 1];
                sheet1.Cells[9 + countclearitems + countitems, 1].Font.Bold = true;
                myRange.Value2 = "Расходы:";

                myRange = (Range)sheet1.Cells[9 + countclearitems + countitems, 2];
                sheet1.Cells[9 + countclearitems + countitems, 2].Font.Bold = true;
                myRange.Value2 = textBoxMinus.Text;
                
                myRange = (Range)sheet1.Cells[11 + countclearitems + countitems, 1];
                sheet1.Cells[11 + countclearitems + countitems, 1].Font.Bold = true;
                myRange.Value2 = "Итого:";

                myRange = (Range)sheet1.Cells[11 + countclearitems + countitems, 2];
                sheet1.Cells[11 + countclearitems + countitems, 2].Font.Bold = true;
                myRange.Value2 = textBoxCount.Text;
            }
        }

        private void ButtonClearIncome_Click(object sender, RoutedEventArgs e)
        {
            Window create_window = new IncomeRecordWindow(WaysToOpenForm.WaysToOpen.create);
            create_window.ShowDialog();
            string mounth = (csMounth.MounthOfIncome + 1).ToString();
            string query_text1 = "select orders.id_Order as id, orders.Total_Price as Count  from orders " +
                                "where month(orders.Date_Of_Delievery) = " + mounth + " and year(orders.Date_Of_Delievery) = year(curdate()) and (orders.Statuses_Of_Order_id_Status_Of_Order = 2 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 3 or " +
                                "orders.Statuses_Of_Order_id_Status_Of_Order = 4); ";
            RefreshListDirty(query_text1);
            string query_text2 = "select costs.id as id, costs.Amount as Count  from costs " +
                                "where month(costs.Date_Of_Cost) = " + mounth + " and year(costs.Date_Of_Cost) = year(curdate())";
            RefreshListClear(query_text2);
            float countplus = 0;
            float countminus = 0;

            foreach (DataRowView row in incomeGrid.Items)
            {
                countplus = countplus + (float)row.Row.ItemArray[1];
            }
            textBoxPlus.Text = countplus.ToString();

            foreach (DataRowView row in clearincomeGrid.Items)
            {
                countminus = countminus + (float)row.Row.ItemArray[1];
            }
            textBoxMinus.Text = countminus.ToString();
            textBoxCount.Text = (countplus - countminus).ToString();
            clearincomeGrid.Visibility = Visibility.Visible;
            LabelCosts.Visibility = Visibility.Visible;
            LabelMinus.Visibility = Visibility.Visible;
            textBoxMinus.Visibility = Visibility.Visible;
            LabelMinus.Visibility = Visibility.Visible;
        }
    }
}
