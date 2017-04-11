using System;
using System.Collections.Generic;

namespace CBR.Core.Models
{
    /// <summary>
    /// All book types
    /// </summary>
    public enum DocumentType
    {
        None, All, ImageFile, PDF, RARBased, ZIPBased, XPS, ePUB, HTML, FB2, CSV, XML
    }

    /// <summary>
    /// Group all files properties and service
    /// </summary>
    public class DocumentInfo
    {
        /// <summary>
        /// the book enum type 
        /// </summary>
        public DocumentType Type { get; set; }
        
        /// <summary>
        /// the associated service
        /// </summary>
        public Type Service { get; set; }

        /// <summary>
        /// the associated model, if null not editable
        /// </summary>
        public Type Model { get; set; }

        /// <summary>
        /// the associated view model as STRING, created by reflection !
        /// </summary>
        public string ViewModel { get; set; }

        /// <summary>
        /// can register the open command and icon
        /// </summary>
        public bool CanRegister { get; set; }

        /// <summary>
        /// path to the icon file used for register file type
        /// </summary>
        public string IconFile { get; set; }

		/// <summary>
		/// Converion reader contract
		/// </summary>
		public Type ConversionReader { get; set; }

		/// <summary>
		/// Conversion writer contract
		/// </summary>
		public Type ConversionWriter { get; set; }

        /// <summary>
        /// the book enum type we can convert to
        /// </summary>
        public List<DocumentType> CanConvertTo { get; set; }

		public Type Publisher { get; set; }

        /// <summary>
        /// the file extension
        /// </summary>
        public string Extension { get; set; }

		/// <summary>
		/// the human readable description
		/// </summary>
		public string Description { get; set; }

        /// <summary>
        /// the dialog file description
        /// </summary>
        public string DialogDescription { get; set; }

        /// <summary>
        /// calculated dialog filter
        /// </summary>
        public string DialogFilter
        {
            get { return DialogDescription + "|" + StarExtension; }
        }

        /// <summary>
        /// calculated extension STAR.EXTention
        /// </summary>
        public string StarExtension
        {
            get { return "*" + Extension; }
        }
    }
}
