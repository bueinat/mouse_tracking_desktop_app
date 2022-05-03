using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Shapes;

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

            ////List<FeaturesTimeBar> features = new List<FeaturesTimeBar>();
            //List<string> featuresNames = new List<string>(ConfigurationManager.AppSettings["FeaturesList"].Split(','));
            //TextBlock tb = new TextBlock
            //{
            //    Text = featuresNames[0]
            //};
            ////tb.ro
            //_ = featuresGrid.Children.Add(tb);
            //Grid.SetRow(tb, 1);
            //Grid.SetColumn(tb, 1);
            //TextBlock tb2 = new TextBlock
            //{
            //    Text = featuresNames[0]
            //};
            ////tb.ro
            //_ = featuresGrid.Children.Add(tb2);
            //Grid.SetRow(tb2, 2);
            //Grid.SetColumn(tb2, 1);

        }
    }

    //public class FeaturesTimeBar
    //{
    //    public string Name { get; set; }
    //    public string Fillable { get; set; }
    //}
}