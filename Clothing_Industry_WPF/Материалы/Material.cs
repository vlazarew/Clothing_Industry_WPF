using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Материалы
{
    public class Material : IDataErrorInfo
    {
        public int vendorCode { get; set; }
        public string name { get; set; }
        public float cost { get; set; }
        public string notes { get; set; }
        public byte[] photo { get; set; }
        public string unitName { get; set; }
        public string groupOfMaterialName { get; set; }
        public string countryName { get; set; }
        public string typeOfMaterialName { get; set; }

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
