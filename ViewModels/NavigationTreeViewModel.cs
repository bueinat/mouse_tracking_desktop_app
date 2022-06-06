using mouse_tracking_web_app.MVVM;
using System.Windows.Input;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.ViewModels
{
    // MainVm for ATreeDemo

    public partial class NavigationTreeViewModel : ViewModelBase
    {
        #region JustForSingleTreeDemo

        private bool includeFiles;

        private RelayCommand rebuildTreeCommand;

        // and some properties for setting parameters in Demo program
        private int rootNr;

        public bool IncludeFiles
        {
            get => includeFiles;
            set
            {
                if (!SetProperty(ref includeFiles, value, "IncludeFiles")) return;
                SingleTree.RebuildTree(RootNr, includeFiles);
            }
        }

        public ICommand RebuildTreeCommand => rebuildTreeCommand ?? (rebuildTreeCommand = new RelayCommand(RebuildSingleTree));

        public int RootNr
        {
            get => rootNr;
            set
            {
                if (!SetProperty(ref rootNr, value, "RootNr")) return;
                SingleTree.RebuildTree(RootNr, includeFiles);
            }
        }

        // Single tree for Demo
        public NavTreeVm SingleTree { get; set; }

        public void RebuildSingleTree(object p)
        {
            SingleTree.RebuildTree(RootNr, IncludeFiles);
        }

        #endregion JustForSingleTreeDemo

        // Selected path set by command call when item is clicked
        private string selectedPath;

        // For now SelectedPath common to all trees
        private RelayCommand selectedPathFromTreeCommand;

        // constructor constructs Single Tree and TabbedNavTreesVm
        public NavigationTreeViewModel(string nTreePath)
        {
            // Construct Single tree
            SingleTree = new NavTreeVm(nTreePath);
        }

        public string SelectedPath
        {
            get => selectedPath;
            set => SetProperty(ref selectedPath, value, "SelectedPath");
        }

        public ICommand SelectedPathFromTreeCommand => selectedPathFromTreeCommand ??
                                       (selectedPathFromTreeCommand =
                              new RelayCommand(x => SelectedPath = x as string));
    }
}