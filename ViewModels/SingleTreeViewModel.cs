using mouse_tracking_web_app.MVVM;
using mouse_tracking_web_app.NavigationTree;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.ViewModels
{
    public class NavTreeVm : ViewModelBase
    {
        // public ICommand SelectedPathFromTreeCommand moved to ViewModel

        // RootChildren are used to bind to TreeView
        private ObservableCollection<INavTreeItem> rootChildren = new ObservableCollection<INavTreeItem> { };

        // RootNr determines nr of RootItem that is used as RootNode
        private int rootNr;

        // a Name to bind to the NavTreeTabs
        private string treeName = "";

        // Constructors
        public NavTreeVm(string nTreePath, int pRootNumber = 0, bool pIncludeFileChildren = false)
        {
            // create a new RootItem given rootNumber using convention
            RootNr = pRootNumber;
            NavTreeItem treeRootItem = NavTreeRootItemUtils.ReturnRootItem(pRootNumber, pIncludeFileChildren, nTreePath);
            TreeName = treeRootItem.FriendlyName;

            // Delete RootChildren and init RootChildren ussing treeRootItem.Children
            foreach (INavTreeItem item in RootChildren) { item.DeleteChildren(); }
            RootChildren.Clear();

            foreach (INavTreeItem item in treeRootItem.Children) { RootChildren.Add(item); }
        }

        // Well I suppose with the implicit values these are just for the record / illustration
        public NavTreeVm(string nTreePath, int rootNumber) : this(nTreePath, rootNumber, false) { }

        public NavTreeVm(string nTreePath) : this(nTreePath, 0)
        {
        }

        public ObservableCollection<INavTreeItem> RootChildren
        {
            get => rootChildren;
            set => _ = SetProperty(ref rootChildren, value, "RootChildren");
        }

        public int RootNr
        {
            get => rootNr;
            set => _ = SetProperty(ref rootNr, value, "RootNr");
        }

        public string TreeName
        {
            get => treeName;
            set => _ = SetProperty(ref treeName, value, "TreeName");
        }
        public void RebuildTree(int pRootNr = -1, bool pIncludeFileChildren = false)
        {
            // First take snapshot of current expanded items
            List<string> SnapShot = NavTreeUtils.TakeSnapshot(rootChildren);

            // As a matter of fact we delete and construct the tree//RoorChildren again.....
            // Delete all rootChildren
            foreach (INavTreeItem item in rootChildren) item.DeleteChildren();
            rootChildren.Clear();

            // Create treeRootItem
            if (pRootNr != -1) RootNr = pRootNr;
            NavTreeItem treeRootItem = NavTreeRootItemUtils.ReturnRootItem(RootNr, pIncludeFileChildren);
            if (pRootNr != -1) TreeName = treeRootItem.FriendlyName;

            // Copy children treeRootItem to RootChildren, set up new tree
            foreach (INavTreeItem item in treeRootItem.Children) { RootChildren.Add(item); }

            //Expand previous snapshot
            NavTreeUtils.ExpandSnapShotItems(SnapShot, treeRootItem);
        }
    }
}