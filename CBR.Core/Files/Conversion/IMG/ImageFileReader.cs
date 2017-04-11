using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using CBR.Core.Helpers;
using CBR.Core.Services;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    class ImageFileReader : IReaderContract
    {
        public bool Read(string inputFileorFolder, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("ImageFileReader.Read");
            try
            {
                string[] files = Directory.GetFiles(inputFileorFolder, "*.*", SearchOption.TopDirectoryOnly);

                foreach (string filename in files)
                {
                    if (DocumentFactory.Instance.ImageExtension.Contains(Path.GetExtension(filename).ToUpper()))
                    {
                        BitmapImage myImage = new BitmapImage();
                        myImage.BeginInit();
                        myImage.UriSource = new Uri(filename, UriKind.RelativeOrAbsolute);
                        myImage.CacheOption = BitmapCacheOption.OnLoad;
                        myImage.EndInit();

                        imageBytes.Add(StreamToImage.BufferFromImage(myImage));
                        imageNames.Add(Path.GetFileName(filename));
                    }
                }
				string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageFound", "{0} images founded...");
                progress(string.Format(msg, imageBytes.Count));
            }
            catch (Exception err)
            {
                LogHelper.Manage("ImageFileReader:Read", err);
                settings.Result = false;
                return false;
            }
			finally
			{
				LogHelper.End("ImageFileReader.Read");
			}  
            return true;
        }
    }
}
