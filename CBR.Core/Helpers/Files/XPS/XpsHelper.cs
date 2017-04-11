using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.IO.Packaging;
using System.Xml;
using System.Windows.Xps.Serialization;

namespace CBR.Core.Helpers
{
    internal enum ThumbnailQuality
    {
        Low = 3,
        Medium = 2, 
        Good = 1
    }

    internal class XpsHelper
    {
        public MemoryStream GenerateThumbnailFromFirstPage(string xpsFilePath, XpsImageType imgType, ThumbnailQuality imgQuality = ThumbnailQuality.Medium)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("XpsHelper.GenerateThumbnailFromFirstPage");
			try
			{
				BitmapEncoder bitmapEncoder = null;

				XpsDocument xpsDocument = new XpsDocument(xpsFilePath, FileAccess.Read);
				FixedDocumentSequence documentPageSequence = xpsDocument.GetFixedDocumentSequence();

				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(xpsFilePath);
				string fileExtension = string.Empty;

				switch (imgType)
				{
					case XpsImageType.JpegImageType:
						bitmapEncoder = new JpegBitmapEncoder();
						break;
					case XpsImageType.PngImageType:
						bitmapEncoder = new PngBitmapEncoder();
						break;
				}

				double imageQualityRatio = 1.0 / (double)imgQuality;

				DocumentPage documentPage = documentPageSequence.DocumentPaginator.GetPage(0);
				RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)(documentPage.Size.Width * imageQualityRatio),
					(int)(documentPage.Size.Height * imageQualityRatio),
					96.0 * imageQualityRatio,
					96.0 * imageQualityRatio,
					PixelFormats.Pbgra32);
				targetBitmap.Render(documentPage.Visual);

				bitmapEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));

				MemoryStream memoryStream = new MemoryStream();
				bitmapEncoder.Save(memoryStream);
				xpsDocument.Close();

				return memoryStream;
			}
			catch (Exception err)
			{
				LogHelper.Manage("XpsHelper.GenerateThumbnailFromFirstPage", err);
			}
			finally
			{
				LogHelper.End("XpsHelper.GenerateThumbnailFromFirstPage");
			}  

            return null;
        }

        public BitmapImage GetXpsThumbnail(string xpsFilePath)
        {
            XpsDocument document = null;
            BitmapImage thumbImage = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("XpsHelper.GetXpsThumbnail");
			try
			{
				document = new XpsDocument(xpsFilePath, FileAccess.Read);
				XpsThumbnail thumb = document.Thumbnail;

				if (thumb != null)
				{
					using (Stream xpsThumbStream = thumb.GetStream())
					{
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
						{
							thumbImage = new BitmapImage();
							thumbImage.BeginInit();
							thumbImage.DecodePixelWidth = 70;
							thumbImage.CacheOption = BitmapCacheOption.OnLoad;
							thumbImage.StreamSource = xpsThumbStream;
							thumbImage.EndInit();
						});
					}
				}
				else
				{
					using (Stream xpsThumb = GenerateThumbnailFromFirstPage(xpsFilePath, XpsImageType.JpegImageType, ThumbnailQuality.Low))
					{
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
						{
							thumbImage = new BitmapImage();
							thumbImage.BeginInit();
							thumbImage.DecodePixelWidth = 70;
							thumbImage.CacheOption = BitmapCacheOption.OnLoad;
							thumbImage.StreamSource = xpsThumb;
							thumbImage.EndInit();
						});
					}
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("XpsHelper.GetXpsThumbnail", err);
			}
			finally
			{
				if (document != null)
					document.Close();

				LogHelper.End("XpsHelper.GetXpsThumbnail");
			}  

            return thumbImage;
        }


        public bool WriteDocument(List<byte[]> images, string file_out)
        {
            XpsDocument doc = null;
            IXpsFixedDocumentSequenceWriter docSeqWriter = null;
            IXpsFixedDocumentWriter docWriter = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("XpsHelper.WriteDocument");
			try
			{
				// xps doc does not manage overwrite, so delete before if exist
				if (File.Exists(file_out))
					File.Delete(file_out);

				// create the document 
				doc = new XpsDocument(file_out, FileAccess.ReadWrite, CompressionOption.NotCompressed);

				// Create the document sequence
				docSeqWriter = doc.AddFixedDocumentSequence();

				// Create the document
				docWriter = docSeqWriter.AddFixedDocument();

				for (int i = 0; i < images.Count; ++i)
				{
					// Create the Page
					IXpsFixedPageWriter pageWriter = docWriter.AddFixedPage();

					// Get the XmlWriter
					XmlWriter xmlWriter = pageWriter.XmlWriter;

					//create the xps image resource
					XpsImage xpsImage = pageWriter.AddImage(XpsImageType.JpegImageType);
					using (Stream dstImageStream = xpsImage.GetStream())
					{
						dstImageStream.Write(images[i], 0, images[i].Length);
						xpsImage.Commit();
					}

					//this is just to get the real WPF image size as WPF display units and not image pixel size !!
					using (MemoryStream ms = new MemoryStream(images[i]))
					{
						BitmapImage myImage = new BitmapImage();
						myImage.BeginInit();
						myImage.CacheOption = BitmapCacheOption.OnLoad;
						myImage.StreamSource = ms;
						myImage.EndInit();

						//write the page
						WritePageContent(xmlWriter, xpsImage, myImage.Width, myImage.Height);
					}

					//with the first page, create the thumbnail of the xps document
					if (i == 0)
					{
						XpsThumbnail thumb = doc.AddThumbnail(XpsImageType.JpegImageType);

						using (MemoryStream ms = new MemoryStream(images[i]))
						{
							BitmapImage thumbImage = new BitmapImage();
							thumbImage.BeginInit();
							thumbImage.DecodePixelWidth = 256;
							thumbImage.CacheOption = BitmapCacheOption.OnLoad;
							thumbImage.StreamSource = ms;
							thumbImage.EndInit();

							using (Stream xpsThumbStream = thumb.GetStream())
							{
								JpegBitmapEncoder encoder = new JpegBitmapEncoder();
								encoder.Frames.Add(BitmapFrame.Create(thumbImage));
								encoder.Save(xpsThumbStream);
							}
						}
						thumb.Commit();
					}

					xmlWriter.Flush();

					// Close the page
					pageWriter.Commit();
				}

				return true;
			}
			catch (Exception err)
			{
				LogHelper.Manage("XpsHelper.WriteDocument", err);
				return false;
			}
			finally
			{
				if (docWriter != null)
					docWriter.Commit();

				if (docSeqWriter != null)
					docSeqWriter.Commit();

				if (doc != null)
					doc.Close();

				LogHelper.End("XpsHelper.WriteDocument");
			}  
        }

        /// <summary>
        /// write the xaml for the page
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="res"></param>
        /// <param name="wpfWidth"></param>
        /// <param name="wpfHeight"></param>
        private void WritePageContent(System.Xml.XmlWriter xmlWriter, XpsResource res, double wpfWidth, double wpfHeight)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("XpsHelper.WritePageContent");
            try
            {
                xmlWriter.WriteStartElement("FixedPage");
                xmlWriter.WriteAttributeString("xmlns", @"http://schemas.microsoft.com/xps/2005/06");
                xmlWriter.WriteAttributeString("Width", "794");
                xmlWriter.WriteAttributeString("Height", "1123");
                xmlWriter.WriteAttributeString("xml:lang", "en-US");

                xmlWriter.WriteStartElement("Canvas");

                if (res is XpsImage)
                {
                    xmlWriter.WriteStartElement("Path");
                    xmlWriter.WriteAttributeString("Data", "M 0,0 L 794,0 794,1123 0,1123 z");
                    xmlWriter.WriteStartElement("Path.Fill");
                    xmlWriter.WriteStartElement("ImageBrush");
                    xmlWriter.WriteAttributeString("ImageSource", res.Uri.ToString());
                    xmlWriter.WriteAttributeString("Viewbox", string.Format("0,0,{0},{1}",
                        System.Convert.ToInt32(wpfWidth), System.Convert.ToInt32(wpfHeight)));
                    xmlWriter.WriteAttributeString("ViewboxUnits", "Absolute");
                    xmlWriter.WriteAttributeString("Viewport", "0,0,794,1123");
                    xmlWriter.WriteAttributeString("ViewportUnits", "Absolute");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
            catch (Exception err)
            {
				LogHelper.Manage("XpsHelper:WritePageContent", err);
            }
			finally
			{
				LogHelper.End("XpsHelper.WritePageContent");
			}  
        }

        public FixedDocument ConvertToFixed( FlowDocument document )
        {
            SaveAsXps("test.xps", document);

            return new XpsDocument("test.xps", FileAccess.Read).GetFixedDocumentSequence().References[0].GetDocument(true);

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            var package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
            var packUri = new Uri("pack://temp.xps");
            //PackageStore.RemovePackage(packUri);
            PackageStore.AddPackage(packUri, package);
            var xps = new XpsDocument(package, CompressionOption.SuperFast, packUri.ToString());
            XpsDocument.CreateXpsDocumentWriter(xps).Write(paginator);
            FixedDocument doc = xps.GetFixedDocumentSequence().References[0].GetDocument(true);
            PackageStore.RemovePackage(packUri);
            return doc;
        }


        public void SaveAsXps(string fileName, FlowDocument document)
        {
            using (System.IO.Packaging.Package container = System.IO.Packaging.Package.Open(fileName, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;

                    //16.5=6.4960=623.616
                    //11=4.3307=415.74
                    paginator = new DocumentPaginatorWrapper(paginator, new Size(416, 624), new Size(15, 15));

                    rsm.SaveAsXaml(paginator);
                }
            }
        }
    }
}
