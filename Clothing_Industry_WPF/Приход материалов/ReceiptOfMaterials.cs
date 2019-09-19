using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Приход_материалов
{
    public class ReceiptOfMaterials : IDataErrorInfo
    {
        public int id { get; set; }
        public string defaulFolder { get; set; }
        public string nameOfDocument { get; set; }
        public DateTime dateOfEntry { get; set; }
        public string notes{ get; set; }
        public float totalPrice{ get; set; }
        public string paymentStateName{ get; set; }
        public string typeOfTransactionName{ get; set; }
        public string supplierName{ get; set; }

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
