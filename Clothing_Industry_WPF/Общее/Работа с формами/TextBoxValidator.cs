using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Clothing_Industry_WPF.Клиенты
{
    public class TextBoxValidator : ValidationRule
    {
        private int minimumLength = 0;
        private int maximumLength = 0;
        private float minimumValue = 0;
        private float maximumValue = 0;
        private string errorMessage;

        public int MinimumLength
        {
            get { return minimumLength; }
            set { minimumLength = value; }
        }

        public int MaximumLength
        {
            get { return maximumLength; }
            set { maximumLength = value; }
        }

        public float MinimumValue
        {
            get { return minimumValue; }
            set { minimumValue = value; }
        }

        public float MaximumValue
        {
            get { return maximumValue; }
            set { maximumValue = value; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            string inputString = value == null ? "" : value.ToString();

            float minValue;
            float.TryParse(inputString, out minValue);

            float maxValue;
            float.TryParse(inputString, out maxValue);

            if ((inputString.Length < MinimumLength && MinimumLength != 0) || (inputString.Length > MaximumLength && MaximumLength != 0)
                    || (minValue < MinimumValue && MinimumValue != 0) || (maxValue > MaximumValue && MaximumValue != 0))
            {
                result = new ValidationResult(false, ErrorMessage);
            }
            return result;
        }
    }
}
