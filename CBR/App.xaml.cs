using System.Globalization;
using System.Windows;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Helpers.State;
using CBR.Core.Services;
using CBR.ViewModels;
using System;
using CBR.Views.Others;
using CBR.Core.Helpers.Splash;

namespace CBR
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, IWpfSingleApp
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			LogHelper.Begin("App.OnStartup");
			try
			{
				// check if IE emulator is well configured
				if (!ProcessHelper.CheckIERegistry())
				{
					LogHelper.Trace("IE registry is not OK !");
					//ProcessHelper.RegisterIE();
				}

				this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

				SplashScreenManager.Splash.Message = "Process settings";

				//load element position and size
				ElementStateOperations.Load();

				//load settings from properties
				WorkspaceService.Instance.Settings = CBR.Properties.Settings.Default.CatalogSetting;

				//set the starting language from settings if different
				SplashScreenManager.Splash.Message = "Process cultures";
				CultureManager.Instance.UICulture = CultureInfo.GetCultureInfo(WorkspaceService.Instance.Settings.StartingLanguageCode);

				// Create the ViewModel and expose it using the View's DataContext
				SplashScreenManager.Splash.Message = "Create the window"; 
				Views.MainView view = new Views.MainView(e.Args);
				view.Show();
			}
			catch (Exception err)
			{
				LogHelper.Manage("App.OnStartup", err);
			}
			LogHelper.End("App.OnStartup");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			LogHelper.Manage("Unhandled exception", e.Exception);
			LogHelper.Trace(e.Exception.TargetSite.ToString());
			LogHelper.Trace(e.Exception.StackTrace.ToString());
			LogHelper.Trace(e.Exception.Source.ToString());
		}

		/// <summary>
		/// Exiting the application
		/// </summary>
		/// <param name="e"></param>
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			LogHelper.Begin("App.OnExit");
			try
			{
                //save catalog list
                CatalogService.Instance.SaveRepository();

				//save the settings
				CBR.Properties.Settings.Default.CatalogSetting = WorkspaceService.Instance.Settings;
				CBR.Properties.Settings.Default.Save();
#if DEBUG
				//save the default resources that are not yet translated
				CultureManager.Instance.SaveResources();
#endif
				//save element position and size
				ElementStateOperations.Save();

				//delete the temp folder
				DirectoryHelper.Cleanup();
			}
			catch (Exception err)
			{
				LogHelper.Manage("App.OnExit", err);
			}
			LogHelper.End("App.OnExit");
		}

		/// <summary>
		/// Process possible document as starting arguments
		/// </summary>
		/// <param name="args"></param>
		public void ProcessArguments(string[] args)
		{
			LogHelper.Begin("App.ProcessArguments");
			try
			{
				MainViewModel main = (this.MainWindow as Views.MainView).DataContext as MainViewModel;
				main.ManageStartingDocument(args[1]);
			}
			catch (Exception err)
			{
				LogHelper.Manage("App.ProcessArguments", err);
			}
			LogHelper.End("App.ProcessArguments");
		}
		
	}
}
