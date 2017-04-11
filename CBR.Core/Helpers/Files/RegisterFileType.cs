﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace CBR.Core.Helpers
{
	public static class RegisterFiletype
	{
		public static void RegisterToMe(string extension, string description, string iconPath)
		{
			string mainExe = Assembly.GetEntryAssembly().Location;

            Register(extension,
                             description,
			                 '"' + Path.GetFullPath(mainExe) + '"' + " \"%1\"",
                             Path.GetFullPath(iconPath));
		}
		
		public static bool IsRegisteredToMe(string extension)
		{
			string openCommand = GetOpenCommand(extension);
			
			if (string.IsNullOrEmpty(openCommand))
				return false;
			
			string mainExe = Assembly.GetEntryAssembly().Location;
			return openCommand.StartsWith(mainExe) || openCommand.StartsWith('"' + mainExe);
		}
		
		const string explorerFileExts = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts";
		
		public static bool IsRegistered(string extension)
		{
			try {
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("." + extension)) {
					if (key != null)
						return true;
				}
			} catch (System.Security.SecurityException) {
				// registry access might be denied
			}
			try {
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey(explorerFileExts + "\\." + extension)) {
					if (key != null)
						return true;
				}
			} catch (System.Security.SecurityException) {
				// registry access might be denied
			}
			return false;
		}
		
		static string GetOpenCommand(string extension)
		{
			try {
				string clsKeyName = null;
				using (RegistryKey extKey = Registry.CurrentUser.OpenSubKey(explorerFileExts + "\\." + extension)) {
					if (extKey != null)
						clsKeyName = (string)extKey.GetValue("Progid", "");
				}
				if (string.IsNullOrEmpty(clsKeyName)) {
					using (RegistryKey extKey = Registry.ClassesRoot.OpenSubKey("." + extension)) {
						if (extKey != null)
							clsKeyName = (string)extKey.GetValue("", "");
						else
							return null;
					}
				}
				using (RegistryKey cmdKey = Registry.ClassesRoot.OpenSubKey(clsKeyName + "\\shell\\open\\command")) {
					if (cmdKey != null)
						return (string)cmdKey.GetValue("", "");
					else
						return null;
				}
			} catch (System.Security.SecurityException) {
				// registry access might be denied
				return null;
			}
		}
		
		public static void Register(string extension, string description, string command, string icon)
		{
			try {
				Register(Registry.ClassesRoot, extension, description, command, icon);
			} catch (UnauthorizedAccessException) {
				try {
					Register(Registry.CurrentUser.CreateSubKey("Software\\Classes"), extension, description, command, icon);
				} catch {}
			}
			NotifyShellAfterChanges();
		}
		
		static void Register(RegistryKey rootKey, string extension, string description, string command, string icon)
		{
			RegistryKey extKey, clsKey, openKey;
			extKey = rootKey.CreateSubKey("." + extension);
			// save previous association
			string prev = (string)extKey.GetValue("", "");
			if (prev != "" && prev != ("SD." + extension + "file")) {
				extKey.SetValue("PreSD", extKey.GetValue(""));
			}
			extKey.SetValue("", "SD." + extension + "file");
			extKey.Close();
			
			try {
				extKey = Registry.CurrentUser.OpenSubKey(explorerFileExts + "\\." + extension, true);
				if (extKey != null) {
					extKey.DeleteValue("Progid");
					extKey.Close();
				}
			} catch {}
			
			clsKey = rootKey.CreateSubKey("SD." + extension + "file");
			
			
			clsKey.SetValue("", description);
			clsKey.CreateSubKey("DefaultIcon").SetValue("", '"' + icon + '"');
			openKey = clsKey.CreateSubKey("shell\\open\\command");
			openKey.SetValue("", command);
			openKey.Close();
			clsKey.Close();
		}
		
		public static void UnRegister(string extension)
		{
			UnRegister(extension, Registry.ClassesRoot);
			try
            {
				UnRegister(extension, Registry.CurrentUser.CreateSubKey("Software\\Classes"));
			}
            catch {} // catch CreateSubKey(Software\Classes)-exceptions
			
            NotifyShellAfterChanges();
		}
		
		static void UnRegister(string extension, RegistryKey root)
		{
			try {
				root.DeleteSubKeyTree("SD." + extension + "file");
			} catch {}
			try {
				RegistryKey extKey;
				extKey = root.OpenSubKey("." + extension, true);
				
				// if no association return
				if (extKey == null) return;
				// if other association return too
				if ((string)extKey.GetValue("", "") != ("SD." + extension + "file")) return;
				
				// restore previous association
				string prev = (string)extKey.GetValue("PreSD", "");
				if(prev != "") {
					extKey.SetValue("", prev);
				}
				extKey.Close();
				if (prev != null) {
					root.DeleteSubKeyTree("." + extension);
				}
			} catch {}
		}
		
		[System.Runtime.InteropServices.DllImport("shell32.dll")]
		static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
		
		/// <summary>
		/// Notify Windows explorer that shortcut icons have changed.
		/// </summary>
		static void NotifyShellAfterChanges()
		{
			const int SHCNE_ASSOCCHANGED = 0x08000000;
			const int SHCNF_IDLIST       = 0x0;
			
			SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
		}
	}
}
