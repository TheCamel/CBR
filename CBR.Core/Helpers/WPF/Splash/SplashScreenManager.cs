using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace CBR.Core.Helpers.Splash
{
    public class SplashScreenManager
    {
        private static object mutex = new object();
		private static SplashScreenWindowViewModel _splash = null;

		public static ISplashScreen Splash
		{
			get { return SplashScreenManager._splash; }
			set { SplashScreenManager._splash = (SplashScreenWindowViewModel)value; }
		}

		public static ISplashScreen CreateSplashScreen(Type contentType)
        {
            lock (mutex)
            {
				_splash = new SplashScreenWindowViewModel();

                AutoResetEvent ev = new AutoResetEvent(false);

                Thread uiThread = new Thread(() =>
                {
					_splash.Dispatcher = Dispatcher.CurrentDispatcher;
					_splash.SetContentObject(contentType);
                    ev.Set();

                    Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate()
                    {
                        SplashScreenWindow splashScreenWindow = new SplashScreenWindow();
						splashScreenWindow.DataContext = _splash;
                        splashScreenWindow.Show();
                    });

                    Dispatcher.Run();
                });

                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.IsBackground = true;
                uiThread.Start();
                ev.WaitOne();

				return _splash;
            }
        }

    }
}
