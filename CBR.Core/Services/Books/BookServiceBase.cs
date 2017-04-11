using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CBR.Core.Helpers;
using CBR.Core.Models;

namespace CBR.Core.Services
{
	/// <summary>
	/// based book service class to group all common methods
	/// </summary>
    public class BookServiceBase
    {
        #region -----------------BOOK INFO-----------------

        public Book CreateBook(string filePath)
        {
            return new Book(null, filePath);
        }

		public Book CreateBookWithCover(string folder, FileInfo file)
		{
            string fileInfo = Path.Combine(folder, file.DirectoryName.Replace(file.Directory.Root.Name, "").Replace('\\', '.') + "." + file.Name + ".cbb");

            if (File.Exists(fileInfo))
            {
                return new BookInfoService().LoadBookInfo(fileInfo);
            }
            else
            {
                Book bk = new Book(fileInfo, file.FullName);
                bk.Size = file.Length;

                Thread t = new Thread(new ParameterizedThreadStart(LoadCoverThread));
                t.IsBackground = true;
                t.Priority = ThreadPriority.Highest;
                t.Start(bk);
                return bk;
            }
		}

		virtual internal void LoadCoverThread(object param)
		{
            throw new NotImplementedException();
		}

		virtual public object LoadBook(Book bk)
		{
            WorkspaceService.Instance.AddRecent(bk);

            return null;
		}

		virtual public void UnloadBook(Book bk)
		{
			bk.Pages.Clear();
		}

        #endregion

        #region -----------------BOOK-----------------

        virtual public void EditBook(Book bk)
		{
            throw new NotImplementedException();
		}

        virtual public Book SaveBook(Book bk)
        {
            throw new NotImplementedException();
        }

        public bool IsDynamic(Book bk)
        {
            try
            {
                if( bk != null )
                   return (bk.Pages.AsParallel().Count(p => p.Frames.Count != 0) != 0);
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookServiceBase:IsDynamic", err);
            }

            return false;
        }

        public void SynchronizeProperties(Book bk)
        {
            try
            {
                IDictionary<string, object> dict = (IDictionary<string, object>)bk.Dynamics;

                // add the properties from the settings if missing
                foreach( string k in WorkspaceService.Instance.Settings.Dynamics )
                {
                    if (!dict.Keys.Contains(k))
                        dict.Add(k, string.Empty);
                }

                // remove old properties that were removed from settings
                foreach (string k in dict.Keys)
                {
                    if (!WorkspaceService.Instance.Settings.Dynamics.Contains(k))
                        dict.Keys.Remove(k);
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookServiceBase:SynchronizeProperties", err);
            }
        }

        #endregion

        #region -----------------BOOKMARK-----------------

        #region -----------------for pages-----------------
        virtual public void SetMark(Book bk, Page pg)
		{
			if (bk != null && pg != null)
				bk.Bookmark = pg.FilePath;
		}

        virtual public Page GotoMark(Book bk)
		{
			return bk.Pages.First(p => p.FilePath == bk.Bookmark);
		}

        virtual public void ClearMark(Book bk)
        {
            if (bk != null)
                bk.Bookmark = string.Empty;
        }

        virtual public bool HasMark(Book bk)
        {
            if (bk != null)
                return !string.IsNullOrEmpty(bk.Bookmark);
            else
                return false;
        }
        #endregion

        #region -----------------for anything else-----------------
        virtual public void SetMark(Book bk, string content)
        {
            if (bk != null)
                bk.Bookmark = content;
        }

        virtual public string GetMark(Book bk)
        {
            return bk.Bookmark;
        }

        #endregion
#endregion

        #region -----------------PAGE NAVIGATION-----------------

        virtual public Page GotoPage(Book bk, Page currentPage, int step)
		{
			int pos = 0;

			if (currentPage != null)
				pos = bk.Pages.IndexOf(currentPage);

			if (step < 0)
			{
				return (pos == 0) ? currentPage : bk.Pages[pos - 1];
			}
			else
			{
				return (pos >= bk.Pages.Count - step) ? currentPage : bk.Pages[pos + step];
			}
		}

        virtual public bool CheckPageRange(Book bk, Page currentPage, int step)
		{
			int pos = 0;

			if (currentPage != null)
				pos = bk.Pages.IndexOf(currentPage);

			if (step < 0)
			{
				return (pos == 0) ? false : true;
			}
			else
			{
				return (pos >= bk.Pages.Count - step) ? false : true;
			}
		}

        virtual public bool CanGotoPage(Book bk, int pageNumber)
        {
            return (pageNumber >= 0 && pageNumber <= bk.Pages.Count);
        }

        virtual public Page GotoPage(Book bk, int pageNumber)
        {
            return bk.Pages[pageNumber - 1];
        }
        
        #endregion
        
        #region -----------------FRAMES-----------------

        public void GotoFrame(Book bk, ref Page currentPage, ref Zone currentFrame, int step)
        {
            currentFrame = GotoFrame(currentPage, currentFrame, step);

            //we reach the min or max, change the page
			if (currentFrame == null)
			{
				currentPage = GotoPage(bk, currentPage, step);
				GotoFrame(currentPage, currentFrame, step);
			}
        }

        internal Zone GotoFrame(Page currentPage, Zone currentFrame, int step)
        {
            //if no frames, return a page frame
            if (currentPage.Frames.Count <= 0)
            {
                if (currentFrame == null)
                    return new Zone() { Duration = 15, OrderNum = 0, X = 0, Y = 0, Type = FrameType.Page };
                else
                    return null;
            }

            //search next frame position
			if (currentFrame != null)
				return currentPage.Frames.First(p => p.OrderNum == currentFrame.OrderNum + step);
			else
				return currentPage.Frames[0];
        }

		#endregion

		#region -----------------ACCESSORIES-----------------

		public string GetUnknownCoverFileName()
		{
			return DirectoryHelper.Combine(CBRFolders.Dependencies, "unknown.png");
		}

		public void GetUnknownCover(Book bk)
		{
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
			{
				BitmapImage myImage = new BitmapImage();
				myImage.BeginInit();
				myImage.UriSource = new Uri(GetUnknownCoverFileName(), UriKind.RelativeOrAbsolute);
				myImage.CacheOption = BitmapCacheOption.OnLoad;
				myImage.DecodePixelWidth = 70;
				myImage.DecodePixelHeight = 70;
				myImage.EndInit();

				bk.Cover = myImage;
			});
		}

        virtual public void Delete(Book bk)
        {
            try
            {
                //delete the book
                File.Delete(bk.FilePath);
                //delete the bin
                File.Delete(bk.BookInfoFilePath);
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookServiceBase:Delete", err);
            }
        }

        virtual public void Protect(Book bk, bool status, string password)
        {
            if (bk != null)
            {
                if (status == false) //remove protection
                {
                    if (bk.Password != password)
                        return;
                }
                bk.Password = password;
                bk.IsSecured = status;
            }
        }

        virtual public long ManageCache(Book bk)
		{
			return 0;
        }

		virtual public bool CanManageCache()
		{
			return false;
		}
        #endregion
    }
}
