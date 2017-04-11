using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CBR.Components.Controls;
using CBR.Components.Dialogs;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace CBR.ViewModels
{
	public class ComicViewModel : BookViewModelBase
	{
		#region ----------------CONSTRUCTOR----------------

		public ComicViewModel(Book bk)
			: base(bk)
        {
            this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type.png";

			if (Data != null)
                CurrentPage = Service.LoadBook(Data) as Page;
		}

		public ComicViewModel(Book bk, int currentPageIndex, DisplayFitMode mode, double previous)
			: base(bk)
		{
			if (Data != null)
				CurrentPage = Data.Pages[currentPageIndex];

			PreviousScale = previous;
			FitMode = mode;
		}

		#endregion

		#region -----------------PROPERTIES-----------------

		private Page _currentPage = null;
		public Page CurrentPage
		{
			get { return _currentPage; }
			set
			{
				if (_currentPage != value)
				{
					_currentPage = value;
					RaisePropertyChanged("CurrentPage");
					RaisePropertyChanged("ImgSource");
					RaisePropertyChanged("PageInfo");
					RaisePropertyChanged("CacheInfo");
                    RaisePropertyChanged("FrameList");
				}
			}
		}

		public BitmapImage ImgSource
		{
			get { if (_currentPage != null) return _currentPage.Image; else return null; }
			set
			{
				if (_currentPage.Image != value)
				{
					_currentPage.Image = value;
                    RaisePropertyChanged("ImgSource");
                    RaisePropertyChanged("FrameList");
				}
			}
		}

		public ObservableCollection<Zone> FrameList
        {
            get { if (_currentPage != null) return _currentPage.Frames; else return null; }
            set
            {
                if (_currentPage.Frames != value)
                {
                    _currentPage.Frames = value;
                    RaisePropertyChanged("FrameList");
                }
            }
        }

		new public string PageInfo
		{
			get
			{
				if (Data != null && _currentPage != null)
					return string.Format("Page : {0} ({1}/{2})", CurrentPage.FilePath, CurrentPage.Index, Data.PageCount);
				else
					return string.Empty;
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

		#region simulate command
		private ICommand simulateCommand;
        public ICommand SimulateCommand
        {
            get
            {
                if (simulateCommand == null)
					simulateCommand = new RelayCommand<string>(SimulateCommandExecute, SimulateCommandCanExecute);
                return simulateCommand;
            }
        }

		public bool SimulateCommandCanExecute(string param)
		{
			return Data != null && Service.IsDynamic(Data);
		}

		public void SimulateCommandExecute(string param)
        {
			SimulateDialog Dlg = new SimulateDialog();
			if (!param.Equals("Phone"))
			{
				string[] screenParams = param.Split( ';' );
				double screenSizeX, screenSizeY;
				if (double.TryParse(screenParams[0], out screenSizeX) && double.TryParse(screenParams[1], out screenSizeY))
				{
					Dlg.Width = screenSizeX * 0.039370078740157 * 96;
					Dlg.Height = screenSizeY * 0.039370078740157 * 96;
				}
				Dlg.Content.Style = (Style)Dlg.FindResource("TabletGridContainer");
			}
			else
			{
				Dlg.RenderSize = new Size(680, 400);
				Dlg.Content.Style = (Style)Dlg.FindResource("PhoneGridContainer");
			}
            Dlg.Owner = Application.Current.MainWindow;
            Dlg.BookData = Data;
            Dlg.ShowDialog();
        }
        #endregion

		#region edit command
		
		override public bool CanEditBook(Book bk)
		{
			return true;
		}

		#endregion

        #region debug page command
        private ICommand debugPageCommand;
        public ICommand DebugPageCommand
        {
            get
            {
                if (debugPageCommand == null)
                    debugPageCommand = new RelayCommand(SaveCommandExecute, delegate() { return Data != null && Service.IsDynamic(Data); });
                return debugPageCommand;
            }
        }

        public void DebugPage()
        {
            if (Data != null)
            {
            }
        }
        #endregion

        #region save command

        override public bool SaveCommandCanExecute()
        {
            return Data != null && Service.IsDynamic(Data);
        }

        override public void SaveCommandExecute()
        {
            if (Data != null)
            {
                Data = Service.SaveBook(Data);
                CurrentPage = (Page)Service.LoadBook(Data);
            }
        }

        #endregion

        #region close command

		/// <summary>
		/// Child classes can override this method to perform 
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();
			CurrentPage = null;
		}

        #endregion

        #region bookmark commands

        override public bool BookmarkCommandCanExecute()
        {
            return CurrentPage != null;
        }

        override public void BookmarkCommandExecute()
        {
            Service.SetMark(Data, _currentPage);
        }

        override public bool GotoBookmarkCommandCanExecute()
        {
            return Service.HasMark(Data);
        }

        override public void GotoBookmarkCommandExecute()
        {
            CurrentPage = Service.GotoMark(Data);
        }

        #endregion

        #region fit mode command

        override public bool FitModeCommandCanExecute(string param)
        {
            return CurrentPage != null;
        }

        #endregion

        #region change page command

        override public bool CanGotoPage(string step)
        {
            return Service.CheckPageRange(Data, _currentPage, Convert.ToInt32(step));
        }

        override public void GotoPage(string step)
        {
            CurrentPage = Service.GotoPage(Data, _currentPage, Convert.ToInt32(step));

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
                CurrentPage = Service.GotoPage(Data, pageNumber);
        }

        override public void GotoLastPageCommandExecute(string param)
        {
            if (Data != null)
                CurrentPage = Service.GotoPage(Data, Data.PageCount);
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
