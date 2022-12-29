using MaterialDesignThemes.Wpf;
using mouse_tracking_web_app.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for CurrentDataPresentor.xaml
    /// </summary>
    public partial class CurrentDataPresentor : UserControl
    {
        private Color textColor;
        private VideoControllerViewModel vm;
        public CurrentDataPresentor()
        {
            InitializeComponent();
            DataContext = (Application.Current as App).VCVM;
            textColor = new PaletteHelper().GetTheme().SecondaryDark.Color;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            if (vm is null)
            {
                vm = DataContext as VideoControllerViewModel;
                vm.PropertyChanged +=
                    delegate (object _sender, PropertyChangedEventArgs _e)
                    {
                        if (_e.PropertyName == "VMVC_Features")
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
            if (vm.VMVC_Features is null)
                return;
            layoutRoot.Children.RemoveRange(8 * 2, layoutRoot.Children.Count - 1);

            for (int i = 0; i < vm.VMVC_FeaturesList.Count; i++)
            {
                TextBlock tb_label = new TextBlock()
                {
                    Text = vm.VMVC_FeaturesList[i].Replace('_', ' ') + ": ",
                    Margin = new Thickness(5, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(textColor),
                    Style = FindResource("MaterialDesignCaptionTextBlock") as Style
                };
                Grid.SetRow(tb_label, i + 8);
                Grid.SetColumn(tb_label, 0);
                _ = layoutRoot.Children.Add(tb_label);

                bool v = vm.VMVC_Features[vm.VMVC_FeaturesList[i]];
                TextBlock tb_value = new TextBlock()
                {
                    Text = v.ToString(),
                    Margin = new Thickness(5, 0, 15, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = FindResource("MaterialDesignCaptionTextBlock") as Style,
                    Foreground = new SolidColorBrush(v ? Colors.LimeGreen : Colors.IndianRed)
                };

                Grid.SetRow(tb_value, i + 8);
                Grid.SetColumn(tb_value, 2);
                _ = layoutRoot.Children.Add(tb_value);
            }
        }
    }
}