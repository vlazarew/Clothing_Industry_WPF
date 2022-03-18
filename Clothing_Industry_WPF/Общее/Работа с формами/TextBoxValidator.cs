using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Clothing_Industry_WPF.Общее.Работа_с_формами
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

        // Ввод только букв в int численные поля 
        private static readonly Regex intRegex = new Regex("[^0-9]");
        // Ввод только букв и запятой во float численные поля 
        private static readonly Regex floatRegex = new Regex("[^0-9,]");

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)

        {
            ValidationResult result = new ValidationResult(true, null);
            string inputString = value == null ? "" : value.ToString();

            float.TryParse(inputString, out float numericValue);

            if ((inputString.Length < MinimumLength && MinimumLength != 0) || (inputString.Length > MaximumLength && MaximumLength != 0)
                    || (numericValue < MinimumValue && MinimumValue != 0) || (numericValue > MaximumValue && MaximumValue != 0))
            {
                result = new ValidationResult(false, ErrorMessage);
            }
            return result;
        }

        public static bool IsIntTextAllowed(string text)
        {
            return !intRegex.IsMatch(text);
        }

        public static bool IsFloatTextAllowed(string text)
        {
            return !floatRegex.IsMatch(text);
        }
    }
}
