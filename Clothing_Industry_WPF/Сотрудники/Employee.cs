using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Сотрудники
{
    public class Employee : IDataErrorInfo
    {
        public string login { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string patronymic { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string passportData { get; set; }
        public string notes { get; set; }
        public byte[] photo { get; set; }
        public DateTime added { get; set; }
        public float lastSalary { get; set; }
        public string positionName { get; set; }

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
