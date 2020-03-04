using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.MainForms
{
    public class Login : IDataErrorInfo
    {
        public string login { get; set; }
        public string password { get; set; }
        public string ip { get; set; }

        // Наглые заглушки
        public string this[string columnName] => "";
        public string Error => "";
        // Конец
    }
}
