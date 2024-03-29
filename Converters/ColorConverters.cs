﻿using System;
using System.Globalization;
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

    [ValueConversion(typeof(UtilTypes.DisplayableVideo.State), typeof(SolidColorBrush))]
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string colorCode;
            switch ((UtilTypes.DisplayableVideo.State)value)
            {
                case UtilTypes.DisplayableVideo.State.Waiting:
                    colorCode = "#BEE5F7";
                    break;

                case UtilTypes.DisplayableVideo.State.ExtractVideo:
                    colorCode = "#F9D2A8";
                    break;

                case UtilTypes.DisplayableVideo.State.FindRatFeatures:
                    colorCode = "#EEEF9B";
                    break;

                case UtilTypes.DisplayableVideo.State.FindRatPath:
                    colorCode = "#E8FDAC";
                    break;

                case UtilTypes.DisplayableVideo.State.SaveToDataBase:
                    colorCode = "#C5FDA2";
                    break;

                case UtilTypes.DisplayableVideo.State.Successful:
                    colorCode = "#86FD8D";
                    break;

                case UtilTypes.DisplayableVideo.State.Failed:
                case UtilTypes.DisplayableVideo.State.Canceled:
                    colorCode = "#FD9584";
                    break;

                default:
                    colorCode = "#000000";
                    break;
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(UtilTypes.DisplayableVideo.State), typeof(SolidColorBrush))]
    public class StateToProgColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string colorCode;
            switch ((UtilTypes.DisplayableVideo.State)value)
            {
                case UtilTypes.DisplayableVideo.State.Successful:
                    colorCode = "#5DA84A";
                    break;

                case UtilTypes.DisplayableVideo.State.Failed:
                case UtilTypes.DisplayableVideo.State.Canceled:
                    colorCode = "#C7191A";
                    break;

                default:
                    colorCode = "#FDAA48";
                    break;
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}