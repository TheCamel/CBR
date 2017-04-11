using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Models;
using CBR.Core.Helpers;
using CBR.Components.Controls;
using System.Collections.ObjectModel;
using CBR.Core.Services;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class TwoPageViewModel : BookViewModelBase
	{
		#region ----------------CONSTRUCTOR----------------

		public TwoPageViewModel(Book bk)
			: base(bk)
		{
            this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type.png";

			if (Data != null && Data.Pages.Count == 0 )
				Service.LoadBook(Data);

			Pages = new ObservableCollection<Page>(Data.Pages);
		}

		public TwoPageViewModel(Book bk, int currentPageIndex, DisplayFitMode mode, double previous)
			: this(bk)
		{
			CurrentPageIndex = currentPageIndex;
			PreviousScale = previous;
			FitMode = mode;
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		private ObservableCollection<Page> _pages = null;
		public ObservableCollection<Page> Pages
		{
			get { return _pages; }
			set
			{
				if (_pages != value)
				{
					_pages = value;
					RaisePropertyChanged("Pages");
				}
			}
		}

		private int _currentPageIndex;
		public int CurrentPageIndex
		{
			get { return _currentPageIndex; }
			set
			{
				if (_currentPageIndex != value)
				{
					_currentPageIndex = value;
					RaisePropertyChanged("CurrentPageIndex");
				}
			}
		}

		new public string CacheInfo
		{
			get
			{
				if (Data != null)
					return string.Format("Image in cache: {0}/{1}; Size: {2} Mo",
						Data.Pages.Where(p => p.ImageExist == true).Count(),
						WorkspaceService.Instance.Settings.ImageCacheCount, CacheSize
				);
				else
					return string.Empty;
			}
		}

		#endregion

		#region -----------------COMMANDS-----------------

		#region fit mode command

		override public bool FitModeCommandCanExecute(string param)
		{
			return true;
		}

		#endregion

		#region bookmark commands

		override public bool BookmarkCommandCanExecute()
		{
			return Data.Pages[CurrentPageIndex] != null;
		}

		override public void BookmarkCommandExecute()
		{
			Service.SetMark(Data, Data.Pages[CurrentPageIndex]);
		}

		override public bool GotoBookmarkCommandCanExecute()
		{
			return Service.HasMark(Data);
		}

		override public void GotoBookmarkCommandExecute()
		{
			CurrentPageIndex = Service.GotoMark(Data).Index;
		}

		#endregion

		#region change page command

		override public bool CanGotoPage(string step)
		{
			return Service.CheckPageRange(Data, Data.Pages[CurrentPageIndex], Convert.ToInt32(step)*3);
		}

		override public void GotoPage(string step)
		{
			CurrentPageIndex = Service.GotoPage(Data, Data.Pages[CurrentPageIndex], Convert.ToInt32(step)*3).Index;

		}
		#endregion

		#region goto page command

		override public bool GotoPageCommandCanExecute(string param)
		{
			int pageNumber = 0;
			if (Data != null && Int32.TryParse(param, out pageNumber))
				return Service.CanGotoPage(Data, pageNumber);
			else
				return true;
		}

		override public void GotoPageCommandExecute(string param)
		{
			int pageNumber = 0;
			if (Data != null && Int32.TryParse(param, out pageNumber))
				CurrentPageIndex = Service.GotoPage(Data, pageNumber).Index;
		}

		override public void GotoLastPageCommandExecute(string param)
		{
			if (Data != null)
				CurrentPageIndex = Service.GotoPage(Data, Data.PageCount).Index;
		}

		#endregion

		#region two page view command

		override public bool TwoPageCommandCanExecute()
		{
			return true;
		}

		override public void TwoPageCommandExecute()
		{
			Messenger.Default.Send<BookViewModelBase>(this, ViewModelMessages.SwapTwoPageView);
		}

		#endregion

		#endregion
	}
}
