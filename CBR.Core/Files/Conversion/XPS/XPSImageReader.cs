using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    internal class XPSImageReader : IReaderContract
    {
        public bool Read(string filePath, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
		{

			if (LogHelper.CanDebug())
				LogHelper.Begin("XPSImageReader.Read");
			try
			{
				XpsDocument xpsDocument = null;

				// read the document 
				xpsDocument = new XpsDocument(filePath, FileAccess.Read, CompressionOption.NotCompressed);

				//
				IXpsFixedDocumentSequenceReader fixedDocSeqReader = xpsDocument.FixedDocumentSequenceReader;

				ICollection<IXpsFixedDocumentReader> fixedDocuments = fixedDocSeqReader.FixedDocuments;

				IEnumerator<IXpsFixedDocumentReader> enumerator = fixedDocuments.GetEnumerator();
				enumerator.MoveNext();
				ICollection<IXpsFixedPageReader> fixedPages = enumerator.Current.FixedPages;

				string imageFileName = Path.GetFileNameWithoutExtension(filePath);
				int i = 0;
				foreach (IXpsFixedPageReader fixedPageReader in fixedPages)
				{
					try
					{
						ICollection<XpsImage> list = fixedPageReader.Images;

						if (list.Count == 1)
						{
							XpsImage img = list.First();
							if (img != null)
							{
								using (Stream xpsThumbStream = img.GetStream())
								{
									byte[] buf = new byte[xpsThumbStream.Length];
									xpsThumbStream.Read(buf, 0, buf.Length);

									imageBytes.Add(buf);
									imageNames.Add(imageFileName + "_" + i + ".jpg");
								}
							}
						}
						i++;
					}
					catch (System.InvalidOperationException)
					{
					}
				}

				xpsDocument.Close();

				string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageExtracted", "{0} images extracted...");
				progress(string.Format(msg, imageBytes.Count));
			}
			catch (Exception err)
			{
				LogHelper.Manage("XPSImageReader.Read", err);
                settings.Result = false;
                return false;
			}
			finally
			{
				LogHelper.End("XPSImageReader.Read");
			}  

            return true;
        }
    }
}
