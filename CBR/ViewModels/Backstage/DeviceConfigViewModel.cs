using System.Collections.Generic;
using System.Collections.ObjectModel;
using CBR.Core.Files;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using System.ComponentModel;
using System.Windows.Data;
using System;

namespace CBR.ViewModels
{
	public class DeviceConfigViewModel : ViewModelBaseExtended
    {
        #region ----------------CONSTRUCTOR----------------

        public DeviceConfigViewModel(List<DeviceInfo> data)
		{
            Data = new ObservableCollection<DeviceInfo>(data);
		}

		#endregion

        new public ObservableCollection<DeviceInfo> Data
        {
            get;
            set;
        }


        public ICollectionView SupportedDevices
        {
            get
            {
                if (Data != null)
                {
                    return CollectionViewSource.GetDefaultView(Data);
                }
                else
                    return null;
            }
        }

        private string _searchedText = string.Empty;
        public string SearchedText
        {
            get { return _searchedText; }
            set
            {
                _searchedText = value;

                SupportedDevices.Filter = delegate(object obj)
                {
                    if (string.IsNullOrEmpty(_searchedText))
                        return true;

                    DeviceInfo data = obj as DeviceInfo;
                    if (data == null)
                        return false;

                    return (
                        (data.Manufacturer.IndexOf(_searchedText, 0, StringComparison.InvariantCultureIgnoreCase) > -1) ||
                        data.Model.IndexOf(_searchedText, 0, StringComparison.InvariantCultureIgnoreCase) > -1
                        );
                };
            }
        }
    }
}
