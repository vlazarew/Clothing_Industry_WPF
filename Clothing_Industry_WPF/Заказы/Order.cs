using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Заказы
{
    public class Order : IDataErrorInfo
    {
        public int id { get; set; }
        public DateTime dateOfOrder { get; set; }
        public float discountPerCent { get; set; }
        public float totalPrice { get; set; }
        public float paid { get; set; }
        public float debt { get; set; }
        public DateTime dateOfDelievery { get; set; }
        public float addedPriceForComplexity { get; set; }
        public string notes { get; set; }
        public float salaryToExecutor { get; set; }
        public string responsible { get; set; }
        public string executor { get; set; }
        public string customerLogin { get; set; }
        public string statusName { get; set; }
        public string typeOfOrderName { get; set; }

        // Пустой констуктор
        public Order()
        {
            id = -1;
            dateOfOrder = DateTime.Now;
            discountPerCent = 0;
            totalPrice = 0;
            paid = 0;
            debt = 0;
            dateOfDelievery = DateTime.Now;
            addedPriceForComplexity = 0;
            notes = "";
            salaryToExecutor = 0;
            responsible = "";
            executor = "";
            customerLogin = "";
            statusName = "";
            typeOfOrderName = "";
        }

        public string this[string columnName]
        {
            get
            { return ""; }

        }

        public string Error => throw new NotImplementedException();
    }
}
