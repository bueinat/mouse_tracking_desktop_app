using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;

namespace mouse_tracking_web_app.ValidationRules
{
    public class ConnectionStringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value))
                return ValidationResult.ValidResult;
            try
            {
                Uri uri = new Uri((string)value);
                return ValidationResult.ValidResult;
            }
            catch (UriFormatException)
            {
                return new ValidationResult(false, "not a valid connection string");
            }
        }
    }

    public class PythonPathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value))
                return ValidationResult.ValidResult;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo((string)value)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = @"C:\ProgramData\yolov5\test_python.py"
                };
                Process processResults = Process.Start(start);
                return string.IsNullOrEmpty(processResults.StandardError.ReadToEnd()) && processResults.StandardOutput.ReadToEnd().StartsWith("python version")
                    ? ValidationResult.ValidResult
                    : throw new Exception();
            }
            catch (Exception)
            {
                return new ValidationResult(false, "not a python path");
            }
        }
    }

    public class MarkerSizeValidationRule : ValidationRule
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