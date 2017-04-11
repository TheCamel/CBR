using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CBR.Core.Services;
using CBR.Core.Models;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
    class DriveExplorerViewModel : ToolViewModel
    {
        #region ----------------CONSTRUCTOR----------------

        public DriveExplorerViewModel()
            : base(CultureManager.Instance.GetLocalization("ByCode", "DriveExplorerView.Title", "Drive Explorer"))
		{
            Data = new List<LogicalDiskInfo>();

            //register to the mediator for messages
			Messenger.Default.Register<LogicalDiskInfo>(this, ViewModelMessages.DeviceAdded, HandleDeviceAdd);
			Messenger.Default.Register<LogicalDiskInfo>(this, ViewModelMessages.DeviceRemoved, HandleDeviceRemove);

            this.ContentId = "DriveExplorerViewModel";
            this.Icon = "pack://application:,,,/Resources/Images/32x32/device/device.png";
            base.IsVisible = false;

            CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);

            Task.Factory.StartNew(() =>
            {
				try
				{
					List<LogicalDiskInfo> dr = DirectoryHelper.GetDrives();
					foreach (LogicalDiskInfo item in dr)
						HandleDeviceAdd(item);
				}
				catch (Exception err)
				{
					LogHelper.Manage("DriveExplorerViewModel.DriveExplorerViewModel start wmi get drives", err);
				}
            });
		}

		/// <summary>
		/// Child classes can override this method to perform 
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();

			Messenger.Default.Unregister<LogicalDiskInfo>(this, ViewModelMessages.DeviceAdded, HandleDeviceAdd);
			Messenger.Default.Unregister<LogicalDiskInfo>(this, ViewModelMessages.DeviceRemoved, HandleDeviceRemove);

            CultureManager.Instance.UICultureChanged -= new CultureEventArrived(Instance_UICultureChanged);
		}

		#endregion

        #region -----------------PROPERTIES-----------------

        new public List<LogicalDiskInfo> Data
        {
            get { return base.Data as List<LogicalDiskInfo>; }
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

        #region -----------------HANDLERS-----------------

        private void Instance_UICultureChanged(object sender, CultureEventArgs e)
        {
            DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DriveExplorerView.Title", "Drive Explorer");
        }

        internal void HandleDeviceAdd(LogicalDiskInfo disk)
        {
            if (!Data.Exists(p => p.Name == disk.Path))
            {
                Data.Add(disk);

                SysDriveViewModel sysDrive = new SysDriveViewModel(disk.Path);
                sysDrive.Name = string.Format("{0} - {1}", disk.Name, disk.Model);
                _Drives.Add(sysDrive);

                RaisePropertyChanged("Drives");
            }
        }

        internal void HandleDeviceRemove(LogicalDiskInfo disk)
        {
            Data.Remove(disk);
            _Drives.Remove(_Drives.Where(p => p.FullPath == disk.Path).First());
            RaisePropertyChanged("Drives");
        }
		
        #endregion
    }
}
