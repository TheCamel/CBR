using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Helpers;
using CBR.Core.Models;
using CBR.Core.Services;
using System.Windows.Input;
using System.Windows.Threading;
using CBR.Core.Files;
using CBR.Components;
using CBR.Components.Controls;
using System.Collections;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;

namespace CBR.ViewModels
{
	public class BookViewModelBase : DocumentViewModel
	{
		#region ----------------CONSTRUCTOR----------------

		public BookViewModelBase(Book bk)
		{
			if (bk != null)
			{
				Service = DocumentFactory.Instance.GetService(bk);
                Data = bk;

                //this is used by avalon seralizer to re create the model
                this.ContentId = "BookViewModelBase;" + bk.FilePath;
			}

			Messenger.Default.Register<WorkspaceInfo>(this, ViewModelMessages.SettingsChanged, HandleSettingsChange);
			Messenger.Default.Register<WorkspaceInfo>(this, ViewModelMessages.ExtendedSettingsChanged, HandleExtendedSettingsChange);

			if (Service.CanManageCache())
			{
				//create a dispatch timer to load the image cache
				_CacheTimerClock = new DispatcherTimer();
				_CacheTimerClock.Interval = new TimeSpan(0, 0, 5);
				_CacheTimerClock.IsEnabled = true;
				_CacheTimerClock.Tick += new EventHandler(CacheTimerClockElapsed);
			}

			PreviousScale = Scale;
		}

		/// <summary>
		/// Child classes can override this method to perform clean-up logic, such as removing event handlers.
		/// </summary>
		override public void Cleanup()
		{
			base.Cleanup();

			if (Data != null)
				Service.UnloadBook(Data);
			Data = null;

			if (_CacheTimerClock != null)
			{
				_CacheTimerClock.IsEnabled = false;
				_CacheTimerClock = null;
			}

			Messenger.Default.Unregister(this);
		}

		#endregion

		#region -----------------PROPERTIES-----------------

        private DispatcherTimer _CacheTimerClock;

		private bool _ShowZoomFlyer = WorkspaceService.Instance.Settings.Extended.ShowZoomFlyer;
		public bool ShowZoomFlyer
		{
			get { return _ShowZoomFlyer; }
			set
			{
				if (_ShowZoomFlyer != value)
				{
					_ShowZoomFlyer = value;
					RaisePropertyChanged("ShowZoomFlyer");
				}
			}
		}

		private double _MagnifierSize = WorkspaceService.Instance.Settings.MagnifierSize;
		public double MagnifierSize
		{
			get { return _MagnifierSize; }
			set
			{
				if (_MagnifierSize != value)
				{
					_MagnifierSize = value;
					RaisePropertyChanged("MagnifierSize");
				}
			}
		}

		private double _MagnifierScale = WorkspaceService.Instance.Settings.MagnifierScaleFactor;
		public double MagnifierScale
		{
			get { return _MagnifierScale; }
			set
			{
				if (_MagnifierScale != value)
				{
					_MagnifierScale = value;
					RaisePropertyChanged("MagnifierScale");
				}
			}
		}

        new public Book Data
        {
            get { return base.Data as Book; }
			set
			{
				base.Data = value;
				if( base.Data!=null)
					DisplayName = (base.Data as Book).FileName;
			}
        }

		/// <summary>
		/// backup before a fit mode
		/// </summary>
		public double PreviousScale { get; set; }

        private double _scale = 1;
        /// <summary>
        /// Scale factor
        /// </summary>
		public double Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;

                    RaisePropertyChanged("Scale");

					//back it up
					if (_FitMode == DisplayFitMode.None)
						PreviousScale = _scale;
                }
            }
        }

		private DisplayFitMode _FitMode = (DisplayFitMode)WorkspaceService.Instance.Settings.AutoFitMode;
		/// <summary>
		/// Fit mode
		/// </summary>
		public DisplayFitMode FitMode
		{
			get { return _FitMode; }
			set
			{
				if (_FitMode != value)
				{
					//back it up
					if (_FitMode == DisplayFitMode.None)
						PreviousScale = Scale;

					_FitMode = value;
					RaisePropertyChanged("FitMode");

					if (_FitMode == DisplayFitMode.None)
						Scale = PreviousScale;
				}
			}
		}

        public BookServiceBase Service { get; set; }

        public long CacheSize { get; set; }

        public string CacheInfo
        {
            get
            {
                return "No cache information";
            }
        }

        public string PageInfo
        {
            get
            {
                return string.Empty;
            }
        }

        private bool _isInEditMode = false;
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (_isInEditMode != value)
                {
                    _isInEditMode = value;
                    RaisePropertyChanged("IsInEditMode");
					RaisePropertyChanged("IsDynamic");
                }
            }
        }

		public bool IsDynamic
		{
			get { return _isInEditMode || Service.IsDynamic(Data); }
		}



		virtual public bool HasTableOfContent
		{
			get { return false; }
		}

		virtual public IList TableOfContent
		{
			get { return null; }
		}

		virtual public object TableOfContentIndex
		{
			set { return; }
		}

        #endregion

        #region -----------------COMMANDS-----------------

		#region print command

		private ICommand bookPrintCommand;
		public ICommand BookPrintCommand
		{
			get
			{
				if (bookPrintCommand == null)
					bookPrintCommand = new RelayCommand(PrintCommandExecute, PrintCommandCanExecute);
				return bookPrintCommand;
			}
		}

		virtual public bool PrintCommandCanExecute()
		{
			return false;
		}

		virtual public void PrintCommandExecute()
		{
		}

		#endregion

        #region save command

        private ICommand bookSaveCommand;
        public ICommand BookSaveCommand
        {
            get
            {
                if (bookSaveCommand == null)
                    bookSaveCommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
                return bookSaveCommand;
            }
        }

        virtual public bool SaveCommandCanExecute()
        {
            return false;
        }

        virtual public void SaveCommandExecute()
        {
        }

        #endregion

        #region bookmark command

        private ICommand bookmarkCommand;
        public ICommand BookmarkCommand
        {
            get
            {
                if (bookmarkCommand == null)
                    bookmarkCommand = new RelayCommand(BookmarkCommandExecute, BookmarkCommandCanExecute);
                return bookmarkCommand;
            }
        }

        virtual public bool BookmarkCommandCanExecute()
        {
            return false;
        }

        virtual public void BookmarkCommandExecute()
        {
        }

        #endregion

        #region goto bookmark command

        private ICommand gotoBookmarkCommand;
        public ICommand GotoBookmarkCommand
        {
            get
            {
                if (gotoBookmarkCommand == null)
                    gotoBookmarkCommand = new RelayCommand(GotoBookmarkCommandExecute, GotoBookmarkCommandCanExecute);
                return gotoBookmarkCommand;
            }
        }

        virtual public bool GotoBookmarkCommandCanExecute()
        {
            return Service.HasMark(Data);
        }

        virtual public void GotoBookmarkCommandExecute()
        {
        }

        #endregion

        #region clear bookmark command

        private ICommand clearBookmarkCommand;
        public ICommand ClearBookmarkCommand
        {
            get
            {
                if (clearBookmarkCommand == null)
                    clearBookmarkCommand = new RelayCommand(ClearBookmarkCommandExecute, ClearBookmarkCommandCanExecute);
                return clearBookmarkCommand;
            }
        }

        virtual public bool ClearBookmarkCommandCanExecute()
        {
            return Service.HasMark(Data);
        }

        virtual public void ClearBookmarkCommandExecute()
        {
            Service.ClearMark(Data);
        }

        #endregion

        #region fit mode command

        private ICommand fitModeCommand;
        public ICommand FitModeCommand
        {
            get
            {
                if (fitModeCommand == null)
                    fitModeCommand = new RelayCommand<string>(FitModeCommandExecute, FitModeCommandCanExecute);
                return fitModeCommand;
            }
        }

        virtual public bool FitModeCommandCanExecute(string param)
        {
            return false;
        }

        virtual public void FitModeCommandExecute(string param)
        {
			if (param == "None")
				FitMode = DisplayFitMode.None;
			if (param == "Width")
				FitMode = DisplayFitMode.Width;
			if (param == "Height")
				FitMode = DisplayFitMode.Height;
        }

        #endregion

		#region two page view command
		private ICommand twoPageCommand;
        public ICommand TwoPageCommand
        {
            get
            {
				if (twoPageCommand == null)
					twoPageCommand = new RelayCommand(TwoPageCommandExecute, TwoPageCommandCanExecute);
				return twoPageCommand;
            }
        }

		virtual public bool TwoPageCommandCanExecute()
        {
            return false;
        }

		virtual public void TwoPageCommandExecute()
        {
		}

        #endregion

        #region change page command
        private ICommand bookChangePageCommand;
        public ICommand BookChangePageCommand
        {
            get
            {
                if (bookChangePageCommand == null)
                    bookChangePageCommand = new RelayCommand<string>(GotoPage, CanGotoPage);
                return bookChangePageCommand;
            }
        }

        virtual public bool CanGotoPage(string step)
        {
            return false;
        }

        virtual public void GotoPage(string step)
        {
            return;

        }
        #endregion

        #region goto page command

        private ICommand bookGotoPageCommand;
        public ICommand BookGotoPageCommand
        {
            get
            {
                if (bookGotoPageCommand == null)
                    bookGotoPageCommand = new RelayCommand<string>(GotoPageCommandExecute, GotoPageCommandCanExecute);
                return bookGotoPageCommand;
            }
        }

        virtual public bool GotoPageCommandCanExecute(string param)
        {
            if (Data != null)
                return Service.CanGotoPage(Data, Convert.ToInt32(param));
            else
                return false;
        }

        virtual public void GotoPageCommandExecute(string param)
        {
            if (Data != null)
                Service.GotoPage(Data, Convert.ToInt32(param));
        }

        #endregion

        #region goto page command

        private ICommand bookGotoLastPageCommand;
        public ICommand BookGotoLastPageCommand
        {
            get
            {
                if (bookGotoLastPageCommand == null)
                    bookGotoLastPageCommand = new RelayCommand<string>(GotoLastPageCommandExecute, delegate(string param) { return true; });
                return bookGotoLastPageCommand;
            }
        }

        virtual public void GotoLastPageCommandExecute(string param)
        {
            if (Data != null)
                Service.GotoPage(Data, Data.PageCount);
        }

        #endregion

		#region edit command
		private ICommand bookEditCommand;
		public ICommand BookEditCommand
		{
			get
			{
				if (bookEditCommand == null)
					bookEditCommand = new RelayCommand<Book>(EditBook, CanEditBook);
				return bookEditCommand;
			}
		}

		virtual public bool CanEditBook(Book bk)
		{
			return false;
		}

		virtual public void EditBook(Book bk)
		{
			try
			{
				IsInEditMode = !IsInEditMode;
			}
			catch (Exception err)
			{
				LogHelper.Manage("BookViewModelBase:EditBook", err);
			}
		}
		#endregion

        #endregion

        #region -----------------HANDLERS-----------------

		private void HandleSettingsChange( WorkspaceInfo o )
		{
			MagnifierSize = WorkspaceService.Instance.Settings.MagnifierSize;
			MagnifierScale = WorkspaceService.Instance.Settings.MagnifierScaleFactor;
		}

		private void HandleExtendedSettingsChange(WorkspaceInfo o)
		{
			ShowZoomFlyer = WorkspaceService.Instance.Settings.Extended.ShowZoomFlyer;
		}

        public void CacheTimerClockElapsed(object tag, EventArgs args)
        {
            try
            {
                if (this.Data != null)
                {
                    CacheSize = Service.ManageCache(this.Data);
                    RaisePropertyChanged("CacheInfo");
                }
            }
            catch (Exception err)
            {
                LogHelper.Manage("BookViewModelBase:CacheTimerClockElapsed", err);
            }
        }

        #endregion
    }
}
