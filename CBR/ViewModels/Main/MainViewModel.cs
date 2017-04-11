using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CBR.Components;
using CBR.Core;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Models;
using CBR.Core.Services;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using CBR.ViewModels.Messages;

namespace CBR.ViewModels
{
	public partial class MainViewModel : ViewModelBaseExtended
	{
		#region ----------------CONSTRUCTOR----------------

		public MainViewModel(string param)
		{
            Messenger.Default.Register<DocumentViewModel>(this, ViewModelMessages.DocumentRequestClose,
                (DocumentViewModel o) =>
                {
                    this.Documents.Remove(o);
                    o.Cleanup();
                });

			//register to the mediator for messages
			Messenger.Default.Register<Book>(this, ViewModelMessages.BookSelected,
				(Book o) =>
				{
					CurrentExplorerItem = o;
				});

			Messenger.Default.Register<Catalog>(this, ViewModelMessages.CatalogChanged,
				(Catalog o) =>
                {
					if (Data != null && Data.Books != null)
					{
						Data.Books.CollectionChanged -= new NotifyCollectionChangedEventHandler(Books_CollectionChanged);
						RaisePropertyChanged("CatalogItemCount");
					}
					Data = o;
					if (Data != null && Data.Books != null)
					{
						Data.Books.CollectionChanged += new NotifyCollectionChangedEventHandler(Books_CollectionChanged);
						RaisePropertyChanged("CatalogItemCount");
					}
					
					RaisePropertyChanged("Title");
                } );

			Messenger.Default.Register<List<string>>(this, ViewModelMessages.CatalogUpdate,
                (List<string> o) =>
                {
                    foreach( string file in o )
                        AddBookFileCommand.Execute(file);
                } );

			Messenger.Default.Register<CommandContext>(this, ViewModelBaseMessages.ContextCommand,
				(CommandContext o) =>
                {
                    ExecuteDistantCommand( o );
                } );

			Messenger.Default.Register<BookViewModelBase>(this, ViewModelMessages.SwapTwoPageView,
				(BookViewModelBase o) =>
				{
					SwapTwoPageMode(o);
				});

			Messenger.Default.Register<CultureInfo>(this, ViewModelMessages.LanguagesChanged,
				(CultureInfo o) =>
				{
					RaisePropertyChanged("Languages");
				});

			Messenger.Default.Register<WorkspaceInfo>(this, ViewModelMessages.ExtendedSettingsChanged,
                (WorkspaceInfo o) =>
                {
                    if (IsSharing)
                    {
                        StopSharing();
                        StartSharing();
                    }
                });

            Task.Factory.StartNew(() =>
            {
				try
				{
					if (WorkspaceService.Instance.Settings.Extended.ShareOnStartup)
						StartSharing();
				}
				catch (Exception err)
				{
					LogHelper.Manage("MainViewModel.MainViewModel start sharing", err);
				}
            });

            BackStageIsOpen = false;

            if (!string.IsNullOrEmpty(param))
                ManageStartingDocument(param);

			NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChangeNetworkAddressChanged);
			HasNetwork = NetworkInterface.GetIsNetworkAvailable();
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			Messenger.Default.Unregister(this); 
            StopSharing();
		}

        public void ResetLayout()
        {
            Documents.Clear();
            _Tools.Clear();

            HomeViewModel hvm = new HomeViewModel();
            Documents.Add(hvm);
            
            _Tools.Add(new ExplorerViewModel());
            _Tools.Add(new TocViewModel());
            _Tools.Add(new DriveExplorerViewModel());
        }

		#endregion

		#region ----------------PROPERTIES----------------

        public string Title
        {
            get
            {
                // no catalog
                if (Data == null)
                    return "CBR";
                else
                    return "CBR : " + Data.CatalogFilePath;
            }
        }

        public ObservableCollection<LanguageMenuItemViewModel> Languages
        {
            get
            {
                ObservableCollection<LanguageMenuItemViewModel> result = new ObservableCollection<LanguageMenuItemViewModel>();
                
                foreach( CultureInfo info in CultureManager.Instance.GetAvailableCultures() )
                    result.Add( new LanguageMenuItemViewModel(info) );
                
                return result;
            }
        }

        private bool _backStageIsOpen = false;
        public bool BackStageIsOpen
        {
            get { return _backStageIsOpen; }
            set
            {
                if (_backStageIsOpen != value)
                {
                    _backStageIsOpen = value;
                    RaisePropertyChanged("BackStageIsOpen");
                }
            }
        }


        private int _backStageIndex = -1;
        public int BackStageIndex
        {
            get { return _backStageIndex; }
            set
            {
                if (_backStageIndex != value)
                {
                    _backStageIndex = value;
                    RaisePropertyChanged("BackStageIndex");
                }
            }
        }

		public int CatalogItemCount
		{
			get
			{
				if (Data != null)
					return Data.Books.Count;
				else return 0;
			}
		}

        new public Catalog Data
        {
            get { return base.Data as Catalog; }
            set { base.Data = value; }
        }

		private ObservableCollection<DocumentViewModel> _Documents;
		public ObservableCollection<DocumentViewModel> Documents
		{
			get
			{
				if (_Documents == null)
				{
					_Documents = new ObservableCollection<DocumentViewModel>();
				}
				return _Documents;
			}
		}
		
		private DocumentViewModel _ActiveDocument = null;
		public DocumentViewModel ActiveDocument 
        {
            get
            {
				return _ActiveDocument;
            }
			set
			{
				if (_ActiveDocument != value)
				{
					_ActiveDocument = value;

					RaisePropertyChanged("ActiveDocument");
					//RaisePropertyChanged("USBDeviceViewModel");
					RaisePropertyChanged("HasDriveActive");
					RaisePropertyChanged("HasBookActive");
					RaisePropertyChanged("HasRssActive");
                    RaisePropertyChanged("HasLibraryActive");

					//notify the view about activation for tools update
					Messenger.Default.Send<DocumentViewModel>(_ActiveDocument, ViewModelMessages.DocumentActivChanged);
				}
			}
        }

        private ObservableCollection<ToolViewModel> _Tools = new ObservableCollection<ToolViewModel>();
        public ObservableCollection<ToolViewModel> Tools
		{
			get
			{
				return _Tools;
			}
		}

		public bool HasRssActive
		{
			get
			{
				return ActiveDocument is FeedViewModel;
			}
		}

		public bool HasBookActive
		{
			get
			{
				return ActiveDocument is BookViewModelBase;
			}
		}

		public bool HasDriveActive
		{
			get
			{
				return ActiveDocument is DriveViewModel;
			}
		}

        public bool HasLibraryActive
        {
            get
            {
                return ActiveDocument is LibraryViewModel;
            }
        }

        //public USBDeviceViewModel USBDeviceViewModel
        //{
        //    get
        //    {
        //        if (Documents.Count(p => p is USBDeviceViewModel) > 0)
        //            return Documents.Where(p => p is USBDeviceViewModel).First() as USBDeviceViewModel;
        //        else
        //            return null;
        //    }
        //}

		private bool _hasNetwork = false;
		public bool HasNetwork
		{
			get { return _hasNetwork; }
			set
			{
				if (_hasNetwork != value)
				{
					_hasNetwork = value;
					RaisePropertyChanged("HasNetwork");
				}
			}
		}

		public ObservableCollection<FeedItemInfo> _Feeds = null;
		public ObservableCollection<FeedItemInfo> Feeds
		{
			get
			{
				if( _Feeds == null )
					_Feeds = new ObservableCollection<FeedItemInfo>(WorkspaceService.Instance.Settings.Feed.Feeds);
				return _Feeds;
			}
		}

		private Book _CurrentExplorerItem = null;
		public Book CurrentExplorerItem
		{
			get { return _CurrentExplorerItem; }
			set
			{
				if (_CurrentExplorerItem != value)
				{
					_CurrentExplorerItem = value;
					RaisePropertyChanged("CurrentExplorerItem");
				}
			}
		}

		#endregion

		#region ----------------SYSTEM COMMANDS----------------

        #region help command
        private ICommand sysHelpCommand;
        public ICommand SysHelpCommand
        {
            get
            {
                if (sysHelpCommand == null)
                    sysHelpCommand = new RelayCommand(
                        delegate()
                        {
							string code = CultureManager.Instance.GetNearestCode();
							string file = DirectoryHelper.Combine(CBRFolders.Dependencies, string.Format("QuickStart.{0}.xps", code)); 
							BookOpenFileCommand.Execute( file );
                        },
                        delegate() { return true;}
                        );
                return sysHelpCommand;
            }
        }
        #endregion

        #region exit command
        private ICommand sysExitCommand;
		public ICommand SysExitCommand
		{
			get
			{
                if (sysExitCommand == null)
                    sysExitCommand = new RelayCommand(
						delegate() { CloseCatalog(); Application.Current.MainWindow.Close(); },
						delegate()
						{
							if (Application.Current != null && Application.Current.MainWindow != null)
								return true;
							return false;
						});
                return sysExitCommand;
			}
		}
        #endregion

        #region view command
        private ICommand sysExplorerViewCommand;
        public ICommand SysExplorerViewCommand
        {
            get
            {
                if (sysExplorerViewCommand == null)
                    sysExplorerViewCommand = new RelayCommand<string>(
						delegate(string param) { Messenger.Default.Send<string>(param, ViewModelMessages.ExplorerViewModeChanged); },
                        delegate(string param)
                        {
                            return Data != null;
                        });
                return sysExplorerViewCommand;
            }
        }
		#endregion

		#region color command
		private ICommand sysChangeColorCommand;
		public ICommand SysChangeColorCommand
		{
			get
			{
				if (sysChangeColorCommand == null)
					sysChangeColorCommand = new RelayCommand<string>(
						delegate(string param)
						{
							Messenger.Default.Send<NotificationMessage>(new NotificationMessage(param), "Color");
						},
						delegate(string param)
						{
							return true;
						});
				return sysChangeColorCommand;
			}
		}
		#endregion

		#region show backstage command
		private ICommand showBackstageCommand;
		public ICommand ShowBackstageCommand
		{
			get
			{
				if (showBackstageCommand == null)
					showBackstageCommand = new RelayCommand<string>(ShowBackstageIndex,
						delegate(string param) { return true; }
						);
				return showBackstageCommand;
			}
		}

		void ShowBackstageIndex(string param)
		{
			try
			{
				BackStageIndex = Convert.ToInt32(param);
				BackStageIsOpen = true;
			}
			catch (Exception err)
			{
				LogHelper.Manage("MainViewModel:ShowBackstageIndex", err);
			}
		}

        #endregion

        #region add usb device command
        private ICommand sysDeviceAddCommand;
        public ICommand SysDeviceAddCommand
        {
            get
            {
                if (sysDeviceAddCommand == null)
                    sysDeviceAddCommand = new RelayCommand<LogicalDiskInfo>(DeviceAdd,
                        delegate(LogicalDiskInfo param) { return true; });
                return sysDeviceAddCommand;
            }
        }

        void DeviceAdd(LogicalDiskInfo param)
        {
            try
            {
				Messenger.Default.Send<LogicalDiskInfo>(param, ViewModelMessages.DeviceAdded);
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:DeviceAdd", err);
            }
        }

        #endregion

        #region remove usb device command
        private ICommand sysDeviceRemoveCommand;
        public ICommand SysDeviceRemoveCommand
        {
            get
            {
                if (sysDeviceRemoveCommand == null)
                    sysDeviceRemoveCommand = new RelayCommand<LogicalDiskInfo>(DeviceRemove,
                        delegate(LogicalDiskInfo param) { return true; }
                        );
                return sysDeviceRemoveCommand;
            }
        }

        void DeviceRemove(LogicalDiskInfo param)
        {
            try
            {
				Messenger.Default.Send<LogicalDiskInfo>(param, ViewModelMessages.DeviceRemoved);
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:DeviceAdd", err);
            }
        }

        #endregion

        #region showtools command
        private ICommand showToolsCommand;
        public ICommand ShowToolsCommand
        {
            get
            {
                if (showToolsCommand == null)
                    showToolsCommand = new RelayCommand<string>(
                        delegate(string param)
                        {
                            ViewModelFactory.Instance.CreateFrom(this, param);
                        },
                        delegate(string param)
                        {
                            return true;
                        });
                return showToolsCommand;
            }
        }
        #endregion

        #endregion

        #region ----------------CATALOG COMMANDS----------------

        #region new catalog command
        private ICommand catalogNewCommand;
        public ICommand CatalogNewCommand
        {
            get
            {
                if (catalogNewCommand == null)
                    catalogNewCommand = new RelayCommand(NewCatalog, delegate() { return true; });
                return catalogNewCommand;
            }
        }

        void NewCatalog()
        {
            try
            {
                // check if opened and not save before

                //create a new one
                using (System.Windows.Forms.SaveFileDialog browser = new System.Windows.Forms.SaveFileDialog())
                {
					browser.InitialDirectory = DirectoryHelper.UserPath;
                    browser.AddExtension = true;
                    browser.Filter = DocumentFactory.Instance.CatalogFilterAll;
                    browser.DefaultExt = DocumentFactory.Instance.CatalogFilterDefaultExtension;
                    browser.FilterIndex = DocumentFactory.Instance.CatalogFilterDefaultIndex;

                    if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
                    {
						Messenger.Default.Send<Catalog>(new Catalog(browser.FileName), ViewModelMessages.CatalogChanged);
                    }
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:OpenCatalog", err);
            }
        }
        #endregion

		#region open catalog with dialog command
		private ICommand catalogOpenWithDialogCommand;
		public ICommand CatalogOpenWithDialogCommand
		{
			get
			{
                if (catalogOpenWithDialogCommand == null)
                    catalogOpenWithDialogCommand = new RelayCommand(OpenWithDialogCatalog, delegate() { return true; });
                return catalogOpenWithDialogCommand;
			}
		}

        void OpenWithDialogCatalog()
		{
			try
			{
				using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
				{
					browser.InitialDirectory = DirectoryHelper.UserPath;
                    browser.Filter= DocumentFactory.Instance.CatalogFilterAll;
                    browser.FilterIndex = DocumentFactory.Instance.CatalogFilterDefaultIndex;

                    if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
					{
                        OpenFileCatalog(browser.FileName);
					}
				}
			}
			catch (Exception err)
			{
                LogHelper.Manage("MainViewModel:OpenWithDialogCatalog", err);
			}
		}
		#endregion

        #region open file catalog command
        private ICommand catalogOpenFileCommand;
        public ICommand CatalogOpenFileCommand
        {
            get
            {
                if (catalogOpenFileCommand == null)
                    catalogOpenFileCommand = new RelayCommand<string>(OpenFileCatalog, delegate(string param) { return true; });
                return catalogOpenFileCommand;
            }
        }

        void OpenFileCatalog(string param)
        {
            try
            {
                if( File.Exists( param ) )
                {
                    Catalog catalog = CatalogService.Instance.Open(param);
                    Messenger.Default.Send<Catalog>(catalog, ViewModelMessages.CatalogChanged);
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:OpenFileCatalog", err);
            }
        }
        #endregion

		#region save catalog command
		private ICommand catalogSaveCommand;
		public ICommand CatalogSaveCommand
		{
			get
			{
                if (catalogSaveCommand == null)
                    catalogSaveCommand = new RelayCommand(SaveCatalog, delegate() { return (Data != null); });
                return catalogSaveCommand;
			}
		}

		void SaveCatalog()
		{
            CatalogService.Instance.Save(Data);
		}
		#endregion

		#region save as catalog command
		private ICommand catalogSaveAsCommand;
		public ICommand CatalogSaveAsCommand
		{
			get
			{
                if (catalogSaveAsCommand == null)
                    catalogSaveAsCommand = new RelayCommand(SaveAsCatalog, delegate() { return (Data != null); });
                return catalogSaveAsCommand;
			}
		}

		void SaveAsCatalog()
		{
			try
			{
				using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
				{
					browser.InitialDirectory = DirectoryHelper.UserPath;
                    browser.Filter = DocumentFactory.Instance.CatalogFilterAll;
                    browser.FilterIndex = DocumentFactory.Instance.CatalogFilterDefaultIndex;
                    browser.DefaultExt = DocumentFactory.Instance.CatalogFilterDefaultExtension;

                    if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
					{
                        CatalogService.Instance.SaveAs(Data, browser.FileName);
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("MainViewModel:SaveAsCatalog", err);
			}
		}
		#endregion

		#region refresh catalog command
		private ICommand catalogRefreshCommand;
        public ICommand CatalogRefreshCommand
		{
			get
			{
                if (catalogRefreshCommand == null)
                    catalogRefreshCommand = new RelayCommand(RefreshCatalog,
						delegate()
						{
                            return (Data != null);
						});
                return catalogRefreshCommand;
			}
		}

		void RefreshCatalog()
		{
            CatalogService.Instance.Refresh(Data);
		}
		#endregion

		#region close catalog command
		private ICommand catalogCloseCommand;
		public ICommand CatalogCloseCommand
		{
			get
			{
                if (catalogCloseCommand == null)
					catalogCloseCommand = new RelayCommand(CloseCatalog, CanExecuteCloseCatalog);
                return catalogCloseCommand;
			}
		}

		private bool CanExecuteCloseCatalog()
		{
			return Data != null;
		}
		void CloseCatalog()
		{
            if (Data != null)
			{
                if (CatalogService.Instance.IsDirty(Data))
				{
					string msg = CultureManager.Instance.GetLocalization("ByCode", "Warning.Save", "Save the catalog and book changes ?");
					string title = CultureManager.Instance.GetLocalization("ByCode", "Warning", "Warning");

					if (MessageBox.Show(msg, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        CatalogService.Instance.Save(Data);
				}

				Messenger.Default.Send<Catalog>(null, ViewModelMessages.CatalogChanged);
				Messenger.Default.Send<Book>(null, ViewModelMessages.BookSelected);
			}
		}
		#endregion

        #region remove catalog command
        private ICommand catalogRemoveCommand;
        public ICommand CatalogRemoveCommand
        {
            get
            {
                if (catalogRemoveCommand == null)
                    catalogRemoveCommand = new RelayCommand<string>(RemoveCatalog,
                        delegate(string param)
                        {
                            if (this.ActiveDocument is LibraryViewModel)
                                return (this.ActiveDocument as LibraryViewModel).Catalogs.CurrentItem != null;
                            else
                                return false;
                        });
                return catalogRemoveCommand;
            }
        }

        void RemoveCatalog(string param)
        {
            if (!string.IsNullOrEmpty(param))
            {
                Catalog catlog = CatalogService.Instance.CatalogRepository.Single(p => p.CatalogFilePath == param);
                CatalogService.Instance.RepositoryRemove(catlog);
            }
        }
        #endregion

        #region delete catalog command
        private ICommand catalogDeleteCommand;
        public ICommand CatalogDeleteCommand
        {
            get
            {
                if (catalogDeleteCommand == null)
                    catalogDeleteCommand = new RelayCommand<string>(DeleteCatalog,
                        delegate(string param)
                        {
                            if (this.ActiveDocument is LibraryViewModel)
                                return (this.ActiveDocument as LibraryViewModel).Catalogs.CurrentItem != null;
                            else
                                return false;
                        });
                return catalogDeleteCommand;
            }
        }

        void DeleteCatalog(string param)
        {
            if (!string.IsNullOrEmpty(param))
            {
                Catalog catlog = CatalogService.Instance.CatalogRepository.Single(p => p.CatalogFilePath == param);
                CatalogService.Instance.RepositoryDelete(catlog);
            }
        }
        #endregion

        #region catalog share command
        private ICommand catalogShareCommand;
        public ICommand CatalogShareCommand
        {
            get
            {
                if (catalogShareCommand == null)
                    catalogShareCommand = new RelayCommand(LibShare,
                        delegate()
                        {
                            if (this.ActiveDocument is LibraryViewModel)
                                return (this.ActiveDocument as LibraryViewModel).Catalogs.CurrentItem != null;
                            else
                                return false;
                        } );
                return catalogShareCommand;
            }
        }

        void LibShare()
        {
            try
            {
                if (this.ActiveDocument is LibraryViewModel)
                {
                    CatalogViewModel current = (this.ActiveDocument as LibraryViewModel).Catalogs.CurrentItem as CatalogViewModel;
                    current.IsShared = !current.IsShared;
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainViewModel:LibShare", err);
            }
        }

        #endregion

        #region add catalog command
        private ICommand catalogAddCommand;
        public ICommand CatalogAddCommand
        {
            get
            {
                if (catalogAddCommand == null)
                    catalogAddCommand = new RelayCommand(AddCatalog,
                        delegate()
                        {
                            return true;
                        });
                return catalogAddCommand;
            }
        }

        void AddCatalog()
        {
            using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
            {
				browser.InitialDirectory = DirectoryHelper.UserPath;
                browser.Filter = DocumentFactory.Instance.CatalogFilterAll;
                browser.FilterIndex = DocumentFactory.Instance.CatalogFilterDefaultIndex;

                if (browser.ShowDialog(new Wpf32Window()) == System.Windows.Forms.DialogResult.OK)
                {
                    CatalogService.Instance.RepositoryAdd(browser.FileName);
                }
            }
        }
        #endregion

		#region Refresh catalog cover command
		private ICommand catalogRefreshCoverCommand;
		public ICommand CatalogRefreshCoverCommand
		{
			get
			{
				if (catalogRefreshCoverCommand == null)
					catalogRefreshCoverCommand = new RelayCommand<string>(RefreshCover,
						delegate( string param )
						{
							if (this.ActiveDocument is LibraryViewModel)
								return (this.ActiveDocument as LibraryViewModel).Catalogs.CurrentItem != null;
							else
								return false;
						});
				return catalogRefreshCoverCommand;
			}
		}

		void RefreshCover(string param)
		{
			try
			{
				if (this.ActiveDocument is LibraryViewModel)
				{
					CatalogViewModel current = (this.ActiveDocument as LibraryViewModel).Catalogs.CurrentItem as CatalogViewModel;
					Messenger.Default.Send<string>(ViewModelMessages.CatalogRefreshCover, current.Data.CatalogFilePath);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("MainViewModel:RefreshCover", err);
			}
		}
		#endregion

		#endregion

		#region ----------------METHODS----------------

		public void ManageStartingDocument(string param)
		{
			FileInfo fi = new FileInfo(param);
			if (fi.Exists)
			{
				if (DocumentFactory.Instance.FindCatalogFilterByExt(fi.Extension) != null)
					this.OpenFileCatalog(param);

				if (DocumentFactory.Instance.FindBookFilterByExtWithModel(fi.Extension) != null)
					this.OpenFileBook(param);
			}
		}

		public void SetActiveWorkspace(DocumentViewModel wmb)
		{
			if (wmb != null)
			{
				Debug.Assert(this.Documents.Contains(wmb));
				ActiveDocument = wmb;
			}
		}

		#endregion

		#region ----------------HANDLERS----------------

		///// <summary>
		///// current view model is changes
		///// </summary>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//void _ViewModels_CurrentChanged(object sender, EventArgs e)
		//{
		//    //notify the view about activation for tools update
		//    Messenger.Default.Send(ViewModelMessages.ActiveDocumentChanged, ActiveDocument);

		//    RaisePropertyChanged("ActiveDocument");
		//    RaisePropertyChanged("USBDeviceViewModel");

		//    RaisePropertyChanged("HasUSBDeviceActive");
		//    RaisePropertyChanged("HasBookActive");
		//    RaisePropertyChanged("HasRssActive");
		//}

        ///// <summary>
        ///// view model collection source is changed
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void OnViewModelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null && e.NewItems.Count != 0)
        //        foreach (DocumentViewModel document in e.NewItems)
        //            document.RequestClose += this.OnDocumentRequestClose;

        //    if (e.OldItems != null && e.OldItems.Count != 0)
        //        foreach (DocumentViewModel document in e.OldItems)
        //            document.RequestClose -= this.OnDocumentRequestClose;
        //}

        ///// <summary>
        ///// View request for closing, we destroy the view model
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void OnDocumentRequestClose(object sender, EventArgs e)
        //{
        //    DocumentViewModel workspace = sender as DocumentViewModel;
        //    this.Documents.Remove(workspace);
        //    workspace.Dispose();
        //}

		void NetworkChangeNetworkAddressChanged(object sender, EventArgs e)
		{
			HasNetwork = NetworkInterface.GetIsNetworkAvailable();
		}

		void Books_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove )
				RaisePropertyChanged("CatalogItemCount");
		}

		#endregion
		
		#region ----------------INTERNALS----------------

		internal void SwapTwoPageMode(BookViewModelBase o)
		{
			//Documents.Remove(o);

			BookViewModelBase newModel = null;
			BookViewModelBase oldModel = null;
			if (o is ComicViewModel)
			{
				ComicViewModel comic = o as ComicViewModel;
				newModel = new TwoPageViewModel(o.Data, comic.CurrentPage.Index, comic.FitMode, comic.PreviousScale);
			}
			else
			{
				TwoPageViewModel comic = o as TwoPageViewModel;
				newModel = new ComicViewModel(o.Data, comic.CurrentPageIndex, comic.FitMode, comic.PreviousScale);
			}

			oldModel = o;
			Documents.Add(newModel);
			SetActiveWorkspace(newModel);

			Documents.Remove(oldModel);
		}

        internal void ExecuteDistantCommand(CommandContext context)
        {
            if (context != null)
            {
                ReflectionHelper.ExecuteICommand( this, context.CommandName, context.CommandParameter );
            }
        }

        #endregion

        #region -----------------SHARING-----------------

        ServiceHost host = null;

        public bool IsSharing
        {
            get
            {
                return (host != null && host.State == CommunicationState.Opened);
            }
            set
            {
                if (value == true)
                    StartSharing();
                else
                    StopSharing();
            }
        }

        public void ToggleSharing()
        {
            if (IsSharing)
                StopSharing();
            else
                StartSharing();
        }

        public void StartSharing()
        {
            host = new ServiceHost(typeof(WinRTService));
 
            ServiceEndpoint end = host.Description.Endpoints.First(p => p.Name == "netTcpEndpoint");
            end.Address = new EndpointAddress( 
                string.Format("net.tcp://{0}:{1}/WinRTService/", WorkspaceService.Instance.Settings.Extended.ShareAdress,
                WorkspaceService.Instance.Settings.Extended.SharePort));
            //end.Address = new EndpointAddress(
            //    new Uri(string.Format("net.tcp://{0}:9999/WinRTService/", WorkspaceService.Instance.Settings.Extended.ShareAdress)),
            //    EndpointIdentity.CreateDnsIdentity(WorkspaceService.Instance.Settings.Extended.ShareAdress));
            
#if DEBUG
            end = host.Description.Endpoints.First(p => p.Name == "mexTcpEndpoint");
            end.Address = new EndpointAddress(
                string.Format("net.tcp://{0}:{1}/WinRTService/mex", WorkspaceService.Instance.Settings.Extended.ShareAdress,
                WorkspaceService.Instance.Settings.Extended.SharePort));
#endif

            host.Open();
            //(host.SingletonInstance as WinRTService).Secure(WorkspaceService.Instance.Settings.Extended.ShareUserName);

            RaisePropertyChanged("IsSharing");
        }

        public void StopSharing()
        {
            if (host != null && host.State == CommunicationState.Opened)
            {
                host.Close();
                host = null;
            }
            RaisePropertyChanged("IsSharing");
        }

        #endregion
    }
}