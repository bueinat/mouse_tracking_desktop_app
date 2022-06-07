using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        }

        private void TreeItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop(this, new DataObject(), DragDropEffects.Copy | DragDropEffects.Move);

            if (sender.GetType().Name == "StackPanel")
                System.Console.WriteLine($"mouse left button down on {sender.GetType().Name}, {((sender as StackPanel).Children[1] as TextBlock).Text}");
            else
                System.Console.WriteLine($"mouse left button down on {sender.GetType().Name}");


        }
    }
}