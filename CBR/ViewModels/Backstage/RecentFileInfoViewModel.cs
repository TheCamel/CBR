using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Models;
using CBR.Core.Helpers;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	class RecentFileInfoViewModel : ViewModelBaseExtended
    {
        #region ----------------CONSTRUCTOR----------------

        public RecentFileInfoViewModel(RecentFileInfo rfi)
		{
            if (rfi != null)
			{
                Data = rfi;
			}
		}

        new public RecentFileInfo Data
        {
            get { return base.Data as RecentFileInfo; }
            set { base.Data = value; }
        }

		#endregion

        public string FilePath
        {
            get { return Data.FilePath; }
            set { Data.FilePath = value; RaisePropertyChanged("FilePath"); }
        }

        public string FileName
        {
            get { return Data.FileName; }
            set { Data.FileName = value; RaisePropertyChanged("FileName"); }
        }

        public bool IsPined
        {
            get { return Data.IsPined; }
            set
            {
                Data.IsPined = value; RaisePropertyChanged("IsPined");
				Messenger.Default.Send<RecentFileInfo>(Data, ViewModelMessages.RecentFileChanged);
            }
        }

        public DateTime LastAccess
        {
            get { return Data.LastAccess; }
            set
            {
                Data.LastAccess = value; RaisePropertyChanged("LastAcess");
				Messenger.Default.Send<RecentFileInfo>(Data, ViewModelMessages.RecentFileChanged);
            }
        }
    }
}
