using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace mouse_tracking_web_app.ValidationRules
{
    public class RangeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string v = (string)value;
            string pattern = @"^\s*\d+\.?\d*\s*$";
            if (string.IsNullOrEmpty(v))
                return ValidationResult.ValidResult;
            string[] vSplit = v.Split('-');
            if (vSplit.Length == 1)
            {
                MatchCollection matches = Regex.Matches(vSplit[0], pattern);
                if (matches.Count == 0)
                    return new ValidationResult(false, $"given value is not float");
            }
            else if (vSplit.Length == 2)
            {
                MatchCollection match1 = Regex.Matches(vSplit[0], pattern);
                if (match1.Count == 0)
                    return new ValidationResult(false, $"given value is not float");
                MatchCollection match2 = Regex.Matches(vSplit[1], pattern);
                if (match2.Count == 0)
                    return new ValidationResult(false, $"given value is not float");
                if (double.Parse(vSplit[0]) >= double.Parse(vSplit[1]))
                    return new ValidationResult(false, $"{vSplit[1]}  >= {vSplit[0]}");
            }
            else
                return new ValidationResult(false, "Illegal characters");
            return ValidationResult.ValidResult;
        }
    }
}