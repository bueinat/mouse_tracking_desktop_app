using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace mouse_tracking_web_app.ViewModels
{
    public class MainControllerViewModel : INotifyPropertyChanged
    {
        private readonly Models.MainControllerModel model;
        private VideoControllerViewModel vc_vm;
        public VideoControllerViewModel VM_VideoControllerViewModel
        {
            get => vc_vm;
            set => vc_vm = value;
        }

        public MainControllerViewModel(Models.MainControllerModel mainController)
        {
            model = mainController;
            model.PropertyChanged +=
            delegate (object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged("VM_" + e.PropertyName);
            };
            VM_VideoControllerViewModel = (Application.Current as App).VCVM;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string VM_ErrorMessage
        {
            get => model.ErrorMessage;
            set => model.ErrorMessage = value;
        }

        public bool VM_HasErrorMessage => model.HasErrorMessage;

        public string VM_VideoName
        {
            get => model.VideoName;
            set => model.VideoName = value;
        }

        public string VM_FramePath
        {
            get => model.FramePath;
            set => model.FramePath = value;
        }

        public int VM_FramesNumber
        {
            get => model.FramesNumber;
            set => model.FramesNumber = value;
        }

        public bool VM_IsLoading
        {
            get => model.IsLoading;
            set => model.IsLoading = value;
        }

        public void Connect()
        {
            model.DBHandler.Connect();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}