using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Примерки
{
    public class Fitting : IDataErrorInfo
    {
        public int orderId { get; set; }
        public DateTime date { get; set; }
        public string notes { get; set; }
        public string typeOfFitting { get; set; }

        public string this[string columnName]
        {
            get
            {
                return "";
            }
        }

        public string Error => throw new NotImplementedException();
    }
}
