using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
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
    /// Логика взаимодействия для ProductRecordWindow.xaml
    /// </summary>
    public partial class ProductRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string image_path;
        private byte[] image_bytes;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int old_id_Product = -1;
        public ProductRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id_Product = -1)
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
            string query_text = "select products.Name_Of_Product, products.Fixed_Price, products.MoneyToEmployee," +
                                " products.Description, products.Photo" +
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
                    textBoxMoneyToEmployee.Text = reader.GetString(2);
                   
                    if (reader.GetValue(3).ToString() != "")
                    {
                        textBoxDescription.Text = reader.GetString(3);
                    }

                    image_bytes = null;
                    try
                    {
                        image_bytes = (byte[])(reader[4]);
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

                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    this.Title += " (Изменение)";

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
                Border.Visibility = Visibility.Hidden;
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
            if (textBoxMoneyToEmployee.Text == "")
            {
                result += result == "" ? "Выплата сотруднику" : ", Выплата сотруднику";
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

                //try
               // {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    this.Close();
               // }
               // catch
               // {
                //    transaction.Rollback();
                //    System.Windows.MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
               // }


                connection.Close();
                //this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MySqlCommand actionInDBCommand(MySqlConnection connection)
        {
            string query = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO products " +
                        "(Name_Of_Product, Fixed_Price, MoneyToEmployee," +
                        " Description, Photo)" +
                        " VALUES (@Name_Of_Product, @Fixed_Price, @MoneyToEmployee, @Description, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update products set Name_Of_Product = @Name_Of_Product, Fixed_Price = @Fixed_Price, MoneyToEmployee = @MoneyToEmployee," +
                        "Description = @Description, Photo = @image" +
                        " where id_Product = @old_id_Product;";

            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name_Of_Product", textBoxName_Of_Product.Text);
            command.Parameters.AddWithValue("@Fixed_Price", textBoxFixed_Price.Text);
            command.Parameters.AddWithValue("@MoneyToEmployee", textBoxMoneyToEmployee.Text);
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

        private void TextBoxFixedPrice_Of_Material_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxValidator.IsFloatTextAllowed(e.Text);
        }

        private void TextBoxMoneyToEmployee_Of_Material_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxValidator.IsFloatTextAllowed(e.Text);
        }
    }
}
