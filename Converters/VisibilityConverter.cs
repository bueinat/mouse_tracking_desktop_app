using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace mouse_tracking_web_app.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : (object)Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class AddErrorPrefix : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Error Message: " + (string)value;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //[ValueConversion(typeof(string), typeof(string))]
    public class EnableDefaultImage : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value;
            if (File.Exists((string)value))
                return value;
            return "../Images/default_image.png";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // TODO: fix this thing up. Really understand what conversion should be made.
    [ValueConversion(typeof(string), typeof(int))]
    public class PathToNumCoverter : IValueConverter
    {
        private string path_suffix;
        private string path_prefix;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] subs = ((string)value).Split('\\');
            if (subs.Length > 1)
            {
                string fileName = subs[subs.Length - 1];        // the last item is the filename
                subs = subs.Take(subs.Length - 1).ToArray();    // remove filename from path
                path_prefix = string.Join("\\", subs);          // prefix is joining the directories
                subs = fileName.Split('.');                     // extracting filename without extention

                string num = subs[0].Substring(5);
                path_prefix = path_prefix + "\\" + subs[0].Substring(0, 5);
                path_suffix = "." + subs[1];
                return int.Parse(num);
            }
            return 0;
        }

        // this is the one causing exception
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            return path_prefix + (int)v + path_suffix;
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

    
}
