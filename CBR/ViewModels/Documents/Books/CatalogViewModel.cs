using System;
using System.IO;
using System.Windows.Media.Imaging;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class CatalogViewModel : ViewModelBaseExtended
	{
		#region ----------------CONSTRUCTOR----------------

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="inf"></param>
		public CatalogViewModel(Catalog data)
		{
			Data = data;
			Messenger.Default.Register<string>(this, ViewModelMessages.CatalogRefreshCover, HandleCoverUpdate);
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			Messenger.Default.Unregister<string>(this, ViewModelMessages.CatalogRefreshCover, HandleCoverUpdate);
		}

		#endregion

		#region ----------------PROPERTIES----------------

		private Uri _DefaultCoverUri = new Uri("pack://application:,,,/Resources/Images/book128.png");

		new public Catalog Data
		{
			get { return base.Data as Catalog; }
			set
			{
				base.Data = value;

				BookCount = value.BookInfoFilePath.Count;
				Title = value.Title;
				Description = value.Description;
				IsShared = value.IsShared;
				//RaisePropertyChanged("CoverImage");
			}
		}

		private int _BookCount;
		public int BookCount
		{
			get { return _BookCount; }
			set
			{
				if (_BookCount != value)
				{
					_BookCount = value;
					RaisePropertyChanged("BookCount");
				}
			}
		}

		private string _Title;
		public string Title
		{
			get { return _Title; }
			set
			{
				if (_Title != value)
				{
					_Title = value;
					RaisePropertyChanged("Title");
				}
			}
		}

		private string _Description;
		public string Description
		{
			get { return _Description; }
			set
			{
				if (_Description != value)
				{
					_Description = value;
					Data.Description = _Description;
					RaisePropertyChanged("Description");
				}
			}
		}

		private bool _IsShared;
		public bool IsShared
		{
			get { return _IsShared; }
			set
			{
				if (_IsShared != value)
				{
					_IsShared = value;
					Data.IsShared = _IsShared;
					RaisePropertyChanged("IsShared");
				}
			}
		}

		private BitmapImage _CoverImage = null;
		/// <summary>
		/// the image 
		/// </summary>
		public BitmapImage CoverImage
		{
			get
			{
				if (_CoverImage == null)
					_CoverImage = CatalogService.Instance.GenerateCatalogCover(Data, true);
				return _CoverImage;
			}
			set { _CoverImage = value; }
		}

		#endregion

		#region -----------------HANDLERS-----------------

		private void HandleCoverUpdate(string param)
		{
			if (param == this.Data.CatalogFilePath)
			{
				BitmapImage tmp = CatalogService.Instance.GenerateCatalogCover(Data, false);
				if (tmp != null)
				{
					_CoverImage = tmp;
					RaisePropertyChanged("CoverImage");
				}
			}
		}
		#endregion
	}
}
