using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Поиск_и_фильтры
{
    public static class FilterHandler
    {
        // Описание структуры, по которой мы будем искать
        public struct FilterDescription
        {
            // Поле поиска
            public string field { get; set; }
            // Вид сравнения
            public TypeOfFilter.TypesOfFilter typeOfFilter { get; set; }
            public string filterValue { get; set; }
            // Значение сравнения
            public string value { get; set; }
            // Является ли поле числовым
            public bool isNumber;
            // Является ли поле датой
            public bool isDate;
            public bool active { get; set; }

            public FilterDescription(string field, TypeOfFilter.TypesOfFilter typesOfFilter)
            {
                this.field = field;
                this.typeOfFilter = typesOfFilter;
                this.filterValue = null;
                this.value = null;
                this.isNumber = false;
                this.isDate = false;
                this.active = true;
            }

        }

        public static List<KeyValuePair<TypeOfFilter.TypesOfFilter, string>> FillTypesOfFilter()
        {
            List<KeyValuePair<TypeOfFilter.TypesOfFilter, string>> typesOfFilter = new List<KeyValuePair<TypeOfFilter.TypesOfFilter, string>>();
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.equally, "Равно"));
            //typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.contains, "Содержит"));
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.isFilled, "Заполнено"));
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.isNotFilled, "Не заполнено"));
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.less, "Меньше"));
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.lessOrEqually, "Меньше или равно"));
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.more, "Больше"));
            typesOfFilter.Add(new KeyValuePair<TypeOfFilter.TypesOfFilter, string>(TypeOfFilter.TypesOfFilter.moreOrEqually, "Больше или равно"));

            return typesOfFilter;
        }

        public static string TakeFilter(TypeOfFilter.TypesOfFilter filter)
        {
            switch (filter)
            {
                case TypeOfFilter.TypesOfFilter.equally:
                    {
                        return "=";
                    }
                case TypeOfFilter.TypesOfFilter.isFilled:
                    {
                        return "is null";
                    }
                case TypeOfFilter.TypesOfFilter.isNotFilled:
                    {
                        return "is null";
                    }
                /*case TypeOfFilter.TypesOfFilter.contains:
                    {
                        return "in (";
                    }*/
                case TypeOfFilter.TypesOfFilter.less:
                    {
                        return "<";
                    }
                case TypeOfFilter.TypesOfFilter.lessOrEqually:
                    {
                        return "<=";
                    }
                case TypeOfFilter.TypesOfFilter.more:
                    {
                        return ">";
                    }
                case TypeOfFilter.TypesOfFilter.moreOrEqually:
                    {
                        return ">=";
                    }
                default:
                    {
                        return "=";
                    }
            }
        }

        public static (string editedQuery, List<FilterDescription> filterDescription) MakeFilterQuery(List<FilterDescription> currentFilterDescription, List<FindHandler.FieldParameters> listOfField, string query)
        {
            var filterWindow = new FilterWindow(currentFilterDescription, listOfField);
            if (filterWindow.ShowDialog().Value)
            {
                currentFilterDescription = filterWindow.Result;
            }
            else
            {
                return (editedQuery: "", filterDescription: new List<FilterDescription>());
            }

            string editedQuery = EditFilterQuery(currentFilterDescription, listOfField, query);

            return (editedQuery: editedQuery, filterDescription: currentFilterDescription);
        }

        private static string EditFilterQuery(List<FilterDescription> filter, List<FindHandler.FieldParameters> listOfField, string query)
        {
            string result = query;

            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result = result.Replace(";", " where ");
                    break;
                }
            }

            int index = 0;
            foreach (var filterRecord in filter)
            {
                if (filterRecord.active)
                {
                    result += AddСondition(filterRecord, listOfField);
                    index++;
                    if (index < filter.Count)
                    {
                        result += " and ";
                    }
                }
            }

            return result;
        }

        private static string AddСondition(FilterDescription filter, List<FindHandler.FieldParameters> listOfField)
        {
            string result = "";
            var field = listOfField.Where(kvp => kvp.application_name == filter.field).First().db_name;
            var typeFilter = TakeFilter(filter.typeOfFilter);
            if (filter.typeOfFilter == TypeOfFilter.TypesOfFilter.isFilled)
            {
                result += "NOT ";
            }

            if (!filter.isDate)
            {
                result += string.Format(field + " " + typeFilter + "\"{0}\"", filter.value);
            }
            else
            {
                string day = filter.value.Substring(0, 2);
                string month = filter.value.Substring(3, 2);
                string year = filter.value.Substring(6, 4);
                result += string.Format(field + " " + typeFilter + " \'{0}-{1}-{2}\'", year, month, day);
                //result += string.Format(" DATE_FORMAT(" + field + ", '%d.%m.%Y') = \'{0}\'", filter.value);
            }

            /*if (filter.typeOfFilter == TypeOfFilter.TypesOfFilter.contains)
            {
                result += ") ";
            }*/

            return result;
        }

        public static (DataTable dataTable, List<FilterDescription> filterDescription) GetDataWithFilter(List<FilterDescription> currentFilterDescription, MySqlConnection connection, List<FindHandler.FieldParameters> listOfField, string query)
        {
            (string editedQuery, List<FilterDescription> filterDescription) result = MakeFilterQuery(currentFilterDescription, listOfField, query);

            if (result.editedQuery == "")
            {
                return (dataTable: null, filterDescription: new List<FilterDescription>());
            }

            connection.Open();
            var dataTable = FormLoader.ExecuteQuery(result.editedQuery, connection);
            connection.Close();

            return (dataTable: dataTable, filterDescription: result.filterDescription);
        }
    }
}
