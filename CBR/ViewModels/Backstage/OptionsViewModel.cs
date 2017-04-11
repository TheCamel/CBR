using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Models;
using CBR.Core.Services;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	class OptionsViewModel : ViewModelBaseExtended
	{
		#region ----------------CONSTRUCTOR----------------

		public OptionsViewModel()
		{
			//register to the mediator for messages
			Messenger.Default.Register<CultureInfo>(this, ViewModelMessages.LanguagesChanged,
				(CultureInfo o) =>
                {
                    Languages = null;
                } );
		}

		override public void Cleanup()
		{
			base.Cleanup();

			Messenger.Default.Unregister(this);
		}

		#endregion

		#region ----------------PROPERTIES----------------

        public int ImageCacheCount
		{
			get
			{
                return WorkspaceService.Instance.Settings.ImageCacheCount;
			}
			set
			{
                WorkspaceService.Instance.Settings.ImageCacheCount = value;
			}
		}

        public int ImageCacheDuration
        {
            get
            {
                return WorkspaceService.Instance.Settings.ImageCacheDuration;
            }
            set
            {
                WorkspaceService.Instance.Settings.ImageCacheDuration = value;
            }
        }

        public int AutoFitMode
        {
            get
            {
                return WorkspaceService.Instance.Settings.AutoFitMode;
            }
            set
            {
                if( value != -1 )
                    WorkspaceService.Instance.Settings.AutoFitMode = value;
            }
        }

        public double MagnifierScaleFactor
        {
            get
            {
                return WorkspaceService.Instance.Settings.MagnifierScaleFactor;
            }
            set
            {
                WorkspaceService.Instance.Settings.MagnifierScaleFactor = value;
            }
        }

        public double MagnifierSize
        {
            get
            {
                return WorkspaceService.Instance.Settings.MagnifierSize;
            }
            set
            {
                WorkspaceService.Instance.Settings.MagnifierSize = value;
            }
        }

        public int MaxRecentFile
        {
            get
            {
                return WorkspaceService.Instance.Settings.MaxRecentFile;
            }
            set
            {
                WorkspaceService.Instance.Settings.MaxRecentFile = value;
            }
        }

        public LanguageMenuItemViewModel StartingLanguage
        {
            get
            {
				return Languages.First(p => p.Data.IetfLanguageTag == WorkspaceService.Instance.Settings.StartingLanguageCode);
            }
            set
            {
				if( value != null )
					WorkspaceService.Instance.Settings.StartingLanguageCode = (value as LanguageMenuItemViewModel).Data.IetfLanguageTag;
            }
        }

		private ObservableCollection<LanguageMenuItemViewModel> _Languages;

        public ObservableCollection<LanguageMenuItemViewModel> Languages
        {
            get
            {
				if (_Languages == null)
				{
					_Languages = new ObservableCollection<LanguageMenuItemViewModel>();

					foreach (CultureInfo info in CultureManager.Instance.GetAvailableCultures())
						_Languages.Add(new LanguageMenuItemViewModel(info));
				}
				return _Languages;
            }
			set
			{
				if (_Languages != value)
				{
					_Languages = value;
					RaisePropertyChanged("Languages");
				}
			}
        }

		#endregion

        #region save command
        private ICommand saveSettingsCommand;
        public ICommand SaveSettingsCommand
        {
            get
            {
                if (saveSettingsCommand == null)
                    saveSettingsCommand = new RelayCommand(SaveSetting, delegate() { return true; });
                return saveSettingsCommand;
            }
        }

        void SaveSetting()
        {
            try
            {
                Properties.Settings.Default.CatalogSetting = WorkspaceService.Instance.Settings;
                Properties.Settings.Default.Save();

				Messenger.Default.Send<WorkspaceInfo>(WorkspaceService.Instance.Settings, ViewModelMessages.SettingsChanged);
            }
            catch (Exception err)
            {
                LogHelper.Manage("OptionsViewModel:SaveSetting", err);
            }
        }
        #endregion

        #region reset command
        private ICommand resetSettingsCommand;
        public ICommand ResetSettingsCommand
        {
            get
            {
                if (resetSettingsCommand == null)
                    resetSettingsCommand = new RelayCommand(ResetSetting, delegate() { return true; });
                return resetSettingsCommand;
            }
        }

        void ResetSetting()
        {
            try
            {
                WorkspaceService.Instance.Settings = new WorkspaceInfo();
                Properties.Settings.Default.CatalogSetting = WorkspaceService.Instance.Settings;
                Properties.Settings.Default.Save();

				Messenger.Default.Send<WorkspaceInfo>(WorkspaceService.Instance.Settings, ViewModelMessages.SettingsChanged);
            }
            catch (Exception err)
            {
                LogHelper.Manage("OptionsViewModel:ResetSetting", err);
            }
        }
        #endregion
	}
}
