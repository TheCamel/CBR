using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CBR.Core.Helpers;
using CBR.Core.Services;
using SevenZip;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
    class RARImageReader : IReaderContract
    {
        public bool Read(string inputFileorFolder, string tempFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress)
        {
            SevenZipExtractor temp = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("RARImageReader.Read");
            try
            {
                temp = ZipHelper.Instance.GetExtractor(inputFileorFolder);

                //direct extract
                if (!string.IsNullOrEmpty(tempFolder))
                {
                    DirectoryHelper.Check(tempFolder);

                    temp.PreserveDirectoryStructure = false;
                    temp.ExtractArchive(tempFolder);

					if (progress != null)
					{
						string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ImageExtracted", "{0} images extracted...");
						progress(string.Format(msg, temp.ArchiveFileData.Count));
					}
                    CheckFileNames(tempFolder);
                }
                else //to memory
                {
                    foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
                    {
                        if (!fil.IsDirectory && DocumentFactory.Instance.ImageExtension.Contains(Path.GetExtension(fil.FileName).ToUpper()))
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                temp.ExtractFile(fil.FileName, stream);

                                imageBytes.Add(stream.GetBuffer());
                                imageNames.Add(Path.GetFileName(fil.FileName));
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
				LogHelper.Manage("RARImageReader.Read", err);

                if (settings != null)
                    settings.Result = false;

                return false;
            }
            finally
            {
                ZipHelper.Instance.ReleaseExtractor(temp);
				LogHelper.End("RARImageReader.Read");
            }
            return true;
        }

        private void CheckFileNames(string tempFolder)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("RARImageReader.CheckFileNames");
			try
			{
				Regex MyRegex = new Regex("\\d+",
					RegexOptions.IgnoreCase
					| RegexOptions.Multiline
					| RegexOptions.CultureInvariant
					| RegexOptions.IgnorePatternWhitespace
					| RegexOptions.Compiled
					);

				foreach (string fileName in Directory.GetFiles(tempFolder, "*.*", SearchOption.TopDirectoryOnly))
				{
					FileInfo fi = new FileInfo(fileName);

					MatchCollection ms = MyRegex.Matches(fi.Name);
					Match m = ms[ms.Count - 1];
					string destFileName = fi.Name.Substring(0, m.Index) + string.Format("{0:0000}", Convert.ToInt32(m.Value)) + fi.Extension;
					destFileName = Path.Combine(fi.DirectoryName, destFileName);

					fi.CopyTo(destFileName, true);
					fi.Delete();
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("RARImageReader.CheckFileNames", err);
			}
			finally
			{
				LogHelper.End("RARImageReader.CheckFileNames");
			}  
        }
    }
}
