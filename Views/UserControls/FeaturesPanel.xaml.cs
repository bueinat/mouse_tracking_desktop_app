using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for FeaturesPanel.xaml
    /// </summary>
    public partial class FeaturesPanel : UserControl
    {
        private readonly List<string> featuresNames = new List<string>(ConfigurationManager.AppSettings["FeaturesList"].Split(','));

        public FeaturesPanel()
        {
            InitializeComponent();
            LoadItems();
        }

        public List<Feature> Items { get; set; }

        public void LoadItems()
        {
            if (!(TimesDictionary is null))
            {

                List<Feature> items = new List<Feature>();
                foreach (string feature in featuresNames)
                    items.Add(new Feature() { Name = feature, Label = $"{feature}: ", TimesList = TimesDictionary[feature] });
                lvFeatures.ItemsSource = items;
                Items = items;
            }
        }

        #region MaxLength DP

        /// <summary>
        /// Identified the MaxLength dependency property
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int),
              typeof(FeaturesPanel), new PropertyMetadata(1, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the MaxLength which is displayed next to the field
        /// </summary>
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        #endregion MaxLength DP

        #region TimesDictionary DP

        /// <summary>
        /// Identified the TimesDictionary dependency property
        /// </summary>
        public static readonly DependencyProperty TimesDictionaryProperty =
            DependencyProperty.Register("TimesDictionary", typeof(Dictionary<string, List<Tuple<int, int>>>),
              typeof(FeaturesPanel), new PropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject dependencyObject,
           DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is FeaturesPanel panel)
            {
                Console.WriteLine("test");
                panel.LoadItems();
                //if (e.Property.Name == "TimesDictionary")
                //    TimesDictionary = (Dictionary<string, List<Tuple<int, int>>>)e.NewValue;
            }
        }

        /// <summary>
        /// Gets or sets the TimesDictionary which is displayed next to the field
        /// </summary>
        public Dictionary<string, List<Tuple<int, int>>> TimesDictionary
        {
            get => (Dictionary<string, List<Tuple<int, int>>>)GetValue(TimesDictionaryProperty);
            set => SetValue(TimesDictionaryProperty, value);
        }

        #endregion TimesDictionary DP
        private double normFactor = 1;

        private void RectGridLoaded(object sender, RoutedEventArgs e)
        {
            Button button;
            normFactor = ((Rectangle)((Grid)sender).Children[0]).ActualWidth / MaxLength;
            foreach (Tuple<int, int> timeRange in ((Feature)((Grid)sender).DataContext).TimesList)
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
                _ = ((Grid)sender).Children.Add(button);
            }
        }

        private void TimeRangeClicked(object sender, RoutedEventArgs e)
        {
            Time = (int)(((Button)sender).Margin.Left / normFactor);
        }

        #region Time DP

        /// <summary>
        /// Identified the Time dependency property
        /// </summary>
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(int),
              typeof(FeaturesPanel), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the Time which is displayed next to the field
        /// </summary>
        public int Time
        {
            get => (int)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        #endregion Time DP
    }
    public class Feature
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public List<Tuple<int, int>> TimesList { get; set; }

    }
}