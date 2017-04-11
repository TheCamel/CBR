using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    internal class ImageFileWriter : IWriterContract
    {
        public void Write(string outputFileName, string inputFolder, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("ImageFileWriter.Write");
            try
            {
                for (int i = 0; i < imageBytes.Count; ++i)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes[i]))
                    {
                        BitmapImage myImage = new BitmapImage();
                        myImage.BeginInit();
                        myImage.StreamSource = ms;
                        myImage.EndInit();

                        using (FileStream fs = new FileStream(Path.Combine(outputFolder, outputFileName + "_" + imageNames[i]), FileMode.Create))
                        {
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(myImage));
                            encoder.Save(fs);
                        }
                    }
                }
				string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageSaved", "{0} images saved...");
                progress(string.Format(msg, imageBytes.Count));
            }
            catch (Exception err)
            {
				LogHelper.Manage("ImageFileWriter:Write", err);
                settings.Result = false;
            }
            finally
			{
				LogHelper.End("ImageFileWriter.Write");
            }
        }
    }
}
