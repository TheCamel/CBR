using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using CBR.Core.Helpers;
using System.IO;

namespace CBR.Core.Formats.ePUB
{
	/// <summary>
	/// Models the Open Packaging Format package concept
	/// </summary>
	public class ePUBPackage
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBPackage()
		{
			Manifest = new ePUBManifest();
			Metadata = new ePUBMetadata();
			TableOfContent = new ePUBTableNCX();
			Spine = new ePUBSpine();
			Guide = new ePUBGuide();
			
			RelativPackageFolder = ePUBHelper.Files.PackageFolder;
			PackageFileName = ePUBHelper.Files.PackageFile;
		}

		/// <summary>
		/// Constructor with file parameter
		/// </summary>
		/// <param name="filePath"></param>
		public ePUBPackage(string packRelativFileName)
		{
			Manifest = new ePUBManifest();
			Metadata = new ePUBMetadata();
			TableOfContent = new ePUBTableNCX();
			Spine = new ePUBSpine();
			Guide = new ePUBGuide();
			
			RelativPackageFolder = Path.GetDirectoryName(packRelativFileName);
			PackageFileName = Path.GetFileName(packRelativFileName);
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		[Browsable(true)]
		[Description("Gets the relativ path to the file")]
		public string SysRelativFilePath
		{
			get
			{
				if (!string.IsNullOrEmpty(RelativPackageFolder))
					return RelativPackageFolder + "\\" + PackageFileName;
				else
					return PackageFileName;
			}
		}

		[Browsable(false)]
		[Description("Get the package folder")]
		public string RelativPackageFolder { get; set; }

		[Browsable(false)]
		[Description("Get the package file name")]
		public string PackageFileName { get; set; }

		[Browsable(true)]
		[Description("Get the title of the book, as held in the table of contents")]
		public string Title { get; set; }

		[Browsable(true)]
		[Description("Gets the unique_identifier attribute of the package element ")]
		public string Identifier { get; set; }

		[Browsable(true)]
		[Description("Get the manifest instance for the package")]
		public ePUBManifest Manifest { get; set; }

		[Browsable(true)]
		[Description("Get the metadata instance for the package")]
		public ePUBMetadata Metadata { get; set; }

		[Browsable(true)]
		[Description("Get the toc (table of contents) instance for the package")]
		public ePUBTableNCX TableOfContent { get; set; }

		[Browsable(true)]
		[Description("Get the spine instance for the package")]
		public ePUBSpine Spine { get; set; }

		[Browsable(true)]
		[Description("Get the guide instance for the package")]
		public ePUBGuide Guide { get; set; }

		#endregion

		#region -----------------METHODS-----------------

//        public bool AddDocument(
//    string booksPath,
//    string fileName,
//    string title,
//    string TOCentry
//){
//}

		

//                public void Save()
//            {
//            }

//                    public void SaveEntry(
//    string fileName,
//    string content
//){
//}

//                        public void SetContentPage(
//    XmlDocument contentPage,
//    string title,
//    string uniqueIdentifier
//){
//}

//                            public void SetTitlePage(
//    string title,
//    string uniqueIdentifier,
//    string creator,
//    string date
//)
//                                {
//}
//public void SetTOCHeader(
//    string title,
//    string uniqueIdentifier,
//    string author,
//    string publisher
//)
//{
//}

		#endregion
	}
}
