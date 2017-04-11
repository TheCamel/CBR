using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CBR.Core.Helpers;
using CBR.Core.Models;

namespace CBR.Core.Services
{
	/// <summary>
	/// Manage the BookInfo class model that represent a bin storage for each book in the catalog or 
	/// a single opened document
	/// </summary>
    public class BookInfoService
    {
        #region -----------------BOOK INFO-----------------

		/// <summary>
		/// Save the given BookInfo
		/// </summary>
		/// <param name="param"></param>
        public void SaveBookInfo(object param)
        {
            Book bk = param as Book;
            Stream stream = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("BookInfoService.SaveBookInfo");
			try
			{
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(bk.BookInfoFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                if (stream != null)
                {
                    //the thumbnail
                    using (Stream img = StreamToImage.GetStreamFromImage(bk.Cover))
                    {
                        formatter.Serialize(stream, img);
                    }
                    //mySelf file to be restored
                    formatter.Serialize(stream, bk.BookInfoFilePath);
                    // comic file
                    formatter.Serialize(stream, bk.FilePath);
                    //bookmark tag
                    formatter.Serialize(stream, bk.Bookmark);
                    //IsRead tag
                    formatter.Serialize(stream, bk.IsRead);
                    //IsSecured tag
                    formatter.Serialize(stream, bk.IsSecured);
                    //Password tag
                    formatter.Serialize(stream, bk.Password);
                    //page number
                    formatter.Serialize(stream, bk.PageCount);
                    //Size tag
                    formatter.Serialize(stream, bk.Size);
                    //Rating tag
                    formatter.Serialize(stream, bk.Rating);

                    //manage the dynamic properties
                    IDictionary<string, object> dict = bk.Dynamics as IDictionary<string, object>;
                    int counter = dict.Count(p => !string.IsNullOrEmpty(p.Value.ToString()));

                    //not null property counter
                    formatter.Serialize(stream, counter);

                    //then the key/values
                    foreach (string k in dict.Keys)
                    {
                        if (!string.IsNullOrEmpty(k))
                        {
                            formatter.Serialize(stream, k);
                            formatter.Serialize(stream, dict[k]);
                        }
                    }
                }
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookInfoService.SaveBookInfo", err);
			}
			finally
			{
                if (stream != null)
                    stream.Close();

				LogHelper.End("BookInfoService.SaveBookInfo");
			}  
        }

		/// <summary>
		/// Load the given BookInfo through ThreadExchangeData
		/// </summary>
		/// <param name="param"></param>
        public void LoadBookInfo(object param)
        {
            ThreadExchangeData ted = param as ThreadExchangeData;
            Stream stream = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("BookInfoService.LoadBookInfo");
			try
			{
                Book bk = LoadBookInfo( ted.BookPath );

                if( bk != null )
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                    {
                        ted.ThreadCatalog.Books.Add(bk);
                    });
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookInfoService.LoadBookInfo", err);
			}
			finally
			{
				if (stream != null)
					stream.Close();

				LogHelper.End("BookInfoService.LoadBookInfo");
			}  
        }

		/// <summary>
		/// Load the given file path
		/// </summary>
		/// <param name="fileInfoPath"></param>
		/// <returns></returns>
        public Book LoadBookInfo(string fileInfoPath)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookInfoService.LoadBookInfo");
			try
			{
				//the file exist, try to load
				if (File.Exists(fileInfoPath))
				{
					Book bk = new Book();

					IFormatter formatter = new BinaryFormatter();
					using (Stream stream = new FileStream(fileInfoPath, FileMode.Open, FileAccess.Read, FileShare.None))
					{
						//the thumbnail
						using (MemoryStream coverStream = (MemoryStream)formatter.Deserialize(stream))
						{
							MemoryStream tempStream = new MemoryStream();
							coverStream.WriteTo(tempStream);
							coverStream.Flush();
							coverStream.Close();

							tempStream.Position = 0;

							Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
							{
								BitmapImage myImage = new BitmapImage();
								myImage.BeginInit();
								myImage.CacheOption = BitmapCacheOption.OnLoad;
								myImage.StreamSource = tempStream;
								myImage.EndInit();
								bk.Cover = myImage;
							});
							tempStream = null;
						}
						//mySelf file to be restored
						bk.BookInfoFilePath = (string)formatter.Deserialize(stream);
						// comic file
						bk.FilePath = (string)formatter.Deserialize(stream);
						//bookmark tag
						bk.Bookmark = (string)formatter.Deserialize(stream);
						//IsRead tag
						bk.IsRead = (bool)formatter.Deserialize(stream);
						//IsSecured tag
						bk.IsSecured = (bool)formatter.Deserialize(stream);
						//Password tag
						bk.Password = (string)formatter.Deserialize(stream);
						//page number
						bk.PageCount = (int)formatter.Deserialize(stream);
						//Size tag
						bk.Size = (long)formatter.Deserialize(stream);
						//Rating tag
						bk.Rating = (int)formatter.Deserialize(stream);

						try
						{
							//manage the dynamic properties = not null property counter
							int counter = (int)formatter.Deserialize(stream);

							//then the key/values
							for (int i = 0; i < counter; i++)
							{
								((IDictionary<string, object>)bk.Dynamics).Add((string)formatter.Deserialize(stream), (string)formatter.Deserialize(stream));
							}
						}
						catch (Exception err)
						{
							LogHelper.Manage("BookInfoService:LoadBookInfo no dynamics", err);
						}

						new BookServiceBase().SynchronizeProperties(bk);

						bk.IsDirty = false;
					}

					return bk;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookInfoService.LoadBookInfo", err);
			}
			finally
			{
				LogHelper.End("BookInfoService.LoadBookInfo");
			}  

            return null;
        }

        /// <summary>
        /// extract the cover only
        /// </summary>
        /// <param name="fileInfoPath"></param>
        /// <returns></returns>
        public BitmapImage ExtractBookCover(string fileInfoPath)
        {
            if (LogHelper.CanDebug())
                LogHelper.Begin("BookInfoService.ExtractBookCover");

            BitmapImage myImage = null;
            try
            {
                //the file exist, try to load
                if (File.Exists(fileInfoPath))
                {
                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(fileInfoPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        //the thumbnail
                        using (MemoryStream coverStream = (MemoryStream)formatter.Deserialize(stream))
                        {
                            MemoryStream tempStream = new MemoryStream();
                            coverStream.WriteTo(tempStream);
                            coverStream.Flush();
                            coverStream.Close();

                            tempStream.Position = 0;

                            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                myImage = new BitmapImage();
                                myImage.BeginInit();
                                myImage.CacheOption = BitmapCacheOption.OnLoad;
                                myImage.StreamSource = tempStream;
                                myImage.EndInit();
                            }//);
                            tempStream = null;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookInfoService.ExtractBookCover", err);
            }
            finally
            {
                LogHelper.End("BookInfoService.ExtractBookCover");
            }

            return myImage;
        }
        
        /// <summary>
        /// Load the given file path for simple properties in sharing mode
        /// </summary>
        /// <param name="fileInfoPath"></param>
        /// <returns></returns>
        public WCFBook LoadBookInfoSimple(string fileInfoPath)
        {
            if (LogHelper.CanDebug())
                LogHelper.Begin("BookInfoService.LoadBookInfoSimple");
            try
            {
                //the file exist, try to load
                if (File.Exists(fileInfoPath))
                {
                    WCFBook bk = new WCFBook();

                    IFormatter formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(fileInfoPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        //the thumbnail
                        using (MemoryStream coverStream = (MemoryStream)formatter.Deserialize(stream))
                        {
                            bk.Image = coverStream.GetBuffer();
                        }
                        //mySelf file to be restored
                        bk.FileInfo = (string)formatter.Deserialize(stream);
                        // comic file
                        bk.ID = (string)formatter.Deserialize(stream);
                        bk.Title = Path.GetFileName(bk.ID);
                        //bookmark tag
                        bk.Bookmark = (string)formatter.Deserialize(stream);
                        //IsRead tag
                        bk.IsRead = (bool)formatter.Deserialize(stream);
                        //IsSecured tag
                        bk.IsSecured = (bool)formatter.Deserialize(stream);
                        //Password tag
                        bk.Password = (string)formatter.Deserialize(stream);
                        //page number
                        bk.PageCount = (int)formatter.Deserialize(stream);
                        //Size tag
                        bk.Size = (long)formatter.Deserialize(stream);
                        //Rating tag
                        bk.Rating = (int)formatter.Deserialize(stream);
                    }

                    return bk;
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookInfoService.LoadBookInfoSimple", err);
            }
            finally
            {
                LogHelper.End("BookInfoService.LoadBookInfoSimple");
            }

            return null;
        }
        #endregion
    }
}
