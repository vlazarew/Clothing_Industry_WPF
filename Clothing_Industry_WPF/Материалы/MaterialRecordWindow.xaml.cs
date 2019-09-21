using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Clothing_Industry_WPF.Материал
{
    /// <summary>
    /// Логика взаимодействия для MaterialRecordWindow.xaml
    /// </summary>
    public partial class MaterialRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string image_path;
        private byte[] image_bytes;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string old_vendor_code = "";

        // Ввод только букв в численные поля 
        private static readonly Regex _regex = new Regex("[^0-9,]");

        public MaterialRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, string vendor_code = "")
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();
            FillComboBoxes();

            if (vendor_code != "")
            {
                old_vendor_code = vendor_code;
                FillFields(vendor_code);
            }
        }

        private void FillFields(string vendor_code)
        {
            string query_text = "select materials.Vendor_Code, materials.Name_Of_Material, materials.Cost_Of_Material, materials.Notes, units.Name_Of_Unit," +
                                " groups_of_material.Name_Of_Group, types_of_material.Name_Of_Type, countries.Name_Of_Country, materials.Photo" +
                                " from materials" +
                                " join units on materials.Units_id_Unit = units.id_Unit" +
                                " join groups_of_material on materials.Groups_Of_Material_id_Group_Of_Material = groups_of_material.id_Group_Of_Material" +
                                " join types_of_material on materials.Types_Of_Material_id_Type_Of_Material = types_of_material.id_Type_Of_Material" +
                                " join countries on materials.Countries_id_Country = countries.id_Country" +
                                " where materials.Vendor_Code = @vendor_code;";

            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@vendor_code", vendor_code);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxVendor_Code.Text = reader.GetString(0);
                    textBoxName_Of_Material.Text = reader.GetString(1);
                    textBoxCost_Of_Material.Text = reader.GetString(2);
                    if(reader.GetValue(3).ToString() != "")
                    {
                        textBoxNotes.Text = reader.GetString(3);
                    }

                    comboBoxUnit.SelectedValue = reader.GetString(4);
                    comboBoxGroup.SelectedValue = reader.GetString(5);
                    comboBoxType.SelectedValue = reader.GetString(6);
                    comboBoxCountry.SelectedValue = reader.GetString(7);

                    image_bytes = null;
                    try
                    {
                        image_bytes = (byte[])(reader[8]);
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

        private void FillComboBoxes()
        {
            connection.Open();

            string query = "select Name_Of_Unit from units";
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxUnit.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_Group from groups_of_material";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxGroup.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_Type from types_of_material";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxType.Items.Add(reader.GetString(0));
                }
            }

            query = "select Name_Of_Country from countries";
            command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBoxCountry.Items.Add(reader.GetString(0));
                }
            }

            connection.Close();
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

            if (textBoxVendor_Code.Text == "")
            {
                result += result == "" ? "Артикул" : ", Артикул";
            }
            if (textBoxName_Of_Material.Text == "")
            {
                result += result == "" ? "Название материала" : ", Название материала";
            }
            if (textBoxCost_Of_Material.Text == "")
            {
                result += result == "" ? "Стоимость" : ", Стоимость";
            }
            if (comboBoxUnit.SelectedValue == null)
            {
                result += result == "" ? " Единица измерения" : ",  Единица измерения";
            }
            if (comboBoxGroup.SelectedValue == null)
            {
                result += result == "" ? "Вид" : ", Вид";
            }
            if (comboBoxType.SelectedValue == null)
            {
                result += result == "" ? "Тип" : ", Тип";
            }
            if (comboBoxCountry.SelectedValue == null)
            {
                result += result == "" ? "Страна производитель" : ", Страна производитель";
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
                MySqlCommand commandStrore = actionInStoreCommand(connection);
                commandStrore.Transaction = transaction;
                try
                {
                    command.ExecuteNonQuery();
                    commandStrore.ExecuteNonQuery();
                    transaction.Commit();
                    this.Close();
                }
                catch
                {
                    transaction.Rollback();
                    System.Windows.MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                connection.Close();
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
                query = "INSERT INTO materials " +
                         "(Vendor_Code,Name_Of_Material,Cost_Of_Material,Notes," +
                         " Units_id_Unit, Groups_Of_Material_id_Group_Of_Material,Types_Of_Material_id_Type_Of_Material,Countries_id_Country, Photo)" +
                         " VALUES (@vendor_code, @name_of_material, @cost_of_material, @notes, @unit, @group, @type, @country, @image);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update materials set Vendor_Code = @vendor_code, Name_Of_Material = @name_of_material, Cost_Of_Material = @cost_of_material," +
                        "Notes = @notes," +
                        "Units_id_Unit = @unit, Groups_Of_Material_id_Group_Of_Material = @group, Types_Of_Material_id_Type_Of_Material = @type, Countries_id_Country = @country, Photo = @image" +
                        " where Vendor_Code = @oldvendor_code;";
            }

            MySqlCommand command = new MySqlCommand(query, connection);

            int.TryParse(textBoxVendor_Code.Text, out int vendorCode);
            command.Parameters.AddWithValue("@vendor_code", vendorCode);

            command.Parameters.AddWithValue("@name_of_material", textBoxName_Of_Material.Text);

            float.TryParse(textBoxCost_Of_Material.Text, out float costOfMaterial);
            command.Parameters.AddWithValue("@cost_of_material", costOfMaterial);
            command.Parameters.AddWithValue("@notes", textBoxNotes.Text);

            MySqlCommand commandUnit = new MySqlCommand("select id_Unit from units where Name_Of_Unit = @unit", connection);
            commandUnit.Parameters.AddWithValue("unit", comboBoxUnit.SelectedItem.ToString());
            int id_unit = -1;
            using (DbDataReader reader = commandUnit.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_unit = reader.GetInt32(0);
                }
            }

            MySqlCommand commandGroup = new MySqlCommand("select id_Group_Of_Material from groups_of_material where Name_Of_Group = @group", connection);
            commandGroup.Parameters.AddWithValue("group", comboBoxGroup.SelectedItem.ToString());
            int id_group = -1;
            using (DbDataReader reader = commandGroup.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_group = reader.GetInt32(0);
                }
            }

            MySqlCommand commandType = new MySqlCommand("select id_Type_Of_Material from types_of_material where Name_Of_Type = @type", connection);
            commandType.Parameters.AddWithValue("@type", comboBoxType.SelectedItem.ToString());
            int id_type = -1;
            using (DbDataReader reader = commandType.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_type = reader.GetInt32(0);
                }
            }

            MySqlCommand commandCountry = new MySqlCommand("select id_Country from countries where Name_Of_Country = @country", connection);
            commandCountry.Parameters.AddWithValue("@country", comboBoxCountry.SelectedItem.ToString());
            int id_country = -1;
            using (DbDataReader reader = commandCountry.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_country = reader.GetInt32(0);
                }
            }

            command.Parameters.AddWithValue("@unit", id_unit);
            command.Parameters.AddWithValue("@group", id_group);
            command.Parameters.AddWithValue("@type", id_type);
            command.Parameters.AddWithValue("@country", id_country);

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
                command.Parameters.AddWithValue("@oldvendor_code", old_vendor_code);
            }

            return command;
        }

        private MySqlCommand actionInStoreCommand(MySqlConnection connection)
        {
            string querystore = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                querystore = "INSERT INTO store (Materials_Vendor_Code, Count)" +
                             " VALUES(@vendor_code,0);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                querystore = "Update store set Materials_Vendor_Code = @vendor_code" +
                             " where Materials_Vendor_Code = @oldvendor_code;";

            }

            MySqlCommand command = new MySqlCommand(querystore, connection);

                command.Parameters.AddWithValue("@vendor_code", textBoxVendor_Code.Text);


            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldvendor_code", old_vendor_code);
            }

            return command;
        }

        private void TextBoxCost_Of_Material_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}
    

