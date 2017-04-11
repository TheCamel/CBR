using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;

namespace CBR.Core.Helpers
{
	public class ProcessHelper
	{
		static public void LaunchShellUri(Uri url)
		{
			Process.Start( new ProcessStartInfo( url.AbsoluteUri ) );
		}

		/// <summary>
		/// Check for IE registry emulation mode associated with CBR
		/// <returns></returns>
		static public bool CheckIERegistry()
		{
			RegistryKey key = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ProcessHelper.CheckIERegistry");
			try
			{
				key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION");
#if DEBUG
				if (Convert.ToInt32(key.GetValue("CBR.vshost.exe", null, RegistryValueOptions.None)) != 9999)
					return false;
#endif

				if (Convert.ToInt32(key.GetValue("CBR.exe", null, RegistryValueOptions.None)) != 9999)
					return false;
			}
			catch (Exception err)
			{
				
				LogHelper.Manage("ProcessHelper.CheckIERegistry", err);
				return false;
			}
			finally
			{
				key.Close();
				LogHelper.End("ProcessHelper.CheckIERegistry");
			}  

			return true;
		}

		static public bool RegisterIE()
		{
			RegistryKey key = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ProcessHelper.RegisterIE");
			try
			{
				key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION");
				key.SetValue("CBR.exe", 9999, RegistryValueKind.DWord);
#if DEBUG
				key.SetValue("CBR.vshost.exe", 9999, RegistryValueKind.DWord);
#endif
			}
			catch (Exception err)
			{
				LogHelper.Manage("ProcessHelper.RegisterIE", err);
				return false;
			}
			finally
			{
				key.Close();
				LogHelper.End("ProcessHelper.RegisterIE");
			}  
			return true;
		}
	}
}
