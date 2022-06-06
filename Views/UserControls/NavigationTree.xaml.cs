using mouse_tracking_web_app.ViewModels;
using System.Windows;
using System.Windows.Controls;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for NavigationTree.xaml
    /// </summary>
    public partial class NavigationTree : UserControl
    {
        public NavigationTree()
        {
            InitializeComponent();
            DataContext = new NavigationTreeViewModel(NavigationTreePath).SingleTree;
        }

        #region NavigationTreePath DP

        /// <summary>
        /// Identified the NavigationTreePath dependency property
        /// </summary>
        public static readonly DependencyProperty NavigationTreePathProperty =
            DependencyProperty.Register("NavigationTreePath", typeof(string),
              typeof(NavigationTree), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the NavigationTreePath which is displayed next to the field
        /// </summary>
        public string NavigationTreePath
        {
            get => (string)GetValue(NavigationTreePathProperty);
            set => SetValue(NavigationTreePathProperty, value);
        }

        #endregion NavigationTreePath DP

        //public string NavigationTreePath { get; set; }
    }
}