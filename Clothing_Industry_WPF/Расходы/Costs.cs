using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Расходы
{
    public class Costs : IDataErrorInfo
    {
        public int id { get; set; }
        public string defaultFolder{ get; set; }
        public DateTime dateOfCost{ get; set; }
        public string nameOfDocument{ get; set; }
        public float amount{ get; set; }
        public string notes{ get; set; }
        public string to{ get; set; }
        public string from{ get; set; }
        public string consumptionCategoriesName{ get; set; }
        public string typeOfPaymentName{ get; set; }
        public string periodicityName{ get; set; }


        public string this[string columnName] => "";

        public string Error => "";
    }
}

