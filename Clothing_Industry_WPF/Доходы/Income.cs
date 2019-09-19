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

        public string this[string columnName] => throw new NotImplementedException();

        public string Error => throw new NotImplementedException();
    }
}
