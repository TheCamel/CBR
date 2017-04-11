using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;

namespace CBR.ViewModels
{
    /// <summary>
    /// Class used to bind Expando proterties to the Info view
    /// </summary>
	public class KeyValueProperty : ViewModelBaseExtended
    {
        public KeyValueProperty(string key, string value)
        {
            Key = key; Value = value;
        }

        public bool IsDirty { get; set; }

        private string _key = string.Empty;
        public string Key
        {
            get { return _key; }
            set { _key = value; IsDirty = true; RaisePropertyChanged("Key"); }
        }

        private string _value = string.Empty;
        public string Value
        {
            get { return _value; }
            set { _value = value; IsDirty = true; RaisePropertyChanged("Value"); }
        }
    }
}
