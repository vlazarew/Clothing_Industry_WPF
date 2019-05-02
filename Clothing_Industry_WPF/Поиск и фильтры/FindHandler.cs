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
        // Описание структуры, с помощью которой мы будем производить поиск
        public struct FindDescription
        {
            // Поле, по которому ищем
            public string field;
            // Тип поиска
            public TypeOfFind.TypesOfFind typeOfFind;
            // Значение, которое ввел пользователь
            public string value;
            public bool isDate;
        }

    }
}
