using mouse_tracking_web_app.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.NavigationTree
{
    public static class Extensions
    {
        // this method is taken from here: https://stackoverflow.com/questions/3527203/getfiles-with-multiple-extensions
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, List<string> extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }
    }

    public static class NavTreeRootItemUtils
    {
        // See NavTreeItems for our Model of the tree with RootNode and RootItems
        // RootNode.Children = RootItem(s) or direct RootItem(s).Children dependant RootNode.UseRootItemsAsChildren

        // Convention: Root items end with:
        public const string LastPartRootItemName = "FolderRootItem";

        // Using convention and reflection to construct a List<string> of RootItems defined in the code
        // Note: can we use also MEF??
        public static List<string> ListNavTreeRootItemsByConvention()
        {
            List<string> List = new List<string> { };
            // By convention: all classes that end with "RootItem" form the rootlist
            // Use reflection for list of all NavTreeItem classes,
            IEnumerable<Type> entityTypes =
              from t in System.Reflection.Assembly.GetAssembly(typeof(NavTreeItem)).GetTypes() where t.IsSubclassOf(typeof(NavTreeItem)) select t;

            foreach (Type t in entityTypes)
            {
                if (t.Name.EndsWith(LastPartRootItemName))
                    List.Add(t.Name.Replace(LastPartRootItemName, ""));
            }
            return List;
        }

        // Using convention and reflection to get RootItem iRootNr
        //If iRootNr>= ListNavTreeRootItemsByConvention().Count we use driveItem by default
        public static NavTreeItem ReturnRootItem(SettingsManager sManager, int iRootNr, bool includeFileChildren = false, string rootPath = "")
        {
            // Set default System.Type
            Type selectedType = typeof(FolderItem);
            string selectedName = "Drive";

            // Can you find other type given the conventions ..RootItem name and iRootNr
            IEnumerable<Type> entityTypes =
              from t in System.Reflection.Assembly.GetAssembly(typeof(NavTreeItem)).GetTypes() where t.IsSubclassOf(typeof(NavTreeItem)) select t;

            int i = 0;
            foreach (Type tt in entityTypes)
            {
                if (tt.Name.EndsWith(LastPartRootItemName))
                {
                    if (i == iRootNr)
                    {
                        selectedType = Type.GetType(tt.FullName);
                        selectedName = tt.Name.Replace(LastPartRootItemName, "");
                        break;
                    }
                    i++;
                }
            }

            //// Use selectedType to create root ...
            //if (string.IsNullOrEmpty(rootPath))
            //    NavTreeItem rootItem = (NavTreeItem)Activator.CreateInstance(selectedType);
            //else
            NavTreeItem rootItem = (NavTreeItem)Activator.CreateInstance(selectedType, rootPath, sManager);
            rootItem.FriendlyName = selectedName;
            rootItem.IncludeFileChildren = includeFileChildren;

            return rootItem;
        }
    }

    // General note: Manipulation of the Tree is straightforward in MODEL or VIEWMODEL and all kind of functions
    // can be implemented such as GetParent, AddChild, AddUniqueChild, RemoveChild, SortChildren etc.

    // Here some procedures to support update/refresh the (FileSystem) tree and restore it somewhat to the old visual state
    // First try was to use Union, Intersect and Except using the current children and the new via GetChildren()
    // This became a little complicated, so here we asume that in general not much folders are opened

    // We take a snapshot of the expanded nodes of old tree, reset/build a new tree and try to open all open snapshot nodes
    // - NavTreeUtils.TakeSnapshot(ObservableCollection<INavTreeItem> rootChildren)
    // - NavTreeUtils.ExpandSnapShotItems(List<string> SnapShot, INavTreeItem treeRootItem)

    // - Initially only DriveRootItems were supported (Hirarchical Tree namespace). Simple snapshot suffices.
    //   Now we support also favorites at the cost of a more complex and larger snapshot as a concatenation of
    //   succesive FullPathNames from root to the expanded item.

    //   to do?:
    //   use a FileSystemWatcher to get list of updates, process these

    public static class NavTreeUtils
    {
        private static readonly string strSeparator = "[+]";

        // Procedure used in NavTreeVm. First take snapshot, reconstruct new tree, expand items in snapshot
        public static void ExpandSnapShotItems(List<string> SnapShot, INavTreeItem treeRootItem)
        {
            // try to open all old snapshot nodes
            INavTreeItem Selected = null;
            for (int i = 0; i < SnapShot.Count; i++)
            {
                GetNodeFromName(treeRootItem, SnapShot[i], ref Selected);
                if (Selected != null)
                {
                    Selected.IsExpanded = true;
                }
            }
        }

        // Procedure used in NavTreeVm. First take snapshot, reconstruct new tree, expand items in snapshot
        public static List<string> TakeSnapshot(ObservableCollection<INavTreeItem> rootChildren, SettingsManager sManager)
        {
            List<string> snapShot = new List<string> { };

            // Use a dummy rootnode, is easier to work with
            RootNode rootNode = new RootNode(sManager);
            foreach (INavTreeItem item in rootChildren) rootNode.Children.Add(item);

            // Take snapshot of all expanded nodes
            // new: For handling all kinds of namespaces we take as snapshot concatination of consecutive [Fullnames+separator]
            // For a hierachical namespace currentName+separator not needed
            // Note: it seems possible taking only longest paths and remove subpaths strings from Snapshot
            // note: Length string?? Stringbuilder?
            rootNode.IsExpanded = true;
            string currentName = "";
            VisitExpandedChildrenAndTakeSnapshot(rootNode, currentName, ref snapShot);

            return snapShot;
        }

        private static void GetNodeFromName(INavTreeItem rootNode, string fullPathNames, ref INavTreeItem selectedNode)
        {
            // Just setup a call to GetNodeFromNameLocal to do the work

            // note: to copy or not to copy (pointer, content), all seems ok
            selectedNode = null;

            if ((fullPathNames == null) || (fullPathNames == ""))
            {
                return;
            }

            // make a pathArray.
            // Note now it is not anymore [(drive) (folder)] but [(drive) [(drive) (folder)]]
            string[] separator = new string[] { strSeparator };
            string[] pathArray = fullPathNames.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (pathArray.Length == 0) { return; };

            // Get the node holding the Items
            selectedNode = rootNode;

            int iLevel = 0;
            GetNodeFromNameLocal(ref selectedNode, pathArray, iLevel);
        }

        // Supporting procedure
        private static void GetNodeFromNameLocal(ref INavTreeItem item, string[] pathArray, int iLevel)
        {
            string name;

            // Check children
            name = pathArray[iLevel];
            INavTreeItem child;
            INavTreeItem selected = null;

            for (int i = 0; (i <= item.Children.Count() - 1) && (selected == null); i++)
            {
                child = item.Children[i];
                if (name == child.FullPathName) selected = child;
            }

            item = selected;

            // If we have a hit, step deeper
            iLevel++;
            if ((iLevel <= pathArray.Length - 1) && (item != null)) GetNodeFromNameLocal(ref item, pathArray, iLevel);
        }

        private static void VisitExpandedChildrenAndTakeSnapshot
                                                (INavTreeItem selectedNode, string currentName, ref List<string> snapShot)
        {
            // If node not expanded we do not refresh/repaint rest of the nodes
            if (selectedNode.IsExpanded)
            {
                string newCurrentName = (currentName == strSeparator) ? selectedNode.FullPathName : currentName + strSeparator + selectedNode.FullPathName;
                snapShot.Add(newCurrentName);
                //Console.WriteLine(selectedNode.FullPathName);

                for (int i = 0; i < selectedNode.Children.Count; i++)
                {
                    VisitExpandedChildrenAndTakeSnapshot(selectedNode.Children[i], newCurrentName, ref snapShot);
                }
            }
        }
    }

    // Supporting class, some Tree Operations are easier on (Dummy) RootNode then on RootChildren
    public class RootNode : NavTreeItem
    {
        public RootNode(SettingsManager sManager) : base(sManager)
        {
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            return new ObservableCollection<INavTreeItem> { };
        }

        public override BitmapSource GetMyIcon()
        {
            return myIcon = null;
        }
    }
}