using System;
using System.Collections.Generic;     // add to references
using System.IO;
using System.Windows.Media.Imaging;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

// Note this procedure can be used as part of code that:
// - does some tests on filename so that in specific cases specific icons can be used
// - uses a dictionairy for extensions

namespace mouse_tracking_web_app.Utils
{
    public static class GetIconFn
    {
        public static BitmapSource GetIconDll(string fileName)
        {
            BitmapSource myIcon = null;

            bool validDrive = false;
            foreach (DriveInfo D in DriveInfo.GetDrives())
            {   //D.DriveType.
                if (fileName == D.Name)
                {
                    validDrive = true;
                }
            }

            if (File.Exists(fileName) || Directory.Exists(fileName) || validDrive)
            {
                using (System.Drawing.Icon sysIcon = ShellIcon.GetLargeIcon(fileName))
                {
                    try
                    {
                        myIcon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                        sysIcon.Handle,
                                        System.Windows.Int32Rect.Empty,
                                        BitmapSizeOptions.FromWidthAndHeight(34, 34));
                    }
                    catch
                    {
                        myIcon = null;
                    }
                }
            }
            return myIcon;
        }
    }

    // ImageCache for some speed
    public static class ImageCache
    {
        public static Dictionary<string, BitmapSource> imageList = new Dictionary<string, BitmapSource>();

        public static BitmapSource GetImage(string fullpath)
        {
            string ext = Path.GetExtension(fullpath);
            _ = ext.ToLower();

            // if in the list we are done
            if (imageList.ContainsKey(ext))
            {
                return imageList[ext];
            }

            // get the image
            BitmapSource myIcon;
            myIcon = GetIconFn.GetIconDll(fullpath);

            // put myIcon in the imageList, unless its extension says that it
            if ((ext != "") && (ext != ".exe") && (ext != ".lnk") && (ext != ".ico"))
            {
                imageList.Add(ext, myIcon);
            }
            return myIcon;
        }
    }

    public static class TestCurrentOs
    {
        // Only tested for one 64 bit Windows7 PC, not other operating systems
        public static bool IsWindows7()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            PlatformID platform = osInfo.Platform;
            int majorVersion = osInfo.Version.Major;
            int minorVersion = osInfo.Version.Minor;
            return (platform == PlatformID.Win32NT) && (majorVersion == 6) && (minorVersion >= 1);
        }
    }
}