﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using mouse_tracking_web_app.ViewModels;

namespace mouse_tracking_web_app.Views
{
    /// <summary>
    /// Interaction logic for NavigationTree.xaml
    /// </summary>
    public partial class NavigationTree : UserControl
    {

        private readonly NavigationTreeViewModel vm; 

        public NavigationTree()
        {
            InitializeComponent();
            vm = (Application.Current as App).NTVM;
        }

        private void TreeItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType().Name == "StackPanel")
            {
                string fileName = ((sender as StackPanel).Children[1] as TextBlock).Text;
                if (vm.DragStarted(fileName))
                {
                    DragDrop.DoDragDrop(this, new DataObject(), DragDropEffects.Copy | DragDropEffects.Move);
                    vm.DragEnded();
                }
            }
        }
    }
}