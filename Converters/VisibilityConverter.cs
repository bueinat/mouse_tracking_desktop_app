using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace mouse_tracking_web_app.Converters
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (bool)value ? Colors.LimeGreen : Colors.IndianRed;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //return (double)value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty((string)value) ? double.NaN : (object)double.Parse((string)value);
            //throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToOpenCloseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Close Features Panel" : "Open Features Panel";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals((string)value, "Close Features Panel");
        }
    }

    [ValueConversion(typeof(float), typeof(string))]
    public class FillnaConverter : IValueConverter
    {
        public bool IsNaN(float f)
        {
#pragma warning disable CS1718 // Comparison made to same variable
            return f != f;
#pragma warning restore CS1718 // Comparison made to same variable
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float v = (float)value;
            return IsNaN(v) ? "-" : v.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                value = false;
            return (bool)value ? "PlayCircleOutline" : "PauseCircleOutline";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "PlayCircleOutline";
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class EnableDefaultImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return File.Exists((string)value) ? value : "/Images/default_image.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    public class PercentToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double frame_number = (double)value; // percent / 100.0 * nframes;
            int frame_rate = 45;
            double time = frame_number / frame_rate * 1000; // time is seconds
            TimeSpan t = TimeSpan.FromMilliseconds(time);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds / 10);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //[ValueConversion(typeof(string), typeof(int))]
    public class SpeedToIndexConverter : IValueConverter
    {
        private readonly List<string> speeds = new List<string>
            {
                "0.25",
                "0.50",
                "1.00",
                "1.50",
                "2.00"
            };

        // convert speed from vm to index of option in dropdown menu
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double speed = (double)value;
            switch (speed)
            {
                case 0.25:
                    return 0;

                case 0.5:
                    return 1;

                case 1.0:
                    return 2;

                case 1.5:
                    return 3;

                case 2.0:
                    return 4;

                default:
                    return 2;
            }
        }

        // convert index to speed
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(speeds[(int)value]);
        }
    }
}