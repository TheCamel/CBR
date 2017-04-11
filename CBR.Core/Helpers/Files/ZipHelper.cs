using System;
using System.IO;
using System.Linq;
using SevenZip;

namespace CBR.Core.Helpers
{
    public class ZipHelper
    {
        #region ----------------SINGLETON----------------
        public static readonly ZipHelper Instance = new ZipHelper();

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private ZipHelper()
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("ZipHelper.ZipHelper");
			try
			{
				//be sure 7zip is initialized
				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
					SevenZipExtractor.SetLibraryPath(DirectoryHelper.Combine(CBRFolders.Dependencies, "7z64.dll"));
				else
					SevenZipExtractor.SetLibraryPath(DirectoryHelper.Combine(CBRFolders.Dependencies, "7z.dll"));
			}
			catch (Exception err)
			{
				LogHelper.Manage("ZipHelper.ZipHelper", err);
			}
			finally
			{
				LogHelper.End("ZipHelper.ZipHelper");
			}  
		}

		#endregion

		/// <summary>
		/// ask for an extractor
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
        public SevenZipExtractor GetExtractor( string filePath )
        {
            SevenZipExtractor temp = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ZipHelper.GetExtractor", "filePath: {0}", filePath);
			try
			{
				temp = new SevenZipExtractor(filePath);
			}
			catch (Exception err)
			{
				ReleaseExtractor(temp);
				LogHelper.Manage("ZipHelper.GetExtractor", err);
			}
			finally
			{
				LogHelper.End("ZipHelper.GetExtractor");
			}  

            return temp;
        }

		/// <summary>
		/// release the given extracor
		/// </summary>
		/// <param name="extractor"></param>
        public void ReleaseExtractor(SevenZipExtractor extractor)
        {
			try
			{
				if (extractor != null)
				{
					if (LogHelper.CanDebug())
						LogHelper.Begin("ZipHelper.ReleaseExtractor", "filePath: {0}", extractor.FileName);

					extractor.Dispose();
					extractor = null;
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("ZipHelper.ReleaseExtractor", err);
			}
			finally
			{
				LogHelper.End("ZipHelper.ReleaseExtractor");
			}  
        }
        
        #region ----------------FOLDERS----------------

		/// <summary>
		/// self compress a folder to a content
		/// </summary>
		/// <param name="outputFileName"></param>
		/// <param name="inputFolder"></param>
		/// <param name="resultCount"></param>
		/// <returns></returns>
        public bool CompressFolder(string outputFileName, string inputFolder, out int resultCount)
        {
            SevenZip.SevenZipCompressor cp = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ZipHelper.CompressFolder");
			try
			{
				cp = new SevenZip.SevenZipCompressor();
				cp.ArchiveFormat = SevenZip.OutArchiveFormat.Zip;

				string[] outputFiles = new DirectoryInfo(inputFolder).GetFiles("*.*").Select(p => p.FullName).ToArray();

				using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
				{
					cp.CompressFiles(fs, outputFiles);
				}

				resultCount = outputFiles.Count();
				return true;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ZipHelper.CompressFolder", err);
				resultCount = 0;
				return false;
			}
			finally
			{
				cp = null;
				LogHelper.End("ZipHelper.CompressFolder");
			}  
        }

		/// <summary>
		/// self uncompress a content to folder
		/// </summary>
		/// <param name="inputFile"></param>
		/// <param name="outputFolder"></param>
		/// <param name="resultCount"></param>
		/// <returns></returns>
        public bool UncompressToFolder(string inputFile, string outputFolder, out int resultCount)
        {
            SevenZipExtractor temp = null;

			if (LogHelper.CanDebug())
				LogHelper.Begin("ZipHelper.UncompressToFolder");
			try
			{
                temp = GetExtractor(inputFile);

                DirectoryHelper.Check(outputFolder);

                temp.PreserveDirectoryStructure = false;
                temp.ExtractArchive(outputFolder);
 
                resultCount = temp.ArchiveFileData.Count;

                return true;
			}
			catch (Exception err)
			{
				LogHelper.Manage("ZipHelper.UncompressToFolder", err);
				resultCount = 0;
				return false;
			}
			finally
			{
				ReleaseExtractor(temp);
				LogHelper.End("ZipHelper.UncompressToFolder");
			}  
        }
        #endregion
    }
}
