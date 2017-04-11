using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Windows.Media.Imaging;
using CBR.Core.Helpers;
using CBR.Core.Helpers.NET.Properties;
using CBR.Core.Services;
using GalaSoft.MvvmLight;

namespace CBR.Core.Models
{
	/// <summary>
	/// The book model
	/// </summary>
	public class Book : ViewModelBase
	{
		#region -----------------CONSTRUCTOR-----------------

		/// <summary>
		/// ctor
		/// </summary>
		public Book()
		{
        }

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="fileinfo"></param>
		/// <param name="filePath"></param>
		public Book(string fileinfo, string filePath)
		{
			FilePath = filePath;
			BookInfoFilePath = fileinfo;
		}
		#endregion

		#region -----------------PROPERTIES-----------------

        private object _Tag = null;
        [Browsable(true)]
        public object Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

		private string _bookInfoFilePath = string.Empty;
        [Browsable(true)]
        public string BookInfoFilePath
		{
			get { return _bookInfoFilePath; }
			set { _bookInfoFilePath = value; IsDirty = true; RaisePropertyChanged("BookInfoFilePath"); }
		}

        private string _filePath = string.Empty;
        [Browsable(true)]
		[Description("File path")]
		[UserPropertyAttribute(false, true, "Core.Properties.FilePath")]
		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; IsDirty = true; RaisePropertyChanged("FilePath"); }
		}

		private List<Page> _Pages = null;
        [Browsable(true)]
		public List<Page> Pages
		{
			get { if (_Pages == null) _Pages = new List<Page>(); return _Pages; }
			set { _Pages = value; IsDirty = true; }
		}

        private string _Bookmark = string.Empty;
        [Browsable(true)]
        [Description("Bookmarked page")]
        public string Bookmark
		{
			get { return _Bookmark; }
			set { _Bookmark = value; IsDirty = true; RaisePropertyChanged("HasBookmark"); }
		}

        private bool _IsRead = false;
        [Browsable(true)]
        [Description("Read flag")]
		[UserPropertyAttribute(true, true, "Core.Properties.IsRead")]
		public bool IsRead
        {
            get { return _IsRead; }
			set { _IsRead = value; IsDirty = true; RaisePropertyChanged("IsRead"); }
        }

        private bool _IsSecured = false;
        [Browsable(true)]
        [Description("Secured")]
		[UserPropertyAttribute(true, true, "Core.Properties.IsSecured")]
		public bool IsSecured
        {
            get { return _IsSecured; }
			set { _IsSecured = value; IsDirty = true; RaisePropertyChanged("IsSecured"); }
        }
        
        private string _Password= string.Empty;
        [Browsable(true)]
        public string Password
        {
            get { return _Password; }
			set { _Password = value; IsDirty = true; }
        }

		private BitmapImage _Cover = null;
        [Browsable(true)]
        public BitmapImage Cover
		{
			get { return _Cover; }
			set { _Cover = value; IsDirty = true; RaisePropertyChanged("Cover"); }
		}

        private int _PageCount;
        [Browsable(true)]
        [Description("Page count")]
		[UserPropertyAttribute(true, true, "Core.Properties.PageCount")]
		public int PageCount
		{
            get { return _PageCount; }
            set { _PageCount = value; IsDirty = true; RaisePropertyChanged("PageCount"); }
		}

        private long _Size;
        [Browsable(true)]
        [Description("File size")]
		[UserPropertyAttribute(false, true, "Core.Properties.Size")]
		public long Size
		{
			get { return _Size; }
			set { _Size = value; IsDirty = true; RaisePropertyChanged("Size"); }
		}

        private int _Rating;
        [Browsable(true)]
        [Description("Rating")]
		[UserPropertyAttribute(true, true, "Core.Properties.Rating")]
		public int Rating
		{
			get { return _Rating; }
			set { _Rating = value; IsDirty = true; RaisePropertyChanged("Rating"); }
		}

        [Browsable(true)]
        public bool IsDirty { get; set; }

        private dynamic _dynamics = new ExpandoObject();
        [Browsable(true)]
        public dynamic Dynamics
        {
            get { return _dynamics; }
            set { _dynamics = value; RaisePropertyChanged("Dynamics"); }
        }

		#endregion

		#region -----------------internal and calculated-----------------

		[Browsable(true)]
		[Description("Bookmarked")]
		[UserPropertyAttribute(true, true, "Core.Properties.HasBookmark")]
		public bool HasBookmark
		{
			get { return !string.IsNullOrEmpty(_Bookmark); }
		}

        [Browsable(true)]
        [Description("File name")]
		[UserPropertyAttribute(false, true, "Core.Properties.FileName")]
		public string FileName
		{
			get { return Path.GetFileName(this._filePath); }
		}

        [Browsable(true)]
        [Description("Folder")]
		[UserPropertyAttribute(true, true, "Core.Properties.Folder")]
		public string Folder
        {
            get { return Path.GetDirectoryName(this._filePath); }
        }

        [Browsable(true)]
        [Description("File extension")]
		[UserPropertyAttribute(true, true, "Core.Properties.FileExtension")]
		public string FileExtension
        {
            get { return Path.GetExtension(this._filePath); }
        }

        private DocumentType _Type = DocumentType.None;
        [Browsable(true)]
        [Description("Type of")]
		[UserPropertyAttribute(true, true, "Core.Properties.Type")]
		public DocumentType Type
        {
            get
            {
                if (_Type == DocumentType.None)
                    _Type = DocumentFactory.Instance.FindBookFilterByExtWithModel(FileExtension).Type;

                return _Type;
            }
        }

		/// <summary>
		/// debug helper
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("BookInfoFilePath:{0}, FilePath:{1}",
				this.BookInfoFilePath, this.FilePath);
		}

		#endregion
	}
}
