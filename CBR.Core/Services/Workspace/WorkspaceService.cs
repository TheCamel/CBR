using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CBR.Core.Helpers;
using CBR.Core.Models;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Core.Services
{
    /// <summary>
    /// Provide access to the workspace settings
    /// </summary>
    public class WorkspaceService
    {
        #region ----------------SINGLETON----------------

		public static readonly WorkspaceService Instance = new WorkspaceService();

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private WorkspaceService()
		{
		}

		#endregion

        #region ----------------PROPERTIES----------------

        private WorkspaceInfo _Settings = new WorkspaceInfo();
        public WorkspaceInfo Settings
        {
            get { return _Settings; }
            set
            {
                if (value != null && value != _Settings) _Settings = value;
            }
        }

        #endregion

        #region ----------------METHODS----------------

        public void AddRecent(Catalog catlog)
        {
			if (LogHelper.CanDebug()) 
				LogHelper.Begin("WorkspaceService.AddRecent", "Catalog : {0}", catlog.ToString());
			try
			{
				if (Settings.RecentCatalogList == null)
					Settings.RecentCatalogList = new List<RecentFileInfo>();

				Add(Path.GetDirectoryName(catlog.CatalogFilePath), Path.GetFileName(catlog.CatalogFilePath), Settings.RecentCatalogList);
			}
			catch (Exception err)
			{
				LogHelper.Manage("WorkspaceService.AddRecent", err);
			}
			LogHelper.End("WorkspaceService.AddRecent");
		}

        public void AddRecent(Book bk)
        {
			if (LogHelper.CanDebug()) 
				LogHelper.Begin("WorkspaceService.AddRecent", "Book : {0}", bk.ToString());
			try
			{
				if (Settings.RecentFileList == null)
					Settings.RecentFileList = new List<RecentFileInfo>();

				Add(Path.GetDirectoryName(bk.FilePath), Path.GetFileName(bk.FilePath), Settings.RecentFileList);
			}
			catch (Exception err)
			{
				LogHelper.Manage("WorkspaceService.AddRecent", err);
			}
			LogHelper.End("WorkspaceService.AddRecent");
        }

        internal void Add(string filePath, string fileName, List<RecentFileInfo> list)
        {
			LogHelper.Begin("WorkspaceService.Add");
			try 
			{	        
				RecentFileInfo rfi = list.Find(p => p.FileName == fileName && p.FilePath == filePath);
				if (rfi == null)
				{
					list.Add(new RecentFileInfo()
					{
						FilePath = filePath,
						FileName = fileName,
						IsPined = false,
						LastAccess = DateTime.Now
					});
					Messenger.Default.Send<List<RecentFileInfo>>(list, ViewModelBaseMessages.RecentListChanged);
				}
				else
				{
					rfi.LastAccess = DateTime.Now;
					Messenger.Default.Send<RecentFileInfo>(rfi, ViewModelBaseMessages.RecentFileChanged);
				}
				//check the max
				int execiding = list.Count - Settings.MaxRecentFile;
				if (execiding > 0)
				{
					List<RecentFileInfo> temp = new List<RecentFileInfo>();
					temp.AddRange(list.OrderBy(o => o.LastAccess).Where(p => p.IsPined == false));

					for (int i = 0; i < execiding; i++)
						list.Remove(temp[i]);

					Messenger.Default.Send<List<RecentFileInfo>>(list, ViewModelBaseMessages.RecentListChanged);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage( "WorkspaceService.Add" , err);
			}
		
			LogHelper.End("WorkspaceService.Add");
		}

        #endregion
    }
}
