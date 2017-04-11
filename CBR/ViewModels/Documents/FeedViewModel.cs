using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using CBR.Components;
using CBR.Core.Files;
using CBR.Core.Formats.OPDS;
using CBR.Core.Helpers;
using CBR.Core.Helpers.NET.Properties;
using CBR.Core.Models;
using CBR.Core.Services;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;

namespace CBR.ViewModels
{
	public class FeedViewModel : DocumentViewModel
	{
		#region ----------------CONSTRUCTOR----------------

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="inf"></param>
		public FeedViewModel(FeedItemInfo inf)
		{
            this.ContentId = "FeedViewModel;"+inf.Url;
			this.Icon = "pack://application:,,,/Resources/Images/32x32/rss/rss.png";

			DisplayName = inf.Name;
			Data = inf;

			CurrentUrl = new Uri(inf.Url);

			Messenger.Default.Register<PropertyModel>(this, ViewModelMessages.RssSortChanged,
				(PropertyModel o) =>
				{
					Sort(o);
				});
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
            base.Cleanup();
			Messenger.Default.Unregister(this);
			_HistoryView.CurrentChanged -= new EventHandler(_HistoryView_CurrentChanged);
		}

		#endregion

		#region ----------------PROPERTIES----------------

		/// <summary>
		/// internal parser
		/// </summary>
		private OpdsManager opdsManager = new OpdsManager();

		/// <summary>
		/// FeedItemInfo store name and feed url
		/// </summary>
		new public FeedItemInfo Data
		{
			get { return base.Data as FeedItemInfo; }
			set { base.Data = value; }
		}

		private bool _IsLoading = false;
		/// <summary>
		/// Gets or sets whether the view is loading.
		/// </summary>       
		public bool IsLoading
		{
			get { return _IsLoading; }
			set
			{
				if (_IsLoading != value)
				{
					_IsLoading = value;
					RaisePropertyChanged("IsLoading");
				}
			}
		}

		private double _scale = 1;
		/// <summary>
		/// Scale factor
		/// </summary>
		public double Scale
		{
			get { return _scale; }
			set
			{
				if (_scale != value)
				{
					_scale = value;
					RaisePropertyChanged("Scale");
				}
			}
		}

		private Uri _CurrentUrl;
		/// <summary>
		/// Current uri in the browser
		/// </summary>
		public Uri CurrentUrl
		{
			get { return _CurrentUrl; }
			set
			{
				if (_CurrentUrl != value)
				{
					_CurrentUrl = value;
					if( _CurrentUrl != null )
						GetFeed();
				}
			}
		}
		
		/// <summary>
		/// Shortcut to know if can navigate, for ribbon
		/// </summary>
		public bool HasPaging
		{
			get { return Content != null && (Content.NextUrl != null || Content.PreviousUrl != null); }
		}

		#region feed data

		private OpdsFeed _Content;
		public OpdsFeed Content
		{
			get { return _Content; }
			set
			{
				if (_Content != value)
				{
					_Content = value;
					RaisePropertyChanged("Content");

					_FeedItems = new ObservableCollection<OpdsItemBase>(_Content.Items);
					RaisePropertyChanged("FeedItemView");

					RaisePropertyChanged("HasPaging");
					RaisePropertyChanged("RssPreviousCommand");
					RaisePropertyChanged("RssNextCommand");

					if (HasPaging)
					{
						RaisePropertyChanged("SortBy");
						RaisePropertyChanged("Categories");
					}
				}
			}
		}

		#endregion

		#region feed items

		private ObservableCollection<OpdsItemBase> _FeedItems = new ObservableCollection<OpdsItemBase>();

		public ICollectionView FeedItemView
		{
			get
			{
				return CollectionViewSource.GetDefaultView(_FeedItems);
			}
		}

		#endregion

		#region history

		private ObservableCollection<Uri> _History = new ObservableCollection<Uri>();

		private ICollectionView _HistoryView = null;
		public ICollectionView HistoryView
		{
			get
			{
				if (_HistoryView == null)
				{
					_HistoryView = CollectionViewSource.GetDefaultView(_History);
					_HistoryView.CurrentChanged += new EventHandler(_HistoryView_CurrentChanged);
				}
				return _HistoryView;
			}
		}

		void _HistoryView_CurrentChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged("HistoryView");
			RaisePropertyChanged("RssPreviousCommand");
			RaisePropertyChanged("RssNextCommand");
		}
		#endregion

		#region filters

		public List<PropertyViewModel> SortBy
		{
			get
			{
				if (HasPaging)
					return new PropertyHelper().GetSortViewModels(typeof(OpdsItem));
				else
					return null;
			}
		}

		public List<string> Categories
		{
			get
			{
				if (HasPaging)
					return new List<string>((_Content.Items.Cast<OpdsItem>()).SelectMany(p => p.Categories).Distinct());
				else
					return null;
			}
		}

		private string _filterText = string.Empty;
		public string FilterText
		{
			get { return _filterText; }
			set
			{
				_filterText = value;

				FeedItemView.Filter = delegate(object obj)
				{
					if (string.IsNullOrEmpty(_filterText))
						return true;

					OpdsItemBase data = obj as OpdsItemBase;
					if (data == null)
						return false;

					return (data.Title.IndexOf(_filterText, 0, StringComparison.InvariantCultureIgnoreCase) > -1);
				};
			}
		}
		#endregion

		#endregion

		#region ---------------- COMMANDS ----------------

		#region home command
		private ICommand rssHomeCommand;
		public ICommand RssHomeCommand
		{
			get
			{
				if (rssHomeCommand == null)
					rssHomeCommand = new RelayCommand<string>(
						delegate(string param) { _History.Clear();  CurrentUrl = new Uri(Data.Url); },
						delegate(string param) { return Data.Url != null; });
				return rssHomeCommand;
			}
		}

		#endregion

		#region close command
		private ICommand rssCloseCommand;
		public ICommand RssCloseCommand
		{
			get
			{
				if (rssCloseCommand == null)
					rssCloseCommand = new RelayCommand(
						delegate() { this.CloseCommand.Execute(null); }, 
						delegate() { return true;} );
				return rssCloseCommand;
			}
		}

		#endregion

		#region refresh command
		private ICommand rssRefreshCommand;
		public ICommand RssRefreshCommand
		{
			get
			{
				if (rssRefreshCommand == null)
					rssRefreshCommand = new RelayCommand(RefreshCommandExecute,
						delegate() { return CurrentUrl != null; } );
				return rssRefreshCommand;
			}
		}

		public void RefreshCommandExecute()
		{
			string saved = _CurrentUrl.OriginalString;
			_CurrentUrl = new Uri(saved);
			GetFeed(false, false);
		}
		#endregion

		#region previous command
		private ICommand rssPreviousCommand;
		public ICommand RssPreviousCommand
		{
			get
			{
				if (rssPreviousCommand == null)
					rssPreviousCommand = new RelayCommand(PreviousCommandExecute, 
						delegate() { return HistoryView.CurrentPosition > 0; });
				return rssPreviousCommand;
			}
		}

		public void PreviousCommandExecute()
		{
			HistoryView.MoveCurrentToPrevious();
			_CurrentUrl = HistoryView.CurrentItem as Uri;
			GetFeed( true, false );
		}
		#endregion

		#region next command
		private ICommand rssNextCommand;
		public ICommand RssNextCommand
		{
			get
			{
				if (rssNextCommand == null)
					rssNextCommand = new RelayCommand(NextCommandExecute,
						delegate() { return HistoryView.CurrentPosition < _History.Count - 1; });
				return rssNextCommand;
			}
		}

		public void NextCommandExecute()
		{
			HistoryView.MoveCurrentToNext();
			_CurrentUrl = HistoryView.CurrentItem as Uri;
			GetFeed(true, false);
		}
		#endregion

		#region search command
		private ICommand rssSearchCommand;
		public ICommand RssSearchCommand
		{
			get
			{
				if (rssSearchCommand == null)
					rssSearchCommand = new RelayCommand<string>(SearchCommandExecute, SearchCommandCanExecute);
				return rssSearchCommand;
			}
		}

		public bool SearchCommandCanExecute(string param)
		{
			return Data != null;
		}

		public void SearchCommandExecute(string param)
		{
		}
		#endregion

		#region navigate command
		private ICommand navigateCommand;
		public ICommand NavigateCommand
		{
			get
			{
				if (navigateCommand == null)
					navigateCommand = new RelayCommand<Uri>(
						delegate(Uri param) { CurrentUrl = param; },
						delegate(Uri param) { return param != null; });
				return navigateCommand;
			}
		}

		#endregion

		#region link command
		private ICommand linkCommand;
		public ICommand LinkCommand
		{
			get
			{
				if (linkCommand == null)
					linkCommand = new RelayCommand<Uri>(
						delegate(Uri param) { ProcessHelper.LaunchShellUri(param); },
						delegate(Uri param) { return param != null;  });
				return linkCommand;
			}
		}

		#endregion

		#region download command
		private ICommand downloadCommand;
		public ICommand DownloadCommand
		{
			get
			{
				if (downloadCommand == null)
					downloadCommand = new RelayCommand<OpdsDownload>(
						delegate(OpdsDownload param) { Download(param); },
						delegate(OpdsDownload param) { return param != null; });
				return downloadCommand;
			}
		}

		#endregion

		#region change page command
		private ICommand rssChangePageCommand;
		public ICommand RssChangePageCommand
		{
			get
			{
				if (rssChangePageCommand == null)
					rssChangePageCommand = new RelayCommand<Uri>(
						delegate(Uri param) { CurrentUrl = param; },
						delegate(Uri param) { return param != null; });
				return rssChangePageCommand;
			}
		}

		#endregion

		#endregion

		#region --------------------METHODS--------------------

		/// <summary>
		/// Is call by the view in click case, only moment we reset history
		/// </summary>
		public void ResetHistory()
		{
			//find position
			int pos = _History.IndexOf( HistoryView.CurrentItem as Uri );
			
			//remove what is after
			for (int i = pos + 1; i < _History.Count; i++)
				_History.RemoveAt(i);
		}

		#endregion

		#region --------------------INTERNALS--------------------

		#region download

		/// <summary>
		/// start async downloading
		/// </summary>
		/// <param name="file"></param>
		private void Download(OpdsDownload file)
		{
			try
			{
				IsLoading = true;

				WebClient client = CreateWebClient();
				string filepath = string.Empty;

				client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
				if (!string.IsNullOrEmpty(WorkspaceService.Instance.Settings.Feed.DownloadFolder))
				{
					DocumentInfo fe = DocumentFactory.Instance.BookFilters.First(p => p.Type == file.Type);
					filepath = Path.Combine(WorkspaceService.Instance.Settings.Feed.DownloadFolder, file.Title + fe.Extension);
				}
				else
				{
					using (System.Windows.Forms.SaveFileDialog browser = new System.Windows.Forms.SaveFileDialog())
					{
						DocumentInfo fe = DocumentFactory.Instance.BookFilters.First(p => p.Type == file.Type);
						browser.AddExtension = true;
						browser.Filter = fe.DialogFilter;
						browser.DefaultExt = fe.DialogFilter;
						browser.FileName = file.Title;

						if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
						{
							filepath = browser.FileName;
						}
					}
				}

				client.DownloadFileAsync(file.Link, filepath, filepath );
			}
			catch (Exception err)
			{
				LogHelper.Manage("FeedViewModel:Download", err);
			}
		}

		void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			try
			{
				if (e.Error == null)
				{
					try
					{
						string fileName = e.UserState as string;
						if (WorkspaceService.Instance.Settings.Feed.UpdateCatalog)
						{
							Messenger.Default.Send<CommandContext>(
								new CommandContext() { CommandName = "AddBookFileCommand", CommandParameter = fileName },
								ViewModelBaseMessages.ContextCommand);
						}
						if (WorkspaceService.Instance.Settings.Feed.AutomaticOpen)
						{
							Messenger.Default.Send<CommandContext>(
								new CommandContext() { CommandName = "BookOpenFileCommand", CommandParameter = fileName },
								ViewModelBaseMessages.ContextCommand);
						}
					}
					catch (XmlException err)
					{
						LogHelper.Manage("FeedViewModel:client_DownloadFileCompleted", err);
					}
				}
				else if (e.Error != null)
				{
					LogHelper.Manage("FeedViewModel:client_DownloadFileCompleted", e.Error);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("FeedViewModel:client_DownloadFileCompleted", err);
			}
			finally
			{
				IsLoading = false;
			}
		}

		#endregion

		#region get feed

		internal class AsyncFeedParam
		{
			public bool withHisto { get; set; }
			public Uri currentUri { get; set; }
			public WebClient clientWeb { get; set; }
		}

		/// <summary>
		/// Gets a feed from a Uri.
		/// </summary>
		private void GetFeed(bool withCache = true, bool withHistory = true)
		{
			try
			{
				IsLoading = true;

				if (withCache && opdsManager.CanFromCache(_CurrentUrl))
				{
					OpdsFeed temp = null;

					Task.Factory.StartNew(() =>
					{
						temp = opdsManager.ConvertFeed(_CurrentUrl);
					}).ContinueWith(ant =>
					{
						Content = temp;
						IsLoading = false;

						if (withHistory)
						{
							_History.Add(_CurrentUrl);
							HistoryView.MoveCurrentTo(_CurrentUrl);
						}

						//updates UI no problem as we are using correct SynchronizationContext
					}, TaskScheduler.FromCurrentSynchronizationContext());
				}
				else
				{
					WebClient client = CreateWebClient();

					client.OpenReadCompleted += new OpenReadCompletedEventHandler(Client_OpenReadCompleted);
					client.OpenReadAsync(_CurrentUrl, new AsyncFeedParam()
						{
							currentUri = _CurrentUrl,
							withHisto = withHistory,
							clientWeb = client
						} );
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("FeedViewModel.GetFeed", err);
			}
		}

		/// <summary>
		/// Gets the response, and parses as a feed.
		/// </summary>
		/// <param name="sender">The web client.</param>
		/// <param name="e">Open Read Completed Event Args</param>
		private void Client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			try
			{
				if (e.Error == null && e.Result != null)
				{
					try
					{
						AsyncFeedParam par = e.UserState as AsyncFeedParam;
						par.clientWeb.OpenReadCompleted -= new OpenReadCompletedEventHandler(Client_OpenReadCompleted);
						Content = opdsManager.ConvertFeed(e.Result, par.currentUri);
						
						if (par.withHisto)
						{
							_History.Add(_CurrentUrl);
							HistoryView.MoveCurrentTo(_CurrentUrl);
						}
					}
					catch (XmlException err)
					{
						LogHelper.Manage("FeedViewModel:Client_OpenReadCompleted", err);
					}
				}
				else if (e.Error != null)
				{
					LogHelper.Manage("FeedViewModel:Client_OpenReadCompleted", e.Error);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("FeedViewModel:Client_OpenReadCompleted", err);
			}
			finally
			{
				IsLoading = false;
			}
		}
		#endregion

		/// <summary>
		/// Create a web client with proxy settings if needed
		/// </summary>
		/// <returns></returns>
		private WebClient CreateWebClient()
		{
			WebClient client = new WebClient();
			if (!string.IsNullOrEmpty(WorkspaceService.Instance.Settings.Extended.Proxy.Address))
			{
				client.Proxy = new WebProxy(WorkspaceService.Instance.Settings.Extended.Proxy.Address,
												WorkspaceService.Instance.Settings.Extended.Proxy.Port);
				client.Proxy.Credentials = new NetworkCredential(WorkspaceService.Instance.Settings.Extended.Proxy.UserName,
												WorkspaceService.Instance.Settings.Extended.Proxy.Password,
												WorkspaceService.Instance.Settings.Extended.Proxy.Domain);
			}
			return client;
		}


		private void Sort(PropertyModel model)
		{
			IEnumerable<SortDescription> result =
				FeedItemView.SortDescriptions.Cast<SortDescription>().Where(p => p.PropertyName == model.Name);

			if (result != null && result.Count() == 1)
			{
				FeedItemView.SortDescriptions.Remove(result.First());
			}
			else
			{
				FeedItemView.SortDescriptions.Add(new SortDescription(model.Name, ListSortDirection.Ascending));
			}

			RaisePropertyChanged("FeedItemView");
		}

		#endregion
	}

}