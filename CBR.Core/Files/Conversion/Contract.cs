using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Models;

namespace CBR.Core.Files.Conversion
{
    public delegate void ProgressDelegate(string message);

    public interface IReaderContract
    {
        bool Read(string inputFileorFolder, string tempFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress);
    }

    public interface IWriterContract
    {
        void Write(string outputFileName, string inputFolder, string outputFolder, List<byte[]> imageBytes, List<string> imageNames, ContractParameters settings, ProgressDelegate progress);
    }

    /// <summary>
    /// Input parameters
    /// </summary>
    public class ContractParameters
    {
        /// <summary>
        /// if exist, single file mode conversion
        /// </summary>
        public string InputFile { get; set; }

        /// <summary>
        /// if exist, multi file conversion
        /// </summary>
        public string InputPath { get; set; }

        /// <summary>
        /// input mode = image, rar, pdf or xps
        /// </summary>
        public DocumentInfo InputType { get; set; }

        /// <summary>
        /// if exist, unique destination folder for all conversion
        /// </summary>
        public string DestinationPath { get; set; }
        
        /// <summary>
        /// Output mode = image, zip or xps / 7Zip can't write RAR
        /// </summary>
		public DocumentInfo OutputType { get; set; }

        /// <summary>
        /// check for each file that page=image count
        /// </summary>
        public bool CheckResult { get; set; }
        
        /// <summary>
        /// refresh the library with all converted files
        /// </summary>
        public bool ResfreshLibrary { get; set; }

		/// <summary>
		/// join images by page
		/// </summary>
		public bool JoinImages { get; set; }

        /// <summary>
        /// bool to store the result of the conversion : succeded = true
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// all converted files as result
        /// </summary>
        public List<string> ResultFiles { get; set; }

		#region ----------------DEFAULTs----------------
        /// <summary>
        /// Construtor
        /// </summary>
		public ContractParameters()
        {
			CheckResult = true;
			ResfreshLibrary = false;
			JoinImages = true;
        }
        #endregion

        public bool CheckParameters()
        {
            //source
            if (string.IsNullOrEmpty(InputFile) && string.IsNullOrEmpty(InputPath))
                return false;

            if ( this.InputType == this.OutputType && this.InputType.Type != DocumentType.ZIPBased )
                return false;

            return true;
        }
    }
}
