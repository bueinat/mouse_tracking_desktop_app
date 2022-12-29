using MaterialDesignThemes.Wpf;
using mouse_tracking_web_app.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for PathStatisticsPresentor.xaml
    /// </summary>
    ///
    public partial class PathStatisticsPresentor : UserControl
    {
        private Color textColor;
        private PlottingControllerViewModel vm;

        public PathStatisticsPresentor()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).PCVM;
            textColor = new PaletteHelper().GetTheme().SecondaryDark.Color;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (vm is null)
            {
                vm = DataContext as PlottingControllerViewModel;
                vm.PropertyChanged +=
                    delegate (object _sender, PropertyChangedEventArgs _e)
                    {
                        if (_e.PropertyName == "VMPC_FeaturesPercents") // TODO: find the right feature
                            Reload();
                    };
                Reload();
            }
        }

        private void Reload()
        {
            if (Dispatcher.CheckAccess())
                ReloadWrapped();
            else
                Dispatcher.Invoke(() =>
                {
                    ReloadWrapped();
                });
        }

        private void ReloadWrapped()
        {
            if (vm.VMPC_FeaturesPercents is null)
                return;
            layoutRoot.Children.RemoveRange(4 * 2, layoutRoot.Children.Count - 1);

            for (int i = 0; i < vm.VMPC_FeaturesList.Count; i++)
            {
                TextBlock tb_label = new TextBlock()
                {
                    Text = vm.VMPC_FeaturesList[i].Replace('_', ' ') + ": ",
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(textColor),
                    Style = FindResource("MaterialDesignCaptionTextBlock") as Style
                };

                Grid.SetRow(tb_label, i + 4);
                Grid.SetColumn(tb_label, 0);
                _ = layoutRoot.Children.Add(tb_label);

                TextBlock tb_value = new TextBlock()
                {
                    Text = string.Format("{0:0.00}%", vm.VMPC_FeaturesPercents[vm.VMPC_FeaturesList[i]]),
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = FindResource("MaterialDesignCaptionTextBlock") as Style,
                    Foreground = new BrushConverter().ConvertFrom("#646464") as SolidColorBrush
                };

                Grid.SetRow(tb_value, i + 4);
                Grid.SetColumn(tb_value, 2);
                _ = layoutRoot.Children.Add(tb_value);
            }
        }
    }
}