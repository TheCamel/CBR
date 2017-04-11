using System;
using System.Threading;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;

namespace CBR.Core.Helpers
{
	public interface IWpfSingleApp
	{
		void ProcessArguments( string[] args );
	}

	public static class WpfSingleInstance
	{
		public static void Make(String name, Application app)
		{
			EventWaitHandle eventWaitHandle = null;
			String eventName = Environment.MachineName + "-" + name;

			bool isFirstInstance = false;

			try
			{
				eventWaitHandle = EventWaitHandle.OpenExisting(eventName);
			}
			catch
			{
				// it's first instance
				isFirstInstance = true;
			}

			if (isFirstInstance)
			{
				eventWaitHandle = new EventWaitHandle( false, EventResetMode.AutoReset, eventName);

				ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, waitOrTimerCallback, app, 
					Timeout.Infinite, false);

				// not need more
				eventWaitHandle.Close();
			}
			else
			{
				// !!! delete it if not use
				setArgs();

				eventWaitHandle.Set();

				// For that exit no interceptions
				Environment.Exit(0);
			}
		}

		private delegate void dispatcherInvoker();

		private static void waitOrTimerCallback(Object state, Boolean timedOut)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("WpfSingleInstance.waitOrTimerCallback");
			try
			{
				Application app = (Application)state;

				app.Dispatcher.BeginInvoke(
						new dispatcherInvoker(delegate()
				{
					//activate the main window
					Application.Current.MainWindow.Activate();

					// process arguments
					processArgs();
				}),
						null
					);
			}
			catch (Exception err)
			{
				LogHelper.Manage("WpfSingleInstance.waitOrTimerCallback", err);
			}
			finally
			{
				LogHelper.End("WpfSingleInstance.waitOrTimerCallback");
			}  
		}

		// Args functionality for test purpose and not developed carefuly
		#region Args

		internal static readonly object StartArgKey = "StartArg";

		private static readonly String isolatedStorageFileName = "SomeFileInTheRoot.txt";

		private static void setArgs()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("WpfSingleInstance.setArgs");
			try
			{
				string[] args = Environment.GetCommandLineArgs();
				if (1 < args.Length)
				{
					IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
							null, null);

					IsolatedStorageFileStream isoStream1 = new IsolatedStorageFileStream(isolatedStorageFileName, FileMode.Create, isoStore);
					StreamWriter sw = new StreamWriter(isoStream1);
					sw.Write(string.Join(";", args));
					sw.Close();
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("WpfSingleInstance.setArgs", err);
			}
			finally
			{
				LogHelper.End("WpfSingleInstance.setArgs");
			}  
		}

		private static void processArgs()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("WpfSingleInstance.processArgs");
			try
			{
				IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
						null, null);

				IsolatedStorageFileStream isoStream1 = new IsolatedStorageFileStream(isolatedStorageFileName, FileMode.OpenOrCreate, isoStore);
				StreamReader sr = new StreamReader(isoStream1);
				string arg = sr.ReadToEnd();
				sr.Close();

				isoStore.DeleteFile(isolatedStorageFileName);

				IWpfSingleApp app = Application.Current as IWpfSingleApp;
				app.ProcessArguments(arg.Split(';'));
			}
			catch (Exception err)
			{
				LogHelper.Manage("WpfSingleInstance.processArgs", err);
			}
			finally
			{
				LogHelper.End("WpfSingleInstance.processArgs");
			}  
		}
		#endregion
	}
}
