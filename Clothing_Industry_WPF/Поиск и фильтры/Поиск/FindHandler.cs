using Clothing_Industry_WPF.Клиенты;
using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clothing_Industry_WPF.Поиск_и_фильтры
{
    public static class FindHandler
    {
        // Описание структуры, с помощью которой мы будем производить поиск (хранит результат)
        public struct FindDescription
        {
            // Поле, по которому ищем
            public string field;
            // Тип поиска
            public TypeOfFind.TypesOfFind typeOfFind;
            // Значение, которое ввел пользователь
            public string value;
            public bool isDate;
            public bool isNumber;

            public FindDescription(string field, TypeOfFind.TypesOfFind typesOfFind)
            {
                this.field = field;
                this.typeOfFind = typesOfFind;
                this.value = null;
                this.isDate = false;
                this.isNumber = false;
            }
        }

        // Описание структуры, которая содержит информацию о полях, по которым мы можем делать отбор
        public struct FieldParameters
        {
            // Наименование поля в БД
            public string db_name;
            // Наименование поля в приложении
            public string application_name { get; set; }
            // Тип поля
            public string type;

            public FieldParameters(string db_name, string application_name, string type)
            {
                this.db_name = db_name;
                this.application_name = application_name;
                this.type = type;
            }
        }

        public static (string editedQuery, FindDescription findDescription) MakeFindQuery(FindDescription currentFindDescription, List<FieldParameters> listOfField, string query,
                                                                                            string notNullQuery = "", string nullQuery = "", string groupBy = "", string nullQueryAlex = "")
        {
            var findWindow = new FindWindow(currentFindDescription, listOfField);
            if (findWindow.ShowDialog().Value)
            {
                currentFindDescription = findWindow.Result;
            }
            else
            {
                return (editedQuery: "", findDescription: new FindDescription());
            }

            var field = listOfField.Where(kvp => kvp.application_name == currentFindDescription.field).First().db_name;
            string editedQuery;

            // Разбор огромного поиска заказов
            if (notNullQuery != "")
            {
                if (currentFindDescription.value != "")
                {
                    if (!currentFindDescription.isDate)
                    {
                        editedQuery = notNullQuery.Replace(";", " and " + field + " ");
                        editedQuery += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
                    }
                    else
                    {
                        editedQuery = notNullQuery.Replace(";", " and DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                        editedQuery += string.Format("= \'{0}\'", currentFindDescription.value);
                    }
                    editedQuery = editedQuery.Replace(";", groupBy);
                    editedQuery = editedQuery.Replace(";", " union ");
                    editedQuery += nullQuery;

                    if (!currentFindDescription.isDate)
                    {
                        editedQuery = notNullQuery.Replace(";", " and " + field + " ");
                        editedQuery += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
                    }
                    else
                    {
                        editedQuery = notNullQuery.Replace(";", " and DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                        editedQuery += string.Format("= \'{0}\'", currentFindDescription.value);
                    }

                    editedQuery = editedQuery.Replace(";", groupBy);
                }
                else
                {
                    editedQuery = nullQueryAlex;
                }
            }
            // Остальные формы
            else
            {

                if (!currentFindDescription.isDate)
                {
                    editedQuery = query.Replace(";", " where " + field + " ");
                    editedQuery += string.Format(currentFindDescription.typeOfFind == TypeOfFind.TypesOfFind.byExactCoincidence ? "= \"{0}\"" : "like \"{0}%\"", currentFindDescription.value);
                }
                else
                {
                    editedQuery = query.Replace(";", " where DATE_FORMAT(" + field + ", '%d.%m.%Y')  ");
                    editedQuery += string.Format("= \'{0}\'", currentFindDescription.value);
                }
            }

            return (editedQuery: editedQuery, findDescription: currentFindDescription);
        }

        public static void DescribeHelper(string query, MySqlConnection connection, List<KeyValuePair<string, string>> pairs)
        {
            MySqlCommand command = new MySqlCommand(query, connection);

            using (DbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    pairs.Add(new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1)));
                }
            }
        }

        public static (DataTable dataTable, FindDescription findDescription) GetDataWithFind(FindDescription currentFindDescription, MySqlConnection connection, List<FieldParameters> listOfField, string query,
                                                                                            string notNullQuery = "", string nullQuery = "", string groupBy = "", string nullQueryAlex = "")
        {
            (string editedQuery, FindDescription findDescription) result = MakeFindQuery(currentFindDescription, listOfField, query, notNullQuery, nullQuery, groupBy, nullQueryAlex);

            if (result.editedQuery == "")
            {
                return (dataTable: null, findDescription: new FindDescription());
            }

            connection.Open();
            var dataTable = FormLoader.ExecuteQuery(result.editedQuery, connection);
            connection.Close();

            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show("По отобранному значению ничего не найдено", "Нет строк", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return (dataTable: dataTable, findDescription: result.findDescription);
        }

    }
}
