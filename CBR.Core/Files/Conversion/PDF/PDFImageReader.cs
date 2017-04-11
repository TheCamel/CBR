using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    internal class PDFImageReader : IReaderContract
    {
        public bool Read(string inputFileorFolder, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
        {
            PdfReader reader = null;
            PDFImageListener listener = null;
            try
            {
                reader = new PdfReader(inputFileorFolder);
                PdfReaderContentParser parser = new PdfReaderContentParser(reader);

                listener = new PDFImageListener();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
					listener.PageIndex = i;
                    parser.ProcessContent(i, listener);
                }

				if (settings.CheckResult && reader.NumberOfPages != listener.ImageNames.Count)
				{
					if (settings.JoinImages)
					{
						string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageCountingKO", "Extracting {0} : {1} images for {2} pages - Try to merge !");
						progress(string.Format(msg, inputFileorFolder, listener.ImageNames.Count, reader.NumberOfPages));

						ImageJoiner cp = new ImageJoiner();
						cp.Merge(listener.ImageBytes, listener.ImageNames);

						msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageMerge", "Merge to {0} new images...");
						progress(string.Format(msg, cp.NewImageNames.Count));

						imageBytes.AddRange(cp.NewImageBytes);
						imageNames.AddRange(cp.NewImageNames);
					}
					else
					{
						string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageError", "Error extracting {0} : {1} images for {2} pages !!");
						progress(string.Format(msg, inputFileorFolder, listener.ImageNames.Count, reader.NumberOfPages));
						throw new Exception("PDF check error");
					}
				}
				else
				{
					string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageCountingOK", "Extracting {0} images in {1} pages");
					progress(string.Format(msg, listener.ImageNames.Count, reader.NumberOfPages));

					imageBytes.AddRange(listener.ImageBytes);
					imageNames.AddRange(listener.ImageNames);

					msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageExtracted", "{0} images extracted...");
					progress(string.Format(msg, listener.ImageBytes.Count));
				}
            }
            catch (Exception err)
            {
                LogHelper.Manage("PDFImageReader:Read", err);
                settings.Result = false;
                listener.ImageNames.Clear();
                listener.ImageBytes.Clear();
                return false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return true;
        }
    }
}
