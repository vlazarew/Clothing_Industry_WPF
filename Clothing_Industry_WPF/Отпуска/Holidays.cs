using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothing_Industry_WPF.Отпуска
{
    public class Holidays : IDataErrorInfo
    {
        public string employeeLogin { get; set; }
        public int year { get; set; }
        public int daysOfHolidays { get; set; }
        public int daysUsed { get; set; }
        public int restOfDays { get; set; }
        public DateTime plannedStart { get; set; }
        public DateTime inFactStart { get; set; }
        public DateTime inFactEnd { get; set; }
        public string notes { get; set; }

        public string this[string columnName] => "";

        public string Error => "";
    }
}

