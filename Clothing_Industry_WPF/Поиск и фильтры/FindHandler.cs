using Clothing_Industry_WPF.Перечисления;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public string application_name;
            // Тип поля
            public string type;

            public FieldParameters(string db_name, string application_name, string type)
            {
                this.db_name = db_name;
                this.application_name = application_name;
                this.type = type;
            }
        }

    }
}
