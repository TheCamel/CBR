using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CBR.Core.Formats.ePUB;
using CBR.Core.Helpers;
using CBR.Core.Models;
using SevenZip;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Xps.Packaging;
using System.Windows.Documents;
using CBR.Core.Helpers.Files.HTML;

namespace CBR.Core.Services
{
	/// <summary>
	/// manage the epub formatted documents
	/// </summary>
	public class ePUBBookService : BookServiceBase
	{
		/// <summary>
		/// override to load books
		/// </summary>
		/// <param name="bk"></param>
		/// <returns></returns>
		override public object LoadBook(Book bk)
		{
			SevenZipExtractor temp = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ePUBBookService.LoadBook");
			try
			{
				base.LoadBook(bk);

				temp = ZipHelper.Instance.GetExtractor(bk.FilePath);

				bk.Size = temp.PackedSize;
				bk.PageCount = temp.ArchiveFileNames.Count;

				string outputFolder = DirectoryHelper.CreateTempGuid();
				temp.ExtractArchive(outputFolder);

				bk.Tag = new ePUBManager().ParseExtracted(bk.FilePath, outputFolder);
			}
			catch (Exception err)
			{	
				LogHelper.Manage("ePUBBookService.LoadBook", err);
			}
			finally
			{
				ZipHelper.Instance.ReleaseExtractor(temp);

				LogHelper.End("ePUBBookService.LoadBook");
			}  
			return null;
		}

		/// <summary>
		/// override to load covers
		/// </summary>
		/// <param name="param"></param>
		override internal void LoadCoverThread(object param)
		{
			Book bk = param as Book;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ePUBBookService.LoadCoverThread");
			try
			{
				// all ready unzipped ?
				if (bk != null && bk.Tag != null)
				{
					ePUB docPUB = bk.Tag as ePUB;
					string coverFile = docPUB.GetCoverFile();

					Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
					{
						BitmapImage myImage = new BitmapImage();
						myImage.BeginInit();
						myImage.UriSource = new Uri(coverFile);
						myImage.CacheOption = BitmapCacheOption.OnLoad;
						myImage.DecodePixelWidth = 70;
						myImage.EndInit();

						bk.Cover = myImage;
					});
				}
				else
				{
					ePUB docPUB = new ePUBManager().ParseFileForCoverOnly(bk.FilePath);
					string coverFile = docPUB.GetCoverFile();

					if (coverFile == null)
					{
						// no image or an error, send default one from us
						GetUnknownCover(bk);
					}

					SevenZipExtractor temp = null;

					try
					{
						temp = ZipHelper.Instance.GetExtractor(bk.FilePath);
						bk.Size = temp.PackedSize;

						ArchiveFileInfo fil = temp.ArchiveFileData.Where(p => !p.IsDirectory && p.FileName == coverFile).First();

						using (MemoryStream stream = new MemoryStream())
						{
							temp.ExtractFile(fil.FileName, stream);
							CreateImage(bk, stream);
						}
					}
					catch (Exception err)
					{
						LogHelper.Manage("ePUBBookService:LoadCoverThread", err);
					}
					finally
					{
						ZipHelper.Instance.ReleaseExtractor(temp);
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBBookService.LoadCoverThread", err);
			}
			finally
			{
				LogHelper.End("ePUBBookService.LoadCoverThread");
			}  
		}

		private void CreateImage(Book bk, MemoryStream stream)
		{
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

		/// <summary>
		/// override to unload book, will delete the temporary folder
		/// </summary>
		/// <param name="bk"></param>
		override public void UnloadBook(Book bk)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("ePUBBookService.UnloadBook");
			try
			{
				base.UnloadBook(bk);

				if (bk != null && bk.Tag != null)
				{
					ePUB docPUB = bk.Tag as ePUB;
					if (!string.IsNullOrEmpty(docPUB.ExpandFolder))
						Directory.Delete(docPUB.ExpandFolder, true);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ePUBBookService.UnloadBook", err);
			}
			finally
			{
				LogHelper.End("ePUBBookService.UnloadBook");
			}  
		}

        public IDocumentPaginatorSource ParseToDocument(ePUB data)
        {
            return new HtmlConverter().ConvertToFlowDocument(data);
        }

	}
}
