using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Приход_материалов.Содержание_документа_прихода
{
    public class ContentOfReceiptOfMaterials : IDataErrorInfo
    {
        public string name { get; set; }
        public int count { get; set; }

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
