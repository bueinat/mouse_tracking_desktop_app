using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace mouse_tracking_web_app.Converters
{
    public class Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ItemsControl ic = value as ItemsControl;
            return ic.ActualHeight / ic.Items.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)

        {
            float x = (float)values[0];
            float y = (float)values[1];
            Image baseImage = values[2] as Image;

            double nx = (1 - x / baseImage.Source.Width) * baseImage.ActualWidth;
            double nx_fix = (baseImage.Parent as Grid).ActualWidth - baseImage.ActualWidth;
            double ny = y / baseImage.Source.Height * baseImage.ActualHeight;
            double ny_fix = (baseImage.Parent as Grid).ActualHeight - baseImage.ActualHeight;

            return new Thickness(0, ny + ny_fix / 2, nx + nx_fix, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class DoesStringExist : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty((string)value);
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

    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleFillnaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            return double.IsNaN(v) ? "" : v.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
                return double.NaN;
            return double.Parse((string)value);
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

    [ValueConversion(typeof(DataBase.DisplayableVideo.State), typeof(bool))]
    public class StateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DataBase.DisplayableVideo.State)value)
            {
                case DataBase.DisplayableVideo.State.ExtractVideo:

                case DataBase.DisplayableVideo.State.FindRatPath:

                case DataBase.DisplayableVideo.State.FindRatFeatues:

                case DataBase.DisplayableVideo.State.SaveToDataBase:
                    return true;
                case DataBase.DisplayableVideo.State.Waiting:

                case DataBase.DisplayableVideo.State.Successful:

                case DataBase.DisplayableVideo.State.Failed:

                default:        // includes waiting
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(DataBase.DisplayableVideo.State), typeof(Visibility))]
    public class StateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DataBase.DisplayableVideo.State)value)
            {
                case DataBase.DisplayableVideo.State.ExtractVideo:

                case DataBase.DisplayableVideo.State.FindRatPath:

                case DataBase.DisplayableVideo.State.FindRatFeatues:

                case DataBase.DisplayableVideo.State.SaveToDataBase:

                case DataBase.DisplayableVideo.State.Waiting:

                    return Visibility.Visible;

                case DataBase.DisplayableVideo.State.Successful:

                case DataBase.DisplayableVideo.State.Failed:

                default:        
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}