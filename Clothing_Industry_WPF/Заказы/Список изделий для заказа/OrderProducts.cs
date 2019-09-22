using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа
{
    public class OrderProducts : IDataErrorInfo
    {
        public string product { get; set; }
        public int count { get; set; }

        public string this[string columnName] => "";

        public string Error => "";
    }
}
