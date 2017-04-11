using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Services;
using System.Collections.ObjectModel;
using CBR.Core.Models;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;

namespace CBR.ViewModels
{
	public class FeedConfigViewModel : ViewModelBaseExtended
	{
		#region ----------------CONSTRUCTOR----------------

		public FeedConfigViewModel(FeedInfo info)
		{
			Data = info;
			_feeds = new ObservableCollection<FeedItemInfo>(info.Feeds);
		}

		#endregion

		#region ----------------PROPERTIES----------------

		public int CacheDuration
		{
			get { return Data.CacheDuration; }
			set { Data.CacheDuration = value; }
		}

		public bool AutomaticOpen
		{
			get { return Data.AutomaticOpen; }
			set { Data.AutomaticOpen = value; }
		}

		public string DownloadFolder
		{
			get { return Data.DownloadFolder; }
			set { Data.DownloadFolder = value; }
		}

		public bool AskForDownloadFolder
		{
			get { return string.IsNullOrEmpty(Data.DownloadFolder); }
		}

		public bool AllDownloadInFolder
		{
			get { return !string.IsNullOrEmpty(Data.DownloadFolder); }
		}

		public bool UpdateCatalog
		{
			get { return Data.UpdateCatalog; }
			set { Data.UpdateCatalog = value; }
		}

		new public FeedInfo Data
		{
			get;
			set;
		}

		private ObservableCollection<FeedItemInfo> _feeds = null;
		public ObservableCollection<FeedItemInfo> Feeds
		{
			get { return _feeds; }
			set { _feeds = value; }
		}

		public ICollectionView FeedsView
		{
			get
			{
				if (_feeds != null)
				{
					return CollectionViewSource.GetDefaultView(_feeds);
				}
				else
					return null;
			}
		}

		private ICollectionView _Cultures = null;
		/// <summary>
		/// available culture items collection
		/// </summary>
		public ICollectionView Cultures
		{
			get
			{
				if (_Cultures == null)
				{
					List<string> list = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(p => p.IetfLanguageTag).Select(a => a.IetfLanguageTag).ToList();

					_Cultures = CollectionViewSource.GetDefaultView(list);
				}

				return _Cultures;
			}
		}

		private string _searchedText = string.Empty;
		public string SearchedText
		{
			get { return _searchedText; }
			set
			{
				_searchedText = value;

				FeedsView.Filter = delegate(object obj)
				{
					if (string.IsNullOrEmpty(_searchedText))
						return true;

					FeedItemInfo data = obj as FeedItemInfo;
					if (data == null)
						return false;

					return (
						(data.Name.IndexOf(_searchedText, 0, StringComparison.InvariantCultureIgnoreCase) > -1) ||
						data.Url.IndexOf(_searchedText, 0, StringComparison.InvariantCultureIgnoreCase) > -1
						);
				};
			}
		}

		#endregion
	}
}
