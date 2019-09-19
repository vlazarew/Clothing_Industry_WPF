using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Изделия.Список_материалов_и_фурнитуры_для_изделия
{
    public class MaterialsForProduct : IDataErrorInfo
    {
        public string typeOfMaterial { get; set; }
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
