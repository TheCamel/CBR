using System;
using System.Collections.Generic;
using System.IO;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    internal class ZIPWriter : IWriterContract
    {
        public void Write(string outputFileName, string inputFolder, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("ZIPWriter.Write");
            try
            {
                string file_out; 

                // in this case, just zip the folder in shortFileName
                if (!string.IsNullOrEmpty(inputFolder))
                {
                    file_out = CompressFolder(outputFileName, inputFolder, outputFolder, settings, progress);

                    // temp folders
                    if (inputFolder.Contains(DirectoryHelper.TempPath))
                        Directory.Delete(inputFolder);
                }
                else // write the men to temp folder
                {
					string tempFolder = Path.Combine(DirectoryHelper.TempPath, outputFileName);
                    DirectoryHelper.Check(tempFolder);

                    //first write in temp folder
                    new ImageFileWriter().Write(outputFileName, inputFolder, tempFolder, imageBytes, imageNames, settings, progress);

                    file_out = CompressFolder(outputFileName, tempFolder, outputFolder, settings, progress);

                    Directory.Delete(tempFolder, true);
                }

                if (settings.ResfreshLibrary)
                    settings.ResultFiles.Add(file_out);
            }
            catch (Exception err)
            {
                LogHelper.Manage("ZIPWriter.Write", err);
                settings.Result = false;
            }
			finally
			{
				LogHelper.End("ZIPWriter.Write");
			} 
        }

        internal string CompressFolder(string outputFileName, string inputFolder, string outputFolder, ContractParameters settings, ProgressDelegate progress)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("ZIPWriter.CompressFolder");
            try
            {
                // move up from one directory if same as source in case of files
                if (string.IsNullOrEmpty(settings.DestinationPath) && settings.InputType.Type == DocumentType.ImageFile)
                    outputFolder = Directory.GetParent(outputFolder).FullName;

                string file_out = Path.Combine(outputFolder, outputFileName + ".cbz");

                if (File.Exists(file_out))
                    File.Delete(file_out);

                int iResult = 0;
				if (!ZipHelper.Instance.CompressFolder(file_out, inputFolder, out iResult))
					settings.Result = false;
				else
				{
					string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageZipped", "{0} images zipped...");
					progress(string.Format(msg, iResult));
				}
                return file_out;
            }
            catch (Exception err)
            {
				LogHelper.Manage("ZIPWriter.CompressFolder", err);
                settings.Result = false;
                return string.Empty;
            }
            finally
            {
				LogHelper.End("ZIPWriter.CompressFolder");
            }
        }
    }
}
