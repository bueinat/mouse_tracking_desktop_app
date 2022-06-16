using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace mouse_tracking_web_app.Converters
{
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

    [ValueConversion(typeof(bool), typeof(double))]
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 0.5 : 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ItemsControl ic = value as ItemsControl;
            return ic.ActualHeight / ic.Items.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class DoesStringExist : IValueConverter
    {
        // c# to view
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Tuple<double, double>), typeof(string))]
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string v = (string)value;
            if (string.IsNullOrEmpty(v))
                return new Tuple<double, double>(double.NaN, double.NaN);
            string[] vSplit = v.Split(',');
            return new Tuple<double, double>(double.Parse(vSplit[0]), double.Parse((vSplit.Length > 1) ? vSplit[1] : vSplit[0]));
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

    [ValueConversion(typeof(float), typeof(string))]
    public class FillnaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float v = (float)value;
            return float.IsNaN(v) ? "-" : v.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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

    [ValueConversion(typeof(int), typeof(string))]
    public class LengthRange : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join(",", Enumerable.Range(0, (int)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                ItemsControl ic = values[0] as ItemsControl;
                double actualHeigth = System.Convert.ToDouble(values[1]);
                Console.WriteLine(actualHeigth / ic.Items.Count);
                return actualHeigth / ic.Items.Count;
            }
            catch (Exception)
            {
                return 0.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? Visibility.Hidden : Visibility.Visible;
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