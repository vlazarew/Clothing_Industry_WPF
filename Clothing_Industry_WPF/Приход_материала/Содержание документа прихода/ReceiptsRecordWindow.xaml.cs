﻿using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
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
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;
using PrintDialog = System.Windows.Controls.PrintDialog;

namespace Clothing_Industry_WPF.Приход_материала
{
    public struct HelpStruct
    {
        public int Vendor_Code { get; set; }
        public string Name_Of_Material { get; set; }
        public float Cost_Of_Material { get; set; }
        public string Name_Of_Unit { get; set; }
        public int Count { get; set; }
    }
    public partial class ReceiptsRecordWindow : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;


        private ObservableCollection<HelpStruct> collection;
        private int DocumentId;

        public ReceiptsRecordWindow(int DocumentId)
        {
            InitializeComponent();
            /*currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
            resultProductId = new List<int>();*/
            collection = new ObservableCollection<HelpStruct>();
            this.DocumentId = DocumentId;
            receiptrecordGrid.ItemsSource = collection;
            RefreshList();
        }

        private void RefreshList()
        {
            collection = new ObservableCollection<HelpStruct>();
            string query = "select Vendor_Code, Name_Of_Material, Cost_Of_Material, Name_Of_Unit, Count " +
                            "from materials " +
                            "join documents_of_receipts  on documents_of_receipts.Materials_Vendor_Code = materials.Vendor_Code " +
                            "join units on units.id_Unit = materials.Units_id_Unit " +
                            "where documents_of_receipts.Receipt_Of_Materials_id_Document_Of_Receipt = @DocumentID; ";


            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@DocumentID", DocumentId);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    collection.Add(new HelpStruct()
                    {
                        Vendor_Code = (int)reader.GetValue(0),
                        Name_Of_Material = reader.GetString(1),
                        Cost_Of_Material = (float)reader.GetValue(2),
                        Name_Of_Unit = reader.GetString(3),
                        Count = (int)reader.GetValue(4),
                    });
                }
            }

            receiptrecordGrid.ItemsSource = collection;
            connection.Close();
        }


        private void ButtonAddOne_Click(object sender, RoutedEventArgs e)
        {
            Window window_add = new ReceiptsMaterialRecordWindow(DocumentId);
            window_add.ShowDialog();
            RefreshList();
        }

        private void ButtonDeleteOne_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexesToDelete = new List<int>();
            foreach (HelpStruct row in receiptrecordGrid.SelectedItems)
            {
                indexesToDelete.Add(row.Vendor_Code);
            }

            DeleteFromDB(indexesToDelete);
        }

        private void DeleteFromDB(List<int> ids)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (int Vendor_Code in ids)
            {             
                MySqlTransaction transaction2 = connection.BeginTransaction();
                string totalquery = "Update receipt_of_materials inner join documents_of_receipts on receipt_of_materials.id_Document_Of_Receipt = documents_of_receipts.Receipt_Of_Materials_id_Document_Of_Receipt" +
                    " set receipt_of_materials.Total_Price = receipt_of_materials.Total_Price - @Cost_Of_Material * documents_of_receipts.Count" +
                        " where id_Document_Of_Receipt = @DocumentId;";
                string query_price = "select materials.Cost_Of_Material from materials where materials.Vendor_Code = @Vendor_Code ";
                MySqlCommand command_price = new MySqlCommand(query_price, connection);
                command_price.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);
                float Cost_Of_Material = -1;
                using (DbDataReader reader2 = command_price.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        Cost_Of_Material = (float)reader2.GetValue(0);
                    }
                }
                MySqlCommand commandTotal = new MySqlCommand(totalquery, connection, transaction2);
                commandTotal.Parameters.AddWithValue("@Cost_Of_Material", Cost_Of_Material);
                commandTotal.Parameters.AddWithValue("@DocumentID", DocumentId);
                try
                {
                    commandTotal.ExecuteNonQuery();
                    transaction2.Commit();
                }
                catch
                {
                    transaction2.Rollback();
                    System.Windows.MessageBox.Show("Ошибка удаления");
                }

                MySqlTransaction transaction = connection.BeginTransaction();
                string queryTable = "delete from documents_of_receipts where Materials_Vendor_Code = @Vendor_Code and Receipt_Of_Materials_id_Document_Of_Receipt = @DocumentID";
                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);
                commandTable.Parameters.AddWithValue("@DocumentID", DocumentId);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка удаления");
                }
            }

            connection.Close();
            RefreshList();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonCreateExcel_Click(object sender, RoutedEventArgs e)
        {
            //папка с exe встроенная
            
            string BasePath = "D:\\Git\\Clothing_Industry_WPF\\Документы прихода\\";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            string query_folder = "select receipt_of_materials.Default_Folder, receipt_of_materials.Name_Of_Document from receipt_of_materials where receipt_of_materials.id_Document_Of_Receipt = @DocumentID ";
            MySqlCommand command_folder = new MySqlCommand(query_folder, connection);
            command_folder.Parameters.AddWithValue("@DocumentID", DocumentId);
            string Default_Folder = "";
            string Name_Of_Document = "";
            using (DbDataReader reader = command_folder.ExecuteReader())
            {
                while (reader.Read())
                {
                    Default_Folder = reader.GetString(0);
                    Name_Of_Document = reader.GetString(1);
                }
            }
            Excel.Application excel = new Excel.Application();
            excel.Visible = false;

            Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet sheet1 = (Worksheet)workbook.Sheets[1];
            excel.DisplayAlerts = false;
            for (int j = 0; j < receiptrecordGrid.Columns.Count; j++)
            {
                Range myRange = (Range)sheet1.Cells[1, j + 1];
                sheet1.Cells[1, j + 1].Font.Bold = true;
                sheet1.Columns[j + 1].ColumnWidth = 15;
                myRange.Value2 = receiptrecordGrid.Columns[j].Header;
            }
            for (int i = 0; i < receiptrecordGrid.Columns.Count; i++)
            {
                for (int j = 0; j < receiptrecordGrid.Items.Count; j++)
                {
                    TextBlock b = receiptrecordGrid.Columns[i].GetCellContent(receiptrecordGrid.Items[j]) as TextBlock;
                    Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 2, i + 1];
                    myRange.Value2 = b.Text;
                }
            }
            System.IO.Directory.CreateDirectory(BasePath + Default_Folder);

            workbook.SaveAs(BasePath + Default_Folder + "\\" + Name_Of_Document + ".xls",
                  Excel.XlFileFormat.xlWorkbookNormal);
            workbook.Close(true);
            excel.Quit();
            MessageBox.Show("Документ " + Name_Of_Document + " создан успешно.\n" + "Путь документа: " + BasePath + Default_Folder);
        }
    }
}
