using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CBR.Core.Services;
using CBR.Core.Models;
using CBR.Core.Helpers;
using System.IO;

namespace CBR.Core
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WinRTService : IWinRTService
    {
        public bool IsAlive()
        {
            return true;
        }

        public List<WCFCatalog> GetCatalogList()
        {
			try
			{
				return CatalogService.Instance.CatalogRepository
						.Where(p => p.IsShared)
							.Select(p => new WCFCatalog()
							{
								ID = p.CatalogFilePath,
								BookCount = p.BookInfoFilePath.Count,
								Title = p.Title,
								Description = p.Description,
								Image = StreamToImage.GetByteFromImageFile(p.CoverUri.LocalPath)
							}).ToList();
			}
			catch (Exception)
			{
				throw;
			}
        }

        public WCFCatalog GetCatalog(string ID)
        {
			try
			{
				return CatalogService.Instance.CatalogRepository
						.Where(p => p.IsShared && p.CatalogFilePath == ID)
							.Select(p => new WCFCatalog()
							{
								ID = p.CatalogFilePath,
								BookCount = p.BookInfoFilePath.Count,
								Title = p.Title,
								Description = p.Description,
								Image = StreamToImage.GetByteFromImageFile(p.CoverUri.LocalPath)
							})
							.First();
			}
			catch (Exception)
			{
				throw;
			}
        }

        public List<WCFBook> GetCatalogContent(string ID)
        {
			try
			{
				List<string> files = CatalogService.Instance.CatalogRepository
											.Where(p => p.IsShared && p.CatalogFilePath == ID)
												.SelectMany(p => p.BookInfoFilePath).ToList();

				List<WCFBook> books = new List<WCFBook>();

				BookInfoService srv = new BookInfoService();
				foreach (string file in files)
				{
					books.Add(srv.LoadBookInfoSimple(file));
				}

				return books;
			}
			catch (Exception)
			{
				throw;
			}
        }

        public Stream GetBookStream(string ID)
        {
            try
            {
                FileStream imageFile = File.OpenRead(ID);
                return imageFile;
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }

        public FileRequestMessageReturn GetBookStreamByMessage(FileRequestMessage msg)
        {
            try
            {
                FileStream imageFile = File.OpenRead(msg.FileInfo);
                return new FileRequestMessageReturn() { FileInfo = msg.FileInfo, FileData = imageFile };
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
    }
}
