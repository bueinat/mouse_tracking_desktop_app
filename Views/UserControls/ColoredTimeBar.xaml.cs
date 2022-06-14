using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for ColoredTimeBar.xaml
    /// </summary>
    public partial class ColoredTimeBar : UserControl
    {
        private double normFactor;

        public ColoredTimeBar()
        {
            InitializeComponent();
            mainGrid.DataContext = this;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Button button;
            normFactor = ActualWidth / MaxLength;
            foreach (Tuple<int, int> timeRange in TimesList)
            {
                button = new Button
                {
                    Width = (timeRange.Item2 - timeRange.Item1) * normFactor,
                    Height = 10,
                    Background = new SolidColorBrush(Colors.Navy),
                    Margin = new Thickness(timeRange.Item1 * normFactor, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Style = FindResource("NoHoverButton") as Style
                };
                button.Click += TimeRangeClicked;
                _ = mainGrid.Children.Add(button);
            }
        }

        private void TimeRangeClicked(object sender, RoutedEventArgs e)
        {
            Time = (int)(((Button)sender).Margin.Left / normFactor);
        }

        #region FeatureName DP

        /// <summary>
        /// Identified the FeatureName dependency property
        /// </summary>
        public static readonly DependencyProperty FeatureNameProperty =
            DependencyProperty.Register("FeatureName", typeof(string),
              typeof(ColoredTimeBar), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the FeatureName which is displayed next to the field
        /// </summary>
        public string FeatureName
        {
            get => (string)GetValue(FeatureNameProperty);
            set => SetValue(FeatureNameProperty, value);
        }

        #endregion FeatureName DP

        #region MaxLength DP

        /// <summary>
        /// Identified the MaxLength dependency property
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int),
              typeof(ColoredTimeBar), new PropertyMetadata(0));

        // TODO: maybe there's no need with two different controllers for this stuff. Maybe we can combine it into one.

        /// <summary>
        /// Gets or sets the MaxLength which is displayed next to the field
        /// </summary>
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        #endregion MaxLength DP

        #region Time DP

        /// <summary>
        /// Identified the Time dependency property
        /// </summary>
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(int),
              typeof(ColoredTimeBar), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the Time which is displayed next to the field
        /// </summary>
        public int Time
        {
            get => (int)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        #endregion Time DP

        #region TimesList DP

        /// <summary>
        /// Identified the TimesList dependency property
        /// </summary>
        public static readonly DependencyProperty TimesListProperty =
            DependencyProperty.Register("TimesList", typeof(List<Tuple<int, int>>),
              typeof(ColoredTimeBar), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TimesList which is displayed next to the field
        /// </summary>
        public List<Tuple<int, int>> TimesList
        {
            get => (List<Tuple<int, int>>)GetValue(TimesListProperty);
            set => SetValue(TimesListProperty, value);
        }

        #endregion Times DP
    }
}