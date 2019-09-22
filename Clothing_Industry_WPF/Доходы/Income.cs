using Clothing_Industry_WPF.Общее.Работа_с_формами;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Доходы
{
    public class Income : IDataErrorInfo
    {
        public string month { get; set; }
       
        // Наглые заглушки
        public string this[string columnName] => "";

        public string Error => "";
        // Конец

    }
}
