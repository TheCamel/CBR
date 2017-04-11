using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Models;
using CBR.Core.Helpers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;
using SevenZip;
using CBR.Core.Files;
using System.Text.RegularExpressions;

namespace CBR.Core.Services
{
	/// <summary>
	/// service class that manage the comics cbr/z based books
	/// </summary>
	public class BookService : BookServiceBase
	{
		/// <summary>
		/// thread method to extract from in memory zip file a content
		/// </summary>
		/// <param name="param"></param>
		override internal void LoadCoverThread(object param)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.LoadCoverThread");

			SevenZipExtractor temp = null;
			Book bk = param as Book;
			try
			{
				temp = ZipHelper.Instance.GetExtractor(bk.FilePath);
				bk.Size = temp.PackedSize;
				bk.PageCount = temp.ArchiveFileData.Count(p => !p.IsDirectory);

				foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
				{
					if (!fil.IsDirectory && DocumentFactory.Instance.ImageExtension.Contains(Path.GetExtension(fil.FileName).ToUpper()))
					{
						using (MemoryStream stream = new MemoryStream())
						{
							temp.ExtractFile(fil.FileName, stream);

							using (MemoryStream stream2 = new MemoryStream())
							{
								stream.WriteTo(stream2);
								stream.Flush();
								stream.Close();
								stream2.Position = 0;

								Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
								{
									BitmapImage myImage = new BitmapImage();
									myImage.BeginInit();
									myImage.StreamSource = stream2;
									myImage.CacheOption = BitmapCacheOption.OnLoad;
									myImage.DecodePixelWidth = 70;
									myImage.EndInit();

									bk.Cover = myImage;
								});

								stream2.Flush();
								stream2.Close();
							}
						}
						return;
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.LoadCoverThread", err);
			}
			finally
			{
				ZipHelper.Instance.ReleaseExtractor(temp);
				LogHelper.End("BookService.LoadCoverThread");
			}  
		}

        static private Regex _ExtensionRegex = new Regex("\\d+",
                        RegexOptions.IgnoreCase
                        | RegexOptions.Multiline
                        | RegexOptions.CultureInvariant
                        | RegexOptions.IgnorePatternWhitespace
                        | RegexOptions.Compiled
                        );

        private int CheckFileNames(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            MatchCollection ms = _ExtensionRegex.Matches(fi.Name);
            Match m = ms[ms.Count - 1];
            return Convert.ToInt32(m.Value);
        }

		override public object LoadBook(Book bk)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.LoadBook");

			//allready loaded!
			if (bk.Pages.Count != 0)
				return bk.Pages[0];

			SevenZipExtractor temp = null;
			try
			{
				base.LoadBook(bk);

				temp = ZipHelper.Instance.GetExtractor(bk.FilePath);
				bk.Size = temp.PackedSize;

				foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
				{
					if (!fil.IsDirectory)
					{
						if (DocumentFactory.Instance.ImageExtension.Contains(Path.GetExtension(fil.FileName).ToUpper()))
						{
							Page item = new Page(bk, fil.FileName, bk.Pages.Count);
							//take order from file name
							item.Index = CheckFileNames(fil.FileName);
							bk.Pages.Add(item);
						}
					}
				}

				bk.PageCount = bk.Pages.Count;
				bk.Pages = bk.Pages.OrderBy(p => p.Index).ToList<Page>();

				//reorder index, sometimes file are not good
				int counter = 0;
				foreach (Page pg in bk.Pages)
					pg.Index = counter++;

				foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
				{
					if (!fil.IsDirectory && fil.FileName.Contains(".dynamics.xml"))
					{
						Page pg = bk.Pages.Find(p => p.FileName == fil.FileName.Replace(".dynamics.xml", ""));
						if (pg != null)
						{
							MemoryStream stream = new MemoryStream();
							temp.ExtractFile(fil.FileName, stream);

							MemoryStream stream2 = new MemoryStream();
							stream.WriteTo(stream2);
							stream.Flush();
							stream.Close();
							stream2.Position = 0;

							List<Zone> frames = (List<Zone>)XmlHelper.Deserialize(stream2, typeof(List<Zone>));

							stream2.Close();

							if (frames != null)
								pg.Frames = new System.Collections.ObjectModel.ObservableCollection<Zone>(frames);
						}
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.LoadBook", err);
			}
			finally
			{
				ZipHelper.Instance.ReleaseExtractor(temp);
				LogHelper.End("BookService.LoadBook");
			}  
			return bk.Pages[0];
		}

        #region -----------------BOOK MANAGEMENT-----------------

		/// <summary>
		/// override to edit this type of book
		/// </summary>
		/// <param name="bk"></param>
        override public void EditBook(Book bk)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.EditBook");

			SevenZipExtractor temp = null;
			try
			{
				temp = ZipHelper.Instance.GetExtractor(bk.FilePath);

				foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
				{
					if (!fil.IsDirectory)
					{
						if (fil.FileName.Contains(".dynamics.xml"))
						{
							Page item = new Page(bk, fil.FileName, bk.Pages.Count);
							bk.Pages.Add(item);
						}
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.EditBook", err);
			}
			finally
			{
				ZipHelper.Instance.ReleaseExtractor(temp);
				LogHelper.End("BookService.EditBook");
			}  
		}

		/// <summary>
		/// override to save this type of book
		/// </summary>
		/// <param name="bk"></param>
		/// <returns></returns>
        override public Book SaveBook(Book bk)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.SaveBook");
			try
			{
				//first time we save as dynamic book
				if (IsDynamic(bk) && Path.GetExtension(bk.FilePath) != ".dcb")
				{
					string newFile = FirstSaveDynamicBook(bk);
					UnloadBook(bk);
					return CreateBook(newFile);
				}
				else //only update the frame files
				{
					UpdateDynamicBook(bk);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.SaveBook", err);
			}
			finally
			{
				LogHelper.End("BookService.SaveBook");
			}  
            
            return null;
        }

        internal string FirstSaveDynamicBook(Book bk)
        {
            try
            {
                // create a temp folder
				string tempFolder = DirectoryHelper.Combine(CBRFolders.Temp, Path.GetFileNameWithoutExtension(bk.FilePath));
                DirectoryHelper.Check(tempFolder);

                //extract the book content
                ExtractBook(bk, tempFolder);

                //serialize all frames even empty to create the files in the zip
                foreach (Page pg in bk.Pages)
                {
                    XmlHelper.Serialize(Path.Combine(tempFolder, pg.FileName + ".dynamics.xml"), pg.Frames.ToList());
                }

                // create a new file by compressing all temp folder content
                string newComic = bk.FilePath.Replace(Path.GetExtension(bk.FilePath), ".dcb");

                SevenZip.SevenZipCompressor cp = new SevenZip.SevenZipCompressor();
                cp.ArchiveFormat = SevenZip.OutArchiveFormat.Zip;

                string[] outputFiles = new DirectoryInfo(tempFolder).GetFiles("*.*").Select(p => p.FullName).ToArray();

                using (FileStream fs = new FileStream(newComic, FileMode.Create))
                {
                    cp.CompressFiles(fs, outputFiles);
                }

                //delete the temp folder
                Directory.Delete(tempFolder, true);

                return newComic;
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookService:FirstSaveDynamicBook", err);
            }
            return null;
        }

        internal void UpdateDynamicBook(Book bk)
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookService:UpdateDynamicBook", err);
            }
        }

		//internal Dictionary<int, string> GetBookContentList(Book bk)
		//{
		//    if (LogHelper.CanDebug())
		//        LogHelper.Begin("BookService.GetBookContentList"); 

		//    SevenZipExtractor temp = null;
		//    Dictionary<int, string> content = new Dictionary<int, string>();
		//    try
		//    {
		//        temp = ZipHelper.Instance.GetExtractor(bk.FilePath);

		//        foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
		//        {
		//            if (!fil.IsDirectory)
		//            {
		//                content.Add(content.Count, fil.FileName);
		//            }
		//        }
		//    }
		//    catch (Exception err)
		//    {
		//        LogHelper.Manage("BookService:GetBookContentList", err);
		//    }
		//    finally
		//    {
		//        ZipHelper.Instance.ReleaseExtractor(temp);
		//        LogHelper.End("BookService.GetBookContentList");
		//    }

		//    return content;
		//}

		/// <summary>
		/// extract a book to a given folder
		/// </summary>
		/// <param name="bk"></param>
		/// <param name="outputFolder"></param>
        internal void ExtractBook(Book bk, string outputFolder)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.ExtractBook"); 

            SevenZipExtractor temp = null;
            try
            {
                temp = ZipHelper.Instance.GetExtractor(bk.FilePath);
                temp.PreserveDirectoryStructure = false;
                temp.ExtractArchive(outputFolder);
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookService:ExtractBook", err);
            }
            finally
            {
                ZipHelper.Instance.ReleaseExtractor(temp);
				LogHelper.End("BookService.ExtractBook");
            }
        }
       
		#endregion

		/// <summary>
		/// extract an image from a zipped content for the given filename
		/// </summary>
		/// <param name="zipFilePath"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public BitmapImage GetImageFromStream(string zipFilePath, string fileName)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.GetImageFromStream"); 
			
			SevenZipExtractor temp = null;
            BitmapImage result = null;
			try
			{
                temp = ZipHelper.Instance.GetExtractor(zipFilePath);

			    MemoryStream stream = new MemoryStream();
                temp.ExtractFile(fileName, stream);

			    result = StreamToImage.GetImageFromStreamBug(stream, 0);
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookService:GetImageFromStream", err);
            }
            finally
            {
                ZipHelper.Instance.ReleaseExtractor(temp);
				LogHelper.End("BookService.GetImageFromStream");
            }
            return result;
        }

		#region -----------------page navigation management-----------------

		/// <summary>
		/// Goto a page for the given step
		/// </summary>
		/// <param name="bk"></param>
		/// <param name="currentPage"></param>
		/// <param name="step"></param>
		/// <returns></returns>
		override public Page GotoPage(Book bk, Page currentPage, int step)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.GotoPage");
			try
			{
				Page result = base.GotoPage(bk, currentPage, step);

				if (step == 1)
				{
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
					{
						PreparePageCache(result);
					});
				}

				return result;
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.GotoPage", err);
				return null;
			}
			finally
			{
				LogHelper.End("BookService.GotoPage");
			}  
		}

		/// <summary>
		/// Check if can move to
		/// </summary>
		/// <param name="bk"></param>
		/// <param name="currentPage"></param>
		/// <param name="step"></param>
		/// <returns></returns>
		public bool CheckPageRange(Book bk, Page currentPage, int step)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.CheckPageRange");
			try
			{
				int pos = 0;

				if (currentPage != null)
					pos = bk.Pages.IndexOf(currentPage);

				if (step == -1)
				{
					return (pos == 0) ? false : true;
				}
				else
				{
					return (pos >= bk.Pages.Count - 1) ? false : true;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.CheckPageRange", err);
				return false;
			}
			finally
			{
				LogHelper.End("BookService.CheckPageRange");
			}  
		}

		/// <summary>
		/// move to a frame with the current step
		/// </summary>
		/// <param name="bk"></param>
		/// <param name="currentPage"></param>
		/// <param name="currentFrame"></param>
		/// <param name="step"></param>
        public void GotoFrame(Book bk, ref Page currentPage, ref Zone currentFrame, int step)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.GotoFrame");
			try
			{
				currentFrame = GotoFrame(currentPage, currentFrame, step);

				//we reach the min or max, change the page
				if (currentFrame == null)
					currentPage = GotoPage(bk, currentPage, step);

				GotoFrame(currentPage, currentFrame, step);
			}
			catch (Exception err)
			{
				
				LogHelper.Manage("BookService.GotoFrame", err);
			}
			finally
			{
				LogHelper.End("BookService.GotoFrame");
			}  
        }

		/// <summary>
		/// move to a frame with the current step
		/// </summary>
		/// <param name="currentPage"></param>
		/// <param name="currentFrame"></param>
		/// <param name="step"></param>
		/// <returns></returns>
        internal Zone GotoFrame(Page currentPage, Zone currentFrame, int step)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.GotoFrame");
			try
			{
				//if no frames, return a page frame
				if (currentPage.Frames.Count <= 0)
				{
					if (currentFrame == null)
						return new Zone() { Duration = 15, OrderNum = 0, X = 0, Y = 0, Type = FrameType.Page };
					else
						return null;
				}

				//search current frame position
				int posFrame = 0;
				if (currentFrame != null)
					posFrame = currentPage.Frames.IndexOf(currentFrame);

				//we go back, manage the min
				if (step == -1)
				{
					return (posFrame == 0) ? null : currentPage.Frames[posFrame - 1];
				}
				else //we go forward, manage the max
				{
					return (posFrame >= currentPage.Frames.Count - 1) ? null : currentPage.Frames[posFrame + 1];
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.GotoFrame", err);
				return null;
			}
			finally
			{
				LogHelper.End("BookService.GotoFrame");
			}  
        }

		internal Page GetNextPage(Page currentPage)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.GetNextPage");
			try
			{
				if (currentPage != null)
				{
					Book parent = currentPage.Parent;

					if (parent != null)
					{
						int next = parent.Pages.IndexOf(currentPage);
						if (next >= parent.Pages.Count - 1)
							return null;
						else
						{
							next = next + 1;
							return parent.Pages[next];
						}
					}
					else return null;
				}
				else return null;
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.GetNextPage", err);
				return null;
			}
			finally
			{
				LogHelper.End("BookService.GetNextPage");
			}
		}

		#endregion

		#region -----------------page navigation management-----------------

		/// <summary>
		/// manage cache and load bitmaps
		/// </summary>
		/// <param name="bk"></param>
		/// <returns></returns>
		override public long ManageCache(Book bk)
		{

			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.ManageCache");
			try
			{
				int counter = bk.Pages.Where(p => p.ImageExist == true).Count();

				//select and delete if more than cache count
				IEnumerable<Page> filter = bk.Pages
							.Where(p => p.ImageExist == true)
							.OrderBy(p => p.Index).Take(counter - WorkspaceService.Instance.Settings.ImageCacheCount);

				foreach (Page item in filter)
					item.Image = null;

				//select and delete if over cache duration
				filter = bk.Pages
							.Where(p => p.ImageExist == true &&
									p.ImageLastAcces < DateTime.Now.AddSeconds(-WorkspaceService.Instance.Settings.ImageCacheDuration))
							.OrderBy(p => p.Index);

				foreach (Page item in filter)
					item.Image = null;

				//return total size in cache
				return bk.Pages.Where(p => p.ImageExist == true).Sum(f => f.Image.StreamSource.Length);
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.ManageCache", err);
				return 0;
			}
			finally
			{
				LogHelper.End("BookService.ManageCache");
			}  
		}

		/// <summary>
		/// override to true
		/// </summary>
		/// <returns></returns>
		override public bool CanManageCache()
		{
			return true;
		}

		/// <summary>
		/// prepare cache for next 3 pages
		/// </summary>
		/// <param name="threadParam"></param>
        internal void PreparePageCache(object threadParam)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookService.PreparePageCache");
			try
			{
				Page currentPage = threadParam as Page;

				if (currentPage != null)
				{
					//load the 3 next pages after current page
					BitmapImage tmpImage = null;

					Page tmpPage = GetNextPage(currentPage);
					if (tmpPage != null) tmpImage = tmpPage.Image;

					tmpPage = GetNextPage(tmpPage);
					if (tmpPage != null) tmpImage = tmpPage.Image;

					tmpPage = GetNextPage(tmpPage);
					if (tmpPage != null) tmpImage = tmpPage.Image;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookService.PreparePageCache", err);
			}
			finally
			{
				LogHelper.End("BookService.PreparePageCache");
			}
		}

		#endregion
	}
}
