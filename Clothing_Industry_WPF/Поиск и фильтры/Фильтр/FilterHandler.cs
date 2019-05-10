using Clothing_Industry_WPF.Перечисления;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Clothing_Industry_WPF.Перечисления.TypeOfFilter;

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
                    { return "=";
                    }
            }
        }

    }
}
