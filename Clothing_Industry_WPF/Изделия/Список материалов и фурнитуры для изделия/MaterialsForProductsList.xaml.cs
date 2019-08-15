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
        public string Name_Of_Group { get; set; }

        public float Count { get; set; }
        public int Groups_Of_Material_id_Group_Of_Material { get;  set; }
    }

    public partial class MaterialsForProductsList : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;


        private ObservableCollection<HelpStruct> collection;
        private int productId;

        public MaterialsForProductsList(int productId)
        {
            InitializeComponent();
            collection = new ObservableCollection<HelpStruct>();
            this.productId = productId;
            productslistGrid.ItemsSource = collection;
            RefreshList();
        }

        private void RefreshList()
        {
            collection = new ObservableCollection<HelpStruct>();
            string query = "select  Groups_Of_Material_id_Group_Of_Material, Name_Of_Group, Count " +
                            "from materials_for_product " +
                            "join groups_of_material on materials_for_product.Groups_Of_Material_id_Group_Of_Material = groups_of_material.id_Group_Of_Material " +
                            "where materials_for_product.Products_id_Product = @productID ; "; 
      

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
                        Groups_Of_Material_id_Group_Of_Material = (int)reader.GetValue(0),
                        Name_Of_Group = reader.GetString(1),
                        Count = (float)reader.GetValue(2),
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
                indexesToDelete.Add(row.Groups_Of_Material_id_Group_Of_Material);
            }

            DeleteFromDB(indexesToDelete);
        }

        private void DeleteFromDB(List<int> ids)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            foreach (int Groups_Of_Material_id_Group_Of_Material in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from materials_for_product where Groups_Of_Material_id_Group_Of_Material = @Groups_Of_Material_id_Group_Of_Material and Products_id_Product = @id_product";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@Groups_Of_Material_id_Group_Of_Material", Groups_Of_Material_id_Group_Of_Material);
                commandTable.Parameters.AddWithValue("@id_product", productId);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                    ButtonDelete.Style = (Style)ButtonDelete.FindResource("NoActive");
                }
                catch
                {
                    transaction.Rollback();
                MessageBox.Show("Ошибка удаления");
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

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            ButtonDelete.Style = (Style)ButtonDelete.FindResource("Active");
        }
    }
}