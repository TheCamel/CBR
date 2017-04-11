using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Windows.Input;
using System.Net;
using System.Xml;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using CBR.Core.Helpers.Localization;
using CBR.Core.Services;
using System.Threading.Tasks;

namespace CBR.ViewModels
{
	public class HomeViewModel : DocumentViewModel
	{
		#region ----------------CONSTRUCTOR----------------

		public HomeViewModel()
		{
			this.ContentId = "HomeViewModel";
			this.Icon = "pack://application:,,,/Resources/Images/32x32/icon/home.png";

			CultureManager.Instance.UICultureChanged += new CultureEventArrived(Instance_UICultureChanged);
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Home", "Home");

			LoadFeed();
		}

		#endregion

		#region ----------------PROPERTIES----------------

		private List<Headline> _Items = null;
		/// <summary>
		/// headline items collection
		/// </summary>
		public ICollectionView ItemsSource
		{
			get
			{
				if (_Items != null)
				{
					return CollectionViewSource.GetDefaultView(_Items);
				}
				else
					return null;
			}
		}

		private bool _IsLoading = false;
		/// <summary>
		/// Gets or sets whether the view is loading headlines.
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

		private bool _HasError = true;
		/// <summary>
		/// Gets or sets whether the view has got error.
		/// </summary>
		public bool HasError
		{
			get { return _HasError; }
			set
			{
				if (_HasError != value)
				{
					_HasError = value;
					RaisePropertyChanged("HasError");
				}
			}
		}

		/// <summary>
		/// footer application version
		/// </summary>
		public string ApplicationVersion
		{
			get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); }
		}

		#endregion

		#region ---------------- COMMANDS ----------------

		#region forward command

		protected override CommandContext GetForwardCommandContext(string param)
		{
			return new CommandContext()
			{
				CommandName = param,
				CommandParameter = null
			};
		}

		protected override bool CanForwardCommand(string param)
		{
			return true;
		}

		#endregion
		#endregion

		#region --------------------METHODS--------------------

		/// <summary>
		/// Load the feed.
		/// </summary>
		public void LoadFeed(bool reload = false)
		{
			if (IsLoading) return;

			IsLoading = true;

			if (_Items == null || reload)
			{
				Uri uri = null;
				string code = CultureManager.Instance.GetNearestCode();
				string file = string.Format("http://guillaume.waser.free.fr/hidden/headlines.{0}.xaml", code);

				if (!string.IsNullOrEmpty(file) && Uri.TryCreate(file, UriKind.Absolute, out uri))
				{
					this.GetFeed(uri);
				}
			}
			else IsLoading = false;
		}

		public void GetCacheHeader(HttpWebRequest client)
		{
			string file = DirectoryHelper.Combine(CBRFolders.Cache, "RssCache.xml");
			if (File.Exists(file))
			{
				client.IfModifiedSince = File.GetLastAccessTimeUtc(file);
			}
		}

		public HeadlineCollection GetCache()
		{
			string file = DirectoryHelper.Combine(CBRFolders.Cache, "RssCache.xml");
			if (File.Exists(file))
			{
				return (HeadlineCollection)XmlHelper.Deserialize(file, typeof(HeadlineCollection));
			}
			else return null;
		}

		/// <summary>
		/// Gets a feed from a Uri.
		/// </summary>
		/// <param name="uri">The Uri of the feed.</param>
		private void GetFeed(Uri uri)
		{
			Task.Factory.StartNew(() =>
				{
					GetFeedTask( uri );
				});
		}

		private void GetFeedTask(Uri uri)
		{
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
				GetCacheHeader(httpWebRequest);

				if (!string.IsNullOrEmpty(WorkspaceService.Instance.Settings.Extended.Proxy.Address))
				{
					httpWebRequest.Proxy = new WebProxy(WorkspaceService.Instance.Settings.Extended.Proxy.Address,
													WorkspaceService.Instance.Settings.Extended.Proxy.Port);
					httpWebRequest.Proxy.Credentials = new NetworkCredential(WorkspaceService.Instance.Settings.Extended.Proxy.UserName,
													WorkspaceService.Instance.Settings.Extended.Proxy.Password,
													WorkspaceService.Instance.Settings.Extended.Proxy.Domain);
				}

				WebResponse resp = null;
				_Items = new List<Headline>();
				try
				{
					resp = httpWebRequest.GetResponse();

					HeadlineCollection col = (HeadlineCollection)XmlHelper.Deserialize(resp.GetResponseStream(), typeof(HeadlineCollection));
					if (col != null)
					{
						_Items.AddRange(col.HeadlineItems);
						RaisePropertyChanged("ItemsSource");
						HasError = false;

						XmlHelper.Serialize(DirectoryHelper.Combine(CBRFolders.Cache, "RssCache.xml"), col);
					}
					else HasError = false;

				}
				catch (Exception err)
				{
					//any error or not modified, load cache
					HeadlineCollection col = GetCache();
					if (col != null)
					{
						_Items.AddRange(col.HeadlineItems);
						RaisePropertyChanged("ItemsSource");
						HasError = false;
					}
					else HasError = false;
				}

				IsLoading = false;
			}
			catch (Exception err)
			{
				LogHelper.Manage("HomeViewModel.GetFeedTask", err);
			}
		}

		#endregion

		#region -----------------HANDLERS-----------------

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			CultureManager.Instance.UICultureChanged -= new CultureEventArrived(Instance_UICultureChanged);
		}

		void Instance_UICultureChanged(object sender, CultureEventArgs e)
		{
			DisplayName = CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Home", "Home");
			LoadFeed(true);
		}

		override protected void OnRequestClose()
		{
			return;
		}

		#endregion
	}
}
