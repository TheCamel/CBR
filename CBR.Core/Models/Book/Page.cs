using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using CBR.Core.Services;

namespace CBR.Core.Models
{
	/// <summary>
	/// The page model, represent any paging entity depending on the book model
	/// </summary>
	public class Page
	{
		#region -----------------constructors-----------------

		/// <summary>
		/// ctor
		/// </summary>
		public Page()
		{
		}

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="index"></param>
		public Page(Book parent, int index)
		{
			Parent = parent;
			Index = index;
		}

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="filePath"></param>
		/// <param name="index"></param>
		public Page(Book parent, string filePath, int index)
		{
			Parent = parent;
			Index = index;
			FilePath = filePath;
			FileName = new FileInfo(FilePath).Name;
		}

		#endregion

		#region -----------------properties-----------------

		/// <summary>
		/// index to order pages
		/// </summary>
		public int Index
		{
			get;
			set;
		}

		/// <summary>
		/// full file path
		/// </summary>
		public string FilePath
		{
			get;
			set;
		}

		/// <summary>
		/// file name only
		/// </summary>
		public string FileName
		{
			get;
			set;
		}

		private ObservableCollection<Zone> _Zones = null;
		/// <summary>
		/// frame collection for dynamic book
		/// </summary>
		public ObservableCollection<Zone> Frames
		{
			get { if (_Zones == null) _Zones = new ObservableCollection<Zone>(); return _Zones; }
			set { _Zones = value; }
		}

		#endregion

		#region -----------------calculated-----------------

		private BitmapImage _Image = null;
		/// <summary>
		/// the image 
		/// </summary>
		public BitmapImage Image
		{
			get
			{
				if (_Image == null)
					_Image = (DocumentFactory.Instance.GetService(Parent) as BookService).GetImageFromStream(Parent.FilePath, FilePath);

				ImageLastAcces = DateTime.Now;
				return _Image;
			}
			set { _Image = value; }
		}

		/// <summary>
		/// tag on image existance
		/// </summary>
		public bool ImageExist
		{
			get { return _Image == null ? false : true; }
		}

		/// <summary>
		/// tag on last image access
		/// </summary>
		public DateTime ImageLastAcces
		{
			get;
			set;
		}

		/// <summary>
		/// book parent entity
		/// </summary>
		public Book Parent
		{
			get;
			set;
		}

		/// <summary>
		/// debug helper
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("FileName:{0}, FilePath:{1}, Index:{2}, Parent:{3}",
				this.FileName, this.FilePath, this.Index, this.Parent.FileName);
		}
		#endregion
	}
}
