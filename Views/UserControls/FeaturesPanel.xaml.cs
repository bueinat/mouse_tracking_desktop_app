using mouse_tracking_web_app.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for FeaturesPanel.xaml
    /// </summary>
    public partial class FeaturesPanel : UserControl
    {
        public FeaturesPanel()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            layoutRoot.Children.RemoveRange(1, layoutRoot.Children.Count - 1);

            for (int i = 0; i < layoutRoot.RowDefinitions.Count; i++)
            {
                TextBlock tb = new TextBlock()
                {
                    Text = FeaturesList[i].Substring(2) + ": ",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#476D76"))
                };
                Grid.SetRow(tb, i);
                Grid.SetColumn(tb, 0);
                layoutRoot.Children.Add(tb);

                ColoredTimeBar ctb = new ColoredTimeBar()
                {
                    Margin = new Thickness(5, 5, 0, 0),
                    FeatureName = FeaturesList[i],
                    MaxLength = MaxLength,
                    TimesList = TimesDictionary is null ? null : TimesDictionary.ContainsKey(FeaturesList[i]) ? TimesDictionary[FeaturesList[i]] : null
                };
                Binding binding = new Binding("VMVC_StepCounter")
                {
                    Source = DataContext as VideoControllerViewModel,
                    Mode = BindingMode.OneWayToSource,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ctb.SetBinding(ColoredTimeBar.TimeProperty, binding);
                Grid.SetRow(ctb, i);
                Grid.SetColumn(ctb, 1);
                layoutRoot.Children.Add(ctb);
            }
        }

        #region TimesDictionary DP

        /// <summary>
        /// Identified the TimesDictionary dependency property
        /// </summary>
        public static readonly DependencyProperty TimesDictionaryProperty =
            DependencyProperty.Register("TimesDictionary", typeof(Dictionary<string, List<Tuple<int, int>>>),
              typeof(FeaturesPanel), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TimesDictionary which is displayed next to the field
        /// </summary>
        public Dictionary<string, List<Tuple<int, int>>> TimesDictionary
        {
            get => (Dictionary<string, List<Tuple<int, int>>>)GetValue(TimesDictionaryProperty);
            set => SetValue(TimesDictionaryProperty, value);
        }

        #endregion TimesDictionary DP

        #region FeaturesList DP

        /// <summary>
        /// Identified the FeaturesList dependency property
        /// </summary>
        public static readonly DependencyProperty FeaturesListProperty =
            DependencyProperty.Register("FeaturesList", typeof(List<string>),
              typeof(FeaturesPanel), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the FeaturesList which is displayed next to the field
        /// </summary>
        public List<string> FeaturesList
        {
            get => (List<string>)GetValue(FeaturesListProperty);
            set => SetValue(FeaturesListProperty, value);
        }

        #endregion FeaturesList DP

        #region MaxLength DP

        /// <summary>
        /// Identified the MaxLength dependency property
        /// </summary>
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int),
              typeof(FeaturesPanel), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the MaxLength which is displayed next to the field
        /// </summary>
        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        #endregion MaxLength DP
    }
}