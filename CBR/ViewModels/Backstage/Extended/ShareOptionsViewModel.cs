using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Services;

namespace CBR.ViewModels
{
	public class ShareOptionsViewModel : ViewModelBaseExtended
    {
        #region ----------------PROPERTIES----------------

        public string ShareAdress
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.ShareAdress;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.ShareAdress = value;
            }
        }

        public int SharePort
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.SharePort;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.SharePort = value;
            }
        }

        public bool ShareOnStartup
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.ShareOnStartup;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.ShareOnStartup = value;
            }
        }

        #endregion
    }
}
