using mouse_tracking_web_app.MVVM;
using System.ComponentModel;
using System.Windows.Input;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.ViewModels
{
    // MainVm for ATreeDemo

    public partial class NavigationTreeViewModel : ViewModelBase, INotifyPropertyChanged
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

        private string rootPath;
        public string NTVM_FileExplorerDirectory
        {
            get => model.FileExplorerDirectory;
            set
            {
                _ = SetProperty(ref rootPath, value, "NTVM_FileExplorerDirectory");
                SingleTree = new NavTreeVm(rootPath); // TODO: it's important that this would happen!!!
            }
        }

        //public string NTVM_FileExplorerDirectory
        //{

        //}
        private readonly Models.MainControllerModel model;
        public NavigationTreeViewModel(Models.MainControllerModel mainController)
        {
            model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("NTVM_" + e.PropertyName);
                if (e.PropertyName == "FileExplorerDirectory")
                    SingleTree = new NavTreeVm(NTVM_FileExplorerDirectory);
            };

            // Construct Single tree
            //SingleTree = new NavTreeVm(nTreePath);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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