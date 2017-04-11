using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;
using CBR.Core.Models;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.Core.Services
{
	/// <summary>
	/// manage all entry to the catalog class model
	/// </summary>
	public class CatalogService
	{
		#region ----------------SINGLETON----------------
		public static readonly CatalogService Instance = new CatalogService();

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
		private CatalogService()
		{
		}

		#endregion

        #region ----------------PROPERTIES----------------

        private List<Catalog> _CatalogRepository = new List<Catalog>();
        public ReadOnlyCollection<Catalog> CatalogRepository
        {
            get { return _CatalogRepository.AsReadOnly(); }
        }

        #endregion

        #region -----------------CATALOG REPOSITORY-----------------

		/// <summary>
		/// load the full catalog repository
		/// </summary>
        public void LoadRepository()
        {
            if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.LoadRepository");
            try
            {
				List<string> files = (List<string>)XmlHelper.Deserialize(DirectoryHelper.Combine(CBRFolders.Cache, "Catalogs.xml"), typeof(List<string>));
                foreach (string fl in files)
                {
                    if (File.Exists(fl))
                        OpenSimple(fl);
                }
            }
            catch (Exception err)
            {
				LogHelper.Manage("CatalogService.LoadRepository", err);
            }
            finally
            {
				LogHelper.End("CatalogService.LoadRepository");
            }
        }

		/// <summary>
		/// Save the catalog repository
		/// </summary>
        public void SaveRepository()
        {
            if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.SaveRepository");
            try
            {
                List<string> files = _CatalogRepository.Select(p => p.CatalogFilePath).ToList();
				XmlHelper.Serialize(DirectoryHelper.Combine(CBRFolders.Cache, "Catalogs.xml"), files);

				//foreach (Catalog cat in _CatalogRepository)
				//{
				//	if (cat.IsDirty)
				//		SaveSimple(cat);
				//}
            }
            catch (Exception err)
            {
				LogHelper.Manage("CatalogService.SaveRepository", err);
            }
            finally
            {
				LogHelper.End("CatalogService.SaveRepository");
            }
        }

		/// <summary>
		/// Generate a catalog cover from several books
		/// </summary>
		/// <param name="catlog"></param>
        public BitmapImage GenerateCatalogCover(Catalog catlog, bool useFile)
        {
            try
            {
				BitmapImage bmpResult;
				string file = Path.Combine(DirectoryHelper.CachePath, Path.GetFileNameWithoutExtension(catlog.CatalogFilePath) + ".png");

				if (File.Exists(file))
				{
					catlog.CoverUri = new Uri(file);
					bmpResult = new BitmapImage(catlog.CoverUri);
				}
				else
				{
					BookInfoService srv = new BookInfoService();
					List<BitmapImage> bmps = new List<BitmapImage>();
					for (int i = 0; i <= 3 && i < catlog.BookInfoFilePath.Count; i++)
					{
						bmps.Add(srv.ExtractBookCover(catlog.BookInfoFilePath[i]));
					}

					RenderTargetBitmap temp = new RenderTargetBitmap((int)150, (int)150, 96d, 96d, PixelFormats.Pbgra32);

					DrawingVisual dv = new DrawingVisual();
					using (DrawingContext ctx = dv.RenderOpen())
					{
						for (int i = 0; i < bmps.Count; i++)
						{
							ctx.DrawImage(bmps[i], new System.Windows.Rect(i * 30, i * 30, bmps[i].PixelWidth, bmps[i].PixelHeight));
						}
						ctx.Close();
					}

					temp.Render(dv);
					bmps.Clear();

					using (MemoryStream memStream = new MemoryStream())
					{
						PngBitmapEncoder encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(temp));
						encoder.Save(memStream);
						bmpResult = StreamToImage.GetImageFromStreamBug(memStream, 0);
					}

					using (FileStream filStream = new FileStream(file, FileMode.Create))
					{
						PngBitmapEncoder encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(temp));
						encoder.Save(filStream);
						catlog.CoverUri = new Uri(file);
					}
				}
				return bmpResult;
            }
            catch (Exception err)
            {
                LogHelper.Manage("CatalogService.GenerateCatalogCover", err);
				return null;
            }
        }

		/// <summary>
		/// Remove a given catalog item from repository
		/// </summary>
		/// <param name="catlog"></param>
		public void RepositoryRemove(Catalog catlog)
		{
			RepositoryProcessItemRemove(catlog);
		}

		/// <summary>
		/// delete a given catalog item from repository
		/// </summary>
		/// <param name="catlog"></param>
		public void RepositoryDelete(Catalog catlog)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.RepositoryDelete", "path:{0}", catlog.CatalogFilePath);
			try
			{
				//clean all files!!
				File.Delete(catlog.CatalogFilePath);

				foreach (string item in catlog.BookInfoFilePath)
				{
					if (_CatalogRepository.SelectMany(p => p.BookInfoFilePath).Where(q => q == catlog.CatalogFilePath).Count() < 1)
						File.Delete(item);
				}
				RepositoryProcessItemRemove(catlog);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.RepositoryDelete", err);
			}
			finally
			{
				LogHelper.End("CatalogService.RepositoryDelete");
			}
		}

		/// <summary>
		/// add a given catalog item into repository
		/// </summary>
		/// <param name="filePath"></param>
		public void RepositoryAdd(string filePath)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.RepositoryAdd", "path:{0}", filePath);
			try
			{
				Catalog catlog = OpenSimple(filePath);
				RepositoryProcessItemAdd(catlog);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.RepositoryAdd", err);
			}
			finally
			{
				LogHelper.End("CatalogService.RepositoryAdd");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="catlog"></param>
		public void RepositoryProcessItemChange(Catalog catlog)
		{
			//replace the existing catalog, or we will be desynch
			if (_CatalogRepository.Where(p => p.CatalogFilePath == catlog.CatalogFilePath).Count() == 1)
			{
				_CatalogRepository.Remove(_CatalogRepository.Single(p => p.CatalogFilePath == catlog.CatalogFilePath));
				_CatalogRepository.Add(catlog);
			}
			Messenger.Default.Send<Catalog>(catlog, ViewModelBaseMessages.CatalogListItemChanged);
		}

		/// <summary>
		/// Process the add if not exist and notify
		/// </summary>
		/// <param name="catlog"></param>
        private void RepositoryProcessItemAdd(Catalog catlog)
        {
            if (_CatalogRepository.Where(p => p.CatalogFilePath == catlog.CatalogFilePath).Count() < 1 )
            {
                _CatalogRepository.Add(catlog);
                Messenger.Default.Send<Catalog>(catlog, ViewModelBaseMessages.CatalogListItemAdded);
            }
        }

		/// <summary>
		/// process the remove and notify
		/// </summary>
		/// <param name="catlog"></param>
        private void RepositoryProcessItemRemove(Catalog catlog)
        {
            _CatalogRepository.Remove(catlog);
            Messenger.Default.Send<Catalog>(catlog, ViewModelBaseMessages.CatalogListItemRemoved);
        }

        #endregion

        #region -----------------LOAD-----------------

        public Catalog OpenSimple(string path)
        {
            Catalog catlog = null;

            if (LogHelper.CanDebug())
                LogHelper.Begin("CatalogService.OpenSimple", "path:{0}", path);
            try
            {
                //the file exist, try to load
                if (File.Exists(path))
                    catlog = (Catalog)BinaryHelper.Deserialize(path);

                //complete the load by loading the books
                if (catlog != null)
                {
                    catlog.CatalogFilePath = path;

                    RepositoryProcessItemAdd(catlog);
                    WorkspaceService.Instance.AddRecent(catlog);
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("CatalogService.OpenSimple", err);
            }
            finally
            {
                LogHelper.End("CatalogService.OpenSimple");
            }
            return catlog;
        }

        public Catalog Open(string path)
		{
			Catalog catlog = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.Open", "path:{0}", path);
			try
			{
				//the file exist, try to load
				if (File.Exists(path))
					catlog = (Catalog)BinaryHelper.Deserialize(path);

				//complete the load by loading the books
				if (catlog != null)
				{
					catlog.CatalogFilePath = path;

                    Thread t = new Thread(new ParameterizedThreadStart(LaunchLoadBookThreads));
                    t.IsBackground = true;
                    t.Priority = ThreadPriority.Lowest;
                    t.Start(catlog);

                    RepositoryProcessItemAdd(catlog);
                    WorkspaceService.Instance.AddRecent(catlog);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.Open", err);
			}
			finally
			{
				LogHelper.End("CatalogService.Open");
			}  
			return catlog;
		}

        private void LaunchLoadBookThreads(object param)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.LaunchLoadBookThreads", "catalog:{0}", param );
			try
			{
                Catalog catlog = param as Catalog;

                foreach (string filepath in catlog.BookInfoFilePath)
                {
					if (File.Exists(filepath))
					{
						// load the book info + cover
						Thread t = new Thread(new ParameterizedThreadStart(new BookInfoService().LoadBookInfo));
						t.IsBackground = true;
						t.Priority = ThreadPriority.Lowest;
						t.Start(new ThreadExchangeData() { BookPath = filepath, ThreadCatalog = catlog });
					}
                }
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.LaunchLoadBookThreads", err);
			}
			finally
			{
				LogHelper.End("CatalogService.LaunchLoadBookThreads");
			}  
        }

		#endregion

		#region -----------------SAVE-----------------

		//public void SaveSimple(Catalog catlog)
		//{
		//	if (LogHelper.CanDebug())
		//		LogHelper.Begin("CatalogService.SaveSimple", "catalog: {0}", catlog);
		//	try
		//	{
		//		//save catalog file info
		//		BinaryHelper.Serialize(catlog.CatalogFilePath, catlog);
		//	}
		//	catch (Exception err)
		//	{
		//		LogHelper.Manage("CatalogService.SaveSimple", err);
		//	}
		//	finally
		//	{
		//		LogHelper.End("CatalogService.SaveSimple");
		//	}
		//}

		public void SaveAs(Catalog catlog, string newFileName)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.SaveAs", "catlog:{0}, newFileName:{1} ", catlog, newFileName);
			try
			{
				catlog.CatalogFilePath = newFileName;
				Save(catlog);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.SaveAs", err);
			}
			finally
			{
				LogHelper.End("CatalogService.SaveAs");
			}  
		}

		public void Save(Catalog catlog)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.Save", "catalog: {0}", catlog);
			try
			{
				if (IsDirty(catlog))
				{
					//remove the book without covers
					//RemoveDirtyBooks(CurrentCatalog);

					//save catalog file info
					BinaryHelper.Serialize(catlog.CatalogFilePath, catlog);

					//complete the save by saving the books
					if (catlog.BookInfoFilePath.Count > 0)
					{
						foreach (Book bk in catlog.Books.Where(bkk => bkk.IsDirty))
						{
                            Thread t = new Thread(new ParameterizedThreadStart(new BookInfoService().SaveBookInfo));
							t.IsBackground = true;
							t.Priority = ThreadPriority.BelowNormal;
							t.Start(bk);
						}
					}

                    RepositoryProcessItemAdd(catlog);
                    WorkspaceService.Instance.AddRecent(catlog);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.Save", err);
			}
			finally
			{
				LogHelper.End("CatalogService.Save");
			}  
		}

		#endregion

		#region -----------------DIRECTORY PARSING-----------------

		internal void ParseDirectoryRecursive(object param)
		{
			ThreadExchangeData ted = param as ThreadExchangeData;

			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.ParseDirectoryRecursive");
			try
			{
				DirectoryInfo directory = new DirectoryInfo((string)ted.BookPath);
				if (!directory.Exists)
				{
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
					{
						string msg = CultureManager.Instance.GetLocalization("ByCode", "Warning.CatalogPath", "Catalog path does not exist! Please check the options box");
						MessageBox.Show(msg);
					});
					return;
				}
				foreach (FileInfo file in directory.GetFiles("*.*"))
				{
                    if (DocumentFactory.Instance.FindBookFilterByExtWithModel(file.Extension) != null)
					{
						if (!BookExistInCollection(ted.ThreadCatalog, file.FullName))
						{
							CreateBookInfo(ted, file);
						}
					}
				}
				foreach (DirectoryInfo dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
				{
					ted.BookPath = dir.FullName;
					ParseDirectoryRecursive(ted);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.ParseDirectoryRecursive", err);
			}
			finally
			{
				LogHelper.End("CatalogService.ParseDirectoryRecursive");
			}  
		}

		/// <summary>
		/// Find the service and create the book info and cover image
		/// </summary>
		/// <param name="ted"></param>
		/// <param name="file"></param>
		internal void CreateBookInfo(ThreadExchangeData ted, FileInfo file)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.CreateBookInfo");
			try
			{
				Book bkk = DocumentFactory.Instance.GetService(file.FullName).CreateBookWithCover(DirectoryHelper.BookInfoPath, file);

				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
				{
					ted.ThreadCatalog.Books.Add(bkk);
				});
				ted.ThreadCatalog.BookInfoFilePath.Add(bkk.BookInfoFilePath);
				ted.ThreadCatalog.IsDirty = true;
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.CreateBookInfo", err);
			}
			finally
			{
				LogHelper.End("CatalogService.CreateBookInfo");
			}  
		}

		/// <summary>
		/// Search for a book filename
		/// </summary>
		/// <param name="catlog"></param>
		/// <param name="filepath"></param>
		/// <returns></returns>
		internal bool BookExistInCollection(Catalog catlog, string filepath)
		{
            try
            {
                return catlog.Books.Count(p => p.FilePath == filepath) == 0 ? false : true;
            }
            catch
            {
                return false;
            }
		}

		#endregion

		#region -----------------REFRESH-----------------

		/// <summary>
		/// Resfresh a catalog based on is folder
		/// </summary>
		/// <param name="catlog"></param>
		public void Refresh(Catalog catlog)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.Refresh", "catalog: {0}", catlog);
			try
			{
				Thread t = new Thread(new ParameterizedThreadStart(RefreshThread));
				t.IsBackground = true;
				t.Priority = ThreadPriority.Highest;
                t.Start(new ThreadExchangeData() { BookPath = catlog.BookFolder, ThreadCatalog = catlog });

			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.Refresh", err);
			}
			finally
			{
				LogHelper.End("CatalogService.Refresh");
			}  
		}

		/// <summary>
		/// Refresh thread method
		/// </summary>
		/// <param name="param"></param>
		internal void RefreshThread(object param)
		{
			ThreadExchangeData ted = param as ThreadExchangeData;

			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.RefreshThread");
			try
			{
				//first, remove unfounded books
				List<Book> temp = new List<Book>();
				foreach (Book bk in ted.ThreadCatalog.Books)
				{
					if (!File.Exists(bk.FilePath))
						temp.Add(bk);
				}

				foreach (Book bk in temp)
				{
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
					{
                        ted.ThreadCatalog.BookInfoFilePath.Remove(bk.BookInfoFilePath);
                        ted.ThreadCatalog.Books.Remove(bk);
                        ted.ThreadCatalog.IsDirty = true;
                    });
					ted.ThreadCatalog.IsDirty = true;
				}

				//then add the new ones
				ParseDirectoryRecursive(ted);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.RefreshThread", err);
			}
			finally
			{
				LogHelper.End("CatalogService.RefreshThread");
			}  
		}
		#endregion

		#region -----------------BOOKS-----------------

        public void AddBook(Catalog catlog, string bookFile)
        {
            ThreadExchangeData ted = new ThreadExchangeData();

			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.AddBook", "catalog: {0}, bookFile: {1}", catlog, bookFile);
			try
			{
                FileInfo fi = new FileInfo(bookFile);
                if (DocumentFactory.Instance.FindBookFilterByExtWithModel(fi.Extension) != null)
                {
                    if (!BookExistInCollection(catlog, fi.FullName))
                    {
						Book bkk = DocumentFactory.Instance.GetService(fi.FullName).CreateBookWithCover(DirectoryHelper.BookInfoPath, fi);
                        catlog.Books.Add(bkk);
                        catlog.BookInfoFilePath.Add(bkk.BookInfoFilePath);
                        catlog.IsDirty = true;

						RepositoryProcessItemChange(catlog);
                    }
                }
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.AddBook", err);
			}
			finally
			{
				LogHelper.End("CatalogService.AddBook");
			}  
        }

		public void RemoveBook(Catalog catlog, Book book)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.RemoveBook", "catalog: {0}, bookFile: {1}", catlog, book);
			try
			{
				catlog.BookInfoFilePath.Remove(book.BookInfoFilePath);
				catlog.Books.Remove(book);
				catlog.IsDirty = true;

				RepositoryProcessItemChange(catlog);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.RemoveBook", err);
			}
			finally
			{
				LogHelper.End("CatalogService.RemoveBook");
			}
		}

		public void DeleteBook(Catalog catlog, Book book)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("CatalogService.DeleteBook", "catalog: {0}, bookFile: {1}", catlog, book);
			try
			{
				//remove
				catlog.BookInfoFilePath.Remove(book.BookInfoFilePath);
				catlog.Books.Remove(book);
				catlog.IsDirty = true;

				//file deletion
				DocumentFactory.Instance.GetService(book).Delete(book);

				RepositoryProcessItemChange(catlog);
			}
			catch (Exception err)
			{
				LogHelper.Manage("CatalogService.DeleteBook", err);
			}
			finally
			{
				LogHelper.End("CatalogService.DeleteBook");
			}
		}

		//public void Fill(Catalog cat, string bookPath)
		//{
		//    ThreadExchangeData ted = new ThreadExchangeData();

		//    try
		//    {
		//        cat.BookFolder = bookPath;

		//        ted.ThreadCatalog = cat;
		//        ted.BookPath = cat.BookFolder;

		//        Thread t = new Thread(new ParameterizedThreadStart(ParseDirectoryRecursive));
		//        t.IsBackground = true;
		//        t.Priority = ThreadPriority.BelowNormal;
		//        t.Start(ted);
		//    }
		//    catch (Exception err)
		//    {
		//        LogHelper.Manage("CatalogService:Create", err);
		//    }
		//}

		/// <summary>
		/// Is the catalog subject to  save ?
		/// </summary>
		/// <param name="catlog"></param>
		/// <returns></returns>
        public bool IsDirty(Catalog catlog)
        {
            try
            {
                return (catlog.IsDirty || catlog.Books.AsParallel().Count(p => p.IsDirty == true) != 0 );
            }
            catch (Exception err)
            {
				LogHelper.Manage("CatalogService:IsDirty", err);
            }

            return false;
        }

		/// <summary>
		/// is the catalog dynamic ?
		/// </summary>
		/// <param name="catlog"></param>
		/// <returns></returns>
        public bool IsDynamic(Catalog catlog)
        {
            try
            {
                return (catlog.Books.AsParallel().Count(b => b.Pages.AsParallel().Count( p => p.Frames.Count !=0 ) != 0) != 0);
            }
            catch (Exception err)
            {
                LogHelper.Manage("CatalogService:IsDynamic", err);
            }

            return false;
        }

        #endregion
	}
}
