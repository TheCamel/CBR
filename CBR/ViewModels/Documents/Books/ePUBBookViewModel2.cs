using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CBR.Core.Formats.ePUB;
using CBR.Core.Models;
using CBR.Core.Helpers;
using System.Collections;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Documents;
using CBR.Core.Services;
using CBR.ViewModels.Messages;
using System.Windows.Controls;
using CBR.Views;
using CBR.Components.Controls;
using System.Windows.Input;

namespace CBR.ViewModels
{
    public class ePUBBookViewModel2 : BookViewModelBase
    {
        #region ----------------CONSTRUCTOR----------------

        public ePUBBookViewModel2(Book bk)
            : base(bk)
        {
            this.Icon = "pack://application:,,,/Resources/Images/32x32/book_type/book_type_epub.png";
        }

        /// <summary>
        /// Child classes can override this method to perform clean-up logic, such as removing event handlers.
        /// </summary>
        override public void Cleanup()
        {
            base.Cleanup();
        }

        public void Load()
        {
            if (Data != null)
            {
                Service.LoadBook(Data);
                DocumentContent = (Service as ePUBBookService).ParseToDocument(Data.Tag as ePUB);
            }
        }

        #endregion

        #region -----------------PROPERTIES-----------------

        private ePUBBookView2 _injectedView = null;
        public ePUBBookView2 InjectedView { set { _injectedView = value; } }

        private IDocumentPaginatorSource _DocumentContent = null;
        public IDocumentPaginatorSource DocumentContent
        {
            get { return _DocumentContent; }
            set
            {
                if (_DocumentContent != value)
                {
                    _DocumentContent = value;
                    RaisePropertyChanged("DocumentContent");

                    Messenger.Default.Send<TocChangedNotification>(new TocChangedNotification((Data.Tag as ePUB).TocShortcut));
                }
            }
        }

        override public bool HasTableOfContent
        {
            get { return true; }
        }

        override public IList TableOfContent
        {
            get
            {
                if (Data.Tag != null)
                    return (Data.Tag as ePUB).Container.Package.TableOfContent.Items.Cast<object>().ToList();
                else return null;
            }
        }

        //override public object TableOfContentIndex
        //{
        //	set { DocumentContent =  ; }
        //}

        #endregion

        #region -----------------COMMANDS-----------------

        #region print command

        override public bool PrintCommandCanExecute()
        {
            return true;
        }

        override public void PrintCommandExecute()
        {
            _injectedView.Viewer.Print();
        }

        #endregion

        #region bookmark command

        override public bool BookmarkCommandCanExecute()
        {
            return true;
        }

        override public void BookmarkCommandExecute()
        {
            Service.SetMark(Data, _injectedView.Viewer.PageNumber.ToString());
        }

        #endregion

        #region goto bookmark command

        override public bool GotoBookmarkCommandCanExecute()
        {
            return Service.HasMark(Data);
        }

        override public void GotoBookmarkCommandExecute()
        {
            NavigationCommands.GoToPage.Execute(Convert.ToInt32(Service.GetMark(Data)), _injectedView.Viewer);
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

        //#region fit mode command

        //override public bool FitModeCommandCanExecute(string param)
        //{
        //    return true;
        //}

        //override public void FitModeCommandExecute(string param)
        //{
        //    if (param == "None")
        //    {
        //        FitMode = DisplayFitMode.None;
        //        //_injectedView.Viewer.FitToMaxPagesAcross(1);
        //    }
        //    if (param == "Width")
        //    {
        //        FitMode = DisplayFitMode.Width;
        //        _injectedView.Viewer.ViewingMode = FlowDocumentReaderViewingMode.Page();
        //    }
        //    if (param == "Height")
        //    {
        //        FitMode = DisplayFitMode.Height;
        //        _injectedView.Viewer.FitToHeight();
        //    }
        //}

        //#endregion

        #region two page view command

        override public bool TwoPageCommandCanExecute()
        {
            return true;
        }

        private FlowDocumentReaderViewingMode pageMode = FlowDocumentReaderViewingMode.TwoPage;

        override public void TwoPageCommandExecute()
        {
            if (pageMode == FlowDocumentReaderViewingMode.TwoPage)
                _injectedView.Viewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
            else
                _injectedView.Viewer.ViewingMode = FlowDocumentReaderViewingMode.TwoPage;
        }

        #endregion

        #region change page command

        override public bool CanGotoPage(string step)
        {
            if (_injectedView == null) return false;
            if (step == "-1")
                return _injectedView.Viewer.CanGoToPreviousPage;
            else
                return _injectedView.Viewer.CanGoToNextPage;
        }

        override public void GotoPage(string step)
        {
            if (_injectedView == null) return;
            if (step == "-1")
                NavigationCommands.PreviousPage.Execute(null, _injectedView.Viewer);
            else
                NavigationCommands.NextPage.Execute(null, _injectedView.Viewer);
        }
        #endregion

        #region goto page command

        override public bool GotoPageCommandCanExecute(string param)
        {
            if (_injectedView == null) return false;

            return _injectedView.Viewer.CanGoToPage(Convert.ToInt32(param));
        }

        override public void GotoPageCommandExecute(string param)
        {
            NavigationCommands.GoToPage.Execute(Convert.ToInt32(param), _injectedView.Viewer);
        }

        #endregion

        #region goto last page command

        override public void GotoLastPageCommandExecute(string param)
        {
            NavigationCommands.LastPage.Execute(Convert.ToInt32(param), _injectedView.Viewer);
        }

        #endregion

        #endregion
    }
}
