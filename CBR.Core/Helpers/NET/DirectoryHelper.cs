using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using CBR.Core.Helpers.Localization;
using System.Security.AccessControl;

namespace CBR.Core.Helpers
{
	public enum CBRFolders
	{
		User, Language, Cache, Temp, BookInfo, Dependencies

	}

    public class DirectoryHelper
    {
		#region ----------------USER's file (writeable)----------------

		static private string _userPath;
		static public string UserPath
		{
			get
			{
				if (string.IsNullOrEmpty(_userPath))
					_userPath = System.IO.Path.Combine(
						System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Comic Book Reader" );

				return _userPath;
			}
		}

		static private string _languagePath;
		static public string LanguagePath
		{
			get
			{
				if (string.IsNullOrEmpty(_languagePath))
					_languagePath = System.IO.Path.Combine(UserPath, "Languages");

				if (!Directory.Exists(_languagePath))
					Directory.CreateDirectory(_languagePath);

				return _languagePath;
			}
		}

		static private string _cachePath;
		static public string CachePath
		{
			get
			{
				if (string.IsNullOrEmpty(_cachePath))
				{
					_cachePath = System.IO.Path.Combine(UserPath, "Cache");

					if (!Directory.Exists(_cachePath))
						Directory.CreateDirectory(_cachePath);
				}
				return _cachePath;
			}
		}

		static private string _tempPath;
		static public string TempPath
		{
			get
			{
				if (string.IsNullOrEmpty(_tempPath))
				{
					_tempPath = System.IO.Path.Combine(UserPath, "Temp");

					if (!Directory.Exists(_tempPath))
						Directory.CreateDirectory(_tempPath);
				}
				return _tempPath;
			}
		}

		static public string CreateTempGuid()
		{
			string tempFolder = Path.Combine(TempPath, Guid.NewGuid().ToString());

			try
			{
				if (!Directory.Exists(tempFolder))
					Directory.CreateDirectory(tempFolder);
			}
			catch (Exception err)
			{
				LogHelper.Manage("DirectoryHelper:CreateTempGuid", err);
			}

			return tempFolder;
		}

		static public void Cleanup()
		{
			try
			{
				if (Directory.Exists(TempPath))
					Directory.Delete(TempPath, true);
			}
			catch (Exception err)
			{
				LogHelper.Manage("DirectoryHelper:Cleanup", err);
			}
		}

		static private string _bookInfoPath;
		static public string BookInfoPath
		{
			get
			{
				if (string.IsNullOrEmpty(_bookInfoPath))
				{
					_bookInfoPath = System.IO.Path.Combine(UserPath, "BookInfos");

					if (!Directory.Exists(_bookInfoPath))
						Directory.CreateDirectory(_bookInfoPath);
				}
				return _bookInfoPath;
			}
		}

		#endregion

		#region ----------------CBR's file (not writeable)----------------

		static private string _dependenciesPath;
		static protected string DependenciesPath
		{
			get
			{
				if (string.IsNullOrEmpty(_dependenciesPath))
					_dependenciesPath = System.IO.Path.Combine(ApplicationPath, "Dependencies");

				return _dependenciesPath;
			}
		}

		static private string _applicationPath;
		static protected string ApplicationPath
		{
			get
			{
				if (string.IsNullOrEmpty(_applicationPath))
					_applicationPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

				return _applicationPath;
			}
		}

		#endregion

		#region ----------------Utilities----------------

		static public void Check(string folderPath)
		{
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
		}

		static public List<LogicalDiskInfo> GetDrives()
		{
			return DriveInfo.GetDrives().Select(p => new LogicalDiskInfo()
			{
				Caption = p.Name,
				DriveFormat = p.DriveFormat,
				DriveType = p.DriveType,
				Name = p.Name,
				VolumeLabel = p.VolumeLabel,
				TotalSize = p.TotalSize,
				AvailableFreeSpace = p.AvailableFreeSpace,
				Path = p.RootDirectory.FullName
			}).ToList();
		}

		static public bool CheckAccess(string folder)
		{
			bool isWriteAccess = false;
			try
			{
				AuthorizationRuleCollection collection = Directory.GetAccessControl(folder).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
				foreach (FileSystemAccessRule rule in collection)
				{
					if (rule.AccessControlType == AccessControlType.Allow)
					{
						isWriteAccess = true;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				isWriteAccess = false;
			}

			return isWriteAccess;
		}

		#endregion

		//static public string Combine(string folder)
		//{
		//	return Path.Combine(DirectoryHelper.ApplicationPath, folder);
		//}

		static public string Combine(CBRFolders intern, string element)
		{
			switch (intern)
			{
				case CBRFolders.BookInfo: return Path.Combine(BookInfoPath, element);
				case CBRFolders.Cache: return Path.Combine(CachePath, element);
				case CBRFolders.Language: return Path.Combine(LanguagePath, element);
				case CBRFolders.Temp: return Path.Combine(TempPath, element);
				case CBRFolders.User: return Path.Combine(UserPath, element);
				case CBRFolders.Dependencies: return Path.Combine(DependenciesPath, element);
				default: return string.Empty;
			}
		}
		
    }
}
