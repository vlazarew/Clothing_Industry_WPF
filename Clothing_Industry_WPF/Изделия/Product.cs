using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Изделия
{
    public class Product : IDataErrorInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public float fixedPrice { get; set; }
        public float moneyToEmployee { get; set; }
        public string description { get; set; }
        public byte[] photo { get; set; }

        public string this[string columnName] => "";

        public string Error => "";
       
    }
}
