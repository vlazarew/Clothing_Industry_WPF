using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using Clothing_Industry_WPF.Поиск_и_фильтры;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clothing_Industry_WPF.Материалы
{
    public class Material : IDataErrorInfo
    {
        public string vendorCode { get; set; }
        public string name { get; set; }
        public float cost { get; set; }
        public string notes { get; set; }
        public byte[] photo { get; set; }
        public string unitName { get; set; }
        public string groupOfMaterialName { get; set; }
        public string countryName { get; set; }
        public string typeOfMaterialName { get; set; }

        // Пустой конструктор
        public Material()
        {
            vendorCode = "";
            name = "";
            cost = 0;
            notes = "";
            photo = null;
            unitName = "";
            groupOfMaterialName = "";
            countryName = "";
            typeOfMaterialName = "";
        }

        // Конструктор по Артикулу
        public Material(string vendorCode, MySqlConnection connection)
        {
            this.vendorCode = vendorCode;

            string queryText = "select materials.Vendor_Code, materials.Name_Of_Material, materials.Cost_Of_Material, materials.Notes, units.Name_Of_Unit," +
                                " groups_of_material.Name_Of_Group, types_of_material.Name_Of_Type, countries.Name_Of_Country, materials.Photo" +
                                " from materials" +
                                " join units on materials.Units_id_Unit = units.id_Unit" +
                                " join groups_of_material on materials.Groups_Of_Material_id_Group_Of_Material = groups_of_material.id_Group_Of_Material" +
                                " join types_of_material on materials.Types_Of_Material_id_Type_Of_Material = types_of_material.id_Type_Of_Material" +
                                " join countries on materials.Countries_id_Country = countries.id_Country" +
                                " where materials.Vendor_Code = @vendor_code;";
            MySqlCommand command = new MySqlCommand(queryText, connection);
            command.Parameters.AddWithValue("@vendor_code", vendorCode);
            connection.Open();
            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    name = reader.GetString(1);
                    cost = reader.GetFloat(2);
                    notes = reader.IsDBNull(3) ? "" : reader.GetString(3);

                    unitName = reader.GetString(4);
                    groupOfMaterialName = reader.GetString(5);
                    typeOfMaterialName = reader.GetString(6);
                    countryName = reader.GetString(7);

                    photo = reader.IsDBNull(8) ? null : (byte[])(reader[8]);
                }
            }
            connection.Close();
        }

        // Проверка на заполненность полей
        private string CheckData()
        {
            string result = "";

            if (vendorCode == "")
            {
                result += result == "" ? "Артикул" : ", Артикул";
            }
            if (name == "")
            {
                result += result == "" ? "Название материала" : ", Название материала";
            }
            if (cost == 0)
            {
                result += result == "" ? "Стоимость" : ", Стоимость";
            }
            if (unitName == "")
            {
                result += result == "" ? " Единица измерения" : ",  Единица измерения";
            }
            if (groupOfMaterialName == "")
            {
                result += result == "" ? "Вид" : ", Вид";
            }
            if (typeOfMaterialName == "")
            {
                result += result == "" ? "Тип" : ", Тип";
            }
            if (countryName == "")
            {
                result += result == "" ? "Страна производитель" : ", Страна производитель";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        // Запросец на все материалы
        public static string getQueryText()
        {
            string query_text = "select materials.Vendor_Code, materials.Name_Of_Material, materials.Cost_Of_Material, materials.Notes, units.Name_Of_Unit, groups_of_material.Name_Of_Group, " +
                                "types_of_material.Name_Of_Type, countries.Name_Of_Country" +
                                " from materials" +
                                " join units on materials.Units_id_Unit = units.id_Unit" +
                                " join groups_of_material on materials.Groups_Of_Material_id_Group_Of_Material = groups_of_material.id_Group_Of_Material" +
                                " join types_of_material on materials.Types_Of_Material_id_Type_Of_Material = types_of_material.id_Type_Of_Material" +
                                " join countries on materials.Countries_id_Country = countries.id_Country;";
            return query_text;
        }

        // Получение данных обо всех материалах
        public static DataTable getListMaterials(MySqlConnection connection)
        {
            string queryText = getQueryText();

            connection.Open();
            var dataTable = FormLoader.ExecuteQuery(queryText, connection);
            connection.Close();

            return dataTable;
        }

        // Вызов сохранения и прогон по всей логике
        public bool Save(MySqlConnection connection, WaysToOpenForm.WaysToOpen way, string vendorCodeRecord)
        {
            string warning = CheckData();
            if (warning == "")
            {
                MySqlTransaction transaction;

                connection.Open();
                transaction = connection.BeginTransaction();

                //Создать/изменить запись в таблице Материалы
                MySqlCommand command = SaveInDB(connection, way, vendorCodeRecord);
                command.Transaction = transaction;
                MySqlCommand commandStrore = SaveInStore(connection, way, vendorCodeRecord);
                commandStrore.Transaction = transaction;

                try
                {
                    command.ExecuteNonQuery();
                    commandStrore.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка сохранения!", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                connection.Close();
            }
            else
            {
                MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        // Генерация команды сохранения в БД
        private MySqlCommand SaveInDB(MySqlConnection connection, WaysToOpenForm.WaysToOpen way, string vendorCodeRecord)
        {
            string query = "";

            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                query = "INSERT INTO materials " +
                         "(Vendor_Code, Name_Of_Material, Cost_Of_Material, Notes," +
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
            command.Parameters.AddWithValue("@vendor_code", vendorCode);
            command.Parameters.AddWithValue("@name_of_material", name);
            command.Parameters.AddWithValue("@cost_of_material", cost);
            command.Parameters.AddWithValue("@notes", notes);

            // ГОВНОКОД НАЧАЛО
            MySqlCommand commandUnit = new MySqlCommand("select id_Unit from units where Name_Of_Unit = @unit", connection);
            commandUnit.Parameters.AddWithValue("unit", unitName);
            int id_unit = -1;
            using (DbDataReader reader = commandUnit.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_unit = reader.GetInt32(0);
                }
            }

            MySqlCommand commandGroup = new MySqlCommand("select id_Group_Of_Material from groups_of_material where Name_Of_Group = @group", connection);
            commandGroup.Parameters.AddWithValue("group", groupOfMaterialName);
            int id_group = -1;
            using (DbDataReader reader = commandGroup.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_group = reader.GetInt32(0);
                }
            }

            MySqlCommand commandType = new MySqlCommand("select id_Type_Of_Material from types_of_material where Name_Of_Type = @type", connection);
            commandType.Parameters.AddWithValue("@type", typeOfMaterialName);
            int id_type = -1;
            using (DbDataReader reader = commandType.ExecuteReader())
            {
                while (reader.Read())
                {
                    id_type = reader.GetInt32(0);
                }
            }

            MySqlCommand commandCountry = new MySqlCommand("select id_Country from countries where Name_Of_Country = @country", connection);
            commandCountry.Parameters.AddWithValue("@country", countryName);
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
            // ГОВНОКОД КОНЕЦ

            // Обработка фото
            command.Parameters.AddWithValue("@image", photo);

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldvendor_code", vendorCodeRecord);
            }

            return command;
        }

        // Обновление данных на складе
        private MySqlCommand SaveInStore(MySqlConnection connection, WaysToOpenForm.WaysToOpen way, string vendorCodeRecord)
        {
            string queryStore = "";
            if (way == WaysToOpenForm.WaysToOpen.create)
            {
                queryStore = "INSERT INTO store (Materials_Vendor_Code, Count)" +
                             " VALUES(@vendor_code,0);";
            }
            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                queryStore = "Update store set Materials_Vendor_Code = @vendor_code" +
                             " where Materials_Vendor_Code = @oldvendor_code;";
            }

            MySqlCommand command = new MySqlCommand(queryStore, connection);
            command.Parameters.AddWithValue("@vendor_code", vendorCode);

            if (way == WaysToOpenForm.WaysToOpen.edit)
            {
                command.Parameters.AddWithValue("@oldvendor_code", vendorCodeRecord);
            }

            return command;
        }

        // Удаление материала
        public static void DeleteFromDB(List<string> vendorCodes, MySqlConnection connection)
        {
            connection.Open();

            foreach (string vendorCode in vendorCodes)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                // Удаление из таблицы Склад
                string queryDeleteInStore = "delete from store where Materials_Vendor_Code = @Vendor_code";
                MySqlCommand commandDeleteInStore = new MySqlCommand(queryDeleteInStore, connection, transaction);
                commandDeleteInStore.Parameters.AddWithValue("@Vendor_Code", vendorCode);

                // Удаление из таблицы Материалы
                string queryTable = "delete from materials where vendor_code = @Vendor_Code";
                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@Vendor_Code", vendorCode);

                try
                {
                    commandDeleteInStore.ExecuteNonQuery();
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Удаление материала " + vendorCode + " не удалось", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }

        // Список полей, по которым мы можем делать поиск
        private static List<FindHandler.FieldParameters> FillFindFields(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = TakeDescribe(connection);
            List<FindHandler.FieldParameters> result = new List<FindHandler.FieldParameters>();

            result.Add(new FindHandler.FieldParameters("Vendor_Code", "Артикул", describe.Where(key => key.Key == "Vendor_Code").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Material", "Название", describe.Where(key => key.Key == "Name_Of_Material").First().Value));
            result.Add(new FindHandler.FieldParameters("Cost_Of_Material", "Стоимость", describe.Where(key => key.Key == "Cost_Of_Material").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Unit", "Система измерения", describe.Where(key => key.Key == "Name_Of_Unit").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Group", "Вид", describe.Where(key => key.Key == "Name_Of_Group").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Type", "Тип", describe.Where(key => key.Key == "Name_Of_Type").First().Value));
            result.Add(new FindHandler.FieldParameters("Name_Of_Country", "Страна производитель", describe.Where(key => key.Key == "Name_Of_Country").First().Value));

            return result;
        }

        // Получить полное описание таблиц, по которым мы можем вести поиск
        private static List<KeyValuePair<string, string>> TakeDescribe(MySqlConnection connection)
        {
            List<KeyValuePair<string, string>> describe = new List<KeyValuePair<string, string>>();
            connection.Open();

            // Вот тут нужно проходить по всем таблицам, что мы используем в итоговом запросе
            FindHandler.DescribeHelper("describe materials", connection, describe);
            FindHandler.DescribeHelper("describe units", connection, describe);
            FindHandler.DescribeHelper("describe groups_of_material", connection, describe);
            FindHandler.DescribeHelper("describe types_of_material", connection, describe);
            FindHandler.DescribeHelper("describe countries", connection, describe);
            // Вот тут конец

            connection.Close();

            return describe;
        }

        // Поиск
        public static (DataTable dataTable, FindHandler.FindDescription findDescription) FindListMaterials(FindHandler.FindDescription currentFindDescription, MySqlConnection connection)
        {
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();
            (DataTable dataTable, FindHandler.FindDescription findDescription) result = FindHandler.GetDataWithFind(currentFindDescription, connection, listOfField, query);
            return result;
        }

        // Фильтр
        public static (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) FilterListMaterials(List<FilterHandler.FilterDescription> currentFilterDescription, MySqlConnection connection)
        {
            // Список полей, по которым мы можем делать отбор
            List<FindHandler.FieldParameters> listOfField = FillFindFields(connection);
            var query = getQueryText();

            (DataTable dataTable, List<FilterHandler.FilterDescription> filterDescription) result = FilterHandler.GetDataWithFilter(currentFilterDescription, connection, listOfField, query);
            return result;
        }

        public string this[string columnName]
        {
            get
            {
                return "";
            }
        }

        public string Error => throw new NotImplementedException();
    }
}
