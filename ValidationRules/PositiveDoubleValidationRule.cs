using System;
using System.Globalization;
using System.Windows.Controls;

namespace mouse_tracking_web_app.ValidationRules
{
    public class PositiveDoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                double d = double.Parse((string)value);
                return d > 0 ? ValidationResult.ValidResult : new ValidationResult(false, "size must be larger than zero");
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "value cannot be interpreted as double");
            }
        }
    }
}