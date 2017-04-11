using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers.Splash;
using CBR.Views;
using System.Threading;
using CBR.Core.Helpers;

namespace CBR
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			App app = new App();

			//register the single instance application
			WpfSingleInstance.Make("C.B.R.", app);

			ISplashScreen splashScreen = SplashScreenManager.CreateSplashScreen(typeof(SplashContent));

			SplashScreenManager.Splash.Message = "Starting...";

			app.InitializeComponent();
			app.Run();
		}
	}
}
