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

namespace Clothing_Industry_WPF.Справочник.Типы_материалов
{
    /// <summary>
    /// Логика взаимодействия для TypesOfMaterialRecordWindow.xaml
    /// </summary>
    public partial class TypesOfMaterialRecordWindow : Window
    {
        private WaysToOpenForm.WaysToOpen way;
        private string connectionString = Properties.Settings.Default.main_databaseConnectionString;
        private MySqlConnection connection;
        private int old_id = -1;

        public TypesOfMaterialRecordWindow(WaysToOpenForm.WaysToOpen waysToOpen, int id = -1)
        {
            InitializeComponent();
            way = waysToOpen;
            connection = new MySqlConnection(connectionString);
            setNewTitle();

            if (id != -1)
            {
                old_id = id;
                FillFields(id);
            }
            textBoxName.Focus();
        }

        private void FillFields(int id)
        {
            string query_text = "select id_Type_Of_Material, Name_Of_Type from types_of_material" +
                                " where types_of_material.id_Type_Of_Material = @id;";
            MySqlCommand command = new MySqlCommand(query_text, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    textBoxName.Text = reader.GetString(1);
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

        private string CheckData()
        {
            string result = "";

            if (textBoxName.Text == "")
            {
                result += result == "" ? "Наименование" : ", Наименование";
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
                query = "INSERT INTO types_of_material " +
                                       "(Name_Of_Type)" +
                                       " VALUES (@name);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                query = "Update types_of_material set  Name_Of_Type = @name" +
                        " where id_Type_Of_Material = @old_id;";
            }

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", textBoxName.Text);


            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@old_id", old_id);
            }

            return command;
        }
    }
}
