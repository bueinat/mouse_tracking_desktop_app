using mouse_tracking_web_app.MVVM;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Input;

// based on code from here: https://www.codeproject.com/Articles/390514/Playing-with-a-MVVM-Tabbed-TreeView-for-a-File-Exp

namespace mouse_tracking_web_app.ViewModels
{
    // MainVm for ATreeDemo

    public partial class NavigationTreeViewModel : INotifyPropertyChanged
    {
        #region JustForSingleTreeDemo

        private bool includeFiles = true;

        private RelayCommand rebuildTreeCommand;

        // and some properties for setting parameters in Demo program
        private int rootNr;

        public bool IncludeFiles
        {
            get => includeFiles;
            set
            {
                includeFiles = value;
                NotifyPropertyChanged("IncludeFiles");
            }
        }

        public ICommand RebuildTreeCommand => rebuildTreeCommand ?? (rebuildTreeCommand = new RelayCommand(RebuildSingleTree));

        public int RootNr
        {
            get => rootNr;
            set
            {
                rootNr = value;
                NotifyPropertyChanged("RootNr");
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

        public string NTVM_FileExplorerDirectory
        {
            get => model.FileExplorerDirectory;
            set
            {
                model.FileExplorerDirectory = value;
                NotifyPropertyChanged("NTVM_FileExplorerDirectory");
            }
        }


        private readonly Models.MainControllerModel model;
        public NavigationTreeViewModel(Models.MainControllerModel mainController)
        {
            model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("NTVM_" + e.PropertyName);
                if (e.PropertyName == "FileExplorerDirectory")
                    SingleTree = new NavTreeVm(NTVM_FileExplorerDirectory, 0, true);
            };

            // Construct Single tree
            //SingleTree = new NavTreeVm(nTreePath);

        }

        public bool NTVM_DragEnabled
        {
            get => model.DragEnabled;
            set
            {
                model.DragEnabled = value;
                NotifyPropertyChanged("NTVM_DragEnabled");
            }
        }

        private readonly List<string> videoTypesList = new List<string>(ConfigurationManager.AppSettings["VideoTypesList"].Split(','));

        public bool DragStarted(string fileName)
        {
            if (videoTypesList.Any(s => fileName.EndsWith(s)))
                NTVM_DragEnabled = true;
            return NTVM_DragEnabled;
        }

        public void DragEnded()
        {
            NTVM_DragEnabled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string SelectedPath
        {
            get => selectedPath;
            set
            {
                selectedPath = value;
                NotifyPropertyChanged("SelectedPath");
            }
        }

        public ICommand SelectedPathFromTreeCommand => selectedPathFromTreeCommand ??
                                       (selectedPathFromTreeCommand =
                              new RelayCommand(x => SelectedPath = x as string));
    }
}