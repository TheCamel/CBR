using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using CBR.Core.Files;
using CBR.Core.Helpers;
using CBR.Core.Models;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class InfoViewModel : ViewModelBaseExtended
    {
        #region ----------------CONSTRUCTOR----------------

        public InfoViewModel(Book bk)
		{
            _bookData = bk;

            //register to the mediator for messages
            Messenger.Default.Register<Book>(this, ViewModelMessages.BookSelected,
				(Book o) =>
                {
                    if (_bookData != o)
                    {
                        _bookData = o;

                        RaisePropertyChanged("IsEmpty");
                        RaisePropertyChanged("FileName");
                        RaisePropertyChanged("Folder");
                        RaisePropertyChanged("Cover");
                        RaisePropertyChanged("Type");
                        RaisePropertyChanged("Size");
                        RaisePropertyChanged("PageCount");
                        RaisePropertyChanged("IsRead");
                        RaisePropertyChanged("IsSecured");
                        RaisePropertyChanged("Rating");
                        RaisePropertyChanged("KeyValueList");
                    }
                } );
		}

		override public void Cleanup()
		{
			base.Cleanup();

			Messenger.Default.Unregister(this);
		}

        private Book _bookData = null;

		#endregion

        #region ----------------PROPERTIES----------------

        public bool IsEmpty
        {
            get
            {
                return _bookData == null;
            }
        }

        public string FileName
        {
            get
            {
                if (_bookData != null)
                    return _bookData.FileName;
                else
                    return string.Empty;
            }
        }

        public string Folder
        {
            get
            {
                if (_bookData != null)
                    return _bookData.Folder;
                else
                    return string.Empty;
            }
        }

        public BitmapImage Cover
        {
            get
            {
                if (_bookData != null)
                    return _bookData.Cover;
                else
                    return null;
            }
        }

        public DocumentType Type
        {
            get
            {
                if (_bookData != null)
                    return _bookData.Type;
                else
                    return DocumentType.None;
            }
        }

        public long Size
        {
            get
            {
                if (_bookData != null)
                    return _bookData.Size;
                else
                    return 0;
            }
        }

        public int PageCount
        {
            get
            {
                if (_bookData != null)
                    return _bookData.PageCount;
                else
                    return 0;
            }
        }

        public bool IsRead
        {
            get
            {
                if (_bookData != null)
                    return _bookData.IsRead;
                else
                    return false;
            }
        }

        public bool IsSecured
        {
            get
            {
                if (_bookData != null)
                    return _bookData.IsSecured;
                else
                    return false;
            }
        }

        public int Rating
        {
            get
            {
                if (_bookData != null)
                    return _bookData.Rating;
                else return 0;
            }
            set
            {
                if (_bookData.Rating != value)
                    _bookData.Rating = value;
            }
        }

        #endregion

        private List<KeyValueProperty> _KeyValueList = null;
        public List<KeyValueProperty> KeyValueList
        {
            get
            {
                if (_KeyValueList != null)
                {
                    foreach (KeyValueProperty kv in _KeyValueList)
                        kv.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_data_PropertyChanged);
                }

                if (_bookData != null)
                {
                    _KeyValueList = new List<KeyValueProperty>();
                    IDictionary<string, object> dict = _bookData.Dynamics as IDictionary<string, object>;
                    if (dict != null)
                    {
                        foreach (string k in dict.Keys)
                        {
                            KeyValueProperty keyval = new KeyValueProperty(k, dict[k].ToString());
                            keyval.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_data_PropertyChanged);
                            _KeyValueList.Add(keyval);
                        }
                    }

                    return _KeyValueList;
                }
                else
                    return null;
            }
        }

        internal void _data_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_bookData != null)
            {
                KeyValueProperty from = sender as KeyValueProperty;
                KeyValueProperty find = _KeyValueList.Where(p => p.Key == from.Key).First();
                
                IDictionary<string, object> dict = _bookData.Dynamics as IDictionary<string, object>;
                dict[from.Key] = find.Value;

                _bookData.IsDirty = true;
            }
        }
    }
}
