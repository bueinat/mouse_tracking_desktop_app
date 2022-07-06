using Microsoft.Win32;
using mouse_tracking_web_app.ViewModels;
using System.IO;
using System.Windows;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for TabsWindow.xaml
    /// </summary>
    public partial class TabsWindow : Window
    {
        private readonly MainControllerViewModel vm;

        public TabsWindow()
        {
            InitializeComponent();
            vm = (Application.Current as App).MainVM;
            DataContext = vm;
        }

        internal void SetTab(int index)
        {
            tabs.SelectedIndex = index;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv; *.txt)|*.csv;*.txt|All files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, vm.VM_CSVString);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog inputDialog = new SettingsDialog();
            if (inputDialog.ShowDialog() == true)
                System.Console.WriteLine("dialog success");
            else
                System.Console.WriteLine("dialog closed");
        }
    }
}