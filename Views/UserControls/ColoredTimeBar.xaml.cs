using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for ColoredTimeBar.xaml
    /// </summary>
    public partial class ColoredTimeBar : UserControl
    {
        public ColoredTimeBar()
        {
            InitializeComponent();
            mainGrid.DataContext = this;
            //this.DataContext = this;
            //Rectangle r1 = new Rectangle
            //{
            //    Width = 10,
            //    Fill = new SolidColorBrush(Colors.Navy),
            //    Margin = new Thickness(20, 0, 0, 0),
            //    HorizontalAlignment = HorizontalAlignment.Left
            //};
            //_ = mainGrid.Children.Add(r1);
            //Rectangle r2 = new Rectangle
            //{
            //    Width = 10,
            //    Fill = new SolidColorBrush(Colors.Crimson),
            //    Margin = new Thickness(30, 0, 0, 0),
            //    HorizontalAlignment = HorizontalAlignment.Left
            //};
            //_ = mainGrid.Children.Add(r2);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            //int marginCount = 0;
            Button button;
            double normFactor = ActualWidth / MaxLength;
            if (!(TimesList is null))
            {
                foreach (Tuple<int, int> timeRange in TimesList)
                {
                    button = new Button
                    {
                        Width = (timeRange.Item2 - timeRange.Item1) * normFactor,
                        Background = new SolidColorBrush(Colors.Navy), // TODO: pass this color as well
                        Margin = new Thickness(timeRange.Item1 * normFactor, 0, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Style = FindResource("NoHoverButton") as Style
                    };
                    button.Click += TimeRangeClicked;
                    _ = mainGrid.Children.Add(button);
                }
            }
        }

        // TODO: create some property and tie it to this
        private void TimeRangeClicked(object sender, RoutedEventArgs e)
        {
            //(Button)sender.Margin
            textBlock.Text = $"button clicked {((Button)sender).Margin.Left}";
        }


        //public Dictionary<string, List<Tuple<int, int>>> TimesDictionary { get; set; }
        //public int MaxLength { get; set; }
        //public string FeatureName { get; set; }
        public List<Tuple<int, int>> TimesList => TimesDictionary?[FeatureName];

        #region FeatureName DP

        /// <summary>
        /// Gets or sets the FeatureName which is displayed next to the field
        /// </summary>
        public string FeatureName
        {
            get => (string)GetValue(FeatureNameProperty);
            set => SetValue(FeatureNameProperty, value);
        }

        /// <summary>
        /// Identified the FeatureName dependency property
        /// </summary>
        public static readonly DependencyProperty FeatureNameProperty =
            DependencyProperty.Register("FeatureName", typeof(string),
              typeof(ColoredTimeBar), new PropertyMetadata(""));

        #endregion

        #region MaxLength DP

        /// <summary>
        /// Gets or sets the MaxLength which is displayed next to the field
        /// </summary>
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        /// <summary>
        /// Identified the MaxLength dependency property
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int),
              typeof(ColoredTimeBar), new PropertyMetadata(0));

        #endregion


        //#region TimesList DP

        ///// <summary>
        ///// Gets or sets the TimesList which is displayed next to the field
        ///// </summary>
        //public List<Tuple<int, int>> TimesList
        //{
        //    get => (List<Tuple<int, int>>)GetValue(TimesListProperty);
        //    set => SetValue(TimesListProperty, value);
        //}

        ///// <summary>
        ///// Identified the TimesList dependency property
        ///// </summary>
        //public static readonly DependencyProperty TimesListProperty =
        //    DependencyProperty.Register("TimesList", typeof(List<Tuple<int, int>>),
        //      typeof(ColoredTimeBar), new PropertyMetadata(null));

        //#endregion

        #region TimesDictionary DP

        /// <summary>
        /// Gets or sets the TimesDictionary which is displayed next to the field
        /// </summary>
        public Dictionary<string, List<Tuple<int, int>>> TimesDictionary
        {
            get => (Dictionary<string, List<Tuple<int, int>>>)GetValue(TimesDictionaryProperty);
            set => SetValue(TimesDictionaryProperty, value);
        }

        /// <summary>
        /// Identified the TimesDictionary dependency property
        /// </summary>
        public static readonly DependencyProperty TimesDictionaryProperty =
            DependencyProperty.Register("TimesDictionary", typeof(Dictionary<string, List<Tuple<int, int>>>),
              typeof(ColoredTimeBar), new PropertyMetadata(null));

        #endregion


    }
}