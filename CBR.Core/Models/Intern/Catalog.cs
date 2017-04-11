using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace CBR.Core.Models
{
	[Serializable]
	public class Catalog
	{
		#region ----------------CONSTRUCTORS----------------

		public Catalog()
		{
		}

		public Catalog(string path)
		{
			CatalogFilePath = path;
			IsDirty = true;
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		private string _bookFolder = string.Empty;
		/// <summary>
		/// Which folder to scan to get books
		/// </summary>
		public string BookFolder
		{
			get { return _bookFolder; }
			set { _bookFolder = value; IsDirty = true; }
		}

		private List<string> _BookInfoFilePath = null;
		/// <summary>
		/// All internal file book information files
		/// </summary>
		public List<string> BookInfoFilePath
		{
			get { if (_BookInfoFilePath == null) _BookInfoFilePath = new List<string>(); return _BookInfoFilePath; }
			set { _BookInfoFilePath = value; IsDirty = true; }
		}

		[field: NonSerialized]
		private string _catalogFilePath = string.Empty;
		/// <summary>
		/// The file path to this catalog file
		/// </summary>
		public string CatalogFilePath
		{
			get { return _catalogFilePath; }
			set { _catalogFilePath = value; }
		}

		[field: NonSerialized]
		private ObservableCollection<Book> _Books = null;
		/// <summary>
		/// Book object list in the catalog
		/// </summary>
		public ObservableCollection<Book> Books
		{
			get { if (_Books == null) _Books = new ObservableCollection<Book>(); return _Books; }
			set { _Books = value; IsDirty = true;  }
		}

		/// <summary>
		/// As changed for some reasons, will have to be saved
		/// </summary>
		[field: NonSerialized]
		private bool _IsDirty = false;
		public bool IsDirty
		{
			get { return _IsDirty; }
			set { _IsDirty = value; }
		}

		#endregion		

		public override string ToString()
		{
			return string.Format("CatalogFilePath:{0}, BookFolder:{1}, Books.Count{2}, IsDirty:{3}", 
				this.CatalogFilePath, this.BookFolder, this.Books.Count, this.IsDirty);
		}

        private string _Title = string.Empty;
        public string Title
        {
			get
			{
				if (string.IsNullOrEmpty(_Title))
					_Title = Path.GetFileName(CatalogFilePath);

				return _Title;
			}
            set { _Title = value; IsDirty = true; }
        }

        private string _Description = string.Empty;
        public string Description
        {
			get
			{
				if (string.IsNullOrEmpty(_Description))
					_Description = this.Title; 
				return _Description;
			}
            set { _Description = value; IsDirty = true; }
        }

        private bool _IsShared = false;
        public bool IsShared
        {
            get { return _IsShared; }
            set { _IsShared = value; IsDirty = true; }
        }

        private Uri _CoverUri;
        public Uri CoverUri
        {
            get { return _CoverUri; }
            set { _CoverUri = value; IsDirty = true; }
        }
	}
}
