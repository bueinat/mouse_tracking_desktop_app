using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    [ValueConversion(typeof(string), typeof(int))]
    public class PathToNumCoverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] subs = ((string)value).Split('\\');
            if (subs.Length > 1)
            {
                subs = subs[subs.Length - 1].Split('.');
                string num = subs[0].Substring(5);
                return int.Parse(num);
            }
            return 0;
        }


        // this is the one causing exception
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //[ValueConversion(typeof(int), typeof(string))]
    public class PercentToTimeConverter : IMultiValueConverter
    {

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return "sss";
            int nframes = (int)(value[0]);
            double percent = (double)(value[1]);
            double frame_number = percent / 100.0 * nframes;
            int frame_rate = 45;
            double time = frame_number / frame_rate;
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);
            int millis = (int)(time % 1 * 100);
            return string.Format("{0:00}:{0:00}:{0:00}", minutes, seconds, millis);
            //return $"{minutes}:{seconds}:{millis}";
        }


        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] arr = new object[3];
            arr[0] = (object)0;
            arr[1] = (object)1;
            arr[2] = (object)2;
            return arr;
            throw new NotImplementedException();
        }
    }

    
}
