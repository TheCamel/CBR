using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Helpers.Localization;

namespace CBR.Core.Files.Conversion
{
	/// <summary>
	/// Convert class through Contract parameter class
	/// </summary>
    public class BookFileConverter
    {
        #region ----------------------PROPERTIES----------------------

        /// <summary>
        /// The thread we are running in, but it can be null
        /// </summary>
        public BackgroundWorker Worker
        {
            get;
            set;
        }

        /// <summary>
        /// Thread event arg associated to the background worker, can be null
        /// </summary>
        public DoWorkEventArgs Event
        {
            get;
            set;
        }

        /// <summary>
        /// Thread result as PDFConvertParameters stored in the event
        /// </summary>
        public ContractParameters Settings
        {
            get
            {
                if (Event != null)
                    return (ContractParameters)Event.Result;
                else
                    return null;
            }
            set
            {
                Event.Result = value;
            }
        }

        #endregion

        #region ----------------------CONSTRUCTORS----------------------

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="worker"></param>
		/// <param name="e"></param>
        public BookFileConverter(BackgroundWorker worker, DoWorkEventArgs e)
		{
			Worker = worker;
			Event = e;

            Settings = (ContractParameters)e.Argument;
        }

		#endregion

        #region ----------------------HELPERS----------------------
        /// <summary>
        /// Send progress mesage through the background worker
        /// </summary>
        /// <param name="message"></param>
        private void Progress(string message)
        {
            if (Worker != null)
                Worker.ReportProgress(0, message);
        }

        /// <summary>
        /// Check if cancelling has been asked
        /// </summary>
        /// <returns></returns>
        private bool CancelPending()
        {
            if (Worker == null)
                return false;

            if (Worker.CancellationPending)
            {
                Event.Cancel = true;
                return true;
            }
            else return false;
        }

        #endregion

        /// <summary>
        /// convert pdf files regarding the parameters gived to the thread
        /// </summary>
        public void Convert()
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookFileConverter.Convert");
            try
            {
                Progress(CultureManager.Instance.GetLocalization("ByCode", "Convert.Start", "Starting conversion..."));

                //get the contracts
				IReaderContract reader = (IReaderContract)Activator.CreateInstance(Settings.InputType.ConversionReader);
				IWriterContract writer = (IWriterContract)Activator.CreateInstance(Settings.OutputType.ConversionWriter);

                //create the result file list
                if (Settings.ResfreshLibrary)
                    Settings.ResultFiles = new List<string>();

                // check output path
                if (!string.IsNullOrEmpty(Settings.DestinationPath))
                    DirectoryHelper.Check(Settings.DestinationPath);

                // just one file !
                if ( !string.IsNullOrEmpty(Settings.InputFile) && File.Exists(Settings.InputFile) ) 
                {
                    ConvertFile(reader, writer, Settings.InputFile, Path.GetDirectoryName(Settings.InputFile));
                }
                
                // folder input
                if (!string.IsNullOrEmpty(Settings.InputPath) && Directory.Exists(Settings.InputPath))
                {
                    if( Settings.InputType.Type == DocumentType.ImageFile )
                        ConvertFolder(reader, writer, Settings.InputPath, Settings.InputPath);//image files
                    else
                        ConvertDirectoryRecursively(reader, writer, Settings.InputPath);// or folders
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookFileConverter.Convert", err);
                Settings.Result = false;
            }
            finally
            {
				LogHelper.End("BookFileConverter.Convert");
                Progress("Conversion finished...");
            }
        }

        #region ----------------------INTERNALS----------------------

        /// <summary>
        /// Convert a folder recursively regarding the settings
        /// </summary>
        /// <param name="inputFolder"></param>
        private void ConvertDirectoryRecursively(IReaderContract reader, IWriterContract writer, string inputFolder)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookFileConverter.ConvertDirectoryRecursively");
            try
            {
				string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.ParsingFolder", "Parsing folder {0}...");
                Progress(string.Format(msg, inputFolder));

                string OutputFolder;

                // one single output folder
                if (!string.IsNullOrEmpty(Settings.DestinationPath) && Directory.Exists(Settings.DestinationPath))
                    OutputFolder = Settings.DestinationPath;
                else
                    OutputFolder = inputFolder;

                DirectoryInfo directory = new DirectoryInfo(inputFolder);

				if (Settings.InputType.Type == DocumentType.PDF)
                {
                    ConvertLoop(reader, writer, OutputFolder, directory, "*.pdf");
                }

				if (Settings.InputType.Type == DocumentType.ZIPBased)
                {
                    ConvertLoop(reader, writer, OutputFolder, directory, "*.cbz");
                    ConvertLoop(reader, writer, OutputFolder, directory, "*.zip");
                }

				if (Settings.InputType.Type == DocumentType.RARBased)
                {
                    ConvertLoop(reader, writer, OutputFolder, directory, "*.cbr");
                    ConvertLoop(reader, writer, OutputFolder, directory, "*.rar");
                }

                if (Settings.InputType.Type == DocumentType.XPS)
                {
                    ConvertLoop(reader, writer, OutputFolder, directory, "*.xps");
                }

                foreach (DirectoryInfo dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (CancelPending()) return;
                    ConvertDirectoryRecursively(reader, writer, dir.FullName);
                }
            }
			catch (Exception err)
            {
				LogHelper.Manage("BookFileConverter.ConvertDirectoryRecursively", err);
                Settings.Result = false;
            }
            finally
            {
				LogHelper.End("BookFileConverter.ConvertDirectoryRecursively");
            }
        }

        private void ConvertLoop(IReaderContract reader, IWriterContract writer, string OutputFolder, DirectoryInfo directory, string fileExtension)
        {
			if (LogHelper.CanDebug())
				LogHelper.Begin("BookFileConverter.ConvertLoop");
			try
			{
				if (CancelPending()) return;

				foreach (FileInfo file in directory.GetFiles(fileExtension))
				{
					if (CancelPending())
						return;
					ConvertFile(reader, writer, file.FullName, OutputFolder);
				}
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookFileConverter.ConvertLoop", err);
			}
			finally
			{
				LogHelper.End("BookFileConverter.ConvertLoop");
			}  
        }

        /// <summary>
        /// convert one file to an output folder regarding the settings mode
        /// </summary>
        /// <param name="inputfile"></param>
        /// <param name="outputFolder"></param>
        private void ConvertFile(IReaderContract reader, IWriterContract writer, string inputFile, string outputParam)
        {
            List<byte[]> imageBytes = new List<byte[]>();
            List<string> imageNames = new List<string>();
            try
            {
				string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.Converting", "Converting {0}...");
                Progress(string.Format(msg, inputFile));

                string outputFolder, tmpFolderForReader = null;

                // one single output folder or same as input
                if (!string.IsNullOrEmpty(Settings.DestinationPath) && Directory.Exists(Settings.DestinationPath))
                    outputFolder = Settings.DestinationPath;
                else
                    outputFolder = outputParam;

                // particular case to win time
				if (Settings.InputType.Type == DocumentType.ZIPBased || Settings.InputType.Type == DocumentType.RARBased)
                {
					if (Settings.OutputType.Type == DocumentType.ImageFile) //direct in ouput
                        tmpFolderForReader = outputFolder;

					if (Settings.OutputType.Type == DocumentType.ZIPBased) //use temp folder for compress
						tmpFolderForReader = Path.Combine(DirectoryHelper.TempPath, Path.GetFileNameWithoutExtension(inputFile));
                }

                if(reader.Read(inputFile, tmpFolderForReader, imageBytes, imageNames, Settings, Progress))
                    writer.Write(Path.GetFileNameWithoutExtension(inputFile), null, outputFolder, imageBytes, imageNames, Settings, Progress);
            }
            catch (Exception error)
            {
                LogHelper.Manage("BookFileConverter:ConvertFile", error);
                Settings.Result = false;
            }
            finally
            {
                if( imageBytes != null )
                   imageBytes.Clear();
                if (imageNames != null)
                    imageNames.Clear();
            }
        }

        private void ConvertFolder(IReaderContract reader, IWriterContract writer, string inputParam, string outputParam)
        {
            List<byte[]> imageBytes = new List<byte[]>();
            List<string> imageNames = new List<string>();

			if (LogHelper.CanDebug())
				LogHelper.Begin("BookFileConverter.ConvertFolder");
			try
			{
				string msg = CultureManager.Instance.GetLocalization("ByCode", "Convert.Converting", "Converting {0}...");
                Progress( string.Format( msg, inputParam) );

                string outputFolder, outputFile;

                // one single output folder or same as input
                if (!string.IsNullOrEmpty(Settings.DestinationPath) && Directory.Exists(Settings.DestinationPath))
                    outputFolder = Settings.DestinationPath;
                else
                    outputFolder = outputParam;

                outputFile = inputParam.Split( new char[] { '\\' } ).Last();

                bool result = true;

                //only case we need folder input in memory, else we write directly
				if (Settings.OutputType.Type == DocumentType.XPS)
                    result = reader.Read(inputParam, null, imageBytes, imageNames, Settings, Progress);

                if( result )
                    writer.Write(outputFile, inputParam, outputFolder, imageBytes, imageNames, Settings, Progress);
			}
			catch (Exception err)
			{
				Settings.Result = false;
				LogHelper.Manage("BookFileConverter.ConvertFolder", err);
			}
			finally
			{
				if (imageBytes != null)
					imageBytes.Clear();
				if (imageNames != null)
					imageNames.Clear();

				LogHelper.End("BookFileConverter.ConvertFolder");
			}  
        }
        #endregion
    }
}
