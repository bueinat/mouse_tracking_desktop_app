using Microsoft.Win32;
using mouse_tracking_web_app.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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

        private void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv; *.txt)|*.csv;*.txt|All files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, vm.VM_CSVString);
        }
    }
}