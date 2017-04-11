using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.AccessControl;

namespace CBR.ViewModels
{
    #region ----------------BASE CLASS----------------

    public enum SysElementType
    {
        Drive,
        Folder,
        File
    }

    public class SysElementViewModel : TreeViewItemViewModel
    {
        public SysElementViewModel(TreeViewItemViewModel parent, string fullPath, string name, DateTime access)
            : base(parent, true)
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

        public List<ListSysObjectViewModel> GetListContent()
        {
            List<ListSysObjectViewModel> result = new List<ListSysObjectViewModel>();
            
            foreach (string directory in Directory.GetDirectories(FullPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (DirectoryHelper.CheckAccess(directory))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    result.Add(new ListSysDirectoryViewModel(directory, directoryInfo.Name, directoryInfo.LastWriteTime));
                }
            }
            foreach (string file in Directory.GetFiles(FullPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                result.Add(new ListSysFileViewModel(file, fileInfo.Name, fileInfo.LastWriteTime, fileInfo.Length));
            }
            return result;
        }
    }
    #endregion

    #region ----------------FILE----------------

    public class SysFileViewModel : SysElementViewModel
    {
        public SysFileViewModel(TreeViewItemViewModel parent, string fullPath, string name, DateTime access, long size)
            : base(parent, fullPath, name, access)
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

    public class SysDirectoryViewModel : SysElementViewModel
    {
        public SysDirectoryViewModel(TreeViewItemViewModel parent, string fullPath, string name, DateTime access)
            : base(parent, fullPath, name, access)
        {
            Type = SysElementType.Folder;
        }

        protected override void LoadChildren()
        {
            foreach (string directory in Directory.GetDirectories(FullPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (DirectoryHelper.CheckAccess(directory))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    Children.Add(new SysDirectoryViewModel(this, directory, directoryInfo.Name, directoryInfo.LastWriteTime));
                }
            }
            //foreach (string file in Directory.GetFiles(FullPath))
            //{
            //    FileInfo fileInfo = new FileInfo(file);
            //    Children.Add(new SysFileViewModel(this, file, fileInfo.Name, fileInfo.LastWriteTime, fileInfo.Length));
            //}
        }
    }
    #endregion

    #region ----------------DRIVE----------------

    public class SysDriveViewModel : SysElementViewModel
    {
        public SysDriveViewModel(string fullPath)
            : base(null, fullPath, fullPath, DateTime.Now)
        {
            Type = SysElementType.Drive;
        }

        protected override void LoadChildren()
        {
            foreach (string directory in Directory.GetDirectories(FullPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (DirectoryHelper.CheckAccess(directory))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    Children.Add(new SysDirectoryViewModel(this, directory, directoryInfo.Name, directoryInfo.LastWriteTime));
                }
            }
            //foreach (string file in Directory.GetFiles(FullPath))
            //{
            //    FileInfo fileInfo = new FileInfo(file);
            //    Children.Add(new SysFileViewModel(this, file, fileInfo.Name, fileInfo.LastWriteTime, fileInfo.Length));
            //}
        }
    }
    
    #endregion
}