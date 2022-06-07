using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

// folder icon code taken from here: https://stackoverflow.com/questions/42910628/is-there-a-way-to-get-the-windows-default-folder-icon-using-c

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

// Where did I get this sample?
// For almost same code see http://www.leghumped.com/blog/2008/03/23/retrieving-shell-icons-in-c/

/// <summary>
/// Retrievs shell info associated with a file or filetype
/// </summary>
/// <summary>
/// Get a 32x32 or 16x16 System.Drawing.Icon depending on which function you call
/// either GetSmallIcon(string fileName) or GetLargeIcon(string fileName)
/// </summary>
///
namespace mouse_tracking_web_app.Utils
{
    public class ShellIcon
    {
        private class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0; // Large icon
            public const uint SHGFI_SMALLICON = 0x1; // Small icon
            public const uint SHSIID_FOLDER = 0x3;  // Folder icon
            public const uint USEFILEATTRIBUTES = 0x000000010; // when the full path isn't available

            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);

            [DllImport("shell32.dll")]
            public static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref SHSTOCKICONINFO psii);

            //extra
            [DllImport("Shell32.dll")]
            public static extern int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, uint nIcons);

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        }


        public ShellIcon()
        { }

        public static Icon FolderLarge => GetStockIcon(Win32.SHSIID_FOLDER, Win32.SHGFI_LARGEICON);

        public static Icon GetLargeIcon(string fileName)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            _ = Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
            try
            {
                Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                _ = Win32.DestroyIcon(shinfo.hIcon);
                return icon;
            }
            // can't create a icon
            catch
            {
                // if the path is a directory, simply return directory icon
                if (File.GetAttributes(fileName).HasFlag(FileAttributes.Directory))
                    //return GetStockIcon(SHSIID_FOLDER, SHGSI_LARGEICON);
                    return FolderLarge;

                // if not a directory, copy the file somewhere else and use the extention
                // TODO: maybe I should save some kind of cache of all file types
                string[] sName = fileName.Split('.');
                string tempFileName;
                tempFileName = Path.Combine(".", $"temp.{sName[sName.Length - 1]}");
                File.WriteAllText(tempFileName, "Hello World");

                _ = Win32.SHGetFileInfo(tempFileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
                
                try
                {
                    Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                    _ = Win32.DestroyIcon(shinfo.hIcon);
                    return icon;
                }
                catch
                {
                    Icon icon = new Icon(SystemIcons.Application, 40, 40);
                    return icon;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(tempFileName))
                        File.Delete(tempFileName);
                }
            }
        }

        public static Icon GetSmallIcon(string fileName)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            _ = Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);
            //The icon is returned in the hIcon member of the shinfo struct
            Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            _ = Win32.DestroyIcon(shinfo.hIcon);
            return icon;
        }

        private static Icon GetStockIcon(uint type, uint size)
        {
            var info = new SHSTOCKICONINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);

            Win32.SHGetStockIconInfo(type, Win32.SHGFI_ICON | size, ref info);

            var icon = (Icon)Icon.FromHandle(info.hIcon).Clone(); // Get a copy that doesn't use the original handle
            Win32.DestroyIcon(info.hIcon); // Clean up native icon to prevent resource leak

            return icon;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }
    }
}