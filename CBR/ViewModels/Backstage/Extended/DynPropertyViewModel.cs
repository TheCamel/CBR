using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Collections.ObjectModel;
using CBR.Core.Services;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using CBR.Core.Models;

namespace CBR.ViewModels
{
	public class DynPropertyViewModel : ViewModelBaseExtended
    {

        public ObservableCollection<string> Dynamics
        {
            get { return new ObservableCollection<string>(WorkspaceService.Instance.Settings.Dynamics); }
        }

        #region add command
        private ICommand addPropertyCommand;
        public ICommand AddPropertyCommand
        {
            get
            {
                if (addPropertyCommand == null)
                    addPropertyCommand = new RelayCommand<string>(AddProperty, delegate(string param) { return !string.IsNullOrEmpty(param); });
                return addPropertyCommand;
            }
        }

        void AddProperty(string param)
        {
            try
            {
                if (WorkspaceService.Instance.Settings.Dynamics.Contains(param as string))
                    return;
                else
                {
                    WorkspaceService.Instance.Settings.Dynamics.Add(param as string);

                    Properties.Settings.Default.CatalogSetting = WorkspaceService.Instance.Settings;
                    Properties.Settings.Default.Save();
                }

                RaisePropertyChanged("Dynamics");
                Messenger.Default.Send<WorkspaceInfo>(WorkspaceService.Instance.Settings, ViewModelMessages.SettingsChanged);
            }
            catch (Exception err)
            {
                LogHelper.Manage("OptionsViewModel:AddProperty", err);
            }
        }
        #endregion

        #region delete command
        private ICommand deletePropertyCommand;
        public ICommand DeletePropertyCommand
        {
            get
            {
                if (deletePropertyCommand == null)
                    deletePropertyCommand = new RelayCommand<string>(DeleteProperty, delegate(string param) { return !string.IsNullOrEmpty(param); });
                return deletePropertyCommand;
            }
        }

        void DeleteProperty(string param)
        {
            try
            {
                if (WorkspaceService.Instance.Settings.Dynamics.Contains(param as string))
                {
                    WorkspaceService.Instance.Settings.Dynamics.Remove(param as string);

                    Properties.Settings.Default.CatalogSetting = WorkspaceService.Instance.Settings;
                    Properties.Settings.Default.Save();
                }

                RaisePropertyChanged("Dynamics");
				Messenger.Default.Send<WorkspaceInfo>(WorkspaceService.Instance.Settings, ViewModelMessages.SettingsChanged);
            }
            catch (Exception err)
            {
                LogHelper.Manage("OptionsViewModel:DeleteProperty", err);
            }
        }
        #endregion
    }
}
