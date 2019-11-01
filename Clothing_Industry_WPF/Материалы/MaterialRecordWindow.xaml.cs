using Clothing_Industry_WPF.Материалы;
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
        private Material material;
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private string vendorCodeRecord;

        public MaterialRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, string vendorCode = "")
        {
            material = new Material();
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            vendorCodeRecord = vendorCode;

            // Заполнение шапки, полей
            Title = FormLoader.setNewTitle(way, Title);
            Header.Content = Title;

            FillComboBoxes();

            if (vendorCodeRecord != "")
            {
                material = new Material(vendorCodeRecord, connection);
                Border.Visibility = (material.photo != null) ? Visibility.Hidden : Visibility.Visible;
                FillFields();
            }
        }

        private void FillFields()
        {
            textBoxVendor_Code.Text = material.vendorCode;
            textBoxName_Of_Material.Text = material.name;
            textBoxCost_Of_Material.Text = material.cost.ToString();
            textBoxNotes.Text = material.notes;
            comboBoxUnit.SelectedValue = material.unitName;
            comboBoxGroup.SelectedValue = material.groupOfMaterialName;
            comboBoxType.SelectedValue = material.typeOfMaterialName;
            comboBoxCountry.SelectedValue = material.countryName;

            if (material.photo == null)
            {
                imagePhoto.Source = null;
            }
            else
            {
                MemoryStream stream = new MemoryStream(material.photo);
                imagePhoto.Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        private void FillComboBoxes()
        {
            var outgoingData = new List<(string field, string table, string comboBox)>();
            var comboboxUnitData = (field: "Name_Of_Unit", table: "units", comboBox: "comboBoxUnit");
            var comboboxGroupData = (field: "Name_Of_Group", table: "groups_of_material", comboBox: "comboBoxGroup");
            var comboboxTypeData = (field: "Name_Of_Type", table: "types_of_material", comboBox: "comboBoxType");
            var comboboxCountryData = (field: "Name_Of_Country", table: "countries", comboBox: "comboBoxCountry");

            outgoingData.Add(comboboxUnitData);
            outgoingData.Add(comboboxGroupData);
            outgoingData.Add(comboboxTypeData);
            outgoingData.Add(comboboxCountryData);

            var receivedData = FormLoader.FillComboBoxes(outgoingData, connection);

            foreach (var data in receivedData)
            {
                var combobox = (System.Windows.Controls.ComboBox)FindName(data.Key);
                combobox.ItemsSource = data.Value;
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
                // Отображение на форме
                string imagePath = openFileDialog.FileName;
                imagePhoto.Source = new BitmapImage(new Uri(imagePath));
                // Запоминаем бинарные данные в объекте 
                FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                material.photo = binaryReader.ReadBytes((int)fileStream.Length);
                binaryReader.Close();
                fileStream.Close();
                Border.Visibility = Visibility.Hidden;
            }
        }

        private void ButtonSaveAndExit_Click(object sender, RoutedEventArgs e)
        {
            // Истина - сохранение прошло успешно, ложь - если проблемы
            if (material.Save(connection, way, vendorCodeRecord))
            {
                Close();
            }
        }

        private void TextBoxCost_Of_Material_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxValidator.IsFloatTextAllowed(e.Text);
        }

        #region Обработка изменений данных в полях

        // Обработка всех текстовых полей
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            var textBoxName = textBox.Name;
            var value = textBox.Text;
            switch (textBoxName)
            {
                case "textBoxVendor_Code":
                    material.vendorCode = value;
                    break;
                case "textBoxName_Of_Material":
                    material.name = value;
                    break;
                case "textBoxCost_Of_Material":
                    float.TryParse(value, out float newCost);
                    material.cost = newCost;
                    break;
                case "textBoxNotes":
                    material.notes = value;
                    break;
            }
        }

        // Обработка комбобоксов вводом текста
        private void ComboBox_TextChanged(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ComboBox_TextChanged(sender);
        }

        // Обработка комбобоксов выбором позиции
        private void ComboBox_TextChanged(object sender, EventArgs e)
        {
            ComboBox_TextChanged(sender);
        }

        private void ComboBox_TextChanged(object sender)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            var comboBoxName = comboBox.Name;
            var value = comboBox.Text;
            switch (comboBoxName)
            {
                case "comboBoxUnit":
                    if (comboBoxUnit.Items.IndexOf(value) != -1)
                    {
                        material.unitName = value;
                    }
                    break;
                case "comboBoxGroup":
                    if (comboBoxGroup.Items.IndexOf(value) != -1)
                    {
                        material.groupOfMaterialName = value;
                    }
                    break;
                case "comboBoxType":
                    if (comboBoxType.Items.IndexOf(value) != -1)
                    {
                        material.typeOfMaterialName = value;
                    }
                    break;
                case "comboBoxCountry":
                    if (comboBoxCountry.Items.IndexOf(value) != -1)
                    {
                        material.countryName = value;
                    }
                    break;
            }
        }

        #endregion
    }
}


