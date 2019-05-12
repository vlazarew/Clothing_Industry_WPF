using Clothing_Industry_WPF.Поиск_и_фильтры;
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

namespace Clothing_Industry_WPF.Изделия
{
    public struct HelpStruct
    {
        public int Vendor_Code { get; set; }
        public string Name_Of_Material { get; set; }
        public float Cost_Of_Material { get; set; }
        public string Name_Of_Unit { get; set; }
        public int Count { get; set; }
    }

    public partial class MaterialsForProductsList : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;


        private ObservableCollection<HelpStruct> collection;
        private int productId;

        public MaterialsForProductsList(int productId)
        {
            InitializeComponent();
            /*currentFindDescription = new FindHandler.FindDescription();
            currentFilterDescription = new List<FilterHandler.FilterDescription>();
            resultProductId = new List<int>();*/
            collection = new ObservableCollection<HelpStruct>();
            this.productId = productId;
            productslistGrid.ItemsSource = collection;
            RefreshList();
        }

        private void RefreshList()
        {
            collection = new ObservableCollection<HelpStruct>();
            string query = "select Vendor_Code, Name_Of_Material, Cost_Of_Material, Name_Of_Unit, Count " +
                            "from materials " +
                            "join materials_for_product  on materials_for_product.Materials_Vendor_Code = materials.Vendor_Code " +
                            "join units on units.id_Unit = materials.Units_id_Unit " +
                            "where materials_for_product.Products_id_Product = @productID; "; 
      

            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@productID", productId);

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

            productslistGrid.ItemsSource = collection;
            connection.Close();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexesToDelete = new List<int>();
            foreach (HelpStruct row in productslistGrid.SelectedItems)
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
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from materials_for_product where Materials_Vendor_Code = @Vendor_Code and Products_id_Product = @id_product";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@Vendor_Code", Vendor_Code);
                commandTable.Parameters.AddWithValue("@id_product", productId);

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

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            Window window_add = new MaterialsForProductsRecordWindow(productId);
            window_add.ShowDialog();
            RefreshList();
        }
    }
}