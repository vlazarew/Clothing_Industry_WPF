﻿using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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

namespace Clothing_Industry_WPF.Изделия
{
    /// <summary>
    /// Логика взаимодействия для ProductsRecordWindow.xaml
    /// </summary>
    public partial class ProductsRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string image_path;
        private byte[] image_bytes;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int old_id_Product = -1;
        public ProductsRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id_Product = -1)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();

            if (id_Product != -1)
            {
                old_id_Product = id_Product;
                FillFields(id_Product);
            }
        }

        private void FillFields(int id_Product)
        {
            string query_text = "select products.Name_Of_Product, products.Fixed_Price, products.Per_Cents," +
                                "products.Added_Price_For_Complexity, products.Description, products.Photo" +
                                " from products" +
                                " where products.id_Product = @id_Product;";

            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@id_Product", id_Product);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxName_Of_Product.Text = reader.GetString(0);
                    textBoxFixed_Price.Text = reader.GetString(1);
                    textBoxPer_Cents.Text = reader.GetString(2);
                    if (reader.GetValue(3).ToString() != "")
                        textBoxAdded_Price_For_Complexity.Text = reader.GetString(3);
                    if (reader.GetValue(4).ToString() != "")
                    {
                        textBoxDescription.Text = reader.GetString(4);
                    }

                    image_bytes = null;
                    try
                    {
                        image_bytes = (byte[])(reader[5]);
                    }
                    catch
                    {

                    }

                    if (image_bytes == null)
                    {
                        imagePhoto.Source = null;
                    }
                    else
                    {
                        MemoryStream stream = new MemoryStream(image_bytes);
                        imagePhoto.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
            }
            connection.Close();
        }

        private void setNewTitle()
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    this.Title += " (Создание)";
                    Header.Content += " (Создание)";
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";
                    Header.Content += " (Изменение)";
                    break;
                default:
                    break;

            }
        }
     
        private void ButtonAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Картинки (*.JPG; *.GIF; *.JPEG; *.PNG)|*.JPG; *.GIF; *.JPEG; *.PNG" + "|Все файлы (*.*)|*.* ";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                image_path = openFileDialog.FileName;
                imagePhoto.Source = new BitmapImage(new Uri(image_path));
            }
        }

        private string CheckData()
        {
            string result = "";

            if (textBoxName_Of_Product.Text == "")
            {
                result += result == "" ? "Название" : ", Название";
            }
            if (textBoxFixed_Price.Text == "")
            {
                result += result == "" ? "Цена" : ", Цена";
            }
            if (textBoxPer_Cents.Text == "")
            {
                result += result == "" ? "Стоимость за изготовление" : ", Стоимость за изготовление";
            }
            if (textBoxAdded_Price_For_Complexity.Text == "")
            {
                result += result == "" ? "Дополнительная оплата" : ", Дополнительная оплата";
            }
            if (textBoxDescription.Text == "")
            {
                result += result == "" ? "Описание" : ", Описание";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            string warning = CheckData();
            if (warning == "")
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                //Создать/изменить запись в таблице Материалы
                MySqlCommand command = actionInDBCommand(connection);
                command.Transaction = transaction;

                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    this.Hide();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!");
                }


                connection.Close();
                //this.Hide();
            }
            else
            {
                System.Windows.MessageBox.Show(warning);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO products " +
                                       "(Name_Of_Product,Fixed_Price,Per_Cents,Added_Price_For_Complexity," +
                                       " Description, Photo)" +
                                       " VALUES (@Name_Of_Product, @Fixed_Price, @Per_Cents, @Added_Price_For_Complexity, @Description, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update materials set Vendor_Code = @vendor_code, Name_Of_Material = @name_of_material, Cost_Of_Material = @cost_of_material," +
                        "Notes = @notes," +
                        "Units_id_Unit = @unit, Groups_Of_Material_id_Group_Of_Material = @group, Types_Of_Material_id_Type_Of_Material = @type, Countries_id_Country = @country, Photo = @image" +
                        " where Vendor_Code = @oldvendor_code;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name_Of_Product", textBoxName_Of_Product.Text);
            command.Parameters.AddWithValue("@Fixed_Price", textBoxFixed_Price.Text);
            command.Parameters.AddWithValue("@Per_Cents", textBoxPer_Cents.Text);
            command.Parameters.AddWithValue("@Added_Price_For_Complexity", textBoxAdded_Price_For_Complexity.Text);
            command.Parameters.AddWithValue("@Description", textBoxDescription.Text); 
            // Обработка фото
            if (image_path != null)
            {
                FileStream fileStream = new FileStream(image_path, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                byte[] imageData = binaryReader.ReadBytes((int)fileStream.Length);
                binaryReader.Close();
                fileStream.Close();
                command.Parameters.AddWithValue("@image", imageData);
            }
            else
            {
                if (image_bytes != null)
                {
                    command.Parameters.AddWithValue("@image", image_bytes);
                }
                else
                {
                    command.Parameters.AddWithValue("@image", null);
                }
            }

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@old_id_Product", old_id_Product);
            }

            return command;
        }
    }
}
