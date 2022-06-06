using mouse_tracking_web_app.MVVM;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.NavigationTree
{
    //
    public interface INavTreeItem : INotifyPropertyChanged
    {
        ObservableCollection<INavTreeItem> Children { get; }

        // For text in treeItem
        string FriendlyName { get; set; }

        // Drive/Folder/File naming scheme to retrieve children
        string FullPathName { get; set; }

        // Specific for this application, could be introduced later in more specific interface/classes
        bool IncludeFileChildren { get; set; }

        bool IsExpanded { get; set; }

        // Image used in TreeItem
        BitmapSource MyIcon { get; set; }
        // Design decisions:
        // - do we use INotifyPropertyChanged. Maybe not quite aproporiate in model, but without MVVM framework practical shortcut
        // - do we introduce IsSelected, in most cases I would advice: Yes. I use now button+command to set Path EACH time item pressed
        // bool IsSelected { get; set; }
        // void DeselectAll();
        // For resetting the tree
        void DeleteChildren();
    }

    public class DriveItem : NavTreeItem
    {
        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            DriveInfo drive = new DriveInfo(FullPathName);
            if (!drive.IsReady) return childrenList;

            DirectoryInfo di = new DirectoryInfo(drive.RootDirectory.Name);
            if (!di.Exists) return childrenList;

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                item1 = new FolderItem
                {
                    FullPathName = FullPathName + "\\" + dir.Name,
                    FriendlyName = dir.Name,
                    IncludeFileChildren = IncludeFileChildren
                };
                childrenList.Add(item1);
            }

            if (IncludeFileChildren)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    item1 = new FileItem
                    {
                        FullPathName = FullPathName + "\\" + file.Name,
                        FriendlyName = file.Name
                    };
                    childrenList.Add(item1);
                }
            }
            return childrenList;
        }

        public override BitmapSource GetMyIcon()
        {
            //string Param = "pack://application:,,,/" + "MyImages/diskdrive.png";
            //Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
            //return myIcon = BitmapFrame.Create(uri1);
            return myIcon = Utils.GetIconFn.GetIconDll(FullPathName);
        }
    }

    public class DriveRootItem : NavTreeItem
    {
        public DriveRootItem(string fullPathName)
        {
            //Constructor sets some properties
            FriendlyName = "DriveRoot";
            IsExpanded = true;
            FullPathName = fullPathName;
            //FullPathName = "$xxDriveRoot$";
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            //string[] allDrives = System.Environment.GetLogicalDrives();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady)
                {
                    item1 = new DriveItem();

                    // Some processing for the FriendlyName
                    string fn = drive.Name.Replace(@"\", "");
                    item1.FullPathName = fn;
                    fn = drive.VolumeLabel == string.Empty
                        ? drive.DriveType.ToString() + " (" + fn + ")"
                        : drive.DriveType == DriveType.CDRom
                            ? drive.DriveType.ToString() + " " + drive.VolumeLabel + " (" + fn + ")"
                            : drive.VolumeLabel + " (" + fn + ")";

                    item1.FriendlyName = fn;
                    item1.IncludeFileChildren = IncludeFileChildren;
                    childrenList.Add(item1);
                }
            }

            return childrenList;
        }

        public override BitmapSource GetMyIcon()
        {
            // Note: introduce more "speaking" icons for RootItems
            string Param = "pack://application:,,,/" + "MyImages/bullet_blue.png";
            Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
            return myIcon = BitmapFrame.Create(uri1);
        }
    }

    public class FileItem : NavTreeItem
    {
        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            return childrenList;
        }

        public override BitmapSource GetMyIcon()
        {
            // to do, use a cache for .ext != "" or ".exe" or ".lnk"
            return myIcon = Utils.GetIconFn.GetIconDll(FullPathName);
        }
    }

    public class FolderItem : NavTreeItem
    {
        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            try
            {
                DirectoryInfo di = new DirectoryInfo(FullPathName); // may be acces not allowed
                if (!di.Exists) return childrenList;
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    item1 = new FolderItem
                    {
                        FullPathName = FullPathName + "\\" + dir.Name,
                        FriendlyName = dir.Name,
                        IncludeFileChildren = IncludeFileChildren
                    };
                    childrenList.Add(item1);
                }

                if (IncludeFileChildren)
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        item1 = new FileItem
                        {
                            FullPathName = FullPathName + "\\" + file.Name,
                            FriendlyName = file.Name
                        };
                        childrenList.Add(item1);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            return childrenList;
        }

        public override BitmapSource GetMyIcon()
        {
            return myIcon = Utils.GetIconFn.GetIconDll(FullPathName);
        }
    }
    // Abstact classs next step to implementation
    public abstract class NavTreeItem : ViewModelBase, INavTreeItem
    {
        // Question/ to do. Note that to be sure we use ObservableCollection as property with a notification, remove notification??
        protected ObservableCollection<INavTreeItem> children;

        protected BitmapSource myIcon;

        private bool isExpanded;

        public ObservableCollection<INavTreeItem> Children
        {
            get => children ?? (children = GetMyChildren());
            set => SetProperty(ref children, value, "Children");
        }

        // for display in tree
        public string FriendlyName { get; set; }
        // .. to retrieve info
        public string FullPathName { get; set; }

        public bool IncludeFileChildren { get; set; }

        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value, "IsExpanded");
        }

        public BitmapSource MyIcon
        {
            get => myIcon ?? (myIcon = GetMyIcon());
            set => myIcon = value;
        }
        // Question, not enough C#/Wpf knowledge:
        // - If we delete an NavTreeItem in the root are all its children and corresponding treeview elements garbage collected??
        // - If not, does DeleteChildren() does the work??
        // - For now we decide to use DeleteChildren() but no destructor ~NavTreeItem() that calls DeleteChildren.
        public void DeleteChildren()
        {
            if (children != null)
            {
                // Console.WriteLine(this.FullPathName);

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    children[i].DeleteChildren();
                    children[i] = null;
                    children.RemoveAt(i);
                }

                children = null;
            }
        }

        public abstract ObservableCollection<INavTreeItem> GetMyChildren();

        // We will define these Methods in other derived classes ...
        public abstract BitmapSource GetMyIcon();
        // DeleteChildren, used to
        // 1) remove old tree 2) set children=null, so a new tree is build
    }
}