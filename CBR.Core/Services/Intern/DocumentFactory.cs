using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CBR.Core.Files.Conversion;
using CBR.Core.Files.Publisher;
using CBR.Core.Helpers;
using CBR.Core.Models;

namespace CBR.Core.Services
{
	/// <summary>
	/// Factory to the DocumentInfo class related to all manageable files
	/// </summary>
    public class DocumentFactory
    {
        #region ----------------SINGLETON----------------

        public static readonly DocumentFactory Instance = new DocumentFactory();

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private DocumentFactory()
        {
        }

        #endregion

        #region ----------------IMAGES----------------

		/// <summary>
		/// managed image extensions
		/// </summary>
		public List<string> ImageExtension = new List<string>() { ".PNG", ".BMP", ".JPG" };
		
		private DocumentInfo allExtension = new DocumentInfo()
			{
				Extension = ".*", DialogDescription = "All Files (*.*)"
			};

        #endregion

        #region ----------------BOOKS----------------

		/// <summary>
		/// managed book filters
		/// </summary>
        public List<DocumentInfo> BookFilters = new List<DocumentInfo>()
        {
			new DocumentInfo()
			{	
				Extension= ".CBL", DialogDescription = "Comic Book Reader Librairies (*.CBL)",
				CanRegister = true, 
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "cbl.ico")
			},
			new DocumentInfo()
            {
				Extension= ".PNG; .BMP; .JPG", DialogDescription = "Image Files (.PNG; .BMP; .JPG)", Type= DocumentType.ImageFile,
                ConversionReader = typeof(ImageFileReader), ConversionWriter = typeof(ImageFileWriter),
				CanConvertTo = new List<DocumentType>() { DocumentType.ZIPBased, DocumentType.XPS },
				CanRegister = false
            },

			new DocumentInfo()
            { 
				Extension= ".CBR", DialogDescription = "Comic RAR Files (*.CBR)", Type= DocumentType.RARBased, 
                Model=typeof(Book), Service=typeof(BookService), ViewModel = "CBR.ViewModels.ComicViewModel",
                ConversionReader = typeof(RARImageReader),
				CanConvertTo = new List<DocumentType>() { DocumentType.ImageFile, DocumentType.ZIPBased, DocumentType.XPS },
                CanRegister = true,
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "cbrz.ico")
            },
			new DocumentInfo()
            {
				Extension= ".CBZ", DialogDescription = "Comic ZIP Files (*.CBZ)", Type= DocumentType.ZIPBased,
                Model=typeof(Book), Service=typeof(BookService), ViewModel = "CBR.ViewModels.ComicViewModel",
                ConversionReader = typeof(RARImageReader), ConversionWriter = typeof(ZIPWriter),
				CanConvertTo = new List<DocumentType>() { DocumentType.ImageFile, DocumentType.XPS },
                CanRegister = true,
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "cbrz.ico")
            },
			new DocumentInfo()
            {
				Extension= ".ZIP", DialogDescription = "ZIP Files (*.ZIP)", Type= DocumentType.ZIPBased,
                Model=typeof(Book), Service=typeof(BookService), ViewModel = "CBR.ViewModels.ComicViewModel",
                ConversionReader = typeof(RARImageReader), ConversionWriter = typeof(ZIPWriter),
				CanConvertTo = new List<DocumentType>() { DocumentType.ImageFile, DocumentType.XPS },
                CanRegister = false
            },
			new DocumentInfo()
            {
				Extension= ".RAR", DialogDescription = "RAR Files (*.RAR)", Type= DocumentType.RARBased,
                Model=typeof(Book), Service=typeof(BookService), ViewModel = "CBR.ViewModels.ComicViewModel",
                ConversionReader = typeof(RARImageReader),
				CanConvertTo = new List<DocumentType>() { DocumentType.ImageFile, DocumentType.ZIPBased, DocumentType.XPS },
                CanRegister = true,
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "rar.ico")
            },
			new DocumentInfo()
            {
				Extension= ".CBZD", DialogDescription = "Dynamic Comic ZIP Files (*.CBZD)", Type= DocumentType.ZIPBased,
                Model=typeof(Book), Service=typeof(BookService), ViewModel = "CBR.ViewModels.ComicViewModel",
                ConversionReader = typeof(RARImageReader), ConversionWriter = typeof(ZIPWriter),
				CanConvertTo = new List<DocumentType>(),
                CanRegister = true,
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "cbrz.ico")
            },
			new DocumentInfo()
            {
				Extension= ".XPS", DialogDescription = "XPS Files (*.XPS)", Type= DocumentType.XPS,
                Model=typeof(Book), Service=typeof(XpsBookService), ViewModel = "CBR.ViewModels.XpsBookViewModel",
                ConversionReader = typeof(XPSImageReader), ConversionWriter = typeof(XPSImageWriter),
				CanConvertTo = new List<DocumentType>() { DocumentType.ImageFile, DocumentType.ZIPBased, DocumentType.HTML },
                CanRegister = false,
				Publisher = typeof(HtmlPublisher)
            },
            new DocumentInfo()
            {
				Extension= ".EPUB", DialogDescription = "ePUB Files (*.EPUB)", Type= DocumentType.ePUB, 
                Model=typeof(Book), Service=typeof(ePUBBookService), ViewModel = "CBR.ViewModels.ePUBBookViewModel2",
                CanConvertTo = new List<DocumentType>() { DocumentType.HTML, DocumentType.XPS },
                CanRegister = true, 
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "epub.ico")
            },
            new DocumentInfo()
            {
				Extension= ".PDF", DialogDescription = "PDF Files (*.PDF)", Type= DocumentType.PDF,
				Model=typeof(Book), Service=typeof(PdfService),
                ConversionReader = typeof(PDFImageReader),
				CanConvertTo = new List<DocumentType>() { DocumentType.ImageFile, DocumentType.ZIPBased, DocumentType.XPS },
                CanRegister = false, 
				IconFile = DirectoryHelper.Combine(CBRFolders.Dependencies, "pdf.ico")
            },


			new DocumentInfo()
            { Extension= ".HTML", DialogDescription = "HTML Files (*.HTML)", Type= DocumentType.HTML,
                ConversionReader = null,
				CanConvertTo = new List<DocumentType>() { DocumentType.ePUB, DocumentType.XPS },
                CanRegister = true,
				Publisher = typeof(HtmlPublisher)
            },
			new DocumentInfo()
            { Extension= ".CSV", DialogDescription = "CSV Files (*.CSV)", Type= DocumentType.CSV,
                ConversionReader = null,
				CanConvertTo = null,
                CanRegister = false,
				Publisher = typeof(HtmlPublisher)
            },
			new DocumentInfo()
            { Extension= ".XML", DialogDescription = "XML Files (*.XML)", Type= DocumentType.XML,
                ConversionReader = null,
				CanConvertTo = null,
                CanRegister = false,
				Publisher = typeof(HtmlPublisher)
            }

        };

		/// <summary>
		/// shortcut to register-able books
		/// </summary>
        public List<DocumentInfo> BookExtensionRegister
        {
            get { return BookFilters.Where(p => p.CanRegister == true).ToList(); }
        }

		/// <summary>
		/// shortcut to edit-able books
		/// </summary>
        public string BookFilterAllEditable
        {
			get { return allExtension.DialogFilter + "|" +
				BookFilters.Where(p => p.Model != null).Select(query => query.DialogFilter).Aggregate((a, b) => a + "|" + b); 
			}
        }

		/// <summary>
		///  shortcut to all books
		/// </summary>
        public string BookFilterAll
        {
			get { return allExtension.DialogFilter + "|" +
				BookFilters.Where(p => p.Model != null).Select(query => query.DialogFilter).Aggregate((a, b) => a + "|" + b); 
			}
        }        

		/// <summary>
		/// default book extension index
		/// </summary>
        public int BookFilterDefaultIndex
        {
            get { return 0; }
        }

		/// <summary>
		/// Find a book DocumentInfo by file extension
		/// </summary>
		/// <param name="ext"></param>
		/// <returns></returns>
		public DocumentInfo FindBookFilterByExt(string ext)
		{
			return BookFilters.Find(p => p.Extension == ext.ToUpper());
		}

		/// <summary>
		/// Find a book DocumentInfo by file extension with an associated Model
		/// </summary>
		/// <param name="ext"></param>
		/// <returns></returns>
        public DocumentInfo FindBookFilterByExtWithModel(string ext)
        {
            return BookFilters.Find(p => p.Extension == ext.ToUpper() && p.Model != null);
        }

        #endregion

        #region ----------------CATALOGS----------------

		/// <summary>
		/// All catalog filters
		/// </summary>
		public string CatalogFilterAll
		{
			get { return BookFilters[CatalogFilterDefaultIndex].DialogFilter + "|" + allExtension.DialogFilter; }
		}

		/// <summary>
		/// the default extension
		/// </summary>
		public string CatalogFilterDefaultExtension
		{
			get { return BookFilters[CatalogFilterDefaultIndex].Extension; }
		}

		/// <summary>
		/// the default extension index
		/// </summary>
		public int CatalogFilterDefaultIndex
		{
			get { return 0; }
		}

		/// <summary>
		/// find DocumentInfo based on file extension
		/// </summary>
		/// <param name="ext"></param>
		/// <returns></returns>
		public DocumentInfo FindCatalogFilterByExt(string ext)
		{
			return BookFilters.Find(p => p.Extension == ext.ToUpper());
		}

        #endregion

        #region ----------------OPERATIONS----------------

        public bool CopyToDevice(string srcPath, string destPath, DeviceInfo device)
        {
            // first, find the source format
            DocumentInfo fe = FindBookFilterByExtWithModel(Path.GetExtension(srcPath));

            // device support the source
            if (device.SupportedFormats.Contains(fe.Type))
            {
                File.Copy(srcPath, destPath, true);  //==> TODO Message for overwrite ?
                return true;
            }
            else
            {
                // find the best matching destination format
                if( fe.Type == DocumentType.ZIPBased )
                {
                    if (device.SupportedFormats.Contains(DocumentType.ImageFile))
                        new RARImageReader().Read(srcPath, Path.Combine(destPath, Path.GetFileNameWithoutExtension(srcPath)), null, null, null, null);
                }
                if( fe.Type == DocumentType.RARBased )
                {
                }
                if( fe.Type == DocumentType.XPS )
                {
                }
                if( fe.Type == DocumentType.ePUB )
                {
                }

                return true;
            }
        }

        #endregion

		/// <summary>
		/// return the corresponding service from the config
		/// </summary>
		/// <param name="bk"></param>
		/// <returns></returns>
		public BookServiceBase GetService(Book bk)
		{
			if (bk == null)
				return new BookServiceBase();
			else
				return GetService(bk.FilePath);
		}

		/// <summary>
		/// return the corresponding service from the config
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public BookServiceBase GetService(string filePath)
		{
			if( LogHelper.CanDebug() )
				LogHelper.Begin("DocumentFactory.GetService", "filePath {0}", filePath);
			try
			{
				return (BookServiceBase)Activator.CreateInstance(
					DocumentFactory.Instance.FindBookFilterByExtWithModel(Path.GetExtension(filePath)).Service
					);
			}
			catch (Exception err)
			{
				LogHelper.Manage("DocumentFactory.GetService", err);
				return null;
			}
			finally
			{
				LogHelper.End("DocumentFactory.GetService");
			}  
		}

		/// <summary>
		/// return the corresponding view model from the config
		/// </summary>
		/// <param name="bk"></param>
		/// <returns></returns>
		public string GetViewModel(Book bk)
		{
			if (LogHelper.CanDebug())
				LogHelper.Begin("DocumentFactory.GetViewModel", "Book {0}", bk.ToString());
			try
			{
				return GetViewModel(bk.FilePath);
			}
			catch (Exception err)
			{
				LogHelper.Manage("DocumentFactory.GetViewModel", err);
				return string.Empty;			}
			finally
			{
				LogHelper.End("DocumentFactory.GetViewModel");
			}
		}

		/// <summary>
		/// return the corresponding view model from the config
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public string GetViewModel(string filePath)
		{
			if (LogHelper.CanDebug()) 
				LogHelper.Begin("DocumentFactory.GetViewModel", "filePath {0}", filePath);
			try
			{
				DocumentInfo fe = DocumentFactory.Instance.FindBookFilterByExtWithModel(Path.GetExtension(filePath));
				if (fe != null)
					return fe.ViewModel;
				else
					return string.Empty;
			}
			catch (Exception err)
			{
				LogHelper.Manage("DocumentFactory.GetViewModel", err);
				return string.Empty;
			}
			finally
			{
				LogHelper.End("DocumentFactory.GetViewModel");
			}
		}
    }
}
