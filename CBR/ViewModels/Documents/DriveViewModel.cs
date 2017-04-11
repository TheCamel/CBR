using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Models;
using CBR.Core.Services;
using System.IO;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
    public class DriveViewModel : DocumentViewModel
    {
        #region ----------------CONSTRUCTOR----------------

        public DriveViewModel()
		{
            //register to the mediator for messages
			Messenger.Default.Register<string>(this, ViewModelMessages.DeviceContentChanged, HandleDriveChange);

            this.ContentId = "DriveViewModel";
            this.Icon = "pack://application:,,,/Resources/Images/32x32/device/device.png";

			CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Drives", "Drives");
		}

		/// <summary>
		/// Child classes can override this method to perform 
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();

			Messenger.Default.Unregister<string>(this, ViewModelMessages.DeviceContentChanged, HandleDriveChange);

			CultureManager.Instance.UICultureChanged -= new CultureEventArrived(Instance_UICultureChanged);
		}


		void Instance_UICultureChanged(object sender, CultureEventArgs e)
		{
            DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Drives", "Drives");
		}

		#endregion

        #region -----------------PROPERTIES-----------------

        new public string Data
        {
            get { return base.Data as string; }
			set { base.Data = value; }
        }

        private ObservableCollection<SysDriveViewModel> _Drives = new ObservableCollection<SysDriveViewModel>();
        public ICollectionView Drives
        {
            get
            {
                if (_Drives != null)
                {
                    return CollectionViewSource.GetDefaultView(_Drives);
                }
                else
                    return null;
            }
        }
        
        private SysDriveViewModel _currentDrive= null;
        public SysDriveViewModel CurrentDrive
        {
            get { return _currentDrive; }
            set
            {
                if (_currentDrive != value)
                {
                    _currentDrive = value;
                    RaisePropertyChanged("CurrentDrive");
                    RaisePropertyChanged("CurrentTreeDrive");

                    if (_currentDrive != null)
                    {
                        //find matching device type
                        foreach (DeviceInfo inf in DeviceTypes)
                        {
                            if (_currentDrive.Name.Contains(inf.Manufacturer) && _currentDrive.Name.Contains(inf.Model))
                            {
                                CurrentDriveType = inf;
                                continue;
                            }

                        }
                    }
                }
            }
        }

        private ObservableCollection<SysDriveViewModel> _DriveTypes = new ObservableCollection<SysDriveViewModel>();
        public ICollectionView DeviceTypes
        {
            get
            {
                    return CollectionViewSource.GetDefaultView(WorkspaceService.Instance.Settings.DeviceInfoList);
            }
        }

        private DeviceInfo _currentDriveType = null;
        public DeviceInfo CurrentDriveType
        {
            get { return _currentDriveType; }
            set
            {
                if (_currentDriveType != value)
                {
                    _currentDriveType = value;
                    RaisePropertyChanged("CurrentDriveType");
                }
            }
        }


        public ObservableCollection<SysDriveViewModel> CurrentTreeDrive
        {
            get { return new ObservableCollection<SysDriveViewModel>() { CurrentDrive }; }
        }

        private List<ListSysObjectViewModel> _CurrentListContent = null;
        public List<ListSysObjectViewModel> CurrentListContent
        {
            get { return _CurrentListContent; }
            set
            {
                if (_CurrentListContent != value)
                {
                    _CurrentListContent = value;
                    RaisePropertyChanged("CurrentListContent");
                }
            }
        }

        #endregion

        #region -----------------METHODS-----------------

        internal void HandleDriveChange(string path)
        {
            Data = path;
            //SysDriveViewModel sysDrive = new SysDriveViewModel(path);
            //sysDrive.Name = string.Format("{0} - {1}", disk.Name, disk.Model);
            //_Drives.Add(sysDrive);
            //RaisePropertyChanged("Drives");

            CurrentListContent = GetListContent();
        }

        public List<ListSysObjectViewModel> GetListContent()
        {
            List<ListSysObjectViewModel> result = new List<ListSysObjectViewModel>();

            foreach (string directory in Directory.GetDirectories(Data, "*.*", SearchOption.TopDirectoryOnly))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                result.Add(new ListSysDirectoryViewModel(directory, directoryInfo.Name, directoryInfo.LastWriteTime));
            }
            foreach (string file in Directory.GetFiles(Data))
            {
                FileInfo fileInfo = new FileInfo(file);
                result.Add(new ListSysFileViewModel(file, fileInfo.Name, fileInfo.LastWriteTime, fileInfo.Length));
            }
            return result;
        }

        #endregion
    }
}
