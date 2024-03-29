﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
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

    public class DatabaseNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value))
                return ValidationResult.ValidResult;
            Regex rgx = new Regex("[^A-Za-z0-9]");
            return rgx.IsMatch((string)value) ? new ValidationResult(false, "illegal characters") : ValidationResult.ValidResult;
        }
    }

    public class CommaSeparatedListValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value))
                return ValidationResult.ValidResult;
            Regex rgx = new Regex("[A-Za-z0-9,]+");
            return rgx.Matches((string)value).Count == 1 ? ValidationResult.ValidResult : new ValidationResult(false, "not a comma seperated list");
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
                    Arguments = @"C:\ProgramData\MouseApp\test_python.py"
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
}