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

namespace Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа
{
    /// <summary>
    /// Логика взаимодействия для OrderListOfMaterialsForProduct.xaml
    /// </summary>

    public struct HelpStruct
    {
        public int Groups_Of_Material_id_Group_Of_Material { get; set; }
        public string Name_Of_Group { get; set; }
        public string Name_Of_Material { get; set; }
        public float Count { get; set; }
        public string Name_Of_Unit { get; set; }
    }

    public partial class OrderListOfMaterialsForProduct : Window
    {
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;


        private ObservableCollection<HelpStruct> collection;
        private int productId;
        private int groupofmaterial;
        private int idmaterialproduct;
        private float count;
        public OrderListOfMaterialsForProduct(int productId)
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
            string query = "select  materials_for_product.id_Materials_For_Product, materials_for_product.Groups_Of_Material_id_Group_Of_Material, Name_Of_Group, Name_Of_Material,Count,Name_Of_Unit " +
                            "from materials_for_product " +
                            "join groups_of_material on materials_for_product.Groups_Of_Material_id_Group_Of_Material = groups_of_material.id_Group_Of_Material " +
                            "left join materials on materials_for_product.materials_vendor_code = materials.Vendor_Code " +
                            "left join units on materials.units_id_unit = units.id_unit " +
                            "where materials_for_product.Products_id_Product = @productID;  ";




            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@productID", productId);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    idmaterialproduct = (int)reader.GetValue(0);
                    groupofmaterial = (int)reader.GetValue(1);
                    count = (float)reader.GetValue(4);
                    try
                    {
                        collection.Add(new HelpStruct()
                        {
                            Groups_Of_Material_id_Group_Of_Material = (int)reader.GetValue(1),
                            Name_Of_Group = reader.GetString(2),
                            Name_Of_Material = reader.GetString(3),
                            Count = (float)reader.GetValue(4),
                            Name_Of_Unit = reader.GetString(5),
                        });
                    }
                    catch
                    {
                        collection.Add(new HelpStruct()
                        {
                            Groups_Of_Material_id_Group_Of_Material = (int)reader.GetValue(1),
                            Name_Of_Group = reader.GetString(2),
                            Count = (float)reader.GetValue(4),
                        });
                    }
                }
            }

            productslistGrid.ItemsSource = collection;
            connection.Close();
        }



        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            string check = "";
            foreach (HelpStruct row in productslistGrid.Items)
            {
                check = row.Name_Of_Material;
                if (check == null)
                {
                    System.Windows.MessageBox.Show("Все поля не заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }


            }
            if (check != null)
            {
                this.Close();
            }
            else
            {
                this.Close();
                Window listMaterials = new OrderListOfMaterialsForProduct(productId);
                listMaterials.ShowDialog();
            }
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int row_index = productslistGrid.SelectedIndex;
            int groupofmaterial = -1;
            float count = -1;
            int current_row = 0;
            foreach (HelpStruct row in productslistGrid.Items)
            {
                if (current_row != row_index)
                {
                    current_row++;
                    continue;
                }
                groupofmaterial = row.Groups_Of_Material_id_Group_Of_Material;
                count = row.Count;

                break;
            }





            Window listMaterials = new OrderRecordMaterialForProduct(productId, groupofmaterial, idmaterialproduct, count);
            listMaterials.ShowDialog();
            RefreshList();
        }

    }
}
