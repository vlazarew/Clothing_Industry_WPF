using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Начисление_ЗП
{
    public class Payroll : IDataErrorInfo
    {
        public string employeeLogin { get; set; }
        public string period { get; set; }
        public DateTime dateOfPay { get; set; }
        public float salary { get; set; }
        public float pieceWorkPayment { get; set; }
        public float totalSalary { get; set; }
        public float penalty { get; set; }
        public float toPay { get; set; }
        public string notes { get; set; }
        public bool PaidOff { get; set; }
        

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
