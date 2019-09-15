using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Общее.Работа_с_формами
{
    public class FormLoader
    {
        public static string setNewTitle(WaysToOpenForm.WaysToOpen way, string Title)
        {
            switch (way)
            {
                case WaysToOpenForm.WaysToOpen.create:
                    Title = "Создание " + Title.ToLower() + "а";
                    break;
                case WaysToOpenForm.WaysToOpen.edit:
                    Title += "Изменение " + Title.ToLower() + "а";
                    break;
            }

            return Title;
        }

        /// <summary>
        /// Item1 - наименование поля, что нам надо отображать на форме
        /// Item2 - имя таблицы, откуда тянуть данные
        /// Item3 - combobox, куда вставлять данные
        /// </summary>
        /// <param name="comboboxesAndTables"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, List<string>>> FillComboBoxes(List<Tuple<string, string, string>> comboboxesAndTables, MySqlConnection connection)
        {
            var result = new List<KeyValuePair<string, List<string>>>();
            connection.Open();
            foreach (var comboboxAndTable in comboboxesAndTables)
            {
                string query = "select " + comboboxAndTable.Item1 + " from "+ comboboxAndTable.Item2;
                MySqlCommand command = new MySqlCommand(query, connection);
                
                var resultComboBox = new KeyValuePair<string, List<string>>(comboboxAndTable.Item3, new List<string>());
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        resultComboBox.Value.Add(reader.GetString(0));
                    }
                }

                result.Add(resultComboBox);
            }

            connection.Close();
            return result;
        }

    }
}
