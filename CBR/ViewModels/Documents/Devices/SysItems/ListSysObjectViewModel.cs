 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
    #region ----------------BASE CLASS----------------

	public class ListSysObjectViewModel : ViewModelBaseExtended
    {
        public ListSysObjectViewModel(string fullPath, string name, DateTime access)
        {
            FullPath = fullPath;
            Name = name;
            LastModified = access;
        }

        /// <summary>
        /// Display name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Display type
        /// </summary>
        public SysElementType Type
        {
            get;
            set;
        }

        /// <summary>
        /// last modification date
        /// </summary>
        public DateTime LastModified
        {
            get;
            set;
        }

        /// <summary>
        /// Complete path to file sys object
        /// </summary>
        public string FullPath
        {
            get { return Data as string; }
            set { Data = value; }
        }

        #region ----------------IsSelected----------------

        private bool _isSelected;

        /// <summary>
        /// Gets/sets whether the Item 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.RaisePropertyChanged("IsSelected");

                    Messenger.Default.Send<string>(ViewModelMessages.DeviceContentChanged, this.FullPath);
                }
            }
        }

        #endregion

        #region ----------------IsHighlighted----------------

        private bool _isHighlighted;

        /// <summary>
        /// Gets/sets whether the Item 
        /// associated with this object is selected.
        /// </summary>
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (value != _isHighlighted)
                {
                    _isHighlighted = value;
                    this.RaisePropertyChanged("IsHighlighted");
                }
            }
        }

        #endregion
    }
    #endregion

    #region ----------------FILE----------------

    public class ListSysFileViewModel : ListSysObjectViewModel
    {
        public ListSysFileViewModel(string fullPath, string name, DateTime access, long size)
            : base(fullPath, name, access)
        {
            Type = SysElementType.File;
            Size = size;
        }

        /// <summary>
        /// file size
        /// </summary>
        public long Size
        {
            get;
            set;
        }
    }

    #endregion

    #region ----------------FOLDER----------------

    public class ListSysDirectoryViewModel : ListSysObjectViewModel
    {
        public ListSysDirectoryViewModel(string fullPath, string name, DateTime access)
            : base(fullPath, name, access)
        {
            Type = SysElementType.Folder;
        }
    }
    #endregion

    #region ----------------DRIVE----------------

    public class ListSysDriveViewModel : ListSysObjectViewModel
    {
        public ListSysDriveViewModel(string fullPath)
            : base(fullPath, fullPath, DateTime.Now)
        {
            Type = SysElementType.Drive;
        }
    }

    #endregion
}
