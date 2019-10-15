using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа.Список_материалов_для_изделия
{
    class OrderMaterialForProduct : IDataErrorInfo
    {
        public string nameOfMaterial { get; set; }

        public int productId { get; set; }
        public int groupOfMaterial { get; set; }
        public float count { get; set; }

        public OrderMaterialForProduct()
        {
            nameOfMaterial = "";
            productId = -1;
            groupOfMaterial = -1;
            count = -1;
        }

        public OrderMaterialForProduct(int productId, int groupOfMaterial, float count)
        {
            nameOfMaterial = "";
            this.productId = productId;
            this.groupOfMaterial = groupOfMaterial;
            this.count = count;
        }


        public string this[string columnName] => "";

        public string Error => "";
    }
}
