using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CBR.Core.Helpers;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    internal class XPSImageWriter : IWriterContract
    {
        public void Write(string inputFileorFolder, string inputFolder, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("XPSImageWriter.Write");
			try
			{
                string file_out = Path.Combine(outputFolder, inputFileorFolder) + ".xps";

                if (File.Exists(file_out))
                    File.Delete(file_out);

				if (new XpsHelper().WriteDocument(imageBytes, file_out))
				{
					string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.Output", "Output file written !");
					progress(msg);
				}
                if (settings.ResfreshLibrary)
                    settings.ResultFiles.Add(file_out);
			}
			catch (Exception err)
			{
				LogHelper.Manage("XPSImageWriter.Write", err);
				settings.Result = false;
			}
			finally
			{
				LogHelper.End("XPSImageWriter.Write");
			}  
        }
    }
}
