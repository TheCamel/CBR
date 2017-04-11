using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CBR.Components.Dialogs;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Helpers.Splash;
using CBR.Core.Helpers.State;
using CBR.Core.Models;
using CBR.Core.Services;
using CBR.ViewModels;
using CBR.Views.Others;
using Fluent;
using GalaSoft.MvvmLight.Messaging;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        #region ----------------CONSTRUCTOR----------------

        private WMIEventWatcher _wmiWatcher = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainView(string[] param)
        {
			using (new TimeLogger("MainView.MainView"))
			{
				InitializeComponent();

				if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				{
					DataContext = new MainViewModel(param.Count() > 0 ? param[0] : string.Empty);

					Messenger.Default.Register<NotificationMessage>(this, "Color", (s) =>
					{
						Uri source = new Uri(
							string.Format("/CBR;Component/Resources/XAML/Colors/Colors.{0}.xaml", s.Notification),
							UriKind.Relative);

						Uri source2 = new Uri(
							string.Format("/Fluent;Component/Themes/Office2010/{0}.xaml", s.Notification),
							UriKind.Relative);
					
						Application.Current.Resources.BeginInit();
						Application.Current.Resources.MergedDictionaries.RemoveAt(0);
						Application.Current.Resources.MergedDictionaries.RemoveAt(0);
						Application.Current.Resources.MergedDictionaries.Insert(0, (ResourceDictionary)Application.LoadComponent(source));
						Application.Current.Resources.MergedDictionaries.Insert(0, (ResourceDictionary)Application.LoadComponent(source2));
						Application.Current.Resources.EndInit();
					});
				} 

				//start wmi watcher and load existing devices
				Task.Factory.StartNew(() =>
				{
					try
					{
						_wmiWatcher = new WMIEventWatcher();
						_wmiWatcher.StartWatchUSB();
						_wmiWatcher.EventArrived += new WMIEventArrived(wmi_EventArrived);
					}
					catch (Exception err)
					{
						LogHelper.Manage("MainView.MainView start wmi watcher", err);
					}
				});

				//start task to manage ByCode translations
				Task.Factory.StartNew(() =>
				{
					try
					{
						CultureManager.Instance.GetLocalization("ByCode", "Warning", "Warning");
						CultureManager.Instance.GetLocalization("ByCode", "Warning.Save", "Save the catalog and book changes ?");
						CultureManager.Instance.GetLocalization("ByCode", "Warning.Delete", "Please, confirm the deletion");
						CultureManager.Instance.GetLocalization("ByCode", "Warning.ScanFolder", "Your book folder is allready defined. Do you want to replace it ? Refreshing will work only with the new one.");
						CultureManager.Instance.GetLocalization("ByCode", "Warning.CatalogPath", "Catalog path does not exist! Please check the options box");

						CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Home", "Home");
						CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Drives", "Drives");
						CultureManager.Instance.GetLocalization("ByCode", "DocumentTitle.Libraries", "Libraries");

						CultureManager.Instance.GetLocalization("ByCode", "ExplorerView.Title", "Library Explorer");
						CultureManager.Instance.GetLocalization("ByCode", "TocView.Title", "Table of content");
						CultureManager.Instance.GetLocalization("ByCode", "DriveExplorerView.Title", "Drive Explorer");

						CultureManager.Instance.GetLocalization("ByCode", "MEGA", "(Mb)");
						CultureManager.Instance.GetLocalization("ByCode", "KILO", "(Kb)");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.Start", "Starting conversion...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ParsingFolder", "Parsing folder {0}...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.Converting", "Converting {0}...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageFound", "{0} images founded...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageSaved", "{0} images saved...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageCountingKO", "Extracting {0} : {1} images for {2} pages - Try to merge !");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageMerge", "Merge to {0} new images...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageError", "Error extracting {0} : {1} images for {2} pages !!");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageCountingOK", "Extracting {0} images in {1} pages");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageExtracted", "{0} images extracted...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageZipped", "{0} images zipped...");
						CultureManager.Instance.GetLocalization("ByCode", "Convert.Output", "Output file written !");
					}
					catch (Exception err)
					{
						LogHelper.Manage("MainView.MainView start localize", err);
					}
				});
			}
        }

        #endregion

        #region ----------------OTHERS----------------

        /// <summary>
        /// Display the about box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
        }
        #endregion

        #region ----------------FULL SCREEN----------------

        /// <summary>
        /// Full screen state
        /// </summary>
        private bool IsFullScreen { get; set; }
		private WindowState FullScreenPreviousMode { get; set; }

        /// <summary>
        /// full screen button click handler (binding is not working...)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SwapFullScreenMode();
        }

        /// <summary>
        /// Change the full screen state
        /// </summary>
        private void SwapFullScreenMode()
        {
            try
            {
                if (IsFullScreen)
                {
					this.WindowState = FullScreenPreviousMode;
                    IsFullScreen = false;
                }
                else
                {
					FullScreenPreviousMode = this.WindowState;
                    this.WindowState = WindowState.Maximized;
                    IsFullScreen = true;
                }

				if (WorkspaceService.Instance.Settings.Extended.ShowFullScreenOptimized)
				{
					this.GlassBorderThickness = IsFullScreen ? new Thickness(0, 0, 0, 0) : new Thickness(8, 50, 8, 8);
					this.statusBarInfo.Visibility = IsFullScreen ? Visibility.Collapsed : Visibility.Visible;
					this.ribbonMain.Visibility = IsFullScreen ? Visibility.Collapsed : Visibility.Visible;
				}
				else
				{
					this.ribbonMain.IsMinimized = IsFullScreen;
				}
			}
            catch (Exception err)
            {
                LogHelper.Manage("MainView:SwapFullScreenMode", err);
            }
        }
        
        #endregion

        #region ----------------WINDOW EVENTS----------------

        /// <summary>
        /// handle file drop on the main surface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonWindow_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    MainViewModel mvm = DataContext as MainViewModel;

                    string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                    if (DocumentFactory.Instance.FindCatalogFilterByExt(System.IO.Path.GetExtension(droppedFilePaths[0])) != null)
                        mvm.CatalogOpenFileCommand.Execute(droppedFilePaths[0]);
                    else
                    if (DocumentFactory.Instance.FindBookFilterByExtWithModel(System.IO.Path.GetExtension(droppedFilePaths[0])) != null)
                        mvm.BookOpenFileCommand.Execute(droppedFilePaths[0]);
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("MainView:RibbonWindow_Drop", err);
            }
        }

        /// <summary>
        /// start wmi watcher and load existing devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("MainView.RibbonWindow_Loaded");
			try
			{
				SplashScreenManager.Splash.Message = "Localizing...";

				LoadLayout("AvalonDockConfig.xml");

				CultureManager.Instance.AvailableCulturesChanged += new CultureEventArrived(CultureManager_AvailableCulturesChanged);

				MainViewModel mvm = DataContext as MainViewModel;

				if (mvm != null)
				{
					//add all existing disks
                    foreach (LogicalDiskInfo disk in _wmiWatcher.Devices)
						mvm.SysDeviceAddCommand.Execute(disk);
				}
				
				this.WindowState = System.Windows.WindowState.Maximized;
				this.Visibility = System.Windows.Visibility.Visible;
                this.Activate();
			}
			catch (Exception err)
			{
				LogHelper.Manage("MainView.RibbonWindow_Loaded", err);
			}
			finally
			{
				LogHelper.End("MainView.RibbonWindow_Loaded");
			}  
        }

		/// <summary>
		/// dispose the splash window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RibbonWindow_Activated(object sender, EventArgs e)
		{
			SplashScreenManager.Splash.Dispose();
		}

        /// <summary>
        /// on closing, stop wmi watcher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CultureManager.Instance.AvailableCulturesChanged -= new CultureEventArrived(CultureManager_AvailableCulturesChanged);

			if( _wmiWatcher != null )
				_wmiWatcher.StopWatchUSB();

			SaveLayout("AvalonDockConfig.xml");
        }

        /// <summary>
        /// Update the language menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CultureManager_AvailableCulturesChanged(object sender, CultureEventArgs e)
        {
            MainViewModel mvm = DataContext as MainViewModel;

            if (mvm != null)
                mvm.Languages.Add( new LanguageMenuItemViewModel( e.Culture ) );
        }

        #endregion

        #region ----------------OTHER EVENTS----------------

        /// <summary>
        /// handle wmi events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wmi_EventArrived(object sender, WMIEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, (ThreadStart)delegate
            {
                MainViewModel mvm = DataContext as MainViewModel;

                if (mvm != null)
                {
                    if (e.EventType == WMIActions.Added)
                        mvm.SysDeviceAddCommand.Execute(e.Disk);
                    else
                        if (e.EventType == WMIActions.Removed)
                            mvm.SysDeviceRemoveCommand.Execute(e.Disk);
                }
            });
        }

		/// <summary>
		/// hnadle the key down event in the book ribbon for goto page function
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void PageNumber_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                MainViewModel mvm = DataContext as MainViewModel;

				if (mvm != null && mvm.ActiveDocument is BookViewModelBase)
                {
					(mvm.ActiveDocument as BookViewModelBase).BookGotoPageCommand.Execute(this.PageNumber.Text);
                }
            }
        }

		/// <summary>
		/// Update the culture regarding the dropdown selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void languageGallery_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
			if( e.AddedItems.Count>0)
				CultureManager.Instance.UICulture = (e.AddedItems[0] as LanguageMenuItemViewModel).Data;
        }

		/// <summary>
		/// Show localization editor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void btnLocalize_Click(object sender, RoutedEventArgs e)
        {
            LocalizeView view = new LocalizeView();
            view.Show();
        }

		/// <summary>
		/// manage escaping the full screen mode when optimized
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RibbonWindow_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ( e.Key == System.Windows.Input.Key.Escape && IsFullScreen )
			{
				SwapFullScreenMode();
				e.Handled = true;
			}
		}

		private void FeedMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MainViewModel mvm = this.DataContext as MainViewModel;

			FeedViewModel fvm = new FeedViewModel(((sender as MenuItem).DataContext) as FeedItemInfo);

			mvm.Documents.Add(fvm);
			mvm.SetActiveWorkspace(fvm);
		}

		#endregion

		private void LoadLayout( string fileName )
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("MainView.LoadLayout");
			try
			{
				MainViewModel mvm = this.DataContext as MainViewModel;
				string filePath = DirectoryHelper.Combine(CBRFolders.Cache, fileName);

				if (File.Exists(filePath))
				{
					var serializer = new XmlLayoutSerializer(dockManager);
					serializer.LayoutSerializationCallback += (s, e) =>
					{
                        if (!string.IsNullOrEmpty(e.Model.ContentId))
                        {
                            e.Content = ViewModelFactory.Instance.CreateFrom(mvm, e.Model.ContentId);
                        }
					};

					using (var stream = new StreamReader(filePath))
						serializer.Deserialize(stream);
				}
				else
				{
                    mvm.ResetLayout();
                    SaveLayout("AvalonDockBackup.xml"); 
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("MainView.LoadLayout", err);
			}
			finally
			{
				LogHelper.End("MainView.LoadLayout");
			}  
		}

		private void SaveLayout( string fileName )
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("MainView.SaveLayout");
			try
			{
				var serializer = new XmlLayoutSerializer(dockManager);

				string filePath = DirectoryHelper.Combine(CBRFolders.Cache, fileName);

				using (var stream = new StreamWriter(filePath))
					serializer.Serialize(stream);
			}
			catch (Exception err)
			{
				LogHelper.Manage("MainView.SaveLayout", err);
			}
			finally
			{
				LogHelper.End("MainView.SaveLayout");
			}  
		}

		private void ButtonResetLayout_Click(object sender, RoutedEventArgs e)
		{
            MainViewModel mvm = this.DataContext as MainViewModel;
            mvm.ResetLayout();
            
            //avalon
			//LoadLayout("AvalonDockBackup.config");
			
            //fluent
			this.ribbonMain.ClearQuickAccessToolBar();
			
            //grid
			ElementStateOperations.Reset();
		}
	}
}
