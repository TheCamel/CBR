using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Services;

namespace CBR.ViewModels
{
	internal class BehaveOptionsViewModel : ViewModelBaseExtended
	{
		#region ----------------PROPERTIES----------------

        public bool FullScreenOptimized
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.ShowFullScreenOptimized;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.ShowFullScreenOptimized = value;
            }
        }

        public bool ShowZoomContent
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.ShowZoomFlyer;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.ShowZoomFlyer = value;
            }
        }

        public bool ShowTooltipExplorer
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.ShowTooltipExplorer;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.ShowTooltipExplorer = value;
            }
        }

        public bool ShowGridCover
        {
            get
            {
                return WorkspaceService.Instance.Settings.Extended.ShowGridCover;
            }
            set
            {
                WorkspaceService.Instance.Settings.Extended.ShowGridCover = value;
            }
        }

        public bool IsViewThumbSimple
        {
            get { return WorkspaceService.Instance.Settings.Extended.DefaultExplorerView.Equals("ExplorerImageView"); }
            set
            {
                WorkspaceService.Instance.Settings.Extended.DefaultExplorerView = "ExplorerImageView";
                RaisePropertyChanged("IsViewThumbDetails");
                RaisePropertyChanged("IsViewGrid");
            }
        }
        public bool IsViewThumbDetails
        {
            get { return WorkspaceService.Instance.Settings.Extended.DefaultExplorerView.Equals("ExplorerImageDetailView"); }
            set
            {
                WorkspaceService.Instance.Settings.Extended.DefaultExplorerView = "ExplorerImageDetailView";
                RaisePropertyChanged("IsViewThumbSimple");
                RaisePropertyChanged("IsViewGrid");
            }
        }
        public bool IsViewGrid
        {
            get { return WorkspaceService.Instance.Settings.Extended.DefaultExplorerView.Equals("ExplorerGridView"); }
            set
            {
                WorkspaceService.Instance.Settings.Extended.DefaultExplorerView = "ExplorerGridView";

                RaisePropertyChanged("IsViewThumbSimple");
                RaisePropertyChanged("IsViewThumbDetails");
            }
        }

		public bool BehavePageTempo
		{
			get
			{
				return WorkspaceService.Instance.Settings.Extended.BehavePageTempo;
			}
			set
			{
				WorkspaceService.Instance.Settings.Extended.BehavePageTempo = value;
			}
		}

		#endregion
	}
}
