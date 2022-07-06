using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace mouse_tracking_web_app.NavigationTree
{
    public class DriveRootItem : NavTreeItem
    {
        public DriveRootItem(ViewModels.SettingsManager settingsManager, string fullPathName) : base(settingsManager)
        {
            //Constructor sets some properties
            FriendlyName = "DriveRoot";
            IsExpanded = true;
            FullPathName = fullPathName; // "$xxDriveRoot$";
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady)
                {
                    item1 = new DriveItem(SM);

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

    public class FolderRootItem : NavTreeItem
    {
        public FolderRootItem(string fullPathName, ViewModels.SettingsManager settingsManager) : base(settingsManager)
        {
            //Constructor sets some properties
            FriendlyName = "FolderRoot";
            IsExpanded = true;
            FullPathName = fullPathName;
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            try
            {
                DirectoryInfo di = new DirectoryInfo(FullPathName); // maybe access not allowed
                if (!di.Exists) return childrenList;
                item1 = new FolderItem(SM)
                {
                    FullPathName = FullPathName,
                    FriendlyName = di.Name,
                    IncludeFileChildren = IncludeFileChildren,
                    IsExpanded = true
                };
                childrenList.Add(item1);
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
}