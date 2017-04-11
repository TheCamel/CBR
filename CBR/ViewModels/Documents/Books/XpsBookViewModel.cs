using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBR.Core.Services;
using CBR.Core.Models;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Controls;
using CBR.Views;
using CBR.Components.Controls;

namespace CBR.ViewModels
{
    public class XpsBookViewModel : BookViewModelBase
    {
        #region ----------------CONSTRUCTOR----------------

        public XpsBookViewModel(Book bk)
			: base(bk)
		{
            this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type_xps.png";

            if (Data != null)
                DocumentContent = Service.LoadBook(Data) as IDocumentPaginatorSource;
        }

		#endregion

        #region -----------------PROPERTIES-----------------

		private UserControl _injectedView = null;
		public UserControl InjectedView { set { _injectedView = value; } } 

        public IDocumentPaginatorSource DocumentContent { get; set; }

        new public string PageInfo
        {
            get
            {
                if (Data != null)
                    return string.Format("{0} pages in this book", Data.PageCount);
                else
                    return string.Empty;
            }
        }

        #endregion

		#region -----------------COMMANDS-----------------

		#region print command

		override public bool PrintCommandCanExecute()
		{
			return true;
		}

		override public void PrintCommandExecute()
		{
			(_injectedView as XpsBookView).Viewer.Print();
		}

		#endregion

		#region bookmark command

		override public bool BookmarkCommandCanExecute()
		{
			return true;
		}

		override public void BookmarkCommandExecute()
		{
			string pos = (_injectedView as XpsBookView).Viewer.MasterPageNumber.ToString();
			Service.SetMark(Data, Data.Pages.First(p=>p.FilePath == pos) );
		}

		#endregion

		#region goto bookmark command

		override public bool GotoBookmarkCommandCanExecute()
		{
			return Service.HasMark(Data);
		}

		override public void GotoBookmarkCommandExecute()
		{
			CBR.Core.Models.Page pg = Service.GotoMark(Data);
			(_injectedView as XpsBookView).Viewer.GoToPage(pg.Index);
		}

		#endregion

		#region clear bookmark command

		override public bool ClearBookmarkCommandCanExecute()
		{
			return Service.HasMark(Data);
		}

		override public void ClearBookmarkCommandExecute()
		{
			Service.ClearMark(Data);
		}

		#endregion

		#region fit mode command

		override public bool FitModeCommandCanExecute(string param)
		{
			return true;
		}

		override public void FitModeCommandExecute(string param)
		{
			if (param == "None")
			{
				FitMode = DisplayFitMode.None;
				//(_injectedView as XpsBookView).Viewer.FitToMaxPagesAcross(1);
			}
			if (param == "Width")
			{
				FitMode = DisplayFitMode.Width;
				(_injectedView as XpsBookView).Viewer.FitToWidth();
			}
			if (param == "Height")
			{
				FitMode = DisplayFitMode.Height;
				(_injectedView as XpsBookView).Viewer.FitToHeight();
			}
		}

		#endregion

		#region two page view command

		override public bool TwoPageCommandCanExecute()
		{
			return true;
		}
		
		private int pageCount = 1;

		override public void TwoPageCommandExecute()
		{
			if( pageCount == 1 )
				(_injectedView as XpsBookView).Viewer.FitToMaxPagesAcross(pageCount=2);
			else
				(_injectedView as XpsBookView).Viewer.FitToMaxPagesAcross(pageCount = 1);
		}

		#endregion

		#region change page command

		override public bool CanGotoPage(string step)
		{
			if (_injectedView == null) return false;
			if (step == "-1")
				return (_injectedView as XpsBookView).Viewer.CanGoToPreviousPage;
			else
				return (_injectedView as XpsBookView).Viewer.CanGoToNextPage;
		}

		override public void GotoPage(string step)
		{
			if (_injectedView == null) return;
			if (step == "-1")
				(_injectedView as XpsBookView).Viewer.PreviousPage();
			else
				(_injectedView as XpsBookView).Viewer.NextPage();
		}
		#endregion

		#region goto page command

		override public bool GotoPageCommandCanExecute(string param)
		{
			if (_injectedView == null) return false;
		
			return (_injectedView as XpsBookView).Viewer.CanGoToPage(Convert.ToInt32(param));
		}

		override public void GotoPageCommandExecute(string param)
		{
			(_injectedView as XpsBookView).Viewer.GoToPage(Convert.ToInt32(param));
		}

		#endregion

		#region goto last page command

		override public void GotoLastPageCommandExecute(string param)
		{
			(_injectedView as XpsBookView).Viewer.LastPage();
		}

		#endregion

		#endregion
    }
}
